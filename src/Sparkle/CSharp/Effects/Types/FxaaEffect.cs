using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Shaders;
using Raylib_CSharp.Windowing;

namespace Sparkle.CSharp.Effects.Types;

public class FxaaEffect : Effect {
    
    public int ResolutionLoc { get; private set; }
    public int ReduceMinLoc { get; private set; }
    public int ReduceMulLoc { get; private set; }
    public int SpanMaxLoc { get; private set; }

    public float ReduceMin;
    public float ReduceMul;
    public float SpanMax;
    
    private float _oldReduceMin;
    private float _oldReduceMul;
    private float _oldSpanMax;
    
    /// <summary>
    /// Constructor for creating a FxaaEffect object.
    /// </summary>
    /// <param name="shader">The shader to be used by the FXAA effect.</param>
    /// <param name="reduceMin">Minimum reduction value.</param>
    /// <param name="reduceMul">Reduction multiplier value.</param>
    /// <param name="spanMax">Maximum span value.</param>
    public FxaaEffect(Shader shader, float reduceMin = 1.0F / 128.0F, float reduceMul = 1.0F / 8.0F, float spanMax = 8.0F) : base(shader) {
        this.ReduceMin = reduceMin;
        this.ReduceMul = reduceMul;
        this.SpanMax = spanMax;
    }

    protected internal override void Init() {
        base.Init();
        this.SetLocations();
        this.UpdateResolution();
    }

    public override void Apply(Material? material = default) {
        base.Apply(material);
        
        if (Window.IsResized()) {
            this.UpdateResolution();
        }
        
        if (RayMath.FloatEquals(this.ReduceMin, this._oldReduceMin) != 1) {
            this.Shader.SetValue(this.ReduceMinLoc, this.ReduceMin, ShaderUniformDataType.Float);
            this._oldReduceMin = this.ReduceMin;
        }
        
        if (RayMath.FloatEquals(this.ReduceMul, this._oldReduceMul) != 1) {
            this.Shader.SetValue(this.ReduceMulLoc, this.ReduceMul, ShaderUniformDataType.Float);
            this._oldReduceMul = this.ReduceMul;
        }
        
        if (RayMath.FloatEquals(this.SpanMax, this._oldSpanMax) != 1) {
            this.Shader.SetValue(this.SpanMaxLoc, this.SpanMax, ShaderUniformDataType.Float);
            this._oldSpanMax = this.SpanMax;
        }
    }

    /// <summary>
    /// Sets the locations of shader parameters.
    /// </summary>
    private void SetLocations() {
        this.ResolutionLoc = this.Shader.GetLocation("resolution");
        this.ReduceMinLoc = this.Shader.GetLocation("reduceMin");
        this.ReduceMulLoc = this.Shader.GetLocation("reduceMul");
        this.SpanMaxLoc = this.Shader.GetLocation("spanMax");
    }
    
    /// <summary>
    /// Updates the resolution of the shader parameter.
    /// </summary>
    private void UpdateResolution() {
        this.Shader.SetValue(this.ResolutionLoc, new Vector2(Window.GetRenderWidth(), Window.GetRenderHeight()), ShaderUniformDataType.Vec2);
    }
}