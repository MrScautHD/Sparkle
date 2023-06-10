using Silk.NET.Vulkan;

namespace Sparkle.csharp.graphics.vulkan.descriptor; 

public class GraphicDescriptorPoolBuilder {
    
    private readonly Vk _vk;
    private readonly GraphicsDevice _device;
    
    private readonly List<DescriptorPoolSize> _poolSizes = new();
    private DescriptorPoolCreateFlags _poolFlags;
    private uint _maxSets;

    public GraphicDescriptorPoolBuilder(Vk vk, GraphicsDevice device) {
        this._vk = vk;
        this._device = device;
    }
        
    public GraphicDescriptorPoolBuilder AddPoolSize(DescriptorType descriptorType, uint count) {
        this._poolSizes.Add(new DescriptorPoolSize(descriptorType, count));
        return this;
    }

    public GraphicDescriptorPoolBuilder SetPoolFlags(DescriptorPoolCreateFlags flags) {
        this._poolFlags = flags;
        return this;
    }
    public GraphicDescriptorPoolBuilder SetMaxSets(uint count) {
        this._maxSets = count;
        return this;
    }

    public GraphicDescriptorPool Build() {
        return new GraphicDescriptorPool(this._vk, this._device, this._maxSets, this._poolFlags, this._poolSizes.ToArray());
    }
}