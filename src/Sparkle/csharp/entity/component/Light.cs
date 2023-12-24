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
    public float Intensity;

    public float AmbientIntensity;
    public Color AmbientColor;
    
    public int LightIndex { get; private set; }
    
    private int _lightCountLoc;
    
    private int _ambientLoc;
    private int _ambientColorLoc;
    
    private int _emissivePowerLoc;
    private int _emissiveColorLoc;
    private int _tilingLoc;

    private int _useTexAlbedoLoc;
    private int _useTexNormalLoc;
    private int _useTexMRALoc;
    private int _useTexEmissiveLoc;
    
    private int _enabledLoc;
    private int _typeLoc;
    private int _posLoc;
    private int _targetLoc;
    private int _colorLoc;
    private int _intensityLoc;

    /// <summary>
    /// Represents a light source in a 3D environment.
    /// </summary>
    /// <param name="type">The type of light source.</param>
    /// <param name="target">The position that the light is pointing towards.</param>
    /// <param name="color">The color of the light.</param>
    /// <param name="ambientColor">The ambient color of the light.</param>
    /// <param name="intensity">The intensity of the light.</param>
    /// <param name="ambientIntensity">The ambient intensity of the light.</param>
    public Light(LightType type, Vector3 target, Color color, Color ambientColor, float intensity = 1, float ambientIntensity = 0.02F) {
        this.Enabled = true;
        this.Type = type;
        this.Target = target;
        this.Color = color;
        this.Intensity = intensity;
        this.AmbientIntensity = ambientIntensity;
        this.AmbientColor = ambientColor;
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
        ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.SHADER_LOC_MAP_ALBEDO] = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "albedoMap");
        ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.SHADER_LOC_MAP_METALNESS] = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "mraMap");
        ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.SHADER_LOC_MAP_NORMAL] = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "normalMap");
        ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.SHADER_LOC_MAP_EMISSION] = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "emissiveMap");
        ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.SHADER_LOC_COLOR_DIFFUSE] = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "albedoColor");
        ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW] = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "viewPos");
        
        this._lightCountLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "numOfLights");
        
        this._ambientLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "ambient");
        this._ambientColorLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "ambientColor");
        
        this._emissivePowerLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "emissivePower");
        this._emissiveColorLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "emissiveColor");
        this._tilingLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "tiling");

        this._useTexAlbedoLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "useTexAlbedo");
        this._useTexNormalLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "useTexNormal");
        this._useTexMRALoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "useTexMRA");
        this._useTexEmissiveLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "useTexEmissive");
        
        this._enabledLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, $"lights[{this.LightIndex}].enabled");
        this._typeLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, $"lights[{this.LightIndex}].type");
        this._posLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, $"lights[{this.LightIndex}].position");
        this._targetLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, $"lights[{this.LightIndex}].target");
        this._colorLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, $"lights[{this.LightIndex}].color");
        this._intensityLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, $"lights[{this.LightIndex}].intensity");
    }
    
    /// <summary>
    /// Updates the values of the light source for shader rendering.
    /// </summary>
    private unsafe void UpdateValues() {
        if (SceneManager.MainCam3D == null) return;
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._lightCountLoc, LightCount, ShaderUniformDataType.SHADER_UNIFORM_INT);
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._ambientColorLoc, ColorHelper.Normalize(this.AmbientColor), ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._ambientLoc, this.AmbientIntensity, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._useTexAlbedoLoc, 1, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._useTexNormalLoc, 1, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._useTexMRALoc, 1, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._useTexEmissiveLoc, 1, ShaderUniformDataType.SHADER_UNIFORM_INT);
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW], SceneManager.MainCam3D.Position, ShaderUniformDataType.SHADER_UNIFORM_INT);
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._enabledLoc, this.Enabled ? 1 : 0, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._typeLoc, (int) this.Type, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._posLoc, this.Entity.Position, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._targetLoc, this.Target, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._colorLoc, ColorHelper.Normalize(this.Color), ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this._intensityLoc, this.Intensity, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
    }
    
    /// <summary>
    /// Defines the types of lights, including directional and point lights.
    /// </summary>
    public enum LightType {
        Directional,
        Point,
        Spot
    }

    protected override void Dispose(bool disposing) { }
}