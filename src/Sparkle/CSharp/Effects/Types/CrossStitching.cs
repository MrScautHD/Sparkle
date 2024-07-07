using System.Numerics;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Shaders;
using Raylib_CSharp.Windowing;

namespace Sparkle.CSharp.Effects.Types;

public class CrossStitching : Effect {
    
    public int ResolutionLoc { get; private set; }
    public int StitchingSizeLoc { get; private set; }
    public int InvertLoc { get; private set; }
    
    public float StitchingSize;
    public bool Invert;
    
    /// <summary>
    /// Constructor for creating a CrossStitching object.
    /// </summary>
    /// <param name="shader">The shader to be used by the cross stitching effect.</param>
    /// <param name="stitchingSize">The size of the stitching pattern.</param>
    /// <param name="invert">Indicates whether to invert the stitching effect.</param>
    public CrossStitching(Shader shader, float stitchingSize = 6.0F, bool invert = false) : base(shader) {
        this.StitchingSize = stitchingSize;
        this.Invert = invert;
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
        
        this.Shader.SetValue(this.StitchingSizeLoc, this.StitchingSize, ShaderUniformDataType.Float);
        this.Shader.SetValue(this.InvertLoc, this.Invert ? 1 : 0, ShaderUniformDataType.Float);
    }

    /// <summary>
    /// Sets the locations of shader parameters.
    /// </summary>
    private void SetLocations() {
        this.ResolutionLoc = this.Shader.GetLocation("resolution");
        this.StitchingSizeLoc = this.Shader.GetLocation("stitchingSize");
        this.InvertLoc = this.Shader.GetLocation("invert");
    }
    
    /// <summary>
    /// Updates the resolution of the shader parameter.
    /// </summary>
    private void UpdateResolution() {
        this.Shader.SetValue(this.ResolutionLoc, new Vector2(Window.GetRenderWidth(), Window.GetRenderHeight()), ShaderUniformDataType.Vec2);
    }
}