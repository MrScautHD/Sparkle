using Raylib_CSharp.Materials;
using Raylib_CSharp.Shaders;

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
    /// Sets the locations of shader parameters.
    /// </summary>
    private void SetLocations() {
        this.EnvironmentMapLoc = this.Shader.GetLocation("environmentMap");
    }

    /// <summary>
    /// Updates the shader parameters.
    /// </summary>
    private void UpdateValues() {
        this.Shader.SetValue(this.EnvironmentMapLoc, MaterialMapIndex.Cubemap, ShaderUniformDataType.Int);
    }
}