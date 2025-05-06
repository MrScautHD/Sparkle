using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics;
using Bliss.CSharp.Graphics.Pipelines;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Textures.Cubemaps;
using Veldrid;

namespace Sparkle.CSharp.Graphics.Rendering;

public class SkyBox : Disposable {
    
    /// <summary>
    /// The graphics device used for rendering.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// The cubemap texture used for the skybox.
    /// </summary>
    public Cubemap Cubemap;
    
    /// <summary>
    /// The sampler used for texture sampling.
    /// </summary>
    public Sampler Sampler;
    
    /// <summary>
    /// The color tint applied to the skybox.
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// The effect used for rendering the skybox.
    /// </summary>
    private Effect _effect;

    /// <summary>
    /// The vertex data for the skybox.
    /// </summary>
    private CubemapVertex3D[] _vertices;
    
    /// <summary>
    /// The index data defining the skybox faces.
    /// </summary>
    private uint[] _indices;
    
    /// <summary>
    /// The buffer containing vertex data.
    /// </summary>
    private DeviceBuffer _vertexBuffer;
    
    /// <summary>
    /// The buffer containing index data.
    /// </summary>
    private DeviceBuffer _indexBuffer;
    
    /// <summary>
    /// The buffer containing the projection and view matrices.
    /// </summary>
    private SimpleBuffer<Matrix4x4> _projViewBuffer;

    /// <summary>
    /// The description of the rendering pipeline used for the skybox.
    /// </summary>
    private SimplePipelineDescription _pipelineDescription;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SkyBox"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="cubemap">The cubemap texture used for the skybox.</param>
    /// <param name="sampler">The optional texture sampler.</param>
    /// <param name="color">The optional color tint applied to the skybox.</param>
    public SkyBox(GraphicsDevice graphicsDevice, Cubemap cubemap, Sampler? sampler = null, Color? color = null) {
        this.GraphicsDevice = graphicsDevice;
        this.Cubemap = cubemap;
        this.Sampler = sampler ?? GraphicsHelper.GetSampler(graphicsDevice, SamplerType.Aniso4X);
        this.Color = color ?? Color.White;
        this._effect = GlobalGraphicsAssets.SkyboxEffect;
        
        // Create vertex buffer.
        uint vertexBufferSize = (uint) Marshal.SizeOf<CubemapVertex3D>() * 8;
        this._vertices = this.GenVertices();
        this._vertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));

        // Create index buffer.
        uint indexBufferSize = sizeof(uint) * 36;
        this._indices = this.GenIndices();
        this._indexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(indexBufferSize, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
        graphicsDevice.UpdateBuffer(this._indexBuffer, 0, this._indices);
        
        // Create projection view buffer.
        this._projViewBuffer = new SimpleBuffer<Matrix4x4>(graphicsDevice, 2, SimpleBufferType.Uniform, ShaderStages.Vertex);
        
        // Create pipeline.
        this._pipelineDescription = new SimplePipelineDescription() {
            BlendState = BlendStateDescription.SINGLE_ALPHA_BLEND,
            DepthStencilState = DepthStencilStateDescription.DISABLED,
            RasterizerState = new RasterizerStateDescription() {
                CullMode = FaceCullMode.None,
                FillMode = PolygonFillMode.Solid,
                FrontFace = FrontFace.Clockwise,
                DepthClipEnabled = true,
                ScissorTestEnabled = false
            },
            BufferLayouts = this._effect.GetBufferLayouts(),
            TextureLayouts = this._effect.GetTextureLayouts(),
            ShaderSet = this._effect.ShaderSet,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
        };
    }

    /// <summary>
    /// Renders the skybox using the specified command list and output description.
    /// </summary>
    /// <param name="commandList">The command list used for issuing draw commands.</param>
    /// <param name="output">The output description of the current rendering target.</param>
    public void Draw(CommandList commandList, OutputDescription output) {
        Cam3D? cam3D = Cam3D.ActiveCamera;

        if (cam3D == null) {
            return;
        }
        
        // Update projection/view buffer.
        this._projViewBuffer.SetValue(0, cam3D.GetProjection());
        this._projViewBuffer.SetValue(1, cam3D.GetView());
        this._projViewBuffer.UpdateBuffer(commandList);
        
        // Update pipeline description.
        this._pipelineDescription.Outputs = output;
        
        // Update vertex buffer.
        for (int i = 0; i < this._vertices.Length; i++) {
            this._vertices[i].Color = this.Color.ToRgbaFloatVec4();
        }
        
        commandList.UpdateBuffer(this._vertexBuffer, 0, this._vertices);
        
        // Set vertex and index buffer.
        commandList.SetVertexBuffer(0, this._vertexBuffer);
        commandList.SetIndexBuffer(this._indexBuffer, IndexFormat.UInt32);
        
        // Set pipeline.
        commandList.SetPipeline(this._effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set projection view buffer.
        commandList.SetGraphicsResourceSet(0, this._projViewBuffer.GetResourceSet(this._effect.GetBufferLayout("ProjectionViewBuffer")));
        
        // Set resourceSet of the cubemap.
        commandList.SetGraphicsResourceSet(1, this.Cubemap.GetResourceSet(this.Sampler, this._effect.GetTextureLayout("fCubemap").Layout));
        
        // Apply effect.
        this._effect.Apply();
        
        // Draw.
        commandList.DrawIndexed(36);
    }

    /// <summary>
    /// Generates and returns an array of vertices for the skybox.
    /// </summary>
    /// <returns>An array of <see cref="CubemapVertex3D"/> objects representing the vertices of the skybox.</returns>
    private CubemapVertex3D[] GenVertices() {
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

    /// <summary>
    /// Generates and returns an array of indices that define the connectivity of the vertices for the skybox.
    /// </summary>
    /// <returns>An array of unsigned integer values representing the vertex indices used to construct the skybox.</returns>
    private uint[] GenIndices() {
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
            this._projViewBuffer.Dispose();
        }
    }
}