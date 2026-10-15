using System.Numerics;
using System.Runtime.InteropServices;

namespace Sparkle.CSharp.Graphics.Rendering.Forward.Lighting.Pbr;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PbrLightCluster {
    
    /// <summary>
    /// Gets or sets the starting offset of the cluster's light indices in the light index buffer.
    /// </summary>
    public uint LightOffset;
    
    /// <summary>
    /// Gets or sets the number of lights assigned to this cluster.
    /// </summary>
    public uint LightCount;
    
    /// <summary>
    /// Padding for memory alignment to ensure the correct GPU struct layout.
    /// </summary>
    private Vector2 _padding;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PbrLightCluster"/> structure.
    /// </summary>
    /// <param name="lightOffset">The starting offset of the cluster's light indices in the light index buffer.</param>
    /// <param name="lightCount">The number of lights assigned to this cluster.</param>
    public PbrLightCluster(uint lightOffset, uint lightCount) {
        this.LightOffset = lightOffset;
        this.LightCount = lightCount;
    }
}