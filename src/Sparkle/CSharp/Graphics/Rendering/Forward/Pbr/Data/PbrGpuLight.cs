using System.Numerics;
using System.Runtime.InteropServices;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Pbr.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PbrGpuLight {
    
    /// <summary>
    /// Internal integer representation of the <see cref="LightType"/>.
    /// </summary>
    private int _lightType;
    
    /// <summary>
    /// The effective range of the light.
    /// </summary>
    private float _range;
    
    /// <summary>
    /// The inner spotlight cone angle in radians.
    /// </summary>
    private float _innerSpotAngle;
    
    /// <summary>
    /// The outer spotlight cone angle in radians.
    /// </summary>
    private float _outerSpotAngle;
    
    /// <summary>
    /// The light's position in world space, stored as a <see cref="Vector4"/> for GPU alignment.
    /// </summary>
    private Vector4 _position;
    
    /// <summary>
    /// The light's direction in world space, stored as a <see cref="Vector4"/> for GPU alignment.
    /// </summary>
    private Vector4 _direction;
    
    /// <summary>
    /// The light's color, with W storing intensity.
    /// </summary>
    private Vector4 _color;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PbrGpuLight"/> struct.
    /// </summary>
    /// <param name="type">The type of the light (Directional, Point, or Spot).</param>
    /// <param name="position">The position of the light in world space.</param>
    /// <param name="direction">The direction of the light (used for directional and spot lights).</param>
    /// <param name="color">The base color of the light as a <see cref="Vector3"/>.</param>
    /// <param name="intensity">The brightness or intensity of the light.</param>
    /// <param name="range">The range of the light (used for point and spot lights).</param>
    /// <param name="innerSpotAngle">The inner cone angle of the spotlight in radians, within which the light is at full intensity (used for spot lights).</param>
    /// <param name="outerSpotAngle">The outer cone angle of the spotlight in radians, at which the light fades out completely (used for spot lights).</param>
    public PbrGpuLight(LightType type, Vector3 position = default, Vector3 direction = default, Vector3 color = default, float intensity = 1.0F, float range = 0.0F, float innerSpotAngle = 0.0F, float outerSpotAngle = 0.0F) {
        this._lightType = (int) type;
        this._position = position.AsVector4();
        this._direction = direction.AsVector4();
        this._color = new Vector4(color, intensity); 
        this._range = range;
        this._innerSpotAngle = innerSpotAngle;
        this._outerSpotAngle = outerSpotAngle;
    }
    
    /// <summary>
    /// Gets or sets the type of the light (Directional, Point, or Spot).
    /// </summary>
    public LightType LightType {
        get => (LightType) this._lightType;
        set => this._lightType = (int) value;
    }
    
    /// <summary>
    /// Gets or sets the position of the light in world space.
    /// </summary>
    public Vector3 Position {
        get => this._position.AsVector3();
        set => this._position = value.AsVector4();
    }
    
    /// <summary>
    /// Gets or sets the direction of the light in world space. (Relevant for directional and spot lights)
    /// </summary>
    public Vector3 Direction {
        get => this._direction.AsVector3();
        set => this._direction = value.AsVector4();
    }
    
    /// <summary>
    /// Gets or sets the color of the light as an RGB vector.
    /// </summary>
    public Vector3 Color {
        get => this._color.AsVector3();
        set => this._color = new Vector4(value, this._color.W);
    }
    
    /// <summary>
    /// Gets or sets the intensity (brightness) of the light.
    /// </summary>
    public float Intensity {
        get => this._color.W;
        set => this._color.W = value;
    }
    
    /// <summary>
    /// Gets or sets the range of the light. (Relevant for point and spot lights)
    /// </summary>
    public float Range {
        get => this._range;
        set => this._range = value;
    }
    
    /// <summary>
    /// Gets or sets the cone angle of the spotlight in radians. (Relevant for spot lights)
    /// </summary>
    public float SpotAngle {
        get => this._innerSpotAngle;
        set => this._innerSpotAngle = value;
    }
    
    /// <summary>
    /// Computes the projection matrix for the light based on its type and relevant properties.
    /// </summary>
    /// <returns>
    /// A <see cref="Matrix4x4"/> representing the projection matrix of the light.
    /// The specific matrix depends on whether the light is of type Directional, Spot, or Point.
    /// </returns>
    public Matrix4x4 GetProjection() { // TODO: CHECK FOR WHAT NEEDED
        switch (this.LightType) {
            case LightType.Directional:
                float size = this._range > 0.0F ? this._range : 100.0F;
                return Matrix4x4.CreateOrthographic(size, size, 0.1F, size);
            
            case LightType.Spot:
                return Matrix4x4.CreatePerspectiveFieldOfView(this._innerSpotAngle, 1.0F, 1.0F, this._range);
            
            case LightType.Point:
                return Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2.0F, 1.0F, 0.1F, this._range);
        }
        
        return Matrix4x4.Identity;
    }
    
    /// <summary>
    /// Computes and returns the view matrix for the light based on its position and direction.
    /// </summary>
    /// <returns>The view matrix as a <see cref="Matrix4x4"/> that represents the light's perspective in the scene.</returns>
    public Matrix4x4 GetView() { // TODO: CHECK FOR WHAT NEEDED
        Vector3 target = this.Position + this.Direction;
        return Matrix4x4.CreateLookAt(this.Position, target, Vector3.UnitY);
    }
}