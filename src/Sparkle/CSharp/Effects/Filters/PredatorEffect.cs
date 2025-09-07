using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Materials;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.Effects.Filters;

public class PredatorEffect : Effect {
    
    /// <summary>
    /// Path to the vertex shader.
    /// </summary>
    public static readonly string VertPath = "content/bliss/shaders/full_screen_render_pass.vert";
    
    /// <summary>
    /// Path to the fragment shader.
    /// </summary>
    public static readonly string FragPath = "content/sparkle/shaders/filters/predator.frag";
    
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
    /// Initializes a new instance of the <see cref="PredatorEffect"/> class.
    /// </summary>
    /// <param name="graphicsDevice">Graphics device to create GPU resources.</param>
    /// <param name="vertexLayout">Vertex layout description for the full-screen quad.</param>
    /// <param name="constants">Optional specialization constants for shader compilation.</param>
    public PredatorEffect(GraphicsDevice graphicsDevice, VertexLayoutDescription vertexLayout, SpecializationConstant[]? constants = null) : base(graphicsDevice, vertexLayout, VertPath, FragPath, constants) {
        this._parameters = new Parameters() {
            Resolution = new Vector2(GlobalGraphicsAssets.Window.GetWidth(), GlobalGraphicsAssets.Window.GetHeight())
        };
        
        // Create the params buffer.
        this._parameterBuffer = new SimpleBuffer<Parameters>(graphicsDevice, 1, SimpleBufferType.Uniform, ShaderStages.Fragment);
        this._isDirty = true;
        
        // Add resize event.
        GlobalGraphicsAssets.Window.Resized += this.Resize;
    }
    
    /// <summary>
    /// Applies the predator effect using the current parameter values.
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
    /// Struct holding configurable predator parameters.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Parameters {
        public Vector2 Resolution;
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        
        if (disposing) {
            this._parameterBuffer.Dispose();
            GlobalGraphicsAssets.Window.Resized -= this.Resize;
        }
    }
}