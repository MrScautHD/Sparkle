using Silk.NET.Vulkan;

namespace Sparkle.csharp.graphics.vulkan.descriptor; 

public unsafe class DescriptorLayout : IDisposable {
    
    private readonly Vk _vk;
    private readonly GraphicsDevice _device;
    
    public readonly DescriptorSetLayout Layout;
    public readonly Dictionary<uint, DescriptorSetLayoutBinding> Bindings;

    public DescriptorLayout(Vk vk, GraphicsDevice device, Dictionary<uint, DescriptorSetLayoutBinding> bindings) {
        this._vk = vk;
        this._device = device;
        this.Bindings = bindings;

        fixed (DescriptorSetLayoutBinding* layoutBinding = this.Bindings.Values.ToArray()) {
            DescriptorSetLayoutCreateInfo createInfo = new() {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = (uint) this.Bindings.Count,
                PBindings = layoutBinding
            };

            if (vk.CreateDescriptorSetLayout(device.VkDevice, &createInfo, null, out this.Layout) != Result.Success) {
                throw new ApplicationException($"Failed to create descriptor set layout");
            }
        }
    }

    public void Dispose() {
        this._vk.DestroyDescriptorSetLayout(this._device.VkDevice, this.Layout, null);
        GC.SuppressFinalize(this);
    }
}