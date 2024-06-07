using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Rendering.Gl;
using Raylib_CSharp.Shaders;
using Sparkle.CSharp.Logging;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Effects.Types;

public class PbrEffect : Effect {
    
    public GlVersion GlVersion { get; private set; }
    
    public Color AmbientColor;
    public float AmbientIntensity;
    
    private int _lightBuffer;
    
    private int _lightIds;
    private Dictionary<int, LightData> _lights;
    
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
    
    public PbrEffect(string vertPath, string fragPath, GlVersion glVersion, Color ambientColor, float ambientIntensity = 0.02F) : base(vertPath, fragPath) {
        this.GlVersion = glVersion;
        this.AmbientColor = ambientColor;
        this.AmbientIntensity = ambientIntensity;
        this._lights = new Dictionary<int, LightData>();
    }

    protected internal override void Init() {
        base.Init();
        this.LoadBuffer();
        this.SetLocations();
    }

    protected internal override void Update() {
        base.Update();
        this.UpdateValues();
    }
    
    protected internal override void UpdateMaterialParameters(Material material) {
        base.UpdateMaterialParameters(material);
        
        this.Shader.SetValue(this.TilingLoc, new Vector2(material.Param[0], material.Param[1]), ShaderUniformDataType.Vec2);
        this.Shader.SetValue(this.EmissiveColorLoc, Color.Normalize(material.Maps[(int) MaterialMapIndex.Emission].Color), ShaderUniformDataType.Vec4);
        this.Shader.SetValue(this.EmissivePowerLoc, material.Maps[(int) MaterialMapIndex.Emission].Value, ShaderUniformDataType.Float);
    }

    /// <summary>
    /// Adds a light to the PbrEffect.
    /// </summary>
    /// <param name="enabled">Whether the light should be enabled or disabled.</param>
    /// <param name="type">The type of the light.</param>
    /// <param name="position">The position of the light.</param>
    /// <param name="target">The target direction of the light.</param>
    /// <param name="color">The color of the light.</param>
    /// <param name="intensity">The intensity of the light.</param>
    /// <param name="id">An output parameter to store the ID of the added light.</param>
    /// <returns>Returns true if the light was successfully added, false otherwise.</returns>
    public bool AddLight(bool enabled, LightType type, Vector3 position, Vector3 target, Color color, float intensity, out int id) {
        id = this._lightIds++;

        if (this.GlVersion == GlVersion.OpenGl33) {
            if (this._lights.Count >= 1024) {
                Logger.Warn($"The light with ID: [{id}] cannot be added because the maximum size of the light buffer has been reached.");
                return false;
            }
        }
        
        LightData lightData = new LightData() {
            Enabled = enabled ? 1 : 0,
            Type = (int) type,
            Position = position,
            Target = target,
            Color = Color.Normalize(color) * intensity,
        };
        
        this._lights.Add(id, lightData);
        return true;
    }
    
    /// <summary>
    /// Removes a light from the PBR effect.
    /// </summary>
    /// <param name="id">The ID of the light</param>
    public void RemoveLight(int id) {
        this._lights.Remove(id);
    }

    /// <summary>
    /// Updates the parameters of a specific light.
    /// </summary>
    /// <param name="id">The ID of the light to update.</param>
    /// <param name="enabled">Whether the light is enabled or disabled.</param>
    /// <param name="type">The type of the light.</param>
    /// <param name="position">The position of the light.</param>
    /// <param name="target">The target of the light.</param>
    /// <param name="color">The color of the light.</param>
    /// <param name="intensity">The intensity of the light.</param>
    public void UpdateLightParameters(int id, bool enabled, LightType type, Vector3 position, Vector3 target, Color color, float intensity) {
        LightData lightData = this._lights[id];
        lightData.Enabled = enabled ? 1 : 0;
        lightData.Type = (int) type;
        lightData.Position = position;
        lightData.Target = target;
        lightData.Color = Color.Normalize(color) * intensity;

        this._lights[id] = lightData;
    }
    
    /// <summary>
    /// Sets shader locations for light source parameters.
    /// </summary>
    private void SetLocations() {
        this.Shader.Locs[(int) ShaderLocationIndex.MapAlbedo] = this.Shader.GetLocation("albedoMap");
        this.Shader.Locs[(int) ShaderLocationIndex.MapMetalness] = this.Shader.GetLocation("mraMap");
        this.Shader.Locs[(int) ShaderLocationIndex.MapNormal] = this.Shader.GetLocation("normalMap");
        this.Shader.Locs[(int) ShaderLocationIndex.MapEmission] = this.Shader.GetLocation("emissiveMap");
        this.Shader.Locs[(int) ShaderLocationIndex.ColorDiffuse] = this.Shader.GetLocation("albedoColor");
        this.Shader.Locs[(int) ShaderLocationIndex.VectorView] = this.Shader.GetLocation("viewPos");
        
        this.LightCountLoc = this.Shader.GetLocation("numOfLights");
        
        this.AmbientLoc = this.Shader.GetLocation("ambient");
        this.AmbientColorLoc = this.Shader.GetLocation("ambientColor");
        
        this.EmissivePowerLoc = this.Shader.GetLocation("emissivePower");
        this.EmissiveColorLoc = this.Shader.GetLocation("emissiveColor");
        this.TilingLoc = this.Shader.GetLocation("tiling");

        this.UseTexAlbedoLoc = this.Shader.GetLocation("useTexAlbedo");
        this.UseTexNormalLoc = this.Shader.GetLocation("useTexNormal");
        this.UseTexMraLoc = this.Shader.GetLocation("useTexMRA");
        this.UseTexEmissiveLoc = this.Shader.GetLocation("useTexEmissive");
    }
    
    /// <summary>
    /// Updates the values of the light source for shader rendering.
    /// </summary>
    private void UpdateValues() {
        if (SceneManager.ActiveCam3D == null) return;
        
        this.Shader.SetValue(this.LightCountLoc, this._lights.Count, ShaderUniformDataType.Int);
        
        this.Shader.SetValue(this.AmbientColorLoc, Color.Normalize(this.AmbientColor), ShaderUniformDataType.Vec3);
        this.Shader.SetValue(this.AmbientLoc, this.AmbientIntensity, ShaderUniformDataType.Float);
        
        this.Shader.SetValue(this.UseTexAlbedoLoc, 1, ShaderUniformDataType.Int);
        this.Shader.SetValue(this.UseTexNormalLoc, 1, ShaderUniformDataType.Int);
        this.Shader.SetValue(this.UseTexMraLoc, 1, ShaderUniformDataType.Int);
        this.Shader.SetValue(this.UseTexEmissiveLoc, 1, ShaderUniformDataType.Int);
        
        this.Shader.SetValue(this.Shader.Locs[(int) ShaderLocationIndex.VectorView], SceneManager.ActiveCam3D.Position, ShaderUniformDataType.Vec3);
        
        this.UpdateBuffer();
    }

    private void LoadBuffer() {
        GL.GenBuffers(1, ref this._lightBuffer);
    }

    public unsafe void UpdateBuffer() {
        LightData[] lightData = this._lights.Values.ToArray();
        
        if (this.GlVersion == GlVersion.OpenGl33) {
            
            // TODO: Replace this System with Textures one.
            GL.UseProgram((int) this.Shader.Id);
            GL.BindBuffer(BufferTarget.UniformBuffer, this._lightBuffer);
            GL.BindBufferBase(BufferTarget.UniformBuffer, 0, this._lightBuffer);
            
            GL.BufferData(BufferTarget.UniformBuffer, lightData.Length * Marshal.SizeOf(typeof(LightData)), nint.Zero, BufferUsage.DynamicCopy);
            GL.BufferSubData(BufferTarget.UniformBuffer, 0, lightData);
            
            GL.BindBufferBase(BufferTarget.UniformBuffer, 0, this._lightBuffer);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
            
            GL.UseProgram(0);
        }
        else {
            GL.UseProgram((int) this.Shader.Id);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, this._lightBuffer);
            GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 0, this._lightBuffer);
            
            GL.BufferData(BufferTarget.ShaderStorageBuffer, lightData.Length * Marshal.SizeOf(typeof(LightData)), nint.Zero, BufferUsage.DynamicCopy);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, lightData);
            
            GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 0, this._lightBuffer);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            
            GL.UseProgram(0);
        }
    }

    private void UnloadBuffer() {
        GL.DeleteBuffers(1, this._lightBuffer);
    }

    /// <summary>
    /// Represents light data.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    private struct LightData {
        [FieldOffset(0)] public int Enabled;
        [FieldOffset(4)] public int Type;
        [FieldOffset(16)] public Vector3 Position;
        [FieldOffset(32)] public Vector3 Target;
        [FieldOffset(48)] public Vector4 Color;
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
            this.UnloadBuffer();
        }
    }
}