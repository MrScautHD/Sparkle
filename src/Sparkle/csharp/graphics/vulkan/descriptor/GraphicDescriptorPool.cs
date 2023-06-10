using Silk.NET.Vulkan;

namespace Sparkle.csharp.graphics.vulkan.descriptor; 

public unsafe class GraphicDescriptorPool : IDisposable {
    
    private readonly Vk _vk;
    private readonly GraphicsDevice _device;

    private DescriptorPool _descriptorPool;
    
    private DescriptorPoolSize[] _poolSizes;
    private DescriptorPoolCreateFlags _poolFlags;
    
    private uint _maxSets;

    public GraphicDescriptorPool(Vk vk, GraphicsDevice device, uint maxSets, DescriptorPoolCreateFlags poolFlags, DescriptorPoolSize[] poolSizes) {
        this._vk = vk;
        this._device = device;
        this._maxSets = maxSets;
        this._poolFlags = poolFlags;
        this._poolSizes = poolSizes;

        fixed (DescriptorPool* descriptorPool = &this._descriptorPool) {
            fixed (DescriptorPoolSize* descriptorPoolSizes = poolSizes) {
                DescriptorPoolCreateInfo descriptorPoolInfo = new() {
                    SType = StructureType.DescriptorPoolCreateInfo,
                    PoolSizeCount = (uint) poolSizes.Length,
                    PPoolSizes = descriptorPoolSizes,
                    MaxSets = maxSets,
                    Flags = poolFlags
                };

                if (this._vk.CreateDescriptorPool(this._device.VkDevice, &descriptorPoolInfo, null, descriptorPool) != Result.Success) {
                    throw new ApplicationException("Failed to create descriptor pool");
                }
            }
        }
    }

    public bool AllocateDescriptorSet(DescriptorSetLayout descriptorLayout, ref DescriptorSet descriptorSet) {
        DescriptorSetAllocateInfo allocInfo = new() {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = this._descriptorPool,
            PSetLayouts = &descriptorLayout,
            DescriptorSetCount = 1,
        };
        
        return this._vk.AllocateDescriptorSets(this._device.VkDevice, allocInfo, out descriptorSet) == Result.Success;
    }

    private void FreeDescriptors(ref DescriptorSet[] descriptors) {
        this._vk.FreeDescriptorSets(this._device.VkDevice, this._descriptorPool, descriptors);
    }

    private void ResetPool() {
        this._vk.ResetDescriptorPool(this._device.VkDevice, this._descriptorPool, 0);
    }

    public void Dispose() {
        this._vk.DestroyDescriptorPool(this._device.VkDevice, this._descriptorPool, null);
        GC.SuppressFinalize(this);
    }
}