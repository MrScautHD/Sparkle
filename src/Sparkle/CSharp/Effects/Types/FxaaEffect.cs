using System.Numerics;
using Raylib_CSharp.Shaders;
using Raylib_CSharp.Windowing;

namespace Sparkle.CSharp.Effects.Types;

public class FxaaEffect : Effect {
    
    public int ResolutionLoc { get; private set; }
    public int ReduceMinLoc { get; private set; }
    public int ReduceMulLoc { get; private set; }
    public int SpanMaxLoc { get; private set; }
    
    private float _reduceMin;
    private float _reduceMul;
    private float _spanMax;

    /// <summary>
    /// Constructor for creating a Fxaa effect object.
    /// </summary>
    /// <param name="vertPath">Path to the vertex shader file.</param>
    /// <param name="fragPath">Path to the fragment shader file.</param>
    /// <param name="reduceMin">Minimum reduction value.</param>
    /// <param name="reduceMul">Reduction multiplier value.</param>
    /// <param name="spanMax">Maximum span value.</param>
    public FxaaEffect(string vertPath, string fragPath, float reduceMin = 1.0F / 128.0F, float reduceMul = 1.0F / 8.0F, float spanMax = 8.0F) : base(vertPath, fragPath) {
        this._reduceMin = reduceMin;
        this._reduceMul = reduceMul;
        this._spanMax = spanMax;
    }

    /// <summary>
    /// Gets or sets the minimum reduction value.
    /// </summary>
    public float ReduceMin {
        get => this._reduceMin;
        set {
            this._reduceMin = value;
            this.Shader.SetValue(this.ReduceMinLoc, this._reduceMin, ShaderUniformDataType.Float);
        }
    }

    /// <summary>
    /// Gets or sets the reduction multiplier value.
    /// </summary>
    public float ReduceMul {
        get => this._reduceMul;
        set {
            this._reduceMul = value;
            this.Shader.SetValue(this.ReduceMulLoc, this._reduceMul, ShaderUniformDataType.Float);
        }
    }

    /// <summary>
    /// Gets or sets the maximum span value.
    /// </summary>
    public float SpanMax {
        get => this._spanMax;
        set {
            this._spanMax = value;
            this.Shader.SetValue(this.SpanMaxLoc, this._spanMax, ShaderUniformDataType.Float);
        }
    }
    
    protected internal override void Init() {
        base.Init();
        this.SetLocations();
        this.UpdateResolution();
    }

    protected internal override void Update() {
        base.Update();
        this.UpdateValues();
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
    /// Updates the shader parameters.
    /// </summary>
    private void UpdateValues() {
        if (Window.IsResized()) {
            this.UpdateResolution();
        }
    }

    /// <summary>
    /// Updates the resolution of the shader parameter.
    /// </summary>
    private void UpdateResolution() {
        this.Shader.SetValue(this.ResolutionLoc, new Vector2(Window.GetRenderWidth(), Window.GetRenderHeight()), ShaderUniformDataType.Vec2);
    }
}