using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Sparkle.csharp.graphics.vulkan;

public sealed unsafe class VulkanInstanceBuilder {
    
    public static VulkanInstanceBuilder CreateBuilder(IVkSurfaceSource surfaceSource) => new VulkanInstanceBuilder(Vk.GetApi(), surfaceSource);
    public static VulkanInstanceBuilder CreateBuilder(IVkSurfaceSource surfaceSource, Vk vk) => new VulkanInstanceBuilder(vk, surfaceSource);
    
    private readonly Vk _vk;
    private readonly IVkSurfaceSource _surfaceSource;

    private readonly List<string> _layers = new();
    private bool _enableValidationLayers;

    internal VulkanInstanceBuilder(Vk vk, IVkSurfaceSource surfaceSource) {
        this._vk = vk ?? throw new ArgumentNullException(nameof(vk));
        this._surfaceSource = surfaceSource ?? throw new ArgumentNullException(nameof(surfaceSource));
    }
    
    public VulkanInstanceBuilder EnableValidationLayers() => this.EnableValidationLayers(new[] {
        "VK_LAYER_KHRONOS_validation" 
    });
    
    public VulkanInstanceBuilder EnableValidationLayers(IEnumerable<string> validationLayers) {
        if (validationLayers == null) {
            throw new ArgumentNullException(nameof(validationLayers));
        }

        if (this._enableValidationLayers) return this;

        this._enableValidationLayers = true;
        this._layers.AddRange(validationLayers);
        
        return this;
    }

    public VulkanInstance Build() {
        if (!this.CheckLayerSupport()) {
            throw new Exception("Layers requested, but not available.");
        }

        ApplicationInfo appInfo = new() {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*) Marshal.StringToHGlobalAnsi(Application.Instance.Win.Title),
            PEngineName = (byte*) Marshal.StringToHGlobalAnsi("Sparkle"),
            ApiVersion = Vk.Version13,
            ApplicationVersion = new Version32(1, 0, 0),
            EngineVersion = new Version32(1, 0, 0)
        };

        string[] glfwExtensions = this.GetRequiredExtensions();

        InstanceCreateInfo createInfo = new InstanceCreateInfo() {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,

            EnabledExtensionCount = (uint) glfwExtensions.Length,
            PpEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(glfwExtensions),

            EnabledLayerCount = 0,
            PNext = null,
        };

        if (this._enableValidationLayers) {
            createInfo.EnabledLayerCount = (uint) this._layers.Count;
            createInfo.PpEnabledLayerNames = (byte**) SilkMarshal.StringArrayToPtr(this._layers);

            DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();
            this.debugUtilMessenger!.PopulateCreateInfo(ref debugCreateInfo);
            createInfo.PNext = &debugCreateInfo;
        }
        
        if (this._vk.CreateInstance(createInfo, null, out var instance) != Result.Success) {
            throw new Exception("Failed to create instance!");
        }

        Marshal.FreeHGlobal((IntPtr) appInfo.PApplicationName);
        Marshal.FreeHGlobal((IntPtr) appInfo.PEngineName);
        SilkMarshal.Free((nint) createInfo.PpEnabledExtensionNames);

        if (this._enableValidationLayers) {
            SilkMarshal.Free((nint) createInfo.PpEnabledLayerNames);
            this.debugUtilMessenger!.Build(this._vk, instance);
        }

        var result = new VulkanInstance(this._vk, instance, this.debugUtilMessenger);

        return result;
    }

    private string[] GetRequiredExtensions() {
        var surfaceExtensions = this._surfaceSource.VkSurface!.GetRequiredExtensions(out var surfaceExtensionCount);
        var extensions = SilkMarshal.PtrToStringArray((nint) surfaceExtensions, (int) surfaceExtensionCount);

        if (this._enableValidationLayers) {
            return extensions.Append(ExtDebugUtils.ExtensionName).ToArray();
        }

        return extensions;
    }

    private bool CheckLayerSupport() {
        if (!this._layers.Any()) return true;

        uint layerCount = 0;
        this._vk.EnumerateInstanceLayerProperties(ref layerCount, null);
        var availableLayers = new LayerProperties[layerCount];
        
        fixed (LayerProperties* pAvailableLayers = availableLayers) {
            this._vk.EnumerateInstanceLayerProperties(ref layerCount, pAvailableLayers);
        }
        
        var availableLayerNames = availableLayers.Select(layer => Marshal.PtrToStringAnsi((IntPtr) layer.LayerName)).ToHashSet();
        
        return this._layers.All(availableLayerNames.Contains);
    }
}