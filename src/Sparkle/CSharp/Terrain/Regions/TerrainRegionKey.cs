namespace Sparkle.CSharp.Terrain.Regions;

public readonly struct TerrainRegionKey : IEquatable<TerrainRegionKey> {
    
    /// <summary>
    /// The X grid coordinate of the terrain region.
    /// </summary>
    public readonly int X;
    
    /// <summary>
    /// The Z grid coordinate of the terrain region.
    /// </summary>
    public readonly int Z;
    
    /// <summary>
    /// The level of detail of the terrain region, where lower values represent higher detail.
    /// </summary>
    public readonly int Lod;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainRegionKey"/> struct.
    /// </summary>
    /// <param name="x">The X grid coordinate of the terrain region.</param>
    /// <param name="z">The Z grid coordinate of the terrain region.</param>
    /// <param name="lod">The level of detail of the terrain region.</param>
    public TerrainRegionKey(int x, int z, int lod) {
        this.X = x;
        this.Z = z;
        this.Lod = lod;
    }
    
    /// <summary>
    /// Returns true if two <see cref="TerrainRegionKey"/> instances are equal.
    /// </summary>
    public static bool operator ==(TerrainRegionKey left, TerrainRegionKey right) => left.Equals(right);
    
    /// <summary>
    /// Returns true if two <see cref="TerrainRegionKey"/> instances are not equal.
    /// </summary>
    public static bool operator !=(TerrainRegionKey left, TerrainRegionKey right) => !left.Equals(right);
    
    /// <summary>
    /// Determines whether this key is equal to another <see cref="TerrainRegionKey"/> by comparing the X, Z, and LOD values.
    /// </summary>
    /// <param name="other">The other key to compare against.</param>
    /// <returns>True if both keys have the same X, Z, and LOD values; otherwise false.</returns>
    public bool Equals(TerrainRegionKey other) {
        return this.X.Equals(other.X) && this.Z.Equals(other.Z) && this.Lod.Equals(other.Lod);
    }
    
    /// <summary>
    /// Determines whether this key is equal to a given object.
    /// </summary>
    /// <param name="obj">The object to compare against.</param>
    /// <returns>True if the object is a <see cref="TerrainRegionKey"/> with the same values; otherwise false.</returns>
    public override bool Equals(object? obj) {
        return obj is TerrainRegionKey other && this.Equals(other);
    }
    
    /// <summary>
    /// Returns a hash code for this key based on its X, Z, and LOD values.
    /// </summary>
    /// <returns>A hash code combining the X, Z, and LOD fields.</returns>
    public override int GetHashCode() {
        return HashCode.Combine(this.X.GetHashCode(), this.Z.GetHashCode(), this.Lod.GetHashCode());
    }
}