using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp.Graphics.Pipelines;
using Bliss.CSharp.Graphics.VertexTypes;
using Veldrid;

namespace Sparkle.CSharp.Graphics.VertexTypes;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PhysicsDebugVertex3D : IVertexType {

    /// <summary>
    /// The layout description used by the graphics pipeline to interpret this vertex's data.
    /// </summary>
    public static VertexFormat VertexLayout = new VertexFormat("PhysicsDebugVertex3D", new VertexLayoutDescription(
        new VertexElementDescription("vPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
        new VertexElementDescription("vColor", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
    ));
    
    /// <summary>
    /// Defines the vertex layout configuration used to describe the structure and attributes of a vertex.
    /// </summary>
    VertexFormat IVertexType.VertexLayout => VertexLayout;
    
    /// <summary>
    /// The position of the vertex in 3D space.
    /// </summary>
    public Vector3 Position;
    
    /// <summary>
    /// The color of the vertex, including alpha transparency.
    /// </summary>
    public Vector4 Color;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicsDebugVertex3D"/> struct with the specified position and color.
    /// </summary>
    /// <param name="position">The position of the vertex in 3D space.</param>
    /// <param name="color">The color of the vertex, including alpha transparency.</param>
    public PhysicsDebugVertex3D(Vector3 position, Vector4 color) {
        this.Position = position;
        this.Color = color;
    }
}