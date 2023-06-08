using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Vulkan;

namespace Sparkle.csharp.graphics; 

public class VkGraphicsDevice : IDisposable {
    
    private Vk _vk;

    public VkGraphicsDevice() {
    }

    private unsafe void CreateInstance() {
        this._vk = Vk.GetApi();
        
        /*
        ApplicationInfo appInfo = new ApplicationInfo() {
            SType = StructureType.ApplicationInfo,
            PApplicationName =  (byte*) Marshal.StringToHGlobalAnsi("Hello Triangle"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = (byte*) Marshal.StringToHGlobalAnsi("No Engine"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version11
        };

        InstanceCreateInfo createInfo = new() {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo
        };

        var glfwExtensions = window!.VkSurface!.GetRequiredExtensions(out var glfwExtensionCount);

        createInfo.EnabledExtensionCount = glfwExtensionCount;
        createInfo.PpEnabledExtensionNames = glfwExtensions;
        createInfo.EnabledLayerCount = 0;

        if (this._vk.CreateInstance(createInfo, null, out instance) != Result.Success) {
            throw new Exception("failed to create instance!");
        }

        Marshal.FreeHGlobal((IntPtr)appInfo.PApplicationName);
        Marshal.FreeHGlobal((IntPtr)appInfo.PEngineName);*/
    }

    public void Dispose() {
        this._vk.Dispose();
    }
}