using System.Numerics;
using System.Runtime.InteropServices;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Lighting.Pbr;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PbrGpuLight : IEquatable<PbrGpuLight> {
    
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
    /// Gets or sets the inner angle, in radians, of the spotlight's cone.
    /// </summary>
    public float InnerSpotAngle {
        get => this._innerSpotAngle;
        set => this._innerSpotAngle = value;
    }
    
    /// <summary>
    /// Gets or sets the outer angle, in radians, of the spotlight's cone.
    /// </summary>
    public float OuterSpotAngle {
        get => this._outerSpotAngle;
        set => this._outerSpotAngle = value;
    }
    
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
    /// Determines whether two lights are equal.
    /// </summary>
    /// <param name="left">The first light to compare.</param>
    /// <param name="right">The second light to compare.</param>
    /// <returns><see langword="true"/> if both lights have identical properties; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(PbrGpuLight left, PbrGpuLight right) => left.Equals(right);
    
    /// <summary>
    /// Determines whether two lights are different.
    /// </summary>
    /// <param name="left">The first light to compare.</param>
    /// <param name="right">The second light to compare.</param>
    /// <returns><see langword="true"/> if the lights differ in any property; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(PbrGpuLight left, PbrGpuLight right) => !left.Equals(right);
    
    /// <summary>
    /// Determines whether this light is equal to another light.
    /// </summary>
    /// <param name="other">The light to compare with.</param>
    /// <returns><see langword="true"/> if both lights have identical properties; otherwise, <see langword="false"/>.</returns>
    public bool Equals(PbrGpuLight other) {
        return this._lightType == other._lightType &&
               this._range.Equals(other._range) &&
               this._innerSpotAngle.Equals(other._innerSpotAngle) &&
               this._outerSpotAngle.Equals(other._outerSpotAngle) &&
               this._position.Equals(other._position) &&
               this._direction.Equals(other._direction) &&
               this._color.Equals(other._color);
    }
    
    /// <summary>
    /// Determines whether an object is a <see cref="PbrGpuLight"/> that is equal to this light.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns><see langword="true"/> if the object is an equal light; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) {
        return obj is PbrGpuLight other && this.Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code based on the light's properties.
    /// </summary>
    /// <returns>A hash code suitable for use in hash-based collections.</returns>
    public override int GetHashCode() {
        return HashCode.Combine(
            this._lightType.GetHashCode(),
            this._range.GetHashCode(),
            this._innerSpotAngle.GetHashCode(),
            this._outerSpotAngle.GetHashCode(),
            this._position.GetHashCode(),
            this._direction.GetHashCode(),
            this._color.GetHashCode()
        );
    }
}