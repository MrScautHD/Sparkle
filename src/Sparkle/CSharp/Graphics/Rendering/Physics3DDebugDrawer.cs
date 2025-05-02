using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Jitter2;
using Jitter2.LinearMath;
using Sparkle.CSharp.Graphics.VertexTypes;
using Sparkle.CSharp.Registries.Types;
using Veldrid;

namespace Sparkle.CSharp.Graphics.Rendering;

public class Physics3DDebugDrawer : Disposable, IDebugDrawer {

    /// <summary>
    /// Gets the graphics device associated with this debug drawer.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// Gets the maximum number of vertices this drawer can handle per draw call.
    /// </summary>
    public uint Capacity { get; private set; }
    
    /// <summary>
    /// The shader effect used to render debug geometry.
    /// </summary>
    private Effect _effect;
    
    /// <summary>
    /// The internal array storing vertex data before drawing.
    /// </summary>
    private PhysicsDebugVertex3D[] _vertices;
    
    /// <summary>
    /// A temporary list used to accumulate vertices for the current draw call.
    /// </summary>
    private List<PhysicsDebugVertex3D> _tempVertices;
    
    /// <summary>
    /// The GPU buffer used to upload vertex data.
    /// </summary>
    private DeviceBuffer _vertexBuffer;

    /// <summary>
    /// The uniform buffer used to store projection and view matrices.
    /// </summary>
    private SimpleBuffer<Matrix4x4> _projViewBuffer;
    
    /// <summary>
    /// Describes the pipeline configuration used when rendering.
    /// </summary>
    private SimplePipelineDescription _pipelineDescription;
    
    /// <summary>
    /// Indicates whether the drawer has been prepared for rendering.
    /// </summary>
    private bool _prepared;

    /// <summary>
    /// The current command list used to issue draw calls.
    /// </summary>
    private CommandList _currentCommandList;
    
    /// <summary>
    /// The current output description defining the render targets.
    /// </summary>
    private OutputDescription _currentOutput;
    
    /// <summary>
    /// The current blend state used for rendering.
    /// </summary>
    private BlendStateDescription _currentBlendState;
    
    /// <summary>
    /// The current depth-stencil state used for rendering.
    /// </summary>
    private DepthStencilStateDescription _currentDepthStencilState;
    
    /// <summary>
    /// The current rasterizer state used for rendering.
    /// </summary>
    private RasterizerStateDescription _currentRasterizerState;

    /// <summary>
    /// The current color.
    /// </summary>
    private Color _currentColor;
    
    // TODO: DO it to a Batch system! (the performance is terrible without)
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Physics3DDebugDrawer"/> class.
    /// !Important! This is for debugging purposes only and is not optimized for performance.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to allocate buffers and resources.</param>
    public Physics3DDebugDrawer(GraphicsDevice graphicsDevice) {
        this.GraphicsDevice = graphicsDevice;
        this.Capacity = 6;
        this._effect = GlobalRegistry.PhysicsDebugEffect;
        
        // Create vertex buffer.
        this._vertices = new PhysicsDebugVertex3D[this.Capacity];
        this._tempVertices = new List<PhysicsDebugVertex3D>();
        this._vertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(this.Capacity * (uint) Marshal.SizeOf<PhysicsDebugVertex3D>(), BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        
        // Create projection view buffer.
        this._projViewBuffer = new SimpleBuffer<Matrix4x4>(graphicsDevice, 2, SimpleBufferType.Uniform, ShaderStages.Vertex);
        
        // Create pipeline description.
        this._pipelineDescription = new SimplePipelineDescription();
    }

    /// <summary>
    /// Prepares the debug drawer for rendering by configuring the command list, output description,
    /// and optional rendering states such as blending, depth-stencil, rasterizer, and color.
    /// </summary>
    /// <param name="commandList">The <see cref="CommandList"/> to be used for rendering commands.</param>
    /// <param name="output">The <see cref="OutputDescription"/> defining the rendering target output.</param>
    /// <param name="blendState">Optional blending state description. Defaults to single alpha blending if not specified.</param>
    /// <param name="depthStencilState">Optional depth-stencil state description. Defaults to less-equal depth testing if not specified.</param>
    /// <param name="rasterizerState">Optional rasterizer state description. Defaults to standard rasterizer settings if not specified.</param>
    /// <param name="color">Optional parameter specifying the color used for rendering. Defaults to white if not specified.</param>
    public void Prepare(CommandList commandList, OutputDescription output, BlendStateDescription? blendState = null, DepthStencilStateDescription? depthStencilState = null, RasterizerStateDescription? rasterizerState = null, Color? color = null) {
        this._prepared = true;
        this._currentCommandList = commandList;
        this._currentOutput = output;
        this._currentBlendState = blendState ?? BlendStateDescription.SINGLE_ALPHA_BLEND;
        this._currentDepthStencilState = depthStencilState ?? DepthStencilStateDescription.DEPTH_ONLY_LESS_EQUAL;
        this._currentRasterizerState = rasterizerState ?? RasterizerStateDescription.DEFAULT;
        this._currentColor = color ?? Color.White;
    }

    /// <summary>
    /// Draws a line segment using the specified start and end points in 3D space.
    /// </summary>
    /// <param name="pA">The <see cref="JVector"/> representing the starting point of the segment.</param>
    /// <param name="pB">The <see cref="JVector"/> representing the ending point of the segment.</param>
    public void DrawSegment(in JVector pA, in JVector pB) {
        // Add start vertex.
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = pA,
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        
        // Add end vertex.
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = pB,
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        
        this.DrawVertices(this._tempVertices, PrimitiveTopology.LineList);
    }

    /// <summary>
    /// Draws a triangle using the specified vertices in 3D space.
    /// </summary>
    /// <param name="pA">The <see cref="JVector"/> representing the first vertex of the triangle.</param>
    /// <param name="pB">The <see cref="JVector"/> representing the second vertex of the triangle.</param>
    /// <param name="pC">The <see cref="JVector"/> representing the third vertex of the triangle.</param>
    public void DrawTriangle(in JVector pA, in JVector pB, in JVector pC) {
        // Add 1 side vertices.
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = pA,
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = pB,
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        
        // Add 2 side vertices.
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = pB,
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = pC,
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        
        // Add 3 side vertices.
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = pC,
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = pA,
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        
        this.DrawVertices(this._tempVertices, PrimitiveTopology.LineList);
    }

    /// <summary>
    /// Draws a point at the specified position.
    /// </summary>
    /// <param name="p">The <see cref="JVector"/> representing the position of the point in 3D space.</param>
    public void DrawPoint(in JVector p) {
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = p,
            Color = this._currentColor.ToRgbaFloatVec4()
        });

        this.DrawVertices(this._tempVertices, PrimitiveTopology.PointList);
    }

    /// <summary>
    /// Draws the provided vertices using the specified primitive topology.
    /// </summary>
    /// <param name="vertices">A list of <see cref="PhysicsDebugVertex3D"/> that defines the vertices to be rendered.</param>
    /// <param name="topology">The <see cref="PrimitiveTopology"/> that determines the way the vertices will be rendered.</param>
    private void DrawVertices(List<PhysicsDebugVertex3D> vertices, PrimitiveTopology topology) {
        if (!this._prepared) {
            throw new Exception("The Physics 3D debug drawer has not prepared yet!");
        }
        
        Cam3D? cam3D = Cam3D.ActiveCamera;

        if (cam3D == null) {
            // Clear temp data.
            this._tempVertices.Clear();
            return;
        }

        if (vertices.Count > this.Capacity) {
            throw new InvalidOperationException($"The number of provided vertices exceeds the capacity! [{vertices.Count} > {this.Capacity}]");
        }

        // Add vertices.
        for (int i = 0; i < vertices.Count; i++) {
            this._vertices[i] = vertices[i];
        }
        
        // Update projection view buffer.
        this._projViewBuffer.SetValue(0, cam3D.GetProjection());
        this._projViewBuffer.SetValue(1, cam3D.GetView());
        this._projViewBuffer.UpdateBuffer(this._currentCommandList);
        
        // Update pipeline description.
        this._pipelineDescription.BlendState = this._currentBlendState;
        this._pipelineDescription.DepthStencilState = this._currentDepthStencilState;
        this._pipelineDescription.RasterizerState = this._currentRasterizerState;
        this._pipelineDescription.PrimitiveTopology = topology;
        this._pipelineDescription.BufferLayouts = this._effect.GetBufferLayouts();
        this._pipelineDescription.TextureLayouts = this._effect.GetTextureLayouts();
        this._pipelineDescription.ShaderSet = this._effect.ShaderSet;
        this._pipelineDescription.Outputs = this._currentOutput;
        
        // Update vertex buffer.
        this._currentCommandList.UpdateBuffer(this._vertexBuffer, 0, new ReadOnlySpan<PhysicsDebugVertex3D>(this._vertices, 0, vertices.Count));
        
        // Set vertex buffer.
        this._currentCommandList.SetVertexBuffer(0, this._vertexBuffer);
        
        // Set pipeline.
        this._currentCommandList.SetPipeline(this._effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set projection view buffer.
        this._currentCommandList.SetGraphicsResourceSet(0, this._projViewBuffer.GetResourceSet(this._effect.GetBufferLayout("ProjectionViewBuffer")));
        
        // Apply effect.
        this._effect.Apply();
        
        // Draw.
        this._currentCommandList.Draw((uint) vertices.Count);
        
        // Clear data.
        Array.Clear(this._vertices);
        this._tempVertices.Clear();
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._vertexBuffer.Dispose();
            this._projViewBuffer.Dispose();
        }
    }
}