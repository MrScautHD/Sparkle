using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Sparkle.csharp.graphics.vulkan; 

public class SwapChain : IDisposable {
    
    public static int MaxFramesInFlight = 2;

    private readonly Vk _vk;
    private readonly GraphicsDevice _device;
    private readonly Device _vkDevice;

    private Image[] _depthImages;
    private DeviceMemory[] _depthImageMemorys;
    private ImageView[] _depthImageViews;

    private Image[] _colorImages;
    private DeviceMemory[] _colorImageMemorys;
    private ImageView[] _colorImageViews;

    private Extent2D _windowExtent;

    private Semaphore[] _imageAvailableSemaphores;
    private Semaphore[] _renderFinishedSemaphores;
    private Fence[] _inFlightFences;

    private Fence[] _imagesInFlight;
    private int _currentFrame;

    private SwapChain? _oldSwapChain;
    
    private KhrSwapchain _khrSwapChain;
    
    private Image[] _swapChainImages;
    
    private SwapchainKHR _swapChain;
    public SwapchainKHR VkSwapChain => this._swapChain;
    
    private RenderPass _renderPass;
    public RenderPass RenderPass => this._renderPass;
    
    public bool UseFifo { get; set; }

    public Format SwapChainImageFormat { get; private set; }

    public Format SwapChainDepthFormat { get; private set; }

    public Extent2D SwapChainExtent { get; private set; }
    
    public ImageView[] SwapChainImageViews { get; private set; }

    public Framebuffer[] SwapChainFrameBuffers { get; private set; }
    
    public float GetAspectRatio() => (float) this.SwapChainExtent.Width / this.SwapChainExtent.Height;
    public uint ImageCount() => (uint) this.SwapChainImageViews.Length;
    
    public Framebuffer GetFrameBufferAt(uint i) => this.SwapChainFrameBuffers[i];

    public SwapChain(Vk vk, GraphicsDevice device, Extent2D extent, bool useFifo) {
        this._vk = vk;
        this._device = device;
        this.UseFifo = useFifo;
        this._vkDevice = device.VkDevice;
        this._windowExtent = extent;
        this.CreateSwapChain();
        this.CreateImageViews();
        this.CreateRenderPass();
        this.CreateColorResources();
        this.CreateDepthResources();
        this.CreateFrameBuffers();
        this.CreateSyncObjects();
    }

    public Result AcquireNextImage(ref uint imageIndex) {
        this._vk.WaitForFences(this._device.VkDevice, 1, this._inFlightFences[this._currentFrame], true, ulong.MaxValue);
        return this._khrSwapChain.AcquireNextImage(this._device.VkDevice, this._swapChain, ulong.MaxValue, this._imageAvailableSemaphores[this._currentFrame], default, ref imageIndex);
    }

    public unsafe Result SubmitCommandBuffers(CommandBuffer commandBuffer, uint imageIndex) {
        if (this._imagesInFlight![imageIndex].Handle != default) {
            this._vk.WaitForFences(this._device.VkDevice, 1, this._imagesInFlight[imageIndex], true, ulong.MaxValue);
        }
        
        this._imagesInFlight[imageIndex] = this._inFlightFences[this._currentFrame];

        SubmitInfo submitInfo = new() {
            SType = StructureType.SubmitInfo
        };

        var waitSemaphores = stackalloc[] {
            this._imageAvailableSemaphores[this._currentFrame]
        };
        
        var waitStages = stackalloc[] {
            PipelineStageFlags.ColorAttachmentOutputBit
        };
        
        submitInfo = submitInfo with {
            WaitSemaphoreCount = 1,
            PWaitSemaphores = waitSemaphores,
            PWaitDstStageMask = waitStages,

            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer
        };

        var signalSemaphores = stackalloc[] {
            this._renderFinishedSemaphores[this._currentFrame]
        };
        
        submitInfo = submitInfo with {
            SignalSemaphoreCount = 1,
            PSignalSemaphores = signalSemaphores,
        };

        this._vk.ResetFences(this._device.VkDevice, 1, this._inFlightFences[this._currentFrame]);

        if (this._vk.QueueSubmit(_device.GraphicsQueue, 1, submitInfo, this._inFlightFences[this._currentFrame]) != Result.Success) {
            throw new Exception("Failed to submit draw command buffer!");
        }

        var swapChains = stackalloc[] {
            this._swapChain
        };
        
        PresentInfoKHR presentInfo = new() {
            SType = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = signalSemaphores,
            SwapchainCount = 1,
            PSwapchains = swapChains,
            PImageIndices = &imageIndex
        };

        var result = this._khrSwapChain.QueuePresent(this._device.PresentQueue, presentInfo);
        this._currentFrame = (this._currentFrame + 1) % MaxFramesInFlight;

        return result;
    }

    private unsafe void CreateSwapChain() {
        var swapChainSupport = this._device.QuerySwapChainSupport(this._device.PhysicalDevice);

        SurfaceFormatKHR surfaceFormat = this.ChooseSwapSurfaceFormat(swapChainSupport.Formats);
        PresentModeKHR presentMode = this.ChoosePresentMode(swapChainSupport.PresentModes);
        Extent2D extent = this.ChooseSwapExtent(swapChainSupport.Capabilities);

        uint imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
        
        if (swapChainSupport.Capabilities.MaxImageCount > 0 && imageCount > swapChainSupport.Capabilities.MaxImageCount) {
            imageCount = swapChainSupport.Capabilities.MaxImageCount;
        }

        SwapchainCreateInfoKHR creatInfo = new() {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _device.Surface,
            MinImageCount = imageCount,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
        };

        var indices = this._device.FindQueueFamilies(this._device.PhysicalDevice);
        
        var queueFamilyIndices = stackalloc[] {
            indices.GraphicsFamily!.Value,
            indices.PresentFamily!.Value
        };

        if (indices.GraphicsFamily != indices.PresentFamily) {
            creatInfo = creatInfo with {
                ImageSharingMode = SharingMode.Concurrent,
                QueueFamilyIndexCount = 2,
                PQueueFamilyIndices = queueFamilyIndices,
            };
        }
        else {
            creatInfo.ImageSharingMode = SharingMode.Exclusive;
        }

        creatInfo = creatInfo with {
            PreTransform = swapChainSupport.Capabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = presentMode,
            Clipped = true
        };

        if (this._khrSwapChain == null) {
            if (!this._vk.TryGetDeviceExtension(this._device.Instance, this._vkDevice, out this._khrSwapChain)) {
                throw new NotSupportedException("VK_KHR_swapchain extension not found.");
            }
        }

        creatInfo.OldSwapchain = this._oldSwapChain?.VkSwapChain ?? default;

        if (this._khrSwapChain.CreateSwapchain(this._vkDevice, creatInfo, null, out this._swapChain) != Result.Success) {
            throw new Exception("Failed to create swap chain!");
        }

        this._khrSwapChain.GetSwapchainImages(this._vkDevice, this._swapChain, ref imageCount, null);
        this._swapChainImages = new Image[imageCount];
        
        fixed (Image* swapChainImagesPtr = this._swapChainImages) {
            this._khrSwapChain.GetSwapchainImages(this._vkDevice, this._swapChain, ref imageCount, swapChainImagesPtr);
        }

        this.SwapChainImageFormat = surfaceFormat.Format;
        this.SwapChainExtent = extent;
    }


    private unsafe void CreateImageViews() {
        this.SwapChainImageViews = new ImageView[this._swapChainImages.Length];

        for (int i = 0; i < this._swapChainImages.Length; i++) {
            ImageViewCreateInfo createInfo = new() {
                SType = StructureType.ImageViewCreateInfo,
                Image = this._swapChainImages[i],
                ViewType = ImageViewType.Type2D,
                Format = this.SwapChainImageFormat,
                SubresourceRange = {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }
            };

            if (this._vk.CreateImageView(this._vkDevice, createInfo, null, out this.SwapChainImageViews[i]) != Result.Success) {
                throw new Exception("Failed to create image view!");
            }
        }
    }

    private unsafe void CreateRenderPass() {
        AttachmentDescription depthAttachment = new() {
            Format = _device.FindDepthFormat(),
            Samples = _device.MsaaCount,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.DontCare,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
        };

        AttachmentReference depthAttachmentRef = new() {
            Attachment = 1,
            Layout = ImageLayout.DepthStencilAttachmentOptimal,
        };

        AttachmentDescription colorAttachment = new() {
            Format = SwapChainImageFormat,
            Samples = _device.MsaaCount,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.ColorAttachmentOptimal,
        };

        AttachmentReference colorAttachmentRef = new() {
            Attachment = 0,
            Layout = ImageLayout.ColorAttachmentOptimal,
        };

        AttachmentDescription colorAttachmentResolve = new() {
            Format = SwapChainImageFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.DontCare,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr
        };

        AttachmentReference colorAttachmentResolveRef = new() {
            Attachment = 2,
            Layout = ImageLayout.AttachmentOptimalKhr
        };

        SubpassDescription subpass = new() {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachmentRef,
            PDepthStencilAttachment = &depthAttachmentRef,
            PResolveAttachments = &colorAttachmentResolveRef
        };

        SubpassDependency dependency = new() {
            DstSubpass = 0,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
            SrcSubpass = Vk.SubpassExternal,
            SrcAccessMask = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
        };

        var attachments = new[] {
            colorAttachment,
            depthAttachment,
            colorAttachmentResolve
        };

        fixed (AttachmentDescription* attachmentsPtr = attachments) {
            RenderPassCreateInfo renderPassInfo = new() {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = (uint) attachments.Length,
                PAttachments = attachmentsPtr,
                SubpassCount = 1,
                PSubpasses = &subpass,
                DependencyCount = 1,
                PDependencies = &dependency,
            };

            if (this._vk.CreateRenderPass(this._vkDevice, renderPassInfo, null, out this._renderPass) != Result.Success) {
                throw new Exception("Failed to create render pass!");
            }
        }
    }

    private unsafe void CreateFrameBuffers() {
        this.SwapChainFrameBuffers = new Framebuffer[this.SwapChainImageViews.Length];
        
        for (int i = 0; i < this.SwapChainImageViews.Length; i++) {
            var attachments = new[] {
                this._colorImageViews[i],
                this._depthImageViews[i],
                this.SwapChainImageViews[i]
            };

            fixed (ImageView* attachmentsPtr = attachments) {
                FramebufferCreateInfo framebufferInfo = new() {
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = this._renderPass,
                    AttachmentCount = (uint) attachments.Length,
                    PAttachments = attachmentsPtr,
                    Width = this.SwapChainExtent.Width,
                    Height = this.SwapChainExtent.Height,
                    Layers = 1,
                };

                if (this._vk!.CreateFramebuffer(this._device.VkDevice, framebufferInfo, null, out this.SwapChainFrameBuffers[i]) != Result.Success) {
                    throw new Exception("Failed to create framebuffer!");
                }
            }
        }
    }


    private unsafe void CreateColorResources() {
        Format colorFormat = this.SwapChainImageFormat;

        uint imageCount = ImageCount();
        this._colorImages = new Image[imageCount];
        this._colorImageMemorys = new DeviceMemory[imageCount];
        this._colorImageViews = new ImageView[imageCount];

        for (int i = 0; i < imageCount; i++) {
            ImageCreateInfo imageInfo = new() {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Extent = new() {
                    Width = this.SwapChainExtent.Width,
                    Height = this.SwapChainExtent.Height,
                    Depth = 1,
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Format = colorFormat,
                Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                Usage = ImageUsageFlags.TransientAttachmentBit | ImageUsageFlags.ColorAttachmentBit,
                Samples = this._device.MsaaCount,
                SharingMode = SharingMode.Exclusive,
                Flags = 0
            };

            fixed (Image* imagePtr = &this._colorImages[i]) {
                if (this._vk.CreateImage(this._vkDevice, imageInfo, null, imagePtr) != Result.Success) {
                    throw new Exception("Failed to create color image!");
                }
            }

            this._vk.GetImageMemoryRequirements(this._vkDevice, this._colorImages[i], out var memRequirements);

            MemoryAllocateInfo allocInfo = new() {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memRequirements.Size,
                MemoryTypeIndex = this._device.FindMemoryType(memRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
            };

            fixed (DeviceMemory* imageMemoryPtr = &this._colorImageMemorys[i]) {
                if (this._vk.AllocateMemory(this._vkDevice, allocInfo, null, imageMemoryPtr) != Result.Success) {
                    throw new Exception("Failed to allocate color image memory!");
                }
            }

            this._vk.BindImageMemory(this._vkDevice, this._colorImages[i], this._colorImageMemorys[i], 0);
            
            ImageViewCreateInfo createInfo = new() {
                SType = StructureType.ImageViewCreateInfo,
                Image = this._colorImages[i],
                ViewType = ImageViewType.Type2D,
                Format = colorFormat,
                SubresourceRange = new() {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }
            };

            if (this._vk.CreateImageView(this._vkDevice, createInfo, null, out this._colorImageViews[i]) != Result.Success) {
                throw new Exception("Failed to create color image views!");
            }
        }
    }

    private unsafe void CreateDepthResources() {
        Format depthFormat = this._device.FindDepthFormat();
        this.SwapChainDepthFormat = depthFormat;

        uint imageCount = ImageCount();
        this._depthImages = new Image[imageCount];
        this._depthImageMemorys = new DeviceMemory[imageCount];
        this._depthImageViews = new ImageView[imageCount];

        for (int i = 0; i < imageCount; i++) {
            ImageCreateInfo imageInfo = new() {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Extent = new() {
                    Width = SwapChainExtent.Width,
                    Height = SwapChainExtent.Height,
                    Depth = 1,
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Format = depthFormat,
                Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                Usage = ImageUsageFlags.DepthStencilAttachmentBit,
                Samples = _device.MsaaCount,
                SharingMode = SharingMode.Exclusive,
                Flags = 0
            };

            fixed (Image* imagePtr = &this._depthImages[i]) {
                if (this._vk.CreateImage(this._vkDevice, imageInfo, null, imagePtr) != Result.Success) {
                    throw new Exception("Failed to create depth image!");
                }
            }

            this._vk.GetImageMemoryRequirements(this._vkDevice, this._depthImages[i], out var memRequirements);

            MemoryAllocateInfo allocInfo = new() {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = memRequirements.Size,
                MemoryTypeIndex = this._device.FindMemoryType(memRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
            };

            fixed (DeviceMemory* imageMemoryPtr = &this._depthImageMemorys[i]) {
                if (this._vk.AllocateMemory(this._vkDevice, allocInfo, null, imageMemoryPtr) != Result.Success) {
                    throw new Exception("Failed to allocate depth image memory!");
                }
            }

            this._vk.BindImageMemory(this._vkDevice, this._depthImages[i], this._depthImageMemorys[i], 0);

            ImageViewCreateInfo createInfo = new() {
                SType = StructureType.ImageViewCreateInfo,
                Image = this._depthImages[i],
                ViewType = ImageViewType.Type2D,
                Format = depthFormat,
                SubresourceRange = new() {
                    AspectMask = ImageAspectFlags.DepthBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }
            };

            if (this._vk.CreateImageView(this._vkDevice, createInfo, null, out this._depthImageViews[i]) != Result.Success) {
                throw new Exception("Failed to create depth image views!");
            }
        }
    }
    
    private unsafe void CreateImage(uint width, uint height, uint mipLevels, SampleCountFlags numSamples, Format format, ImageTiling tiling, ImageUsageFlags usage, MemoryPropertyFlags properties, ref Image image, ref DeviceMemory imageMemory) {
        ImageCreateInfo imageInfo = new() {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Extent = new() {
                Width = width,
                Height = height,
                Depth = 1,
            },
            MipLevels = mipLevels,
            ArrayLayers = 1,
            Format = format,
            Tiling = tiling,
            InitialLayout = ImageLayout.Undefined,
            Usage = usage,
            Samples = numSamples,
            SharingMode = SharingMode.Exclusive,
        };

        fixed (Image* imagePtr = &image) {
            if (this._vk.CreateImage(this._vkDevice, imageInfo, null, imagePtr) != Result.Success) {
                throw new Exception("Failed to create image!");
            }
        }

        this._vk.GetImageMemoryRequirements(this._vkDevice, image, out var memRequirements);

        MemoryAllocateInfo allocInfo = new() {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = this._device.FindMemoryType(memRequirements.MemoryTypeBits, properties),
        };

        fixed (DeviceMemory* imageMemoryPtr = &imageMemory) {
            if (this._vk.AllocateMemory(this._vkDevice, allocInfo, null, imageMemoryPtr) != Result.Success) {
                throw new Exception("Failed to allocate image memory!");
            }
        }

        this._vk.BindImageMemory(this._vkDevice, image, imageMemory, 0);
    }

    private unsafe void CreateSyncObjects() {
        this._imageAvailableSemaphores = new Semaphore[MaxFramesInFlight];
        this._renderFinishedSemaphores = new Semaphore[MaxFramesInFlight];
        this._inFlightFences = new Fence[MaxFramesInFlight];
        this._imagesInFlight = new Fence[this._swapChainImages.Length];

        SemaphoreCreateInfo semaphoreInfo = new() {
            SType = StructureType.SemaphoreCreateInfo,
        };

        FenceCreateInfo fenceInfo = new() {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit,
        };

        for (var i = 0; i < MaxFramesInFlight; i++) {
            if (this._vk.CreateSemaphore(this._vkDevice, semaphoreInfo, null, out this._imageAvailableSemaphores[i]) != Result.Success ||
                this._vk.CreateSemaphore(this._vkDevice, semaphoreInfo, null, out this._renderFinishedSemaphores[i]) != Result.Success ||
                this._vk.CreateFence(this._vkDevice, fenceInfo, null, out this._inFlightFences[i]) != Result.Success) {
                throw new Exception("Failed to create synchronization objects for a frame!");
            }
        }
    }


    private SurfaceFormatKHR ChooseSwapSurfaceFormat(IReadOnlyList<SurfaceFormatKHR> availableFormats) {
        foreach (var availableFormat in availableFormats) {
            if (availableFormat.Format == Format.B8G8R8A8Srgb && availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr) {
                return availableFormat;
            }
        }

        return availableFormats[0];
    }

    private PresentModeKHR ChoosePresentMode(IReadOnlyList<PresentModeKHR> availablePresentModes) {
        if (this.UseFifo) return PresentModeKHR.FifoKhr;

        foreach (var availablePresentMode in availablePresentModes) {
            if (availablePresentMode == PresentModeKHR.MailboxKhr) {
                return availablePresentMode;
            }
        }

        return PresentModeKHR.FifoKhr;
    }

    private Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities) {
        if (capabilities.CurrentExtent.Width != uint.MaxValue) {
            return capabilities.CurrentExtent;
        }
        else {
            Extent2D framebufferSize = this._windowExtent;

            Extent2D actualExtent = new() {
                Width = framebufferSize.Width,
                Height = framebufferSize.Height
            };

            actualExtent.Width = Math.Clamp(actualExtent.Width, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width);
            actualExtent.Height = Math.Clamp(actualExtent.Height, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height);

            return actualExtent;
        }
    }

    public unsafe void Dispose() {
        foreach (var framebuffer in this.SwapChainFrameBuffers) {
            this._vk.DestroyFramebuffer(this._device.VkDevice, framebuffer, null);
        }

        foreach (var imageView in this.SwapChainImageViews) {
            this._vk.DestroyImageView(this._device.VkDevice, imageView, null);
        }
        
        Array.Clear(this.SwapChainImageViews);

        for (int i = 0; i < this._depthImages.Length; i++) {
            this._vk.DestroyImageView(this._device.VkDevice, this._depthImageViews[i], null);
            this._vk.DestroyImage(this._device.VkDevice, this._depthImages[i], null);
            this._vk.FreeMemory(this._device.VkDevice, this._depthImageMemorys[i], null);
        }

        for (int i = 0; i < this._colorImages.Length; i++) {
            this._vk.DestroyImageView(this._device.VkDevice, this._colorImageViews[i], null);
            this._vk.DestroyImage(this._device.VkDevice, this._colorImages[i], null);
            this._vk.FreeMemory(this._device.VkDevice, this._colorImageMemorys[i], null);
        }

        this._vk.DestroyRenderPass(this._device.VkDevice, this._renderPass, null);

        for (int i = 0; i < MaxFramesInFlight; i++) {
            this._vk.DestroySemaphore(this._device.VkDevice, this._renderFinishedSemaphores[i], null);
            this._vk.DestroySemaphore(this._device.VkDevice, this._imageAvailableSemaphores[i], null);
            this._vk.DestroyFence(this._device.VkDevice, this._inFlightFences[i], null);
        }

        this._khrSwapChain.DestroySwapchain(this._device.VkDevice, this._swapChain, null);
        this._swapChain = default;

        GC.SuppressFinalize(this);
    }
}