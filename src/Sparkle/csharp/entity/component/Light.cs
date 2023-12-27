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
    
    public int LightCountLoc { get; private set; }
    
    public int AmbientLoc { get; private set; }
    public int AmbientColorLoc { get; private set; }
    
    public int EmissivePowerLoc { get; private set; }
    public int EmissiveColorLoc { get; private set; }
    public int TilingLoc { get; private set; }

    public int UseTexAlbedoLoc { get; private set; }
    public int UseTexNormalLoc { get; private set; }
    public int UseTexMraLoc { get; private set; }
    public int UseTexEmissiveLoc { get; private set; }
    
    public int EnabledLoc { get; private set; }
    public int TypeLoc { get; private set; }
    public int PosLoc { get; private set; }
    public int TargetLoc { get; private set; }
    public int ColorLoc { get; private set; }
    public int IntensityLoc { get; private set; }

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

    protected internal override void Draw() {
        base.Draw();
        SceneManager.MainCam3D!.BeginMode3D();
        
        ModelHelper.DrawSphere(this.Entity.Position, 0.2f, 8, 8, this.Color);
        
        SceneManager.MainCam3D!.EndMode3D();
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
        
        this.LightCountLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "numOfLights");
        
        this.AmbientLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "ambient");
        this.AmbientColorLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "ambientColor");
        
        this.EmissivePowerLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "emissivePower");
        this.EmissiveColorLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "emissiveColor");
        this.TilingLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "tiling");

        this.UseTexAlbedoLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "useTexAlbedo");
        this.UseTexNormalLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "useTexNormal");
        this.UseTexMraLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "useTexMRA");
        this.UseTexEmissiveLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "useTexEmissive");
        
        this.EnabledLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, $"lights[{this.LightIndex}].enabled");
        this.TypeLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, $"lights[{this.LightIndex}].type");
        this.PosLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, $"lights[{this.LightIndex}].position");
        this.TargetLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, $"lights[{this.LightIndex}].target");
        this.ColorLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, $"lights[{this.LightIndex}].color");
        this.IntensityLoc = ShaderHelper.GetLocation(ShaderRegistry.Pbr, $"lights[{this.LightIndex}].intensity");
    }
    
    /// <summary>
    /// Updates the values of the light source for shader rendering.
    /// </summary>
    private unsafe void UpdateValues() {
        if (SceneManager.MainCam3D == null) return;
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.LightCountLoc, LightCount, ShaderUniformDataType.SHADER_UNIFORM_INT);
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.AmbientColorLoc, ColorHelper.Normalize(this.AmbientColor), ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.AmbientLoc, this.AmbientIntensity, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.UseTexAlbedoLoc, 1, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.UseTexNormalLoc, 1, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.UseTexMraLoc, 1, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.UseTexEmissiveLoc, 1, ShaderUniformDataType.SHADER_UNIFORM_INT);
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.SHADER_LOC_VECTOR_VIEW], SceneManager.MainCam3D.Position, ShaderUniformDataType.SHADER_UNIFORM_INT);
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.EnabledLoc, this.Enabled ? 1 : 0, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.TypeLoc, (int) this.Type, ShaderUniformDataType.SHADER_UNIFORM_INT);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.PosLoc, this.Entity.Position, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.TargetLoc, this.Target, ShaderUniformDataType.SHADER_UNIFORM_VEC3);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.ColorLoc, ColorHelper.Normalize(this.Color), ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.IntensityLoc, this.Intensity, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
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