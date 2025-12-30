using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Materials;
using Sparkle.CSharp.Graphics;
using Veldrid;
using Veldrid.SPIRV;

namespace Sparkle.CSharp.Effects.Posts;

public class FxaaEffect : Effect {
    
    /// <summary>
    /// Path to the vertex shader.
    /// </summary>
    public static readonly string VertPath = "content/bliss/shaders/full_screen_render_pass.vert";
    
    /// <summary>
    /// Path to the fragment shader.
    /// </summary>
    public static readonly string FragPath = "content/sparkle/shaders/filters/fxaa.frag";
    
    /// <summary>
    /// Indicates whether the parameters buffer needs to be updated.
    /// </summary>
    private bool _isDirty;
    
    /// <summary>
    /// Stores the configurable parameters.
    /// </summary>
    private Parameters _parameters;
    
    /// <summary>
    /// Uniform buffer used to pass parameters to the shader.
    /// </summary>
    private SimpleUniformBuffer<Parameters> _parameterBuffer;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FxaaEffect"/> class with optional FXAA tuning values.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="vertexLayout">The layout of vertices for the full-screen pass.</param>
    /// <param name="reduceMin">Minimum amount of local contrast reduction.</param>
    /// <param name="reduceMul">Multiplier for local contrast reduction.</param>
    /// <param name="spanMax">Maximum blur span.</param>
    /// <param name="compileOptions">Optional cross-compilation options used when creating the shaders.</param>
    public FxaaEffect(GraphicsDevice graphicsDevice, VertexLayoutDescription vertexLayout, CrossCompileOptions compileOptions, float reduceMin = 1.0F / 128.0F, float reduceMul = 1.0F / 8.0F, float spanMax = 8.0F) : base(graphicsDevice, vertexLayout, VertPath, FragPath, compileOptions) {
        this._parameters = new Parameters() {
            Resolution = new Vector2(GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight()),
            ReduceMin = reduceMin,
            ReduceMul = reduceMul,
            SpanMax = spanMax
        };
        
        // Create the params buffer.
        this._parameterBuffer = new SimpleUniformBuffer<Parameters>(graphicsDevice, 1, ShaderStages.Fragment);
        this._isDirty = true;
        
        // Add resize event.
        GlobalGraphicsAssets.Window.Resized += this.Resize;
    }
    
    /// <summary>
    /// Controls the minimum contrast threshold below which edges are ignored during processing.
    /// </summary>
    public float ReduceMin {
        get => this._parameters.ReduceMin;
        set {
            this._parameters.ReduceMin = value;
            this._isDirty = true;
        }
    }
    
    /// <summary>
    /// Determines the multiplier for reducing contrast, influencing how strongly contrast is diminished.
    /// </summary>
    public float ReduceMul {
        get => this._parameters.ReduceMul;
        set {
            this._parameters.ReduceMul = value;
            this._isDirty = true;
        }
    }
    
    /// <summary>
    /// Specifies the maximum extent of the blur effect, with higher values extending the anti-aliasing span across more pixels.
    /// </summary>
    public float SpanMax {
        get => this._parameters.SpanMax;
        set {
            this._parameters.SpanMax = value;
            this._isDirty = true;
        }
    }
    
    /// <summary>
    /// Applies the FXAA effect using the current parameter values.
    /// </summary>
    /// <param name="commandList">The command list used to issue GPU commands.</param>
    /// <param name="material">An optional material to apply with the effect.</param>
    public override void Apply(CommandList commandList, Material? material = null) {
        base.Apply(commandList, material);
        
        if (this._isDirty) {
            this._parameterBuffer.SetValue(0, this._parameters);
            this._parameterBuffer.UpdateBufferDeferred(commandList);
            this._isDirty = false;
        }
        
        commandList.SetGraphicsResourceSet(this.GetBufferLayoutSlot("ParameterBuffer"), this._parameterBuffer.GetResourceSet(this.GetBufferLayout("ParameterBuffer")));
    }
    
    /// <summary>
    /// Called when the window is resized to update the internal resolution parameter.
    /// </summary>
    protected virtual void Resize() {
        this._parameters.Resolution = new Vector2(GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight());
        this._isDirty = true;
    }
    
    /// <summary>
    /// Struct holding configurable FXAA parameters.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Parameters {
        public Vector2 Resolution;
        public float ReduceMin;
        public float ReduceMul;
        public float SpanMax;
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            this._parameterBuffer.Dispose();
            GlobalGraphicsAssets.Window.Resized -= this.Resize;
        }
    }
}