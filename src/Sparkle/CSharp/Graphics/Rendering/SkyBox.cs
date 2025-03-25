using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics;
using Bliss.CSharp.Graphics.Pipelines;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Textures.Cubemaps;
using Sparkle.CSharp.Graphics.VertexTypes;
using Veldrid;

namespace Sparkle.CSharp.Graphics.Rendering;

public class SkyBox : Disposable {
    
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    private DeviceBuffer _vertexBuffer;
    private DeviceBuffer _indexBuffer;
    
    private SimpleBuffer<Matrix4x4> _projViewBuffer;

    private SimplePipelineDescription _pipelineDescription;
    
    public SkyBox(GraphicsDevice graphicsDevice) {
        this.GraphicsDevice = graphicsDevice;

        // Create vertex buffer.
        uint vertexBufferSize = (uint) Marshal.SizeOf<CubemapVertex3D>() * 8;
        this._vertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        graphicsDevice.UpdateBuffer(this._vertexBuffer, 0, this.GetVertices());

        // Create index buffer.
        uint indexBufferSize = sizeof(uint) * 36;
        this._indexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(indexBufferSize, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
        graphicsDevice.UpdateBuffer(this._vertexBuffer, 0, this.GetIndices());
        
        // Create projection view buffer.
        this._projViewBuffer = new SimpleBuffer<Matrix4x4>(graphicsDevice, 2, SimpleBufferType.Uniform, ShaderStages.Vertex);
        
        // Create pipeline.
        this._pipelineDescription = new SimplePipelineDescription() {
            PrimitiveTopology = PrimitiveTopology.TriangleList
        };
    }

    public void Draw(CommandList commandList, Cubemap cubemap, OutputDescription output, Effect? effect = null, Sampler? sampler = null, BlendStateDescription? blendState = null, DepthStencilStateDescription? depthStencilState = null, RasterizerStateDescription? rasterizerState = null) {
        Cam3D? cam3D = Cam3D.ActiveCamera;

        if (cam3D == null) {
            return;
        }
        
        Effect finalEffect = effect ?? null;
        Sampler finalSampler = sampler ?? GraphicsHelper.GetSampler(this.GraphicsDevice, SamplerType.Point);
        BlendStateDescription finalBlendState = blendState ?? BlendStateDescription.SINGLE_ALPHA_BLEND;
        DepthStencilStateDescription finalDepthStencilState = depthStencilState ?? DepthStencilStateDescription.DISABLED;
        RasterizerStateDescription finalRasterizerState = rasterizerState ?? RasterizerStateDescription.CULL_NONE;
        
        // Update projection/view buffer.
        this._projViewBuffer.SetValue(0, cam3D.GetProjection());
        this._projViewBuffer.SetValue(1, cam3D.GetView());
        this._projViewBuffer.UpdateBuffer(commandList);
        
        // Update pipeline description.
        this._pipelineDescription.BlendState = finalBlendState;
        this._pipelineDescription.DepthStencilState = finalDepthStencilState;
        this._pipelineDescription.RasterizerState = finalRasterizerState;
        this._pipelineDescription.BufferLayouts = finalEffect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = finalEffect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = finalEffect.ShaderSet;
        this._pipelineDescription.Outputs = output;
        
        // Set vertex and index buffer.
        commandList.SetVertexBuffer(0, this._vertexBuffer);
        commandList.SetIndexBuffer(this._indexBuffer, IndexFormat.UInt16);
        
        // Set pipeline.
        commandList.SetPipeline(finalEffect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set projection view buffer.
        commandList.SetGraphicsResourceSet(0, this._projViewBuffer.GetResourceSet(finalEffect.GetBufferLayout("ProjectionViewBuffer")));
        
        // Set resourceSet of the cubemap.
        commandList.SetGraphicsResourceSet(1, cubemap.GetResourceSet(finalSampler, finalEffect.GetTextureLayout("fCubemap")));
        
        // Apply effect.
        finalEffect.Apply();
        
        // Draw.
        commandList.DrawIndexed(36);
    }

    /// <summary>
    /// Generates and returns an array of vertices for the skybox.
    /// </summary>
    /// <returns>An array of <see cref="CubemapVertex3D"/> objects representing the vertices of the skybox.</returns>
    private CubemapVertex3D[] GetVertices() {
        Color color = Color.White;
        
        return [
            new CubemapVertex3D(new Vector3(-1, -1, -1), color.ToRgbaFloatVec4()),
            new CubemapVertex3D(new Vector3(1, -1, -1), color.ToRgbaFloatVec4()),
            new CubemapVertex3D(new Vector3(1, 1, -1), color.ToRgbaFloatVec4()),
            new CubemapVertex3D(new Vector3(-1, 1, -1), color.ToRgbaFloatVec4()),
            
            new CubemapVertex3D(new Vector3(-1, -1, 1), color.ToRgbaFloatVec4()),
            new CubemapVertex3D(new Vector3(1, -1, 1), color.ToRgbaFloatVec4()),
            new CubemapVertex3D(new Vector3(1, 1, 1), color.ToRgbaFloatVec4()),
            new CubemapVertex3D(new Vector3(-1, 1, 1), color.ToRgbaFloatVec4())
        ];
    }
    
    private uint[] GetIndices() {
        return [
            0, 1, 2,
            2, 3, 0,

            4, 5, 6,
            6, 7, 4,
            
            4, 0, 3,
            3, 7, 4,

            1, 5, 6,
            6, 2, 1,

            3, 2, 6,
            6, 7, 3,

            4, 5, 1,
            1, 0, 4
        ];
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._vertexBuffer.Dispose();
            this._indexBuffer.Dispose();
        }
    }
}