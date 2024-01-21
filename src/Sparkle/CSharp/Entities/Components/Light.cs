using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using Raylib_cs;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities.Components; 

public class Light : Component {

    public static int LightCount { get; private set; }
    
    public bool Enabled;
    public LightType Type;
    public Vector3 Target;
    public Color Color;
    public float Intensity;

    public float AmbientIntensity;
    public Color AmbientColor;

    private int _lightBuffer;
    
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
        if (LightCount >= 815) {
            Logger.Error($"The initialization of Component [{ this.GetType().Name }] failed due to reaching the maximum limit of 815 lights.");
            return;
        }
        
        base.Init();
        GL.GenBuffers(1, ref this._lightBuffer);
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
        ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.MapAlbedo] = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "albedoMap");
        ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.MapMetalness] = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "mraMap");
        ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.MapNormal] = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "normalMap");
        ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.MapEmission] = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "emissiveMap");
        ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.ColorDiffuse] = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "albedoColor");
        ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.VectorView] = ShaderHelper.GetLocation(ShaderRegistry.Pbr, "viewPos");
        
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
    }
    
    /// <summary>
    /// Updates the values of the light source for shader rendering.
    /// </summary>
    private unsafe void UpdateValues() {
        if (SceneManager.MainCam3D == null) return;

        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.LightCountLoc, LightCount, ShaderUniformDataType.Int);
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.AmbientColorLoc, ColorHelper.Normalize(this.AmbientColor), ShaderUniformDataType.Vec3);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.AmbientLoc, this.AmbientIntensity, ShaderUniformDataType.Float);
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.UseTexAlbedoLoc, 1, ShaderUniformDataType.Int);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.UseTexNormalLoc, 1, ShaderUniformDataType.Int);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.UseTexMraLoc, 1, ShaderUniformDataType.Int);
        ShaderHelper.SetValue(ShaderRegistry.Pbr, this.UseTexEmissiveLoc, 1, ShaderUniformDataType.Int);
        
        ShaderHelper.SetValue(ShaderRegistry.Pbr, ShaderRegistry.Pbr.Locs[(int) ShaderLocationIndex.VectorView], SceneManager.MainCam3D.Position, ShaderUniformDataType.Vec3);
        
        GL.UseProgram((int) ShaderRegistry.Pbr.Id);
        GL.BindBuffer(BufferTargetARB.UniformBuffer, this._lightBuffer);
        GL.BindBufferBase(BufferTargetARB.UniformBuffer, 0, this._lightBuffer);

        LightData data = new LightData {
            Enabled = this.Enabled ? 1 : 0,
            Type = (int) this.Type,
            Position = this.Entity.Position,
            Target = this.Target,
            Color = ColorHelper.Normalize(this.Color),
            Intensity = this.Intensity
        };

        GL.BufferData(BufferTargetARB.UniformBuffer, sizeof(LightData) * 815, IntPtr.Zero, BufferUsageARB.DynamicDraw);
        GL.BufferSubData(BufferTargetARB.UniformBuffer, IntPtr.Zero, sizeof(LightData) * (LightIndex + 1), data);
        GL.BindBufferBase(BufferTargetARB.UniformBuffer, 0, this._lightBuffer);
        GL.BindBuffer(BufferTargetARB.UniformBuffer, 0);
    }


    /// <summary>
    /// Represents light data.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    private struct LightData {
        [FieldOffset(0)] public int Enabled;
        [FieldOffset(4)] public int Type;
        [FieldOffset(16)] public Vector3 Position;
        [FieldOffset(32)] public Vector3 Target;
        [FieldOffset(48)] public Vector4 Color;
        [FieldOffset(64)] public float Intensity;
    }
    
    /// <summary>
    /// Defines the types of lights, including directional and point lights.
    /// </summary>
    public enum LightType {
        Directional,
        Point,
        Spot
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            GL.DeleteBuffers(1, this._lightBuffer);
        }
    }
}