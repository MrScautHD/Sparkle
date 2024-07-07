using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Shaders;
using Raylib_CSharp.Windowing;

namespace Sparkle.CSharp.Effects.Types;

public class BloomEffect : Effect {
    
    public int ResolutionLoc { get; private set; }
    public int SamplesLoc { get; private set; }
    public int QualityLoc { get; private set; }

    public float Samples;
    public float Quality;

    private float _oldSamples;
    private float _oldQuality;

    /// <summary>
    /// Constructor for creating a BloomEffect object.
    /// </summary>
    /// <param name="shader">The shader to be used by the bloom effect.</param>
    /// <param name="samples">The number of samples for the bloom effect.</param>
    /// <param name="quality">The quality of the bloom effect.</param>
    public BloomEffect(Shader shader, float samples = 5.0F, float quality = 2.5F) : base(shader) {
        this.Samples = samples;
        this.Quality = quality;
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
        
        if (RayMath.FloatEquals(this.Samples, this._oldSamples) != 1) {
            this.Shader.SetValue(this.SamplesLoc, this.Samples, ShaderUniformDataType.Float);
            this._oldSamples = this.Samples;
        }
        
        if (RayMath.FloatEquals(this.Quality, this._oldQuality) != 1) {
            this.Shader.SetValue(this.QualityLoc, this.Quality, ShaderUniformDataType.Float);
            this._oldQuality = this.Quality;
        }
    }

    /// <summary>
    /// Sets the locations of shader parameters.
    /// </summary>
    private void SetLocations() {
        this.ResolutionLoc = this.Shader.GetLocation("resolution");
        this.SamplesLoc = this.Shader.GetLocation("samples");
        this.QualityLoc = this.Shader.GetLocation("quality");
    }

    /// <summary>
    /// Updates the resolution of the shader parameter.
    /// </summary>
    private void UpdateResolution() {
        this.Shader.SetValue(this.ResolutionLoc, new Vector2(Window.GetRenderWidth(), Window.GetRenderHeight()), ShaderUniformDataType.Vec2);
    }
}