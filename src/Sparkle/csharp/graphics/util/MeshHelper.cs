using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class MeshHelper {
    
    public static void UploadMesh(ref Mesh mesh, bool dynamic) => Raylib.UploadMesh(ref mesh, dynamic);
    public static unsafe void UpdateMeshBuffer(Mesh mesh, int index, void* data, int dataSize, int offset) => Raylib.UpdateMeshBuffer(mesh, index, data, dataSize, offset);
    public static void UnloadMesh(ref Mesh mesh) => Raylib.UnloadMesh(ref mesh);
    public static bool ExportMesh(Mesh mesh, string path) => Raylib.ExportMesh(mesh, path);
    
    public static BoundingBox GetMeshBoundingBox(Mesh mesh) => Raylib.GetMeshBoundingBox(mesh);
    public static void GenMeshTangents(ref Mesh mesh) => Raylib.GenMeshTangents(ref mesh);

    public static void DrawMesh(Mesh mesh, Material material, Matrix4x4 transform) => Raylib.DrawMesh(mesh, material, transform);
    public static void DrawMeshInstanced(Mesh mesh, Material material, Matrix4x4[] transforms, int instances) => Raylib.DrawMeshInstanced(mesh, material, transforms, instances);

    public static Mesh GenMeshPoly(int sides, float radius) => Raylib.GenMeshPoly(sides, radius);
    public static Mesh GenMeshPlane(float width, float length, int resX, int resZ) => Raylib.GenMeshPlane(width, length, resX, resZ);
    public static Mesh GenMeshCube(float width, float height, float length) => Raylib.GenMeshCube(width, height, length);
    public static Mesh GenMeshSphere(float radius, int rings, int slices) => Raylib.GenMeshSphere(radius, rings, slices);
    public static Mesh GenMeshHemiSphere(float radius, int rings, int slices) => Raylib.GenMeshHemiSphere(radius, rings, slices);
    public static Mesh GenMeshCylinder(float radius, float height, int slices) => Raylib.GenMeshCylinder(radius, height, slices);
    public static Mesh GenMeshCone(float radius, float height, int slices) => Raylib.GenMeshCone(radius, height, slices);
    public static Mesh GenMeshTorus(float radius, float size, int radSeg, int sides) => Raylib.GenMeshTorus(radius, size, radSeg, sides);
    public static Mesh GenMeshKnot(float radius, float size, int radSeg, int sides) => Raylib.GenMeshKnot(radius, size, radSeg, sides);
    public static Mesh GenMeshHeightmap(Image heightMap, Vector3 size) => Raylib.GenMeshHeightmap(heightMap, size);
    public static Mesh GenMeshCubicMap(Image cubicMap, Vector3 cubeSize) => Raylib.GenMeshCubicmap(cubicMap, cubeSize);
}