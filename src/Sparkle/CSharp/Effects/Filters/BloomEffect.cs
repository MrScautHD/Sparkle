using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Materials;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.Effects.Filters;

public class BloomEffect : Effect {
    
    /// <summary>
    /// Path to the vertex shader.
    /// </summary>
    public static readonly string VertPath = "content/bliss/shaders/full_screen_render_pass.vert";
    
    /// <summary>
    /// Path to the fragment shader.
    /// </summary>
    public static readonly string FragPath = "content/sparkle/shaders/filters/bloom.frag";
    
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
    private SimpleBuffer<Parameters> _parameterBuffer;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BloomEffect"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    /// <param name="vertexLayout">The vertex layout for full-screen rendering.</param>
    /// <param name="samples">The number of blur samples to use in the bloom effect.</param>
    /// <param name="quality">The quality factor of the bloom blur.</param>
    /// <param name="constants">Optional shader specialization constants.</param>
    public BloomEffect(GraphicsDevice graphicsDevice, VertexLayoutDescription vertexLayout, float samples = 5.0F, float quality = 2.5F, SpecializationConstant[]? constants = null) : base(graphicsDevice, vertexLayout, VertPath, FragPath, constants) {
        this._parameters = new Parameters() {
            Resolution = new Vector2(GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight()),
            Samples = samples,
            Quality = quality
        };
        
        // Create the params buffer.
        this._parameterBuffer = new SimpleBuffer<Parameters>(graphicsDevice, 1, SimpleBufferType.Uniform, ShaderStages.Fragment);
        this._isDirty = true;
        
        // Add resize event.
        GlobalGraphicsAssets.Window.Resized += this.Resize;
    }
    
    /// <summary>
    /// Gets or sets the number of blur samples used in the bloom effect (Higher values result in smoother, more spread-out bloom).
    /// </summary>
    public float Samples {
        get => this._parameters.Samples;
        set {
            this._parameters.Samples = value;
            this._isDirty = true;
        }
    }
    
    /// <summary>
    /// Gets or sets the quality factor of the bloom blur (Affects the blur strength or sharpness of the effect).
    /// </summary>
    public float Quality {
        get => this._parameters.Quality;
        set {
            this._parameters.Quality = value;
            this._isDirty = true;
        }
    }
    
    /// <summary>
    /// Applies the bloom effect using the current parameter values.
    /// </summary>
    /// <param name="commandList">The command list used to issue GPU commands.</param>
    /// <param name="material">An optional material to apply with the effect.</param>
    public override void Apply(CommandList commandList, Material? material = null) {
        base.Apply(commandList, material);
        
        if (this._isDirty) {
            this._parameterBuffer.SetValue(0, this._parameters);
            this._parameterBuffer.UpdateBuffer(commandList);
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
    /// Struct holding configurable bloom parameters.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Parameters {
        public Vector2 Resolution;
        public float Samples;
        public float Quality;
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            this._parameterBuffer.Dispose();
            GlobalGraphicsAssets.Window.Resized -= this.Resize;
        }
    }
}