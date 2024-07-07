using System.Numerics;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Rendering.Gl;
using Raylib_CSharp.Shaders;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Logging;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Effects.Types;

public class PbrEffect : Effect {
    
    public GlVersion GlVersion { get; private set; }
    
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
    
    public int LightBufferLoc { get; private set; }
    
    public Color AmbientColor;
    public float AmbientIntensity;
    
    private int _lightBuffer;
    
    private Dictionary<uint, LightData> _activeLights;
    private Dictionary<uint, LightData> _inactiveLights;
    
    private uint _lightIds;
    
    /// <summary>
    /// Constructor for creating a PbrEffect object.
    /// </summary>
    /// <param name="shader">The shader to be used by the PBR effect.</param>
    /// <param name="glVersion">The OpenGL version.</param>
    /// <param name="ambientColor">The ambient color for the effect.</param>
    /// <param name="ambientIntensity">The ambient intensity for the effect.</param>
    public PbrEffect(Shader shader, GlVersion glVersion, Color ambientColor, float ambientIntensity = 0.02F) : base(shader) {
        this.GlVersion = glVersion;
        this.AmbientColor = ambientColor;
        this.AmbientIntensity = ambientIntensity;
        this._activeLights = new Dictionary<uint, LightData>();
        this._inactiveLights = new Dictionary<uint, LightData>();
    }

    protected internal override void Init() {
        base.Init();
        this.LoadBuffer();
        this.SetLocations();
    }

    public override void Apply(Material? material = default) {
        base.Apply(material);
        Cam3D? cam = SceneManager.ActiveCam3D;
        if (cam == null) return;

        if (material != null) {
            Material mat = material.Value;
            this.Shader.SetValue(this.TilingLoc, new Vector2(mat.Param[0], mat.Param[1]), ShaderUniformDataType.Vec2);
            this.Shader.SetValue(this.EmissiveColorLoc, Color.Normalize(mat.Maps[(int) MaterialMapIndex.Emission].Color), ShaderUniformDataType.Vec4);
            this.Shader.SetValue(this.EmissivePowerLoc, mat.Maps[(int) MaterialMapIndex.Emission].Value, ShaderUniformDataType.Float);
            
            this.Shader.SetValue(this.UseTexAlbedoLoc, mat.Maps[(int) MaterialMapIndex.Albedo].Texture.Id != 0 ? 1 : 0, ShaderUniformDataType.Int);
            this.Shader.SetValue(this.UseTexNormalLoc, mat.Maps[(int) MaterialMapIndex.Normal].Texture.Id != 0 ? 1 : 0, ShaderUniformDataType.Int);
            this.Shader.SetValue(this.UseTexMraLoc, mat.Maps[(int) MaterialMapIndex.Metalness].Texture.Id != 0 ? 1 : 0, ShaderUniformDataType.Int);
            this.Shader.SetValue(this.UseTexEmissiveLoc, mat.Maps[(int) MaterialMapIndex.Emission].Texture.Id != 0 ? 1 : 0, ShaderUniformDataType.Int);
        }
        
        this.Shader.SetValue(this.AmbientColorLoc, Color.Normalize(this.AmbientColor), ShaderUniformDataType.Vec3);
        this.Shader.SetValue(this.AmbientLoc, this.AmbientIntensity, ShaderUniformDataType.Float);
        this.Shader.SetValue(this.LightCountLoc, this._activeLights.Count, ShaderUniformDataType.Int);
        this.Shader.SetValue(this.Shader.Locs[(int) ShaderLocationIndex.VectorView], cam.Position, ShaderUniformDataType.Vec3);
        
        this.UpdateBuffer();
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
        
        this.LightBufferLoc = this.Shader.GetLocation("lightBuffer");
    }

    /// <summary>
    /// Adds a light to the PbrEffect.
    /// </summary>
    /// <param name="type">The type of the light.</param>
    /// <param name="position">The position of the light.</param>
    /// <param name="target">The target direction of the light.</param>
    /// <param name="color">The color of the light.</param>
    /// <param name="intensity">The intensity of the light.</param>
    /// <param name="id">The ID of the light.</param>
    /// <returns>Returns true if the light was successfully added, false otherwise.</returns>
    public bool AddLight(LightType type, Vector3 position, Vector3 target, Color color, float intensity, out uint id) {
        id = ++this._lightIds;
        
        if (this.GlVersion == GlVersion.OpenGl33) {
            if (this._activeLights.Count >= 1024) {
                Logger.Warn($"The light with ID: [{id}] cannot be added because the maximum size of the light buffer has been reached.");
                return false;
            }
        }
        
        LightData lightData = new LightData() {
            Type = (int) type,
            Position = position,
            Target = target,
            Color = Color.Normalize(color) * intensity,
        };
        
        this._activeLights.Add(id, lightData);
        return true;
    }
    
    /// <summary>
    /// Removes a light from the PBR effect.
    /// </summary>
    /// <param name="id">The ID of the light</param>
    public void RemoveLight(uint id) {
        if (this._activeLights.ContainsKey(id)) {
            this._activeLights.Remove(id);
        }
        else if (this._inactiveLights.ContainsKey(id)) {
            this._inactiveLights.Remove(id);
        }
        else {
            Logger.Warn($"The light with ID: [{id}] can not be found.");
        }
    }

    /// <summary>
    /// Gets the active state of a light in the PbrEffect.
    /// </summary>
    /// <param name="id">The ID of the light.</param>
    /// <returns>Returns true if the light is active, false otherwise.</returns>
    public bool GetActiveState(uint id) {
        return this._activeLights.ContainsKey(id);
    }

    /// <summary>
    /// Sets the active state of a light in the PbrEffect. If active is true, the light becomes active,
    /// if active is false, the light becomes inactive.
    /// </summary>
    /// <param name="id">The ID of the light to change the state.</param>
    /// <param name="active">The desired active state of the light.</param>
    public void SetActiveState(uint id, bool active) {
        if (active) {
            if (!this.GetActiveState(id) && this._inactiveLights.ContainsKey(id)) {
                this._activeLights.Add(id, this._inactiveLights[id]);
                this._inactiveLights.Remove(id);
            }
        }
        else {
            if (this.GetActiveState(id) && !this._inactiveLights.ContainsKey(id)) {
                this._inactiveLights.Add(id, this._activeLights[id]);
                this._activeLights.Remove(id);
            }
        }
    }

    /// <summary>
    /// Updates the parameters of a specific light.
    /// </summary>
    /// <param name="id">The ID of the light to update.</param>
    /// <param name="type">The type of the light.</param>
    /// <param name="position">The position of the light.</param>
    /// <param name="target">The target of the light.</param>
    /// <param name="color">The color of the light.</param>
    /// <param name="intensity">The intensity of the light.</param>
    public void UpdateLightParams(uint id, LightType type, Vector3 position, Vector3 target, Color color, float intensity) {
        if (this.GetActiveState(id)) {
            LightData lightData = this._activeLights[id];
            lightData.Type = (int) type;
            lightData.Position = position;
            lightData.Target = target;
            lightData.Color = Color.Normalize(color) * intensity;

            this._activeLights[id] = lightData;
        }
    }

    /// <summary>
    /// Loads the light buffer and texture.
    /// </summary>
    private void LoadBuffer() {
        GL.GenBuffer(out this._lightBuffer);
    }
    
    public void UpdateBuffer() {
        if (this.GlVersion == GlVersion.OpenGl33) {
            GL.UseProgram((int) this.Shader.Id);
            
            GL.BindBuffer(BufferTarget.UniformBuffer, this._lightBuffer);
            GL.BindBufferBase(BufferTarget.UniformBuffer, 0, this._lightBuffer);
            
            GL.BufferData(BufferTarget.UniformBuffer, this._activeLights.Count * Marshal.SizeOf(typeof(LightData)), nint.Zero, BufferUsage.DynamicCopy);
            GL.BufferSubData(BufferTarget.UniformBuffer, 0, this._activeLights.Values.ToArray());
            
            GL.BindBufferBase(BufferTarget.UniformBuffer, 0, this._lightBuffer);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
            
            GL.UseProgram(0);
        }
        else {
            GL.UseProgram((int) this.Shader.Id);
            
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, this._lightBuffer);
            GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 0, this._lightBuffer);
            
            GL.BufferData(BufferTarget.ShaderStorageBuffer, this._activeLights.Count * Marshal.SizeOf(typeof(LightData)), nint.Zero, BufferUsage.DynamicCopy);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, this._activeLights.Values.ToArray());
            
            GL.BindBufferBase(BufferTarget.ShaderStorageBuffer, 0, this._lightBuffer);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            
            GL.UseProgram(0);
        }
    }

    /// <summary>
    /// Unloads the buffer and texture.
    /// </summary>
    private void UnloadBuffer() {
        GL.DeleteBuffer(this._lightBuffer);
    }

    /// <summary>
    /// Represents light data.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    private struct LightData {
        [FieldOffset(0)] public int Type;
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