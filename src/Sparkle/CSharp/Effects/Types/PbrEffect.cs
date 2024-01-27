using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Effects.Types;

public class PbrEffect : Effect {
    
    public Color AmbientColor;
    public float AmbientIntensity;
    
    private int _lightBuffer;
    private int _lightIds;
    
    private Dictionary<int, Light> _lights;
    
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
    
    public PbrEffect(string vertPath, string fragPath, Color ambientColor, float ambientIntensity = 0.02F) : base(vertPath, fragPath) {
        this.AmbientColor = ambientColor;
        this.AmbientIntensity = ambientIntensity;
        this._lights = new Dictionary<int, Light>();
    }

    protected internal override void Init() {
        base.Init();
        GL.GenBuffers(1, ref this._lightBuffer);
        this.SetLocations();
    }

    protected internal override void Update() {
        base.Update();
        this.UpdateValues();
    }

    /// <summary>
    /// Adds a new light to the PBR effect.
    /// </summary>
    /// <param name="type">The type of the light.</param>
    /// <param name="position">The position of the light.</param>
    /// <param name="target">The target of the light.</param>
    /// <param name="color">The color of the light.</param>
    /// <param name="intensity">The intensity of the light.</param>
    /// <param name="id">The assigned ID of the light.</param>
    public void AddLight(LightType type, Vector3 position, Vector3 target, Color color, float intensity, out int id) {
        id = this._lightIds++;
        
        if (this._lights.Count >= 815) {
            Logger.Warn($"The light with ID: [{id}] cannot be added because the maximum size of the light buffer has been reached.");
            return;
        }

        Light light = new Light() {
            Enabled = 1,
            Type = (int) type,
            Position = position,
            Target = target,
            Color = ColorHelper.Normalize(color),
            Intensity = intensity
        };

        this._lights.Add(id, light);
    }

    /// <summary>
    /// Removes a light from the PBR effect.
    /// </summary>
    /// <param name="id">The ID of the light</param>
    public void RemoveLight(int id) {
        this._lights.Remove(id);
    }
    
    /// <summary>
    /// Sets shader locations for light source parameters.
    /// </summary>
    private unsafe void SetLocations() {
        this.Shader.Locs[(int) ShaderLocationIndex.MapAlbedo] = ShaderHelper.GetLocation(this.Shader, "albedoMap");
        this.Shader.Locs[(int) ShaderLocationIndex.MapMetalness] = ShaderHelper.GetLocation(this.Shader, "mraMap");
        this.Shader.Locs[(int) ShaderLocationIndex.MapNormal] = ShaderHelper.GetLocation(this.Shader, "normalMap");
        this.Shader.Locs[(int) ShaderLocationIndex.MapEmission] = ShaderHelper.GetLocation(this.Shader, "emissiveMap");
        this.Shader.Locs[(int) ShaderLocationIndex.ColorDiffuse] = ShaderHelper.GetLocation(this.Shader, "albedoColor");
        this.Shader.Locs[(int) ShaderLocationIndex.VectorView] = ShaderHelper.GetLocation(this.Shader, "viewPos");
        
        this.LightCountLoc = ShaderHelper.GetLocation(this.Shader, "numOfLights");
        
        this.AmbientLoc = ShaderHelper.GetLocation(this.Shader, "ambient");
        this.AmbientColorLoc = ShaderHelper.GetLocation(this.Shader, "ambientColor");
        
        this.EmissivePowerLoc = ShaderHelper.GetLocation(this.Shader, "emissivePower");
        this.EmissiveColorLoc = ShaderHelper.GetLocation(this.Shader, "emissiveColor");
        this.TilingLoc = ShaderHelper.GetLocation(this.Shader, "tiling");

        this.UseTexAlbedoLoc = ShaderHelper.GetLocation(this.Shader, "useTexAlbedo");
        this.UseTexNormalLoc = ShaderHelper.GetLocation(this.Shader, "useTexNormal");
        this.UseTexMraLoc = ShaderHelper.GetLocation(this.Shader, "useTexMRA");
        this.UseTexEmissiveLoc = ShaderHelper.GetLocation(this.Shader, "useTexEmissive");
    }
    
    /// <summary>
    /// Updates the values of the light source for shader rendering.
    /// </summary>
    private unsafe void UpdateValues() {
        if (SceneManager.MainCam3D == null) return;

        ShaderHelper.SetValue(this.Shader, this.LightCountLoc, this._lights.Count, ShaderUniformDataType.Int);
        
        ShaderHelper.SetValue(this.Shader, this.AmbientColorLoc, ColorHelper.Normalize(this.AmbientColor), ShaderUniformDataType.Vec3);
        ShaderHelper.SetValue(this.Shader, this.AmbientLoc, this.AmbientIntensity, ShaderUniformDataType.Float);
        
        ShaderHelper.SetValue(this.Shader, this.UseTexAlbedoLoc, 1, ShaderUniformDataType.Int);
        ShaderHelper.SetValue(this.Shader, this.UseTexNormalLoc, 1, ShaderUniformDataType.Int);
        ShaderHelper.SetValue(this.Shader, this.UseTexMraLoc, 1, ShaderUniformDataType.Int);
        ShaderHelper.SetValue(this.Shader, this.UseTexEmissiveLoc, 1, ShaderUniformDataType.Int);
        
        ShaderHelper.SetValue(this.Shader, this.Shader.Locs[(int) ShaderLocationIndex.VectorView], SceneManager.MainCam3D.Position, ShaderUniformDataType.Vec3);
        
        GL.UseProgram((int) this.Shader.Id);
        GL.BindBuffer(BufferTargetARB.UniformBuffer, this._lightBuffer);
        GL.BindBufferBase(BufferTargetARB.UniformBuffer, 0, this._lightBuffer);

        GL.BufferData(BufferTargetARB.UniformBuffer, sizeof(Light) * 815, IntPtr.Zero, BufferUsageARB.DynamicDraw);

        for (int i = 0; i < this._lights.Count; i++) {
            GL.BufferSubData(BufferTargetARB.UniformBuffer, IntPtr.Zero, sizeof(Light) * i, this._lights[i]);
        }
        
        GL.BindBufferBase(BufferTargetARB.UniformBuffer, 0, this._lightBuffer);
        GL.BindBuffer(BufferTargetARB.UniformBuffer, 0);
    }
    
    /// <summary>
    /// Represents light data.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    private struct Light {
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
        base.Dispose(disposing);
        
        if (disposing) {
            GL.DeleteBuffers(1, this._lightBuffer);
        }
    }
}