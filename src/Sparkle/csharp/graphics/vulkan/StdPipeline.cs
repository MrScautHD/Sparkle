using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Sparkle.csharp.graphics.vulkan.pipeline;
using Sparkle.csharp.graphics.vulkan.util;

namespace Sparkle.csharp.graphics.vulkan; 

public class StdPipeline : IDisposable {
    
    private readonly Vk _vk;
    private readonly GraphicsDevice _device;

    private Pipeline _graphicsPipeline;
    public Pipeline GraphicsPipeline => this._graphicsPipeline;

    private ShaderModule _vertShaderModule;
    private ShaderModule _fragShaderModule;
    private ShaderModule _taskShaderModule;
    private ShaderModule _meshShaderModule;

    public StdPipeline(Vk vk, GraphicsDevice device, string vertPath, string fragPath, PipelineConfigInfo configInfo, string renderSystemName = "unknown") {
        this._vk = vk;
        this._device = device;
        this.CreateGraphicsPipelineStd(vertPath, fragPath, configInfo, renderSystemName);
    }

    public void Bind(CommandBuffer commandBuffer) {
        this._vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, this._graphicsPipeline);
    }

    private unsafe void CreateGraphicsPipelineStd(string vertPath, string fragPath, PipelineConfigInfo configInfo, string renderSystemName) {
        byte[] vertBytes = ShaderUtil.GetShaderBytes(vertPath, renderSystemName);
        byte[] fragBytes = ShaderUtil.GetShaderBytes(fragPath, renderSystemName);
        this._vertShaderModule = this.CreateShaderModule(vertBytes);
        this._fragShaderModule = this.CreateShaderModule(fragBytes);

        PipelineShaderStageCreateInfo vertShaderStageInfo = new() {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.VertexBit,
            Module = this._vertShaderModule,
            PName = (byte*) SilkMarshal.StringToPtr("main"),
            Flags = PipelineShaderStageCreateFlags.None,
            PNext = null,
            PSpecializationInfo = null,
        };

        PipelineShaderStageCreateInfo fragShaderStageInfo = new() {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.FragmentBit,
            Module = this._fragShaderModule,
            PName = (byte*) SilkMarshal.StringToPtr("main"),
            Flags = PipelineShaderStageCreateFlags.None,
            PNext = null,
            PSpecializationInfo = null,
        };

        var shaderStages = stackalloc[] {
            vertShaderStageInfo,
            fragShaderStageInfo
        };

        VertexInputBindingDescription[] bindingDescriptions = configInfo.BindingDescriptions;
        VertexInputAttributeDescription[] attributeDescriptions = configInfo.AttributeDescriptions;

        fixed (VertexInputBindingDescription* bindingDescriptionsPtr = bindingDescriptions) {
            fixed (VertexInputAttributeDescription* attributeDescriptionsPtr = attributeDescriptions) {

                PipelineVertexInputStateCreateInfo vertexInputInfo = new() {
                    SType = StructureType.PipelineVertexInputStateCreateInfo,
                    VertexAttributeDescriptionCount = (uint) attributeDescriptions.Length,
                    VertexBindingDescriptionCount = (uint) bindingDescriptions.Length,
                    PVertexAttributeDescriptions = attributeDescriptionsPtr,
                    PVertexBindingDescriptions = bindingDescriptionsPtr,
                };

                Span<DynamicState> dynamicStates = stackalloc DynamicState[] {
                    DynamicState.Viewport,
                    DynamicState.Scissor
                };
                
                PipelineDynamicStateCreateInfo dynamicState = new() {
                    SType = StructureType.PipelineDynamicStateCreateInfo,
                    DynamicStateCount = (uint) dynamicStates.Length,
                    PDynamicStates = (DynamicState*) Unsafe.AsPointer(ref dynamicStates[0])
                };

                GraphicsPipelineCreateInfo pipelineInfo = new() {
                    SType = StructureType.GraphicsPipelineCreateInfo,
                    StageCount = 2,
                    PStages = shaderStages,
                    PVertexInputState = &vertexInputInfo,
                    PInputAssemblyState = &configInfo.InputAssemblyInfo,
                    PViewportState = &configInfo.ViewportInfo,
                    PRasterizationState = &configInfo.RasterizationInfo,
                    PMultisampleState = &configInfo.MultisampleInfo,
                    PColorBlendState = &configInfo.ColorBlendInfo,
                    PDepthStencilState = &configInfo.DepthStencilInfo,
                    PDynamicState = (PipelineDynamicStateCreateInfo*) Unsafe.AsPointer(ref dynamicState),
                    Layout = configInfo.PipelineLayout,
                    RenderPass = configInfo.RenderPass,
                    Subpass = configInfo.SubPass,
                    BasePipelineIndex = -1,
                    BasePipelineHandle = default
                };

                if (this._vk.CreateGraphicsPipelines(this._device.VkDevice, default, 1, pipelineInfo, default, out this._graphicsPipeline) != Result.Success) {
                    throw new Exception("Failed to create graphics pipeline!");
                }
            }
        }

        this._vk.DestroyShaderModule(this._device.VkDevice, this._fragShaderModule, null);
        this._vk.DestroyShaderModule(this._device.VkDevice, this._vertShaderModule, null);

        SilkMarshal.Free((nint) shaderStages[0].PName);
        SilkMarshal.Free((nint) shaderStages[1].PName);
    }
    
    private unsafe void CreateGraphicsPipelineMesh(string taskPath, string meshPath, string fragPath, PipelineConfigInfo configInfo, string renderSystemName) {
        byte[] taskBytes = ShaderUtil.GetShaderBytes(taskPath, renderSystemName);
        byte[] meshBytes = ShaderUtil.GetShaderBytes(meshPath, renderSystemName);
        byte[] fragBytes = ShaderUtil.GetShaderBytes(fragPath, renderSystemName);
        this._taskShaderModule = this.CreateShaderModule(taskBytes);
        this._meshShaderModule = this.CreateShaderModule(meshBytes);
        this._fragShaderModule = this.CreateShaderModule(fragBytes);

        PipelineShaderStageCreateInfo meshShaderStageInfo = new() {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.MeshBitNV,
            Module = this._meshShaderModule,
            PName = (byte*) SilkMarshal.StringToPtr("main"),
            Flags = PipelineShaderStageCreateFlags.None,
            PNext = null,
            PSpecializationInfo = null,
        };

        PipelineShaderStageCreateInfo taskShaderStageInfo = new() {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.TaskBitNV,
            Module = this._taskShaderModule,
            PName = (byte*) SilkMarshal.StringToPtr("main"),
            Flags = PipelineShaderStageCreateFlags.None,
            PNext = null,
            PSpecializationInfo = null,
        };

        PipelineShaderStageCreateInfo fragShaderStageInfo = new() {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.FragmentBit,
            Module = this._fragShaderModule,
            PName = (byte*) SilkMarshal.StringToPtr("main"),
            Flags = PipelineShaderStageCreateFlags.None,
            PNext = null,
            PSpecializationInfo = null,
        };

        var shaderStages = stackalloc[] {
            meshShaderStageInfo,
            taskShaderStageInfo,
            fragShaderStageInfo
        };
        
        Span<DynamicState> dynamicStates = stackalloc DynamicState[] {
            DynamicState.Viewport,
            DynamicState.Scissor
        };
        
        PipelineDynamicStateCreateInfo dynamicState = new() {
            SType = StructureType.PipelineDynamicStateCreateInfo,
            DynamicStateCount = (uint) dynamicStates.Length,
            PDynamicStates = (DynamicState*) Unsafe.AsPointer(ref dynamicStates[0])
        };

        var pipelineInfo = new GraphicsPipelineCreateInfo() {
            SType = StructureType.GraphicsPipelineCreateInfo,
            StageCount = 3,
            PStages = shaderStages,
            PVertexInputState = null,
            PInputAssemblyState = null,
            PViewportState = &configInfo.ViewportInfo,
            PRasterizationState = &configInfo.RasterizationInfo,
            PMultisampleState = &configInfo.MultisampleInfo,
            PColorBlendState = &configInfo.ColorBlendInfo,
            PDepthStencilState = &configInfo.DepthStencilInfo,
            PDynamicState = (PipelineDynamicStateCreateInfo*) Unsafe.AsPointer(ref dynamicState),
            Layout = configInfo.PipelineLayout,
            RenderPass = configInfo.RenderPass,
            Subpass = configInfo.SubPass,
            BasePipelineIndex = -1,
            BasePipelineHandle = default
        };

        if (this._vk.CreateGraphicsPipelines(this._device.VkDevice, default, 1, pipelineInfo, default, out this._graphicsPipeline) != Result.Success) {
            throw new Exception("Failed to create graphics pipeline!");
        }
        
        this._vk.DestroyShaderModule(this._device.VkDevice, this._meshShaderModule, null);
        this._vk.DestroyShaderModule(this._device.VkDevice, this._taskShaderModule, null);
        this._vk.DestroyShaderModule(this._device.VkDevice, this._fragShaderModule, null);

        SilkMarshal.Free((nint) shaderStages[0].PName);
        SilkMarshal.Free((nint) shaderStages[1].PName);
        SilkMarshal.Free((nint) shaderStages[2].PName);
    }
    
    private unsafe ShaderModule CreateShaderModule(byte[] code) {
        ShaderModuleCreateInfo createInfo = new() {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (nuint) code.Length,
        };

        ShaderModule shaderModule;

        fixed (byte* codePtr = code) {
            createInfo.PCode = (uint*) codePtr;

            if (this._vk.CreateShaderModule(this._device.VkDevice, createInfo, null, out shaderModule) != Result.Success) {
                throw new Exception("Failed to create shader module!");
            }
        }

        return shaderModule;
    }

    public static unsafe void DefaultPipelineConfigInfo(ref PipelineConfigInfo configInfo) {
        configInfo.InputAssemblyInfo.SType = StructureType.PipelineInputAssemblyStateCreateInfo;
        configInfo.InputAssemblyInfo.Topology = PrimitiveTopology.TriangleList;
        configInfo.InputAssemblyInfo.PrimitiveRestartEnable = Vk.False;

        configInfo.ViewportInfo.SType = StructureType.PipelineViewportStateCreateInfo;
        configInfo.ViewportInfo.ViewportCount = 1;
        configInfo.ViewportInfo.PViewports = default;
        configInfo.ViewportInfo.ScissorCount = 1;
        configInfo.ViewportInfo.PScissors = default;
        
        configInfo.RasterizationInfo.SType = StructureType.PipelineRasterizationStateCreateInfo;
        configInfo.RasterizationInfo.DepthClampEnable = Vk.False;
        configInfo.RasterizationInfo.RasterizerDiscardEnable = Vk.False;
        configInfo.RasterizationInfo.PolygonMode = PolygonMode.Fill;
        configInfo.RasterizationInfo.LineWidth = 1f;
        configInfo.RasterizationInfo.CullMode = CullModeFlags.None;
        configInfo.RasterizationInfo.FrontFace = FrontFace.CounterClockwise;
        configInfo.RasterizationInfo.DepthBiasEnable = Vk.False;
        configInfo.RasterizationInfo.DepthBiasConstantFactor = 0f;
        configInfo.RasterizationInfo.DepthBiasClamp = 0f;
        configInfo.RasterizationInfo.DepthBiasSlopeFactor = 0f;

        configInfo.MultisampleInfo.SType = StructureType.PipelineMultisampleStateCreateInfo;
        configInfo.MultisampleInfo.SampleShadingEnable = Vk.False;
        configInfo.MultisampleInfo.RasterizationSamples = SampleCountFlags.Count1Bit;
        configInfo.MultisampleInfo.MinSampleShading = 1.0f;
        configInfo.MultisampleInfo.PSampleMask = default;
        configInfo.MultisampleInfo.AlphaToCoverageEnable = Vk.False;
        configInfo.MultisampleInfo.AlphaToOneEnable = Vk.False;
        
        configInfo.ColorBlendAttachment.BlendEnable = Vk.False;
        configInfo.ColorBlendAttachment.SrcColorBlendFactor = BlendFactor.One;
        configInfo.ColorBlendAttachment.DstColorBlendFactor = BlendFactor.Zero;
        configInfo.ColorBlendAttachment.ColorBlendOp = BlendOp.Add;
        configInfo.ColorBlendAttachment.SrcAlphaBlendFactor = BlendFactor.One;
        configInfo.ColorBlendAttachment.DstAlphaBlendFactor = BlendFactor.Zero;
        configInfo.ColorBlendAttachment.AlphaBlendOp = BlendOp.Add;
        configInfo.ColorBlendAttachment.ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit;

        configInfo.ColorBlendInfo.SType = StructureType.PipelineColorBlendStateCreateInfo;
        configInfo.ColorBlendInfo.LogicOpEnable = Vk.False;
        configInfo.ColorBlendInfo.LogicOp = LogicOp.Copy;
        configInfo.ColorBlendInfo.AttachmentCount = 1;
        configInfo.ColorBlendInfo.PAttachments = (PipelineColorBlendAttachmentState*) Unsafe.AsPointer(ref configInfo.ColorBlendAttachment);

        configInfo.ColorBlendInfo.BlendConstants[0] = 0;
        configInfo.ColorBlendInfo.BlendConstants[1] = 0;
        configInfo.ColorBlendInfo.BlendConstants[2] = 0;
        configInfo.ColorBlendInfo.BlendConstants[3] = 0;
        
        configInfo.DepthStencilInfo.SType = StructureType.PipelineDepthStencilStateCreateInfo;
        configInfo.DepthStencilInfo.DepthTestEnable = Vk.True;
        configInfo.DepthStencilInfo.DepthWriteEnable = Vk.True;
        configInfo.DepthStencilInfo.DepthCompareOp = CompareOp.Less;
        configInfo.DepthStencilInfo.DepthBoundsTestEnable = Vk.False;
        configInfo.DepthStencilInfo.MinDepthBounds = 0.0f;
        configInfo.DepthStencilInfo.MaxDepthBounds = 1.0f;
        configInfo.DepthStencilInfo.StencilTestEnable = Vk.False;
        configInfo.DepthStencilInfo.Front = default;
        configInfo.DepthStencilInfo.Back = default;

        configInfo.BindingDescriptions = Vertex.GetBindingDescriptions();
        configInfo.AttributeDescriptions = Vertex.GetAttributeDescriptions();
    }

    public static void EnableAlphaBlending(ref PipelineConfigInfo configInfo) {
        configInfo.ColorBlendAttachment.BlendEnable = Vk.True;
        configInfo.ColorBlendAttachment.SrcColorBlendFactor = BlendFactor.SrcAlpha;
        configInfo.ColorBlendAttachment.DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha;
        configInfo.ColorBlendAttachment.ColorBlendOp = BlendOp.Add;
        configInfo.ColorBlendAttachment.SrcAlphaBlendFactor = BlendFactor.One;
        configInfo.ColorBlendAttachment.DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha;
        configInfo.ColorBlendAttachment.AlphaBlendOp = BlendOp.Add;
        configInfo.ColorBlendAttachment.ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit;
    }
    
    public static void EnableMultiSampling(ref PipelineConfigInfo configInfo, SampleCountFlags msaaSamples) {
        configInfo.MultisampleInfo.RasterizationSamples = msaaSamples;
    }

    public unsafe void Dispose() {
        this._vk.DestroyShaderModule(this._device.VkDevice, this._vertShaderModule, null);
        this._vk.DestroyShaderModule(this._device.VkDevice, this._fragShaderModule, null);
        this._vk.DestroyPipeline(this._device.VkDevice, this._graphicsPipeline, null);

        GC.SuppressFinalize(this);
    }
}