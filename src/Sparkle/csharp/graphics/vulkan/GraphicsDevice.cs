using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace Sparkle.csharp.graphics.vulkan; 

public unsafe class GraphicsDevice : IDisposable {

    public IWindow Window => Application.Instance.Win;
    
    private Vk _vk;
    private Instance _instance;

    private readonly bool _enableValidationLayers;

    private readonly string[] _validationLayers = new[] {
        "VK_LAYER_KHRONOS_validation"
    };
    
    public GraphicsDevice() {
        this._enableValidationLayers = true;
        this._vk = Vk.GetApi();
    }

    private void CreateInstance() {
        ApplicationInfo appInfo = new() {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*) Marshal.StringToHGlobalAnsi(this.Window.Title),
            PEngineName = (byte*) Marshal.StringToHGlobalAnsi("Sparkle"),
            ApiVersion = Vk.Version13,
            ApplicationVersion = new Version32(1, 0, 0),
            EngineVersion = new Version32(1, 0, 0)
        };

        InstanceCreateInfo createInfo = new InstanceCreateInfo() {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo
        };

        var glfwExtensions = this.Window.VkSurface!.GetRequiredExtensions(out var glfwExtensionCount);

        createInfo.EnabledExtensionCount = glfwExtensionCount;
        createInfo.PpEnabledExtensionNames = glfwExtensions;

        if (this._enableValidationLayers) {
            createInfo.EnabledLayerCount = (uint) this._validationLayers.Length;
            createInfo.PpEnabledLayerNames = (byte**) SilkMarshal.StringArrayToPtr(this._validationLayers);

            DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();
            PopulateDebugMessengerCreateInfo(ref debugCreateInfo);
            createInfo.PNext = &debugCreateInfo;
        } 
        else {
            createInfo.EnabledLayerCount = 0;
            createInfo.PNext = null;
        }
        
        if (this._vk.CreateInstance(createInfo, null, out this._instance) != Result.Success) {
            throw new Exception("Failed to create instance!");
        }

        Marshal.FreeHGlobal((IntPtr) appInfo.PApplicationName);
        Marshal.FreeHGlobal((IntPtr) appInfo.PEngineName);
        SilkMarshal.Free((nint) createInfo.PpEnabledExtensionNames);
        
        if (this._enableValidationLayers) {
            SilkMarshal.Free((nint) createInfo.PpEnabledLayerNames);
        }
    }
    
    public void Dispose() {
        this._vk.DestroyInstance(this._instance, null);
        this._vk.Dispose();
    }
}