using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Effects.Types;

public class SkyboxEffect : Effect {
    
    public int EnvironmentMapLoc { get; private set; }
    
    public SkyboxEffect(string vertPath, string fragPath) : base(vertPath, fragPath) { }

    protected internal override void Init() {
        base.Init();
        this.SetLocations();
        this.UpdateValues();
    }

    /// <summary>
    /// Sets shader locations for light source parameters.
    /// </summary>
    private void SetLocations() {
        this.EnvironmentMapLoc = ShaderHelper.GetLocation(this.Shader, "environmentMap");
    }

    /// <summary>
    /// Updates the values of the light source for shader rendering.
    /// </summary>
    private void UpdateValues() {
        ShaderHelper.SetValue(this.Shader, this.EnvironmentMapLoc, (int) MaterialMapIndex.Cubemap, ShaderUniformDataType.Int);
    }
}