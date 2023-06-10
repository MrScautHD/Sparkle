using Silk.NET.Vulkan;

namespace Sparkle.csharp.graphics.vulkan.descriptor; 

public unsafe class DescriptorWriter {

    private readonly Vk _vk;
    private readonly GraphicsDevice _device;
    private readonly DescriptorLayout _layout;

    private WriteDescriptorSet[] _writes = Array.Empty<WriteDescriptorSet>();

    public DescriptorWriter(Vk vk, GraphicsDevice device, DescriptorLayout layout) {
        this._vk = vk;
        this._device = device;
        this._layout = layout;
    }

    public DescriptorWriter WriteBuffer(uint binding, DescriptorBufferInfo bufferInfo) {
        if (!this._layout.Bindings.ContainsKey(binding)) {
            throw new ApplicationException($"Layout does not contain the specified binding at {binding}");
        }

        var bindingDescription = this._layout.Bindings[binding];

        if (bindingDescription.DescriptorCount > 1) {
            throw new ApplicationException($"Binding single descriptor info, but binding expects multiple");
        }

        WriteDescriptorSet write = new() {
            SType = StructureType.WriteDescriptorSet,
            DescriptorType = bindingDescription.DescriptorType,
            DstBinding = binding,
            PBufferInfo = &bufferInfo,
            DescriptorCount = 1,
        };

        int length = this._writes.Length;
        Array.Resize(ref this._writes, length + 1);
        this._writes[length] = write;
        
        return this;
    }

    public DescriptorWriter WriteImage(uint binding, DescriptorImageInfo imageInfo) {
        if (!this._layout.Bindings.ContainsKey(binding)) {
            throw new ApplicationException($"Layout does not contain the specified binding at {binding}");
        }

        var bindingDescription = this._layout.Bindings[binding];

        if (bindingDescription.DescriptorCount > 1) {
            throw new ApplicationException("Binding single descriptor info, but binding expects multiple");
        }

        WriteDescriptorSet write = new() {
            SType = StructureType.WriteDescriptorSet,
            DescriptorType = bindingDescription.DescriptorType,
            DstBinding = binding,
            PImageInfo = &imageInfo,
            DescriptorCount = 1,
        };

        int length = this._writes.Length;
        Array.Resize(ref this._writes, length + 1);
        this._writes[length] = write;
        
        return this;
    }

    public bool Build(GraphicDescriptorPool graphicDescriptorPool, ref DescriptorSet descriptorSet) {
        if (!graphicDescriptorPool.AllocateDescriptorSet(this._layout.Layout, ref descriptorSet)) {
            return false;
        }
        
        this.Overwrite(ref descriptorSet);
        return true;
    }

    private void Overwrite(ref DescriptorSet descriptorSet) {
        for (var i = 0; i < this._writes.Length; i++) {
            this._writes[i].DstSet = descriptorSet;
        }
        
        fixed (WriteDescriptorSet* writesPtr = this._writes) {
            _vk.UpdateDescriptorSets(_device.VkDevice, (uint) this._writes.Length, writesPtr, 0, null);
        }
    }
}