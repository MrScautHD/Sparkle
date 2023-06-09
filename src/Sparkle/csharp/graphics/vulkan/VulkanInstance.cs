using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Sparkle.csharp.graphics.vulkan; 

public unsafe partial class VulkanInstance : IDisposable {
    
    public Vk Vk { get; }
    public Instance Native { get; }
    public VulkanDebugUtilMessenger? DebugUtilMessenger { get; }

    public VkHandle ToHandle() => Native.ToHandle();

    public static implicit operator Instance(VulkanInstance instance) => instance.Native;
    
    public VulkanInstance(Vk api, Instance native, VulkanDebugUtilMessenger? debugUtilMessenger) {
        Vk = api;
        Native = native;
        DebugUtilMessenger = debugUtilMessenger;
    }

    public void Dispose() {
        DebugUtilMessenger?.Dispose();
        Vk.DestroyInstance(Native, null);
        Vk.Dispose();
    }
}