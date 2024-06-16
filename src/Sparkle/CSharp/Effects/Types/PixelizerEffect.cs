using System.Numerics;
using Raylib_CSharp.Shaders;
using Raylib_CSharp.Windowing;

namespace Sparkle.CSharp.Effects.Types;

public class PixelizerEffect : Effect {
    
    public Vector2 PixelSize;
    
    public int ResolutionLoc { get; private set; }
    public int PixelSizeLoc { get; private set; }

    public PixelizerEffect(string vertPath, string fragPath, Vector2? pixelSize = default) : base(vertPath, fragPath) {
        this.PixelSize = pixelSize ?? new Vector2(5.0F, 5.0F);
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
        this.ResolutionLoc = this.Shader.GetLocation("resolution");
        this.PixelSizeLoc = this.Shader.GetLocation("pixelSize");
    }

    /// <summary>
    /// Updates the shader parameters.
    /// </summary>
    private void UpdateValues() {
        this.Shader.SetValue(this.ResolutionLoc, new Vector2(Window.GetRenderWidth(), Window.GetRenderHeight()), ShaderUniformDataType.Vec2);
        this.Shader.SetValue(this.PixelSizeLoc, this.PixelSize, ShaderUniformDataType.Vec2);
    }
}