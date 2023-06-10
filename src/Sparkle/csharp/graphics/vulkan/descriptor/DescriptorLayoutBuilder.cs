using System.Runtime.CompilerServices;
using Silk.NET.Vulkan;

namespace Sparkle.csharp.graphics.vulkan.descriptor;

public unsafe class DescriptorLayoutBuilder {
    
    private readonly GraphicsDevice _device;
    private readonly Vk _vk;

    private readonly Dictionary<uint, DescriptorSetLayoutBinding> _bindings = new();

    public DescriptorLayoutBuilder(Vk vk, GraphicsDevice device) {
        this._vk = vk;
        this._device = device;
    }

    public DescriptorLayoutBuilder AddBinding(uint binding, DescriptorType descriptorType, ShaderStageFlags stageFlags, uint count = 1) {
        if (this._bindings.ContainsKey(binding)) {
            throw new ApplicationException($"Binding {binding} is already in use, can't add");
        }

        DescriptorSetLayoutBinding layoutBinding = new() {
            Binding = binding,
            DescriptorType = descriptorType,
            DescriptorCount = count,
            StageFlags = stageFlags
        };

        this._bindings[binding] = layoutBinding;
        return this;
    }

    public DescriptorLayoutBuilder AddBinding(uint binding, DescriptorType descriptorType, ShaderStageFlags stageFlags, Sampler sampler, uint count = 1) {
        if (this._bindings.ContainsKey(binding)) {
            throw new ApplicationException($"Binding {binding} is already in use, can't add");
        }

        DescriptorSetLayoutBinding layoutBinding = new() {
            Binding = binding,
            DescriptorType = descriptorType,
            DescriptorCount = count,
            StageFlags = stageFlags,
            PImmutableSamplers = (Sampler*)Unsafe.AsPointer(ref sampler)
        };

        this._bindings[binding] = layoutBinding;
        return this;
    }

    public DescriptorLayout Build() {
        return new DescriptorLayout(this._vk, this._device, this._bindings);
    }
}