using System.Numerics;
using Raylib_CSharp.Shaders;
using Raylib_CSharp.Windowing;

namespace Sparkle.CSharp.Effects.Types;

public class BlurEffect : Effect {
    
    public int ResolutionLoc { get; private set; }

    /// <summary>
    /// Initializes a new instance of the BlurEffect class.
    /// </summary>
    /// <param name="vertPath">Path to the vertex shader file.</param>
    /// <param name="fragPath">Path to the fragment shader file.</param>
    public BlurEffect(string vertPath, string fragPath) : base(vertPath, fragPath) { }

    protected internal override void Init() {
        base.Init();
        this.SetLocations();
        this.UpdateResolution();
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
    }

    /// <summary>
    /// Updates the shader parameters.
    /// </summary>
    private void UpdateValues() {
        if (Window.IsResized()) {
            this.UpdateResolution();
        }
    }

    /// <summary>
    /// Updates the resolution of the shader parameter.
    /// </summary>
    private void UpdateResolution() {
        this.Shader.SetValue(this.ResolutionLoc, new Vector2(Window.GetRenderWidth(), Window.GetRenderHeight()), ShaderUniformDataType.Vec2);
    }
}