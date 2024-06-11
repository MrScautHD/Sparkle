using System.Numerics;
using Raylib_CSharp.Shaders;
using Raylib_CSharp.Windowing;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Effects.Types;

public class FxaaEffect : Effect {
    
    public float ReduceMin;
    public float ReduceMul;
    public float SpanMax;
    
    public int TextureLoc { get; private set; }
    public int ResolutionLoc { get; private set; }
    
    public int ReduceMinLoc { get; private set; }
    public int ReduceMulLoc { get; private set; }
    public int SpanMaxLoc { get; private set; }

    /// <summary>
    /// Constructor for creating a Fxaa effect object.
    /// </summary>
    /// <param name="vertPath">Path to the vertex shader file.</param>
    /// <param name="fragPath">Path to the fragment shader file.</param>
    /// <param name="reduceMin">Minimum reduction value.</param>
    /// <param name="reduceMul">Reduction multiplier value.</param>
    /// <param name="spanMax">Maximum span value.</param>
    public FxaaEffect(string vertPath, string fragPath, float reduceMin = 1.0F / 128.0F, float reduceMul = 1.0F / 8.0F, float spanMax = 8.0F) : base(vertPath, fragPath) {
        this.ReduceMin = reduceMin;
        this.ReduceMul = reduceMul;
        this.SpanMax = spanMax;
    }
    
    protected internal override void Init() {
        base.Init();
        this.SetLocations();
        this.UpdateValues();
    }
    
    /// <summary>
    /// Sets the locations of shader parameters.
    /// </summary>
    private void SetLocations() {
        this.TextureLoc = this.Shader.GetLocation("texture0");
        this.ResolutionLoc = this.Shader.GetLocation("resolution");
        this.ReduceMinLoc = this.Shader.GetLocation("reduceMin");
        this.ReduceMulLoc = this.Shader.GetLocation("reduceMul");
        this.SpanMaxLoc = this.Shader.GetLocation("spanMax");
    }
    
    /// <summary>
    /// Updates the values of the shader.
    /// </summary>
    private void UpdateValues() {
        this.Shader.SetValueTexture(this.TextureLoc, SceneManager.FilterTexture.Texture);
        this.Shader.SetValue(this.ResolutionLoc, new Vector2(Window.GetRenderWidth(), Window.GetRenderHeight()), ShaderUniformDataType.Vec2);
        
        this.Shader.SetValue(this.ReduceMinLoc, this.ReduceMin, ShaderUniformDataType.Float);
        this.Shader.SetValue(this.ReduceMulLoc, this.ReduceMul, ShaderUniformDataType.Float);
        this.Shader.SetValue(this.SpanMaxLoc, this.SpanMax, ShaderUniformDataType.Float);
    }
}