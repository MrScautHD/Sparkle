namespace Sparkle.CSharp.Terrain.Regions;

public readonly struct TerrainRegionKey : IEquatable<TerrainRegionKey> {
    
    public readonly int X;
    
    public readonly int Z;
    
    public readonly int Lod;
    
    public TerrainRegionKey(int x, int z, int lod) {
        this.X = x;
        this.Z = z;
        this.Lod = lod;
    }
    
    public static bool operator ==(TerrainRegionKey left, TerrainRegionKey right) => left.Equals(right);
    
    public static bool operator !=(TerrainRegionKey left, TerrainRegionKey right) => !left.Equals(right);
    
    public bool Equals(TerrainRegionKey other) {
        return this.X.Equals(other.X) && this.Z.Equals(other.Z) && this.Lod.Equals(other.Lod);
    }
    
    public override bool Equals(object? obj) {
        return obj is TerrainRegionKey other && this.Equals(other);
    }
    
    public override int GetHashCode() {
        return HashCode.Combine(this.X.GetHashCode(), this.Z.GetHashCode(), this.Lod.GetHashCode());
    }
}