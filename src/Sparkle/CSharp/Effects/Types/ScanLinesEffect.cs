using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Shaders;
using Raylib_CSharp.Windowing;

namespace Sparkle.CSharp.Effects.Types;

public class ScanLinesEffect : Effect {
    
    public int ResolutionLoc { get; private set; }
    public int OffestLoc { get; private set; }
    
    public float Offset;
    
    private float _oldOffset;
    
    /// <summary>
    /// Constructor for creating a ScanLinesEffect object.
    /// </summary>
    /// <param name="shader">The shader to be used by the scan lines effect.</param>
    /// <param name="offset">The offset for the scan lines.</param>
    public ScanLinesEffect(Shader shader, float offset = 0) : base(shader) {
        this.Offset = offset;
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
        
        if (RayMath.FloatEquals(this.Offset, this._oldOffset) != 1) {
            this.Shader.SetValue(this.OffestLoc, this.Offset, ShaderUniformDataType.Float);
            this._oldOffset = this.Offset;
        }
    }

    /// <summary>
    /// Sets the locations of shader parameters.
    /// </summary>
    private void SetLocations() {
        this.ResolutionLoc = this.Shader.GetLocation("resolution");
        this.OffestLoc = this.Shader.GetLocation("offset");
    }
    
    /// <summary>
    /// Updates the resolution of the shader parameter.
    /// </summary>
    private void UpdateResolution() {
        this.Shader.SetValue(this.ResolutionLoc, new Vector2(Window.GetRenderWidth(), Window.GetRenderHeight()), ShaderUniformDataType.Vec2);
    }
}