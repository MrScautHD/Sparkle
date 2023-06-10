using System.Diagnostics;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace Sparkle.csharp.graphics.vulkan;

public class Renderer : IDisposable {

    private readonly Vk _vk;
    private readonly IWindow _window;
    private readonly GraphicsDevice _device;

    public SwapChain SwapChain { get; private set; }
    
    public bool IsFrameStarted { get; private set; }
    
    public CommandBuffer[] CommandBuffers { get; private set; }
    public int CurrentFrameIndex { get; private set; }

    private bool _framebufferResized;
    private uint _currentImageIndex;
    
    public Renderer(Vk vk, IWindow window, GraphicsDevice device, bool useFifo) {
        this._vk = vk;
        this._window = window;
        this._device = device;
        this.RecreateSwapChain(useFifo);
        this.CreateCommandBuffers();
    }

    private void RecreateSwapChain(bool useInfo) {
        Vector2D<int> frameBufferSize = this._window.FramebufferSize;
        
        while (frameBufferSize.X == 0 || frameBufferSize.Y == 0) {
            frameBufferSize = this. _window.FramebufferSize;
            this._window.DoEvents();
        }
        
        this._vk.DeviceWaitIdle(_device.VkDevice);
        
        if (this.SwapChain == null) {
            this.SwapChain = new SwapChain(this._vk, this._device, this.GetWindowExtents(), useInfo);
        }
        else {
            var oldImageFormat = this.SwapChain.SwapChainImageFormat;
            var oldDepthFormat = this.SwapChain.SwapChainDepthFormat;

            this.SwapChain.Dispose();
            this.SwapChain = new SwapChain(this._vk, this._device, this.GetWindowExtents(), useInfo);

            if (this.SwapChain.SwapChainImageFormat != oldImageFormat || this.SwapChain.SwapChainDepthFormat != oldDepthFormat) {
                throw new Exception("Swap chain image(or depth) format has changed!");
            }
        }
    }

    private unsafe void CreateCommandBuffers() {
        this.CommandBuffers = new CommandBuffer[this.SwapChain.ImageCount()];

        CommandBufferAllocateInfo allocInfo = new() {
            SType = StructureType.CommandBufferAllocateInfo,
            Level = CommandBufferLevel.Primary,
            CommandPool = this._device.CommandPool,
            CommandBufferCount = (uint) this.CommandBuffers.Length,
        };

        fixed (CommandBuffer* commandBuffersPtr = this.CommandBuffers) {
            if (this._vk.AllocateCommandBuffers(this._device.VkDevice, allocInfo, commandBuffersPtr) != Result.Success) {
                throw new Exception("Failed to allocate command buffers!");
            }
        }
    }
    
    private unsafe void FreeCommandBuffers() {
        fixed (CommandBuffer* commandBuffersPtr = this.CommandBuffers) {
            this._vk.FreeCommandBuffers(this._device.VkDevice, this._device.CommandPool, (uint) this.CommandBuffers.Length, commandBuffersPtr);
        }
        
        Array.Clear(this.CommandBuffers);
    }
    
    public CommandBuffer? BeginFrame() {
        Debug.Assert(!this.IsFrameStarted, "Can't call beginFrame while already in progress!");

        var result = this.SwapChain.AcquireNextImage(ref _currentImageIndex);

        if (result == Result.ErrorOutOfDateKhr) {
            this.RecreateSwapChain(this.SwapChain.UseFifo);
            return null;
        }
        else if (result != Result.Success && result != Result.SuboptimalKhr) {
            throw new Exception("Failed to acquire next swapchain image");
        }

        this.IsFrameStarted = true;

        CommandBuffer commandBuffer = this.CommandBuffers[this.CurrentFrameIndex];

        CommandBufferBeginInfo beginInfo = new() {
            SType = StructureType.CommandBufferBeginInfo,
        };

        if (this._vk.BeginCommandBuffer(commandBuffer, beginInfo) != Result.Success) {
            throw new Exception("Failed to begin recording command buffer!");
        }

        return commandBuffer;
    }

    public void EndFrame() {
        Debug.Assert(this.IsFrameStarted, "Can't call endFrame while frame is not in progress");

        CommandBuffer commandBuffer = this.CommandBuffers[this.CurrentFrameIndex];

        if (this._vk.EndCommandBuffer(commandBuffer) != Result.Success) {
            throw new Exception("Failed to record command buffer!");
        }

        var result = this.SwapChain.SubmitCommandBuffers(commandBuffer, this._currentImageIndex);
        
        if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr || this._framebufferResized) {
            this._framebufferResized = false;
            this.RecreateSwapChain(this.SwapChain.UseFifo);
        }
        else if (result != Result.Success) {
            throw new Exception("failed to submit command buffers");
        }

        this.IsFrameStarted = false;
        this.CurrentFrameIndex = (this.CurrentFrameIndex + 1) % SwapChain.MaxFramesInFlight;
    }

    public unsafe void BeginSwapChainRenderPass(CommandBuffer commandBuffer) {
        Debug.Assert(this.IsFrameStarted, "Can't call beginSwapChainRenderPass if frame is not in progress");
        Debug.Assert(commandBuffer.Handle == this.CommandBuffers[this.CurrentFrameIndex].Handle, "Can't begin render pass on command buffer from a different frame");

        RenderPassBeginInfo renderPassInfo = new() {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = this.SwapChain.RenderPass,
            Framebuffer = this.SwapChain.GetFrameBufferAt(this._currentImageIndex),
            RenderArea = new() {
                Offset = new() {
                    X = 0,
                    Y = 0
                },
                Extent = this.SwapChain.SwapChainExtent
            }
        };

        ClearValue[] clearValues = new ClearValue[] {
            new ClearValue() {
                Color = new() {
                    Float32_0 = 0.01f,
                    Float32_1 = 0.01f,
                    Float32_2 = 0.01f,
                    Float32_3 = 1
                }
            },
            new ClearValue() {
                DepthStencil = new() {
                    Depth = 1,
                    Stencil = 0
                }
            }
        };
        
        fixed (ClearValue* clearValuesPtr = clearValues) {
            renderPassInfo.ClearValueCount = (uint) clearValues.Length;
            renderPassInfo.PClearValues = clearValuesPtr;

            this._vk.CmdBeginRenderPass(commandBuffer, &renderPassInfo, SubpassContents.Inline);
        }

        Viewport viewport = new() {
            X = 0.0f,
            Y = 0.0f,
            Width = this.SwapChain.SwapChainExtent.Width,
            Height = this.SwapChain.SwapChainExtent.Height,
            MinDepth = 0.0f,
            MaxDepth = 1.0f,
        };
        
        Rect2D scissor = new(new Offset2D(), this.SwapChain.SwapChainExtent);
        
        this._vk.CmdSetViewport(commandBuffer, 0, 1, &viewport);
        this._vk.CmdSetScissor(commandBuffer, 0, 1, &scissor);
    }

    public void EndSwapChainRenderPass(CommandBuffer commandBuffer) {
        Debug.Assert(this.IsFrameStarted, "Can't call endSwapChainRenderPass if frame is not in progress");
        Debug.Assert(commandBuffer.Handle == CommandBuffers[this.CurrentFrameIndex].Handle, "Can't end render pass on command buffer from a different frame");

        this._vk.CmdEndRenderPass(commandBuffer);
    }

    private Extent2D GetWindowExtents() {
        return new Extent2D((uint) this._window.FramebufferSize.X, (uint) this._window.FramebufferSize.Y);
    }

    public void Dispose() {
        this.FreeCommandBuffers();
        GC.SuppressFinalize(this);
    }
}