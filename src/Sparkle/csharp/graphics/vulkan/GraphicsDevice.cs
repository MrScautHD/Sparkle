using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace Sparkle.csharp.graphics.vulkan; 

public unsafe class GraphicsDevice : IDisposable {
    
    private readonly Vk _vk;
    private readonly IWindow _window;
    
    private KhrSurface _khrSurface;
    
    public SurfaceKHR Surface { get; private set; }
    public PhysicalDevice PhysicalDevice { get; private set; }
    public string DeviceName { get; private set; }
    public SampleCountFlags MsaaCount { get; private set; }
    public uint GraphicsFamilyIndex { get; private set; }

    private Instance _instance;
    public Instance Instance => this._instance;
    
    private Device _device;
    public Device VkDevice => this._device;
    
    private Queue _graphicsQueue;
    public Queue GraphicsQueue => this._graphicsQueue;

    private Queue _presentQueue;
    public Queue PresentQueue => this._presentQueue;

    private CommandPool _commandPool;
    public CommandPool CommandPool => this._commandPool;
    
    private readonly string[] _deviceExtensions = new string[] {
        KhrSwapchain.ExtensionName,
        KhrSynchronization2.ExtensionName,
    };

    public GraphicsDevice(Vk vk, IWindow window, SampleCountFlags msaaCount) {
        this._vk = vk;
        this._window = window;
        this.MsaaCount = msaaCount;
        this.DeviceName = "unknown";
        this.CreateInstance();
        this.CreateSurface();
        this.PickPhysicalDevice();
        this.CreateLogicalDevice();
        this.CreateCommandPool();
    }
    
    public PhysicalDeviceProperties GetProperties() {
        this._vk.GetPhysicalDeviceProperties(this.PhysicalDevice, out var properties);
        return properties;
    }

    public void CopyBuffer(Buffer srcBuffer, Buffer dstBuffer, ulong size) {
        CommandBuffer commandBuffer = this.BeginSingleTimeCommands();

        BufferCopy copyRegion = new() {
            Size = size,
        };

        this._vk.CmdCopyBuffer(commandBuffer, srcBuffer, dstBuffer, 1, copyRegion);

        this.EndSingleTimeCommands(commandBuffer);
    }

    public void CreateBuffer(ulong size, BufferUsageFlags usage, MemoryPropertyFlags properties, ref Buffer buffer, ref DeviceMemory bufferMemory) {
        BufferCreateInfo bufferInfo = new() {
            SType = StructureType.BufferCreateInfo,
            Size = size,
            Usage = usage,
            SharingMode = SharingMode.Exclusive,
        };

        fixed (Buffer* bufferPtr = &buffer) {
            if (this._vk.CreateBuffer(this._device, bufferInfo, null, bufferPtr) != Result.Success) {
                throw new Exception("Failed to create vertex buffer!");
            }
        }

        this._vk.GetBufferMemoryRequirements(this._device, buffer, out var memoryRequirements);

        MemoryAllocateInfo allocateInfo = new() {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memoryRequirements.Size,
            MemoryTypeIndex = FindMemoryType(memoryRequirements.MemoryTypeBits, properties),
        };

        fixed (DeviceMemory* bufferMemoryPtr = &bufferMemory) {
            if (this._vk.AllocateMemory(this._device, allocateInfo, null, bufferMemoryPtr) != Result.Success) {
                throw new Exception("Failed to allocate vertex buffer memory!");
            }
        }

        this._vk.BindBufferMemory(this._device, buffer, bufferMemory, 0);
    }
    
    private void CreateInstance() {
        ApplicationInfo appInfo = new() {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*) Marshal.StringToHGlobalAnsi(this._window.Title),
            PEngineName = (byte*) Marshal.StringToHGlobalAnsi("Sparkle"),
            ApplicationVersion = new Version32(1, 0, 0),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version13
        };

        InstanceCreateInfo createInfo = new() {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo
        };

        string[] extensions = this.GetRequiredExtensions();
        
        createInfo.EnabledExtensionCount = (uint) extensions.Length;
        createInfo.PpEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(extensions);
        createInfo.EnabledLayerCount = 0;
        createInfo.PNext = null;
        
        if (this._vk.CreateInstance(createInfo, null, out this._instance) != Result.Success) {
            throw new Exception("failed to create instance!");
        }

        Marshal.FreeHGlobal((IntPtr) appInfo.PApplicationName);
        Marshal.FreeHGlobal((IntPtr) appInfo.PEngineName);
        SilkMarshal.Free((nint) createInfo.PpEnabledExtensionNames);
    }

    private void CreateSurface() {
        if (!this._vk.TryGetInstanceExtension(this._instance, out this._khrSurface)) {
            throw new NotSupportedException("KHR_surface extension not found.");
        }

        if (this._window.VkSurface == null) {
            throw new ApplicationException("VkSurface is null!");
        }

        this.Surface = this._window.VkSurface.Create<AllocationCallbacks>(this._instance.ToHandle(), null).ToSurface();
    }


    private void PickPhysicalDevice() {
        uint deviceCount = 0;
        this._vk.EnumeratePhysicalDevices(this._instance, ref deviceCount, null);

        if (deviceCount == 0) {
            throw new Exception("Failed to find GPUs with Vulkan support!");
        }

        PhysicalDevice[] devices = new PhysicalDevice[deviceCount];
        
        fixed (PhysicalDevice* devicesPtr = devices) {
            this._vk.EnumeratePhysicalDevices(this._instance, ref deviceCount, devicesPtr);
        }

        foreach (PhysicalDevice physicalDevice in devices) {
            if (this.IsDeviceSuitable(physicalDevice)) {
                this.PhysicalDevice = physicalDevice;
                this.MsaaCount = this.GetMaxUsableSampleCount();
                break;
            }
        }

        if (this.PhysicalDevice.Handle == 0) {
            throw new Exception("Failed to find a suitable GPU!");
        }

        this._vk.GetPhysicalDeviceProperties(this.PhysicalDevice, out PhysicalDeviceProperties properties);
        this.DeviceName = this.GetStringFromBytePointer(properties.DeviceName, 50).Trim();
    }


    private string GetStringFromBytePointer(byte* pointer, int length) {
        Span<byte> span = new Span<byte>(pointer, length);
        return Encoding.UTF8.GetString(span);
    }

    private void CreateLogicalDevice() {
        QueueFamilyIndices indices = this.FindQueueFamilies(this.PhysicalDevice);

        uint[] uniqueQueueFamilies = new[] {
            indices.GraphicsFamily!.Value,
            indices.PresentFamily!.Value
        };

        uniqueQueueFamilies = uniqueQueueFamilies.Distinct().ToArray();

        this.GraphicsFamilyIndex = indices.GraphicsFamily.Value;

        using GlobalMemory memory = GlobalMemory.Allocate(uniqueQueueFamilies.Length * sizeof(DeviceQueueCreateInfo));
        var queueCreateInfos = (DeviceQueueCreateInfo*) Unsafe.AsPointer(ref memory.GetPinnableReference());

        float queuePriority = 1.0f;
        for (int i = 0; i < uniqueQueueFamilies.Length; i++) {
            queueCreateInfos[i] = new DeviceQueueCreateInfo() {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = uniqueQueueFamilies[i],
                QueueCount = 1
            };

            queueCreateInfos[i].PQueuePriorities = &queuePriority;
        }

        PhysicalDeviceFeatures deviceFeatures = new() {
            SamplerAnisotropy = true
        };

        PhysicalDeviceSynchronization2FeaturesKHR sync2Features = new() {
            SType = StructureType.PhysicalDeviceSynchronization2FeaturesKhr,
            Synchronization2 = Vk.True
        };

        PhysicalDeviceFeatures2 deviceFeatures2 = new() {
            SType = StructureType.PhysicalDeviceFeatures2,
            PNext = &sync2Features
        };
        
        this._vk.GetPhysicalDeviceFeatures2(this.PhysicalDevice, &deviceFeatures2);

        DeviceCreateInfo createInfo = new() {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint) uniqueQueueFamilies.Length,
            PQueueCreateInfos = queueCreateInfos,
            PEnabledFeatures = &deviceFeatures,
            PNext = &sync2Features,
            EnabledExtensionCount = (uint) this._deviceExtensions.Length,
            PpEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(this._deviceExtensions),
            EnabledLayerCount = 0
        };
        
        if (this._vk.CreateDevice(this.PhysicalDevice, in createInfo, null, out this._device) != Result.Success) {
            throw new Exception("Failed to create logical device!");
        }

        this._vk.GetDeviceQueue(this._device, indices.GraphicsFamily!.Value, 0, out this._graphicsQueue);
        this._vk.GetDeviceQueue(this._device, indices.PresentFamily!.Value, 0, out this._presentQueue);

        SilkMarshal.Free((nint) createInfo.PpEnabledExtensionNames);
    }


    private void CreateCommandPool() {
        CommandPoolCreateInfo poolInfo = new() {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = this.FindQueueFamilies(this.PhysicalDevice).GraphicsFamily!.Value,
            Flags = CommandPoolCreateFlags.TransientBit | CommandPoolCreateFlags.ResetCommandBufferBit
        };

        if (this._vk.CreateCommandPool(this._device, poolInfo, null, out this._commandPool) != Result.Success) {
            throw new Exception("Failed to create command pool!");
        }
    }

    private CommandBuffer BeginSingleTimeCommands() {
        CommandBufferAllocateInfo allocateInfo = new() {
            SType = StructureType.CommandBufferAllocateInfo,
            Level = CommandBufferLevel.Primary,
            CommandPool = this._commandPool,
            CommandBufferCount = 1,
        };

        this._vk.AllocateCommandBuffers(this._device, allocateInfo, out var commandBuffer);

        CommandBufferBeginInfo beginInfo = new() {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
        };

        this._vk.BeginCommandBuffer(commandBuffer, beginInfo);

        return commandBuffer;
    }

    private void EndSingleTimeCommands(CommandBuffer commandBuffer) {
        this._vk.EndCommandBuffer(commandBuffer);

        SubmitInfo submitInfo = new() {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer,
        };

        this._vk.QueueSubmit(this._graphicsQueue, 1, submitInfo, default);
        this._vk.QueueWaitIdle(this._graphicsQueue);
        
        this._vk.FreeCommandBuffers(this._device, this._commandPool, 1, commandBuffer);
    }

    private void PopulateDebugMessengerCreateInfo(ref DebugUtilsMessengerCreateInfoEXT createInfo) {
        createInfo.SType = StructureType.DebugUtilsMessengerCreateInfoExt;
        createInfo.MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt | DebugUtilsMessageSeverityFlagsEXT.WarningBitExt | DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt;
        createInfo.MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt | DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt | DebugUtilsMessageTypeFlagsEXT.ValidationBitExt;
        createInfo.PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT) this.DebugCallback;
    }

    private uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData) {
        if (messageSeverity == DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt) return Vk.False;
        
        string? msg = Marshal.PtrToStringAnsi((nint) pCallbackData -> PMessage);
        Debug.WriteLine($"{messageSeverity} | validation layer: {msg}");
        
        return Vk.False;
    }

    public struct SwapChainSupportDetails {
        public SurfaceCapabilitiesKHR Capabilities;
        public SurfaceFormatKHR[] Formats;
        public PresentModeKHR[] PresentModes;
    }

    public SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice physicalDevice) {
        SwapChainSupportDetails details = new SwapChainSupportDetails();

        this._khrSurface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, this.Surface, out details.Capabilities);

        uint formatCount = 0;
        this._khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, this.Surface, ref formatCount, null);

        if (formatCount != 0) {
            details.Formats = new SurfaceFormatKHR[formatCount];
            fixed (SurfaceFormatKHR* formatsPtr = details.Formats) {
                this._khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, this.Surface, ref formatCount, formatsPtr);
            }
        }
        else {
            details.Formats = Array.Empty<SurfaceFormatKHR>();
        }

        uint presentModeCount = 0;
        this._khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, this.Surface, ref presentModeCount, null);

        if (presentModeCount != 0) {
            details.PresentModes = new PresentModeKHR[presentModeCount];
            fixed (PresentModeKHR* formatsPtr = details.PresentModes) {
                this._khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, this.Surface, ref presentModeCount, formatsPtr);
            }
        }
        else {
            details.PresentModes = Array.Empty<PresentModeKHR>();
        }

        return details;
    }

    private bool IsDeviceSuitable(PhysicalDevice physicalDevice) {
        var indices = this.FindQueueFamilies(physicalDevice);

        bool extensionsSupported = this.CheckDeviceExtensionsSupport(physicalDevice);
        bool swapChainAdequate = false;
        
        if (extensionsSupported) {
            var swapChainSupport = this.QuerySwapChainSupport(physicalDevice);
            swapChainAdequate = swapChainSupport.Formats.Any() && swapChainSupport.PresentModes.Any();
        }

        this._vk.GetPhysicalDeviceFeatures(physicalDevice, out var deviceFeatures);

        PhysicalDeviceSynchronization2FeaturesKHR sync2Features = new() {
            SType = StructureType.PhysicalDeviceSynchronization2FeaturesKhr,
            Synchronization2 = Vk.True
        };

        PhysicalDeviceFeatures2 deviceFeatures2 = new() {
            SType = StructureType.PhysicalDeviceFeatures2,
            PNext = &sync2Features
        };

        this._vk.GetPhysicalDeviceFeatures2(physicalDevice, &deviceFeatures2);

        return indices.IsComplete() && extensionsSupported && swapChainAdequate && deviceFeatures.SamplerAnisotropy && sync2Features.Synchronization2;
    }

    private bool CheckDeviceExtensionsSupport(PhysicalDevice physicalDevice) {
        uint extensionCount = 0;
        this._vk.EnumerateDeviceExtensionProperties(physicalDevice, (byte*) null, ref extensionCount, null);

        var availableExtensions = new ExtensionProperties[extensionCount];
        fixed (ExtensionProperties* availableExtensionsPtr = availableExtensions) {
            this._vk.EnumerateDeviceExtensionProperties(physicalDevice, (byte*) null, ref extensionCount, availableExtensionsPtr);
        }

        var availableExtensionNames = availableExtensions.Select(extension => Marshal.PtrToStringAnsi((IntPtr) extension.ExtensionName)).ToHashSet();

        return this._deviceExtensions.All(availableExtensionNames.Contains);
    }

    public struct QueueFamilyIndices {
        public uint? GraphicsFamily { get; set; }
        public uint? PresentFamily { get; set; }
        
        public bool IsComplete() {
            return this.GraphicsFamily.HasValue && this.PresentFamily.HasValue;
        }
    }

    public QueueFamilyIndices FindQueueFamilies(PhysicalDevice physicalDevice) {
        QueueFamilyIndices indices = new QueueFamilyIndices();

        uint queueFamilyCount = 0;
        this._vk.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, ref queueFamilyCount, null);

        var queueFamilies = new QueueFamilyProperties[queueFamilyCount];
        fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies) {
            this._vk.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, ref queueFamilyCount, queueFamiliesPtr);
        }

        uint i = 0;
        foreach (var queueFamily in queueFamilies) {
            if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit)) {
                indices.GraphicsFamily = i;
            }

            this._khrSurface.GetPhysicalDeviceSurfaceSupport(physicalDevice, i, this.Surface, out var presentSupport);

            if (presentSupport) {
                indices.PresentFamily = i;
            }

            if (indices.IsComplete()) {
                break;
            }

            i++;
        }

        return indices;
    }


    private string[] GetRequiredExtensions() {
        var glfwExtensions = this._window.VkSurface!.GetRequiredExtensions(out var glfwExtensionCount);
        var extensions = SilkMarshal.PtrToStringArray((nint) glfwExtensions, (int) glfwExtensionCount);

        return extensions;
    }

    private SampleCountFlags GetMaxUsableSampleCount() {
        this._vk.GetPhysicalDeviceProperties(this.PhysicalDevice, out var physicalDeviceProperties);

        SampleCountFlags counts = physicalDeviceProperties.Limits.FramebufferColorSampleCounts & physicalDeviceProperties.Limits.FramebufferDepthSampleCounts;
        
        return counts switch {
            var c when (c & SampleCountFlags.Count64Bit) != 0 => SampleCountFlags.Count64Bit,
            var c when (c & SampleCountFlags.Count32Bit) != 0 => SampleCountFlags.Count32Bit,
            var c when (c & SampleCountFlags.Count16Bit) != 0 => SampleCountFlags.Count16Bit,
            var c when (c & SampleCountFlags.Count8Bit) != 0 => SampleCountFlags.Count8Bit,
            var c when (c & SampleCountFlags.Count4Bit) != 0 => SampleCountFlags.Count4Bit,
            var c when (c & SampleCountFlags.Count2Bit) != 0 => SampleCountFlags.Count2Bit,
            _ => SampleCountFlags.Count1Bit
        };
    }

    private Format FindSupportedFormat(IEnumerable<Format> candidates, ImageTiling tiling, FormatFeatureFlags features) {
        foreach (var format in candidates) {
            this._vk.GetPhysicalDeviceFormatProperties(this.PhysicalDevice, format, out var formatProperties);

            if (tiling == ImageTiling.Linear && (formatProperties.LinearTilingFeatures & features) == features) {
                return format;
            }
            else if (tiling == ImageTiling.Optimal && (formatProperties.OptimalTilingFeatures & features) == features) {
                return format;
            }
        }

        throw new Exception("failed to find supported format!");
    }

    public Format FindDepthFormat() {
        IEnumerable<Format> formats = new[] {
            Format.D32Sfloat,
            Format.D32SfloatS8Uint,
            Format.D24UnormS8Uint
        };
        
        return this.FindSupportedFormat(formats, ImageTiling.Optimal, FormatFeatureFlags.DepthStencilAttachmentBit);
    }

    public uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties) {
        this._vk.GetPhysicalDeviceMemoryProperties(this.PhysicalDevice, out var memoryProperties);

        for (int i = 0; i < memoryProperties.MemoryTypeCount; i++) {
            if ((typeFilter & (1 << i)) != 0 && (memoryProperties.MemoryTypes[i].PropertyFlags & properties) == properties) {
                return (uint) i;
            }
        }

        throw new Exception("failed to find suitable memory type!");
    }
    
    public void Dispose() {
        this._vk.DestroyCommandPool(this._device, this._commandPool, null);
        this._vk.DestroyDevice(this._device, null);
        GC.SuppressFinalize(this);
    }
}