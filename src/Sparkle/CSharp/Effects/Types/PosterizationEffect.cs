using Raylib_CSharp;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Shaders;

namespace Sparkle.CSharp.Effects.Types;

public class PosterizationEffect : Effect {
    
    public int GammaLoc { get; private set; }
    public int NumOfColorsLoc { get; private set; }
    
    public float Gamma;
    public int NumOfColors;
    
    private float _oldGamma;
    private int _oldNumOfColors;

    /// <summary>
    /// Constructor for creating a PosterizationEffect object.
    /// </summary>
    /// <param name="shader">The shader to be used by the posterization effect.</param>
    /// <param name="gamma">Gamma value for the effect.</param>
    /// <param name="numOfColors">Number of colors to use in the effect.</param>
    public PosterizationEffect(Shader shader, float gamma = 0.6F, int numOfColors = 8) : base(shader) {
        this.Gamma = gamma;
        this.NumOfColors = numOfColors;
    }

    protected internal override void Init() {
        base.Init();
        this.SetLocations();
    }

    public override void Apply(Material? material = default) {
        base.Apply(material);

        if (RayMath.FloatEquals(this.Gamma, this._oldGamma) != 1) {
            this.Shader.SetValue(this.GammaLoc, this.Gamma, ShaderUniformDataType.Float);
            this._oldGamma = this.Gamma;
        }

        if (this.NumOfColors != this._oldNumOfColors) {
            this.Shader.SetValue(this.NumOfColorsLoc, this.NumOfColors, ShaderUniformDataType.Int);
            this._oldNumOfColors = this.NumOfColors;
        }
    }

    /// <summary>
    /// Sets the locations of shader parameters.
    /// </summary>
    private void SetLocations() {
        this.GammaLoc = this.Shader.GetLocation("gamma");
        this.NumOfColorsLoc = this.Shader.GetLocation("numOfColors");
    }
}