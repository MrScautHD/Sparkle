using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class MeshHelper {
    
    /// <summary> See <see cref="Raylib.UploadMesh(ref Mesh, CBool)"/> </summary>
    public static void Upload(ref Mesh mesh, bool dynamic) => Raylib.UploadMesh(ref mesh, dynamic);
    
    /// <summary> See <see cref="Raylib.UpdateMeshBuffer"/> </summary>
    public static unsafe void UpdateBuffer(Mesh mesh, int index, void* data, int dataSize, int offset) => Raylib.UpdateMeshBuffer(mesh, index, data, dataSize, offset);
    
    /// <summary> See <see cref="Raylib.UnloadMesh(ref Mesh)"/> </summary>
    public static void Unload(ref Mesh mesh) => Raylib.UnloadMesh(ref mesh);
    
    /// <summary> See <see cref="Raylib.ExportMesh(Mesh, string)"/> </summary>
    public static bool Export(Mesh mesh, string path) => Raylib.ExportMesh(mesh, path);
    
    
    /// <summary> See <see cref="Raylib.GetMeshBoundingBox"/> </summary>
    public static BoundingBox GetBoundingBox(Mesh mesh) => Raylib.GetMeshBoundingBox(mesh);
    
    /// <summary> See <see cref="Raylib.GenMeshTangents(ref Mesh)"/> </summary>
    public static void GenTangents(ref Mesh mesh) => Raylib.GenMeshTangents(ref mesh);

    
    /// <summary> See <see cref="Raylib.DrawMesh"/> </summary>
    public static void Draw(Mesh mesh, Material material, Matrix4x4 transform) => Raylib.DrawMesh(mesh, material, transform);
    
    /// <summary> See <see cref="Raylib.DrawMeshInstanced(Mesh, Material, Matrix4x4[], int)"/> </summary>
    public static void DrawInstanced(Mesh mesh, Material material, Matrix4x4[] transforms, int instances) => Raylib.DrawMeshInstanced(mesh, material, transforms, instances);

    
    /// <summary> See <see cref="Raylib.GenMeshPoly"/> </summary>
    public static Mesh GenPoly(int sides, float radius) => Raylib.GenMeshPoly(sides, radius);
    
    /// <summary> See <see cref="Raylib.GenMeshPlane"/> </summary>
    public static Mesh GenPlane(float width, float length, int resX, int resZ) => Raylib.GenMeshPlane(width, length, resX, resZ);
    
    /// <summary> See <see cref="Raylib.GenMeshCube"/> </summary>
    public static Mesh GenCube(float width, float height, float length) => Raylib.GenMeshCube(width, height, length);
    
    /// <summary> See <see cref="Raylib.GenMeshSphere"/> </summary>
    public static Mesh GenSphere(float radius, int rings, int slices) => Raylib.GenMeshSphere(radius, rings, slices);
    
    /// <summary> See <see cref="Raylib.GenMeshHemiSphere"/> </summary>
    public static Mesh GenHemiSphere(float radius, int rings, int slices) => Raylib.GenMeshHemiSphere(radius, rings, slices);
    
    /// <summary> See <see cref="Raylib.GenMeshCylinder"/> </summary>
    public static Mesh GenCylinder(float radius, float height, int slices) => Raylib.GenMeshCylinder(radius, height, slices);
    
    /// <summary> See <see cref="Raylib.GenMeshCone"/> </summary>
    public static Mesh GenCone(float radius, float height, int slices) => Raylib.GenMeshCone(radius, height, slices);
    
    /// <summary> See <see cref="Raylib.GenMeshTorus"/> </summary>
    public static Mesh GenTorus(float radius, float size, int radSeg, int sides) => Raylib.GenMeshTorus(radius, size, radSeg, sides);
    
    /// <summary> See <see cref="Raylib.GenMeshKnot"/> </summary>
    public static Mesh GenKnot(float radius, float size, int radSeg, int sides) => Raylib.GenMeshKnot(radius, size, radSeg, sides);
    
    /// <summary> See <see cref="Raylib.GenMeshHeightmap"/> </summary>
    public static Mesh GenHeightmap(Image heightMap, Vector3 size) => Raylib.GenMeshHeightmap(heightMap, size);
    
    /// <summary> See <see cref="Raylib.GenMeshCubicmap"/> </summary>
    public static Mesh GenCubicMap(Image cubicMap, Vector3 cubeSize) => Raylib.GenMeshCubicmap(cubicMap, cubeSize);
}