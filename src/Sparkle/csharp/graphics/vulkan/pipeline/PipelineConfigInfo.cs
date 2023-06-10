using Silk.NET.Vulkan;

namespace Sparkle.csharp.graphics.vulkan.pipeline; 

public struct PipelineConfigInfo {
    
    public VertexInputBindingDescription[] BindingDescriptions;
    public VertexInputAttributeDescription[] AttributeDescriptions;
    
    public PipelineViewportStateCreateInfo ViewportInfo;
    public PipelineInputAssemblyStateCreateInfo InputAssemblyInfo;
    public PipelineRasterizationStateCreateInfo RasterizationInfo;
    public PipelineMultisampleStateCreateInfo MultisampleInfo;
    public PipelineColorBlendAttachmentState ColorBlendAttachment;
    public PipelineColorBlendStateCreateInfo ColorBlendInfo;
    public PipelineDepthStencilStateCreateInfo DepthStencilInfo;

    public PipelineLayout PipelineLayout;
    public RenderPass RenderPass;
    public uint SubPass;

    public PipelineConfigInfo() {
        SubPass = 0;
        BindingDescriptions = Array.Empty<VertexInputBindingDescription>();
        AttributeDescriptions = Array.Empty<VertexInputAttributeDescription>();
    }
}