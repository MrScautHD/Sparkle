using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.helper;
using Sparkle.csharp.registry.types;
using Sparkle.csharp.scene;

namespace Sparkle.csharp.entity.component; 

public class Light : Component {

    public static int LightCount { get; private set; }
    
    public bool Enabled;
    public LightType Type;
    public Vector3 Target;
    public Color Color;
    public float LightLevel;
    
    public int LightIndex { get; private set; }
    
    private int _ambientLoc;
    private int _lightCountLoc;
    
    private int _enabledLoc;
    private int _typeLoc;
    private int _posLoc;
    private int _targetLoc;
    private int _colorLoc;

    /// <summary>
    /// Initializes a new instance of the Light class with the specified parameters.
    /// </summary>
    /// <param name="type">The type of light.</param>
    /// <param name="target">The position the light is directed towards.</param>
    /// <param name="color">The color of the light.</param>
    /// <param name="lightLevel">The level of brightness for the light. Default value is 0.1F.</param>
    public Light(LightType type, Vector3 target, Color color, float lightLevel = 0.1F) {
        this.Enabled = true;
        this.Type = type;
        this.Target = target;
        this.Color = color;
        this.LightLevel = lightLevel;
    }
    
    protected internal override void Init() {
        base.Init();
        this.SetLocations();
        this.SetLightIndex();
    }

    protected internal override void Update() {
        base.Update();
        this.UpdateValues();
    }
    
    /// <summary>
    /// Sets the index of the current light source.
    /// </summary>
    private void SetLightIndex() {
        this.LightIndex = LightCount;
        LightCount++;
    }

    /// <summary>
    /// Sets shader locations for light source parameters.
    /// </summary>
    private unsafe void SetLocations() {
        ShaderRegistry.Light.Locs[(int) ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW] = ShaderHelper.GetLocation(ShaderRegistry.Light, "viewPos");
        this._ambientLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, "ambient");
        this._lightCountLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, "lightCount");
        
        this._enabledLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, $"lights[{this.LightIndex}].enabled");
        this._typeLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, $"lights[{this.LightIndex}].type");
        this._posLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, $"lights[{this.LightIndex}].position");
        this._targetLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, $"lights[{this.LightIndex}].target");
        this._colorLoc = ShaderHelper.GetLocation(ShaderRegistry.Light, $"lights[{this.LightIndex}].color");
    }
    
    /// <summary>
    /// Updates the values of the light source for shader rendering.
    /// </summary>
    private unsafe void UpdateValues() {
        if (SceneManager.MainCam3D == null) return;
        
        ShaderHelper.SetValue(ShaderRegistry.Light, ShaderRegistry.Light.Locs[(int) ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW], SceneManager.MainCam3D.Position, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        
        Vector4 ambient = new Vector4(this.LightLevel, this.LightLevel, this.LightLevel, 1.0f);
        ShaderHelper.SetValue(ShaderRegistry.Light, this._ambientLoc, ambient, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        ShaderHelper.SetValue(ShaderRegistry.Light, this._lightCountLoc, LightCount, ShaderUniformDataType.SHADER_UNIFORM_INT);
        
        ShaderHelper.SetValue(ShaderRegistry.Light, this._enabledLoc, this.Enabled ? 1 : 0, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Light, this._typeLoc, (int) this.Type, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Light, this._posLoc, this.Entity.Position, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        ShaderHelper.SetValue(ShaderRegistry.Light, this._targetLoc, this.Target, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        ShaderHelper.SetValue(ShaderRegistry.Light, this._colorLoc, ColorHelper.Normalize(this.Color), ShaderUniformDataType.SHADER_UNIFORM_VEC4);
    }
    
    /// <summary>
    /// Defines the types of lights, including directional and point lights.
    /// </summary>
    public enum LightType {
        Directional,
        Pointed
    }

    protected override void Dispose(bool disposing) { }
}