using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Windowing;
using Jitter2;
using Jitter2.LinearMath;
using Sparkle.CSharp.Graphics.VertexTypes;
using Veldrid;

namespace Sparkle.CSharp.Graphics.Rendering;

public class Physics3DDebugDrawer : Disposable, IDebugDrawer {
    
    /// <summary>
    /// Gets the graphics device used by the debug drawer.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// Gets the window associated with the debug drawer.
    /// </summary>
    public IWindow Window { get; private set; }
    
    /// <summary>
    /// Gets the maximum number of vertices the drawer can store.
    /// </summary>
    public uint Capacity { get; private set; }
    
    /// <summary>
    /// Gets the number of draw calls performed during the current frame.
    /// </summary>
    public int DrawCallCount { get; private set; }
    
    /// <summary>
    /// The graphics effect used for rendering debug shapes.
    /// </summary>
    private Effect _effect;
    
    /// <summary>
    /// The buffer of vertices to be drawn in the current batch.
    /// </summary>
    private PhysicsDebugVertex3D[] _vertices;
    
    /// <summary>
    /// Temporary list of vertices used to accumulate draw calls before batching.
    /// </summary>
    private List<PhysicsDebugVertex3D> _tempVertices;
    
    /// <summary>
    /// The GPU buffer storing vertex data for rendering.
    /// </summary>
    private DeviceBuffer _vertexBuffer;
    
    /// <summary>
    /// The buffer storing the projection and view matrices.
    /// </summary>
    private SimpleUniformBuffer<Matrix4x4> _projViewBuffer;
    
    /// <summary>
    /// Description of the graphics pipeline used for debug rendering.
    /// </summary>
    private SimplePipelineDescription _pipelineDescription;
    
    /// <summary>
    /// Indicates whether a draw session has begun.
    /// </summary>
    private bool _begun;
    
    /// <summary>
    /// The command list currently in use for rendering.
    /// </summary>
    private CommandList _currentCommandList;

    /// <summary>
    /// The main <see cref="OutputDescription"/>.
    /// </summary>
    private OutputDescription _mainOutput;
    
    /// <summary>
    /// The current <see cref="OutputDescription"/>.
    /// </summary>
    private OutputDescription _currentOutput;
    
    /// <summary>
    /// The requested <see cref="OutputDescription"/>.
    /// </summary>
    private OutputDescription _requestedOutput;
    
    /// <summary>
    /// The main <see cref="BlendStateDescription"/>.
    /// </summary>
    private BlendStateDescription _mainBlendState;
    
    /// <summary>
    /// The current <see cref="BlendStateDescription"/>.
    /// </summary>
    private BlendStateDescription _currentBlendState;
    
    /// <summary>
    /// The requested <see cref="BlendStateDescription"/>.
    /// </summary>
    private BlendStateDescription _requestedBlendState;
    
    /// <summary>
    /// The main <see cref="DepthStencilStateDescription"/>.
    /// </summary>
    private DepthStencilStateDescription _mainDepthStencilState;
    
    /// <summary>
    /// The current <see cref="DepthStencilStateDescription"/>.
    /// </summary>
    private DepthStencilStateDescription _currentDepthStencilState;
    
    /// <summary>
    /// The requested <see cref="DepthStencilStateDescription"/>.
    /// </summary>
    private DepthStencilStateDescription _requestedDepthStencilState;
    
    /// <summary>
    /// The main <see cref="RasterizerStateDescription"/>.
    /// </summary>
    private RasterizerStateDescription _mainRasterizerState;
    
    /// <summary>
    /// The current <see cref="RasterizerStateDescription"/>.
    /// </summary>
    private RasterizerStateDescription _currentRasterizerState;
    
    /// <summary>
    /// The requested <see cref="RasterizerStateDescription"/>.
    /// </summary>
    private RasterizerStateDescription _requestedRasterizerState;
    
    /// <summary>
    /// The current <see cref="Color"/>.
    /// </summary>
    private Color _mainColor;
    
    /// <summary>
    /// The current <see cref="Color"/>.
    /// </summary>
    private Color _currentColor;

    /// <summary>
    /// The requested <see cref="Color"/>.
    /// </summary>
    private Color _requestedColor;

    /// <summary>
    /// The number of vertices currently in the batch.
    /// </summary>
    private uint _currentBatchCount;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Physics3DDebugDrawer"/> class with the specified graphics device, window, and optional capacity.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="window">The window associated with the rendering context.</param>
    /// <param name="capacity">The maximum number of vertices that can be rendered in a single batch. Default is 30720.</param>
    public Physics3DDebugDrawer(GraphicsDevice graphicsDevice, IWindow window, uint capacity = 30720) {
        this.GraphicsDevice = graphicsDevice;
        this.Window = window;
        this.Capacity = capacity;
        this._effect = GlobalGraphicsAssets.PhysicsDebugEffect;
        
        // Create vertex buffer.
        this._vertices = new PhysicsDebugVertex3D[this.Capacity];
        this._tempVertices = new List<PhysicsDebugVertex3D>();
        this._vertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(this.Capacity * (uint) Marshal.SizeOf<PhysicsDebugVertex3D>(), BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        
        // Create projection view buffer.
        this._projViewBuffer = new SimpleUniformBuffer<Matrix4x4>(graphicsDevice, 2, ShaderStages.Vertex);
        
        // Create pipeline description.
        this._pipelineDescription = new SimplePipelineDescription() {
            BufferLayouts = this._effect.GetBufferLayouts(),
            TextureLayouts = this._effect.GetTextureLayouts(),
            ShaderSet = this._effect.ShaderSet,
            PrimitiveTopology = PrimitiveTopology.LineList
        };
    }

    /// <summary>
    /// Begins a new rendering session with the specified command list and output description.
    /// </summary>
    /// <param name="commandList">The command list used for issuing rendering commands.</param>
    /// <param name="output">The output description for rendering.</param>
    /// <param name="blendState">Optional blend state description. If null, a default is used.</param>
    /// <param name="depthStencilState">Optional depth-stencil state description. If null, a default is used.</param>
    /// <param name="rasterizerState">Optional rasterizer state description. If null, a default is used.</param>
    /// <param name="color">Optional color for rendering debug visuals. If null, white is used.</param>
    /// <exception cref="Exception">Thrown if a rendering session has already begun.</exception>
    public void Begin(CommandList commandList, OutputDescription output, BlendStateDescription? blendState = null, DepthStencilStateDescription? depthStencilState = null, RasterizerStateDescription? rasterizerState = null, Color? color = null) {
        if (this._begun) {
            throw new Exception("The Physics3DDebugDrawer has already begun!");
        }

        this._begun = true;
        this._currentCommandList = commandList;
        this._mainOutput = this._currentOutput = this._requestedOutput = output;
        this._mainBlendState = this._currentBlendState = this._requestedBlendState = blendState ?? BlendStateDescription.SINGLE_DISABLED;
        this._mainDepthStencilState = this._currentDepthStencilState = this._requestedDepthStencilState = depthStencilState ?? DepthStencilStateDescription.DEPTH_ONLY_LESS_EQUAL;
        this._mainRasterizerState = this._currentRasterizerState = this._requestedRasterizerState = rasterizerState ?? RasterizerStateDescription.DEFAULT;
        this._mainColor = this._currentColor = this._requestedColor = color ?? Color.White;
        
        this.DrawCallCount = 0;
    }
    
    /// <summary>
    /// Ends the current rendering session and flushes any remaining draw calls.
    /// </summary>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public void End() {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }

        this._begun = false;
        this.Flush();
    }
    
    /// <summary>
    /// Gets the current output description being used for rendering.
    /// </summary>
    /// <returns>The current <see cref="OutputDescription"/>.</returns>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public OutputDescription GetCurrentOutput() {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }
        
        return this._currentOutput;
    }
    
    /// <summary>
    /// Pushes a new output description to override the current rendering target.
    /// </summary>
    /// <param name="output">The new <see cref="OutputDescription"/> to apply.</param>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public void PushOutput(OutputDescription output) {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }
        
        this._requestedOutput = output;
    }
    
    /// <summary>
    /// Restores the output description back to the one set at <see cref="Begin"/>.
    /// </summary>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public void PopOutput() {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }

        this._requestedOutput = this._mainOutput;
    }
    
    /// <summary>
    /// Gets the current blend state description being used for rendering.
    /// </summary>
    /// <returns>The current <see cref="BlendStateDescription"/>.</returns>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public BlendStateDescription GetCurrentBlendState() {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }
        
        return this._currentBlendState;
    }
    
    /// <summary>
    /// Pushes a new blend state to override the current state.
    /// </summary>
    /// <param name="blendState">The new <see cref="BlendStateDescription"/> to apply.</param>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public void PushBlendState(BlendStateDescription blendState) {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }
        
        this._requestedBlendState = blendState;
    }
    
    /// <summary>
    /// Restores the blend state back to the one set at <see cref="Begin"/>.
    /// </summary>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public void PopBlendState() {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }
        
        this._requestedBlendState = this._mainBlendState;
    }
    
    /// <summary>
    /// Gets the current depth-stencil state being used in rendering.
    /// </summary>
    /// <returns>The current depth-stencil state.</returns>
    /// <exception cref="Exception">Thrown if the drawer has not begun rendering yet.</exception>
    public DepthStencilStateDescription GetCurrentDepthStencilState() {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }
        
        return this._currentDepthStencilState;
    }
    
    /// <summary>
    /// Pushes a new depth-stencil state to override the current state.
    /// </summary>
    /// <param name="depthStencilState">The new <see cref="DepthStencilStateDescription"/> to apply.</param>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public void PushDepthStencilState(DepthStencilStateDescription depthStencilState) {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }
        
        this._requestedDepthStencilState = depthStencilState;
    }
    
    /// <summary>
    /// Restores the depth-stencil state back to the one set at <see cref="Begin"/>.
    /// </summary>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public void PopDepthStencilState() {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }
        
        this._requestedDepthStencilState = this._mainDepthStencilState;
    }
    
    /// <summary>
    /// Gets the current rasterizer state being used in rendering.
    /// </summary>
    /// <returns>The current rasterizer state.</returns>
    /// <exception cref="Exception">Thrown if the drawer has not begun rendering yet.</exception>
    public RasterizerStateDescription GetCurrentRasterizerState() {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }
        
        return this._currentRasterizerState;
    }
    
    /// <summary>
    /// Pushes a new rasterizer state to override the current state.
    /// </summary>
    /// <param name="rasterizerState">The new <see cref="RasterizerStateDescription"/> to apply.</param>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public void PushRasterizerState(RasterizerStateDescription rasterizerState) {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }
        
        this._requestedRasterizerState = rasterizerState;
    }
    
    /// <summary>
    /// Restores the rasterizer state back to the one set at <see cref="Begin"/>.
    /// </summary>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public void PopRasterizerState() {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }
        
        this._requestedRasterizerState = this._mainRasterizerState;
    }
    
    /// <summary>
    /// Gets the current color used for rendering debug shapes.
    /// </summary>
    /// <returns>The current color used for rendering.</returns>
    /// <exception cref="Exception">Thrown if the drawer has not begun rendering yet.</exception>
    public Color GetCurrentColor() {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }

        return this._currentColor;
    }
    
    /// <summary>
    /// Pushes a new color to override the current debug shape color.
    /// </summary>
    /// <param name="color">The new <see cref="Color"/> to apply.</param>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public void PushColor(Color color) {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }

        this._requestedColor = color;
    }
    
    /// <summary>
    /// Restores the debug shape color back to the one set at <see cref="Begin"/>.
    /// </summary>
    /// <exception cref="Exception">Thrown if a rendering session has not begun.</exception>
    public void PopColor() {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }

        this._requestedColor = this._mainColor;
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
        
        this.AddVertices(this._tempVertices);
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
        
        this.AddVertices(this._tempVertices);
    }

    /// <summary>
    /// Draws a point at the specified position.
    /// </summary>
    /// <param name="p">The <see cref="JVector"/> representing the position of the point in 3D space.</param>
    public void DrawPoint(in JVector p) {
        float size = 0.1f;
        
        // Add horizontal line (X-axis)
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = new JVector(p.X - size, p.Y, p.Z),
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = new JVector(p.X + size, p.Y, p.Z),
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        
        // Add vertical line (Y-axis)
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = new JVector(p.X, p.Y + size, p.Z),
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = new JVector(p.X, p.Y - size, p.Z),
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        
        // Add depth line (Z-axis)
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = new JVector(p.X, p.Y, p.Z + size),
            Color = this._currentColor.ToRgbaFloatVec4()
        });
        this._tempVertices.Add(new PhysicsDebugVertex3D() {
            Position = new JVector(p.X, p.Y, p.Z - size),
            Color = this._currentColor.ToRgbaFloatVec4()
        });

        this.AddVertices(this._tempVertices);
    }

    /// <summary>
    /// Adds a collection of vertices to the debug drawer for rendering.
    /// </summary>
    /// <param name="vertices">The list of vertices to be added for rendering.</param>
    private void AddVertices(List<PhysicsDebugVertex3D> vertices) {
        if (!this._begun) {
            throw new Exception("The Physics3DDebugDrawer has not begun yet!");
        }
        
        if (!this._currentOutput.Equals(this._requestedOutput) ||
            !this._currentBlendState.Equals(this._requestedBlendState) ||
            !this._currentDepthStencilState.Equals(this._requestedDepthStencilState) ||
            !this._currentRasterizerState.Equals(this._requestedRasterizerState) ||
            this._currentColor != this._requestedColor) {
            this.Flush();
        }
        
        this._currentOutput = this._requestedOutput;
        this._currentBlendState = this._requestedBlendState;
        this._currentDepthStencilState = this._requestedDepthStencilState;
        this._currentRasterizerState = this._requestedRasterizerState;
        this._currentColor = this._requestedColor;
        
        // Update pipeline description.
        this._pipelineDescription.BlendState = this._currentBlendState;
        this._pipelineDescription.DepthStencilState = this._currentDepthStencilState;
        this._pipelineDescription.RasterizerState = this._currentRasterizerState;
        this._pipelineDescription.Outputs = this._currentOutput;
        
        if (this._currentBatchCount + vertices.Count >= this._vertices.Length) {
            this.Flush();
        }
        
        for (int i = 0; i < vertices.Count; i++) {
            this._vertices[this._currentBatchCount] = vertices[i];
            this._currentBatchCount++;
        }
        
        // Clear temp data.
        this._tempVertices.Clear();
    }

    /// <summary>
    /// Flushes the current batch of vertices to the GPU for rendering.
    /// </summary>
    private void Flush() {
        if (this._currentBatchCount == 0) {
            return;
        }
        
        Cam3D? cam3D = Cam3D.ActiveCamera;

        if (cam3D == null) {
            return;
        }
        
        // Update projection view buffer.
        this._projViewBuffer.SetValue(0, cam3D.GetProjection());
        this._projViewBuffer.SetValue(1, cam3D.GetView());
        this._projViewBuffer.UpdateBufferDeferred(this._currentCommandList);
        
        // Update vertex buffer.
        this._currentCommandList.UpdateBuffer(this._vertexBuffer, 0, new ReadOnlySpan<PhysicsDebugVertex3D>(this._vertices, 0, (int) this._currentBatchCount));
        
        // Set vertex buffer.
        this._currentCommandList.SetVertexBuffer(0, this._vertexBuffer);
        
        // Set pipeline.
        this._currentCommandList.SetPipeline(this._effect.GetPipeline(this._pipelineDescription).Pipeline);
        
        // Set projection view buffer.
        this._currentCommandList.SetGraphicsResourceSet(this._effect.GetBufferLayoutSlot("ProjectionViewBuffer"), this._projViewBuffer.GetResourceSet(this._effect.GetBufferLayout("ProjectionViewBuffer")));
        
        // Apply effect.
        this._effect.Apply(this._currentCommandList);
        
        // Draw.
        this._currentCommandList.Draw(this._currentBatchCount);
        
        // Clear data.
        this._currentBatchCount = 0;
        Array.Clear(this._vertices);

        this.DrawCallCount++;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._vertexBuffer.Dispose();
            this._projViewBuffer.Dispose();
        }
    }
}