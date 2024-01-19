using System.Numerics;
using Raylib_cs;

namespace Sparkle.CSharp.Rendering.Helpers; 

public static class MeshHelper {
    
    /// <inheritdoc cref="Raylib.UploadMesh(ref Mesh, CBool)"/>
    public static void Upload(ref Mesh mesh, bool dynamic) => Raylib.UploadMesh(ref mesh, dynamic);
    
    /// <inheritdoc cref="Raylib.UpdateMeshBuffer"/>
    public static unsafe void UpdateBuffer(Mesh mesh, int index, void* data, int dataSize, int offset) => Raylib.UpdateMeshBuffer(mesh, index, data, dataSize, offset);
    
    /// <inheritdoc cref="Raylib.UnloadMesh(ref Mesh)"/>
    public static void Unload(ref Mesh mesh) => Raylib.UnloadMesh(ref mesh);
    
    /// <inheritdoc cref="Raylib.ExportMesh(Mesh, string)"/>
    public static bool Export(Mesh mesh, string path) => Raylib.ExportMesh(mesh, path);
    
    
    /// <inheritdoc cref="Raylib.GetMeshBoundingBox"/>
    public static BoundingBox GetBoundingBox(Mesh mesh) => Raylib.GetMeshBoundingBox(mesh);
    
    /// <inheritdoc cref="Raylib.GenMeshTangents(ref Mesh)"/>
    public static void GenTangents(ref Mesh mesh) => Raylib.GenMeshTangents(ref mesh);

    
    /// <inheritdoc cref="Raylib.DrawMesh"/>
    public static void Draw(Mesh mesh, Raylib_cs.Material material, Matrix4x4 transform) => Raylib.DrawMesh(mesh, material, transform);
    
    /// <inheritdoc cref="Raylib.DrawMeshInstanced(Mesh, Material, Matrix4x4[], int)"/>
    public static void DrawInstanced(Mesh mesh, Raylib_cs.Material material, Matrix4x4[] transforms, int instances) => Raylib.DrawMeshInstanced(mesh, material, transforms, instances);

    
    /// <inheritdoc cref="Raylib.GenMeshPoly"/>
    public static Mesh GenPoly(int sides, float radius) => Raylib.GenMeshPoly(sides, radius);
    
    /// <inheritdoc cref="Raylib.GenMeshPlane"/>
    public static Mesh GenPlane(float width, float length, int resX, int resZ) => Raylib.GenMeshPlane(width, length, resX, resZ);
    
    /// <inheritdoc cref="Raylib.GenMeshCube"/>
    public static Mesh GenCube(float width, float height, float length) => Raylib.GenMeshCube(width, height, length);
    
    /// <inheritdoc cref="Raylib.GenMeshSphere"/>
    public static Mesh GenSphere(float radius, int rings, int slices) => Raylib.GenMeshSphere(radius, rings, slices);
    
    /// <inheritdoc cref="Raylib.GenMeshHemiSphere"/>
    public static Mesh GenHemiSphere(float radius, int rings, int slices) => Raylib.GenMeshHemiSphere(radius, rings, slices);
    
    /// <inheritdoc cref="Raylib.GenMeshCylinder"/>
    public static Mesh GenCylinder(float radius, float height, int slices) => Raylib.GenMeshCylinder(radius, height, slices);
    
    /// <inheritdoc cref="Raylib.GenMeshCone"/>
    public static Mesh GenCone(float radius, float height, int slices) => Raylib.GenMeshCone(radius, height, slices);
    
    /// <inheritdoc cref="Raylib.GenMeshTorus"/>
    public static Mesh GenTorus(float radius, float size, int radSeg, int sides) => Raylib.GenMeshTorus(radius, size, radSeg, sides);
    
    /// <inheritdoc cref="Raylib.GenMeshKnot"/>
    public static Mesh GenKnot(float radius, float size, int radSeg, int sides) => Raylib.GenMeshKnot(radius, size, radSeg, sides);
    
    /// <inheritdoc cref="Raylib.GenMeshHeightmap"/>
    public static Mesh GenHeightmap(Image heightMap, Vector3 size) => Raylib.GenMeshHeightmap(heightMap, size);
    
    /// <inheritdoc cref="Raylib.GenMeshCubicmap"/>
    public static Mesh GenCubicMap(Image cubicMap, Vector3 cubeSize) => Raylib.GenMeshCubicmap(cubicMap, cubeSize);
}