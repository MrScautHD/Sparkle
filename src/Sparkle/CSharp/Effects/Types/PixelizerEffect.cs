using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Shaders;
using Raylib_CSharp.Windowing;

namespace Sparkle.CSharp.Effects.Types;

public class PixelizerEffect : Effect {
    
    public int ResolutionLoc { get; private set; }
    public int PixelSizeLoc { get; private set; }

    public Vector2 PixelSize;
    
    private Vector2 _oldPixelSize;

    /// <summary>
    /// Constructor for creating a PixelizerEffect object.
    /// </summary>
    /// <param name="shader">The shader to be used by the pixelizer effect.</param>
    /// <param name="pixelSize">Optional pixel size for the effect. Defaults to (5.0, 5.0) if not provided.</param>
    public PixelizerEffect(Shader shader, Vector2? pixelSize = default) : base(shader) {
        this.PixelSize = pixelSize ?? new Vector2(5.0F, 5.0F);
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

        if (RayMath.Vector2Equals(this.PixelSize, this._oldPixelSize) != 1) {
            this.Shader.SetValue(this.PixelSizeLoc, this._oldPixelSize, ShaderUniformDataType.Vec2);
            this._oldPixelSize = this.PixelSize;
        }
    }

    /// <summary>
    /// Sets the locations of shader parameters.
    /// </summary>
    private void SetLocations() {
        this.ResolutionLoc = this.Shader.GetLocation("resolution");
        this.PixelSizeLoc = this.Shader.GetLocation("pixelSize");
    }
    
    /// <summary>
    /// Updates the resolution of the shader parameter.
    /// </summary>
    private void UpdateResolution() {
        this.Shader.SetValue(this.ResolutionLoc, new Vector2(Window.GetRenderWidth(), Window.GetRenderHeight()), ShaderUniformDataType.Vec2);
    }
}