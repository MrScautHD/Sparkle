using System.Numerics;
using System.Runtime.InteropServices;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Lighting.Pbr;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PbrLightData : IEquatable<PbrLightData> {
    
    /// <summary>
    /// Gets or sets the current number of lights in the container.
    /// </summary>
    public int NumOfLights;
    
    /// <summary>
    /// Gets or sets the current number of directional lights in the container.
    /// </summary>
    public int NumOfDirectionalLights;
    
    /// <summary>
    /// Padding for memory alignment to ensure the correct GPU struct layout.
    /// </summary>
    private Vector2 _padding;
    
    /// <summary>
    /// Gets or sets the global ambient color applied to the scene, where W stores intensity.
    /// </summary>
    public Vector4 AmbientColor;
    
    /// <summary>
    /// Determines whether two headers are equal.
    /// </summary>
    /// <param name="left">The first header to compare.</param>
    /// <param name="right">The second header to compare.</param>
    /// <returns><see langword="true"/> if both headers have the same light counts and ambient color; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(PbrLightData left, PbrLightData right) => left.Equals(right);
    
    /// <summary>
    /// Determines whether two headers are different.
    /// </summary>
    /// <param name="left">The first header to compare.</param>
    /// <param name="right">The second header to compare.</param>
    /// <returns><see langword="true"/> if the headers differ in their light counts or ambient color; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(PbrLightData left, PbrLightData right) => !left.Equals(right);
    
    /// <summary>
    /// Determines whether this header is equal to another header.
    /// </summary>
    /// <param name="other">The header to compare with.</param>
    /// <returns><see langword="true"/> if both headers have the same light counts and ambient color; otherwise, <see langword="false"/>.</returns>
    public bool Equals(PbrLightData other) {
        return this.NumOfLights == other.NumOfLights &&
               this.NumOfDirectionalLights == other.NumOfDirectionalLights &&
               this.AmbientColor.Equals(other.AmbientColor);
    }
    
    /// <summary>
    /// Determines whether an object is a <see cref="PbrLightData"/> that is equal to this header.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns><see langword="true"/> if the object is an equal header; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) {
        return obj is PbrLightData other && this.Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code that is based on the light counts and the ambient color.
    /// </summary>
    /// <returns>A hash code suitable for use in hash-based collections.</returns>
    public override int GetHashCode() {
        return HashCode.Combine(this.NumOfLights.GetHashCode(), this.NumOfDirectionalLights.GetHashCode(), this.AmbientColor.GetHashCode());
    }
}