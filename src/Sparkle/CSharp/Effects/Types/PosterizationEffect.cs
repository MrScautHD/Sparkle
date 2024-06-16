using Raylib_CSharp.Shaders;

namespace Sparkle.CSharp.Effects.Types;

public class PosterizationEffect : Effect {
    
    public float Gamma;
    public int NumOfColors;
    
    public int GammaLoc { get; private set; }
    public int NumOfColorsLoc { get; private set; }

    public PosterizationEffect(string vertPath, string fragPath, float gamma = 0.6F, int numOfColors = 8) : base(vertPath, fragPath) {
        this.Gamma = gamma;
        this.NumOfColors = numOfColors;
    }

    protected internal override void Init() {
        base.Init();
        this.SetLocations();
    }

    protected internal override void Update() {
        base.Update();
        this.UpdateValues();
    }

    /// <summary>
    /// Sets the locations of shader parameters.
    /// </summary>
    private void SetLocations() {
        this.GammaLoc = this.Shader.GetLocation("gamma");
        this.NumOfColorsLoc = this.Shader.GetLocation("numOfColors");
    }

    /// <summary>
    /// Updates the shader parameters.
    /// </summary>
    private void UpdateValues() {
        this.Shader.SetValue(this.GammaLoc, this.Gamma, ShaderUniformDataType.Float);
        this.Shader.SetValue(this.NumOfColorsLoc, this.NumOfColors, ShaderUniformDataType.Int);
    }
}