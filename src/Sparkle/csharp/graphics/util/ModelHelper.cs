using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class ModelHelper {
    
    
    /// <summary> See <see cref="Raylib.LoadModel"/> </summary>
    public static Model Load(string path) => Raylib.LoadModel(path);
    
    /// <summary> See <see cref="Raylib.LoadModelFromMesh"/> </summary>
    public static Model LoadFromMesh(Mesh mesh) => Raylib.LoadModelFromMesh(mesh);
    
    /// <summary> See <see cref="Raylib.UnloadModel"/> </summary>
    public static void Unload(Model model) => Raylib.UnloadModel(model);
    
    
    /// <summary> See <see cref="Raylib.IsModelReady"/> </summary>
    public static bool IsReady(Model model) => Raylib.IsModelReady(model);
    
    /// <summary> See <see cref="Raylib.GetModelBoundingBox"/> </summary>
    public static BoundingBox GetBoundingBox(Model model) => Raylib.GetModelBoundingBox(model);
    
    /// <summary> See <see cref="Raylib.SetModelMeshMaterial(ref Model, int, int)"/> </summary>
    public static void SetMeshMaterial(ref Model model, int meshId, int materialId) => Raylib.SetModelMeshMaterial(ref model, meshId, materialId);
    
    
    /// <summary> See <see cref="Raylib.DrawModel"/> </summary>
    public static void DrawModel(Model model, Vector3 pos, float scale, Color color) => Raylib.DrawModel(model, pos, scale, color);
    
    /// <summary> See <see cref="Raylib.DrawModel"/> </summary>
    public static void DrawModel(Model model, Vector3 pos, Vector3 rotationAxis, float rotationAngle, Vector3 scale, Color color) => Raylib.DrawModelEx(model, pos, rotationAxis, rotationAngle, scale, color);
    
    /// <summary> See <see cref="Raylib.DrawModelWires"/> </summary>
    public static void DrawModelWires(Model model, Vector3 pos, float scale, Color color) => Raylib.DrawModelWires(model, pos, scale, color);
    
    /// <summary> See <see cref="Raylib.DrawModelWires"/> </summary>
    public static void DrawModelWires(Model model, Vector3 pos, Vector3 rotationAxis, float rotationAngle, Vector3 scale, Color color) => Raylib.DrawModelWiresEx(model, pos, rotationAxis, rotationAngle, scale, color);
    
    /// <summary> See <see cref="Raylib.DrawBoundingBox"/> </summary>
    public static void DrawBoundingBox(BoundingBox box, Color color) => Raylib.DrawBoundingBox(box, color);

    
    /// <summary> See <see cref="Raylib.DrawLine3D"/> </summary>
    public static void DrawLine3D(Vector3 startPos, Vector3 endPos, Color color) => Raylib.DrawLine3D(startPos, endPos, color);
    
    /// <summary> See <see cref="Raylib.DrawPoint3D"/> </summary>
    public static void DrawPoint3D(Vector3 pos, Color color) => Raylib.DrawPoint3D(pos, color);
    
    /// <summary> See <see cref="Raylib.DrawCircle3D"/> </summary>
    public static void DrawCircle3D(Vector3 center, float radius, Vector3 rotationAxis, float rotationAngle, Color color) => Raylib.DrawCircle3D(center, radius, rotationAxis, rotationAngle, color);
    
    /// <summary> See <see cref="Raylib.DrawTriangle3D"/> </summary>
    public static void DrawTriangle3D(Vector3 v1, Vector3 v2, Vector3 v3, Color color) => Raylib.DrawTriangle3D(v1, v2, v3, color);
    
    /// <summary> See <see cref="Raylib.DrawTriangleStrip3D(Vector3[], int, Color)"/> </summary>
    public static void DrawTriangleStrip3D(Vector3[] points, int pointCount, Color color) => Raylib.DrawTriangleStrip3D(points, pointCount, color);
    
    /// <summary> See <see cref="Raylib.DrawCube"/> </summary>
    public static void DrawCube(Vector3 pos, float width, float height, float length, Color color) => Raylib.DrawCube(pos, width, height, length, color);
    
    /// <summary> See <see cref="Raylib.DrawCubeV"/> </summary>
    public static void DrawCube(Vector3 pos, Vector3 size, Color color) => Raylib.DrawCubeV(pos, size, color);
    
    /// <summary> See <see cref="Raylib.DrawCubeWires"/> </summary>
    public static void DrawCubeWires(Vector3 pos, float width, float height, float length, Color color) => Raylib.DrawCubeWires(pos, width, height, length, color);
    
    /// <summary> See <see cref="Raylib.DrawCubeWiresV"/> </summary>
    public static void DrawCubeWires(Vector3 pos, Vector3 size, Color color) => Raylib.DrawCubeWiresV(pos, size, color);
    
    /// <summary> See <see cref="Raylib.DrawSphere"/> </summary>
    public static void DrawSphere(Vector3 centerPos, float radius, Color color) => Raylib.DrawSphere(centerPos, radius, color);
    
    /// <summary> See <see cref="Raylib.DrawSphereEx"/> </summary>
    public static void DrawSphere(Vector3 centerPos, float radius, int rings, int slices, Color color) => Raylib.DrawSphereEx(centerPos, radius, rings, slices, color);
    
    /// <summary> See <see cref="Raylib.DrawSphereWires"/> </summary>
    public static void DrawSphereWires(Vector3 centerPos, float radius, int rings, int slices, Color color) => Raylib.DrawSphereWires(centerPos, radius, rings, slices, color);
    
    /// <summary> See <see cref="Raylib.DrawCylinder"/> </summary>
    public static void DrawCylinder(Vector3 pos, float radiusTop, float radiusBottom, float height, int slices, Color color) => Raylib.DrawCylinder(pos, radiusTop, radiusBottom, height, slices, color);
    
    /// <summary> See <see cref="Raylib.DrawCylinderEx"/> </summary>
    public static void DrawCylinder(Vector3 startPos, Vector3 endPos, float startRadius, float endRadius, int sides, Color color) => Raylib.DrawCylinderEx(startPos, endPos, startRadius, endRadius, sides, color);
    
    /// <summary> See <see cref="Raylib.DrawCylinderWires"/> </summary>
    public static void DrawCylinderWires(Vector3 pos, float radiusTop, float radiusBottom, float height, int slices, Color color) => Raylib.DrawCylinderWires(pos, radiusTop, radiusBottom, height, slices, color);
    
    /// <summary> See <see cref="Raylib.DrawCylinderWiresEx"/> </summary>
    public static void DrawCylinderWires(Vector3 startPos, Vector3 endPos, float startRadius, float endRadius, int sides, Color color) => Raylib.DrawCylinderWiresEx(startPos, endPos, startRadius, endRadius, sides, color);
    
    /// <summary> See <see cref="Raylib.DrawCapsule"/> </summary>
    public static void DrawCapsule(Vector3 startPos, Vector3 endPos, float radius, int slices, int rings, Color color) => Raylib.DrawCapsule(startPos, endPos, radius, slices, rings, color);
    
    /// <summary> See <see cref="Raylib.DrawCapsuleWires"/> </summary>
    public static void DrawCapsuleWires(Vector3 startPos, Vector3 endPos, float radius, int slices, int rings, Color color) => Raylib.DrawCapsuleWires(startPos, endPos, radius, slices, rings, color);
    
    /// <summary> See <see cref="Raylib.DrawPlane"/> </summary>
    public static void DrawPlane(Vector3 centerPos, Vector2 size, Color color) => Raylib.DrawPlane(centerPos, size, color);
    
    /// <summary> See <see cref="Raylib.DrawRay"/> </summary>
    public static void DrawRay(Ray ray, Color color) => Raylib.DrawRay(ray, color);
    
    /// <summary> See <see cref="Raylib.DrawGrid"/> </summary>
    public static void DrawGrid(int slices, float spacing) => Raylib.DrawGrid(slices, spacing);

    
    /// <summary> See <see cref="Raylib.LoadModelAnimations(string, ref uint)"/> </summary>
    public static ReadOnlySpan<ModelAnimation> LoadAnimations(string path, ref uint animCount) => Raylib.LoadModelAnimations(path, ref animCount);
    
    /// <summary> See <see cref="Raylib.UpdateModelAnimation"/> </summary>
    public static void UpdateAnimation(Model model, ModelAnimation anim, int frame) => Raylib.UpdateModelAnimation(model, anim, frame);
    
    /// <summary> See <see cref="Raylib.UnloadModelAnimation"/> </summary>
    public static void UnloadAnimation(ModelAnimation anim) => Raylib.UnloadModelAnimation(anim);
    
    /// <summary> See <see cref="Raylib.UnloadModelAnimations"/> </summary>
    public static unsafe void UnloadAnimations(ModelAnimation* animations, uint count) => Raylib.UnloadModelAnimations(animations, count);
    
    /// <summary> See <see cref="Raylib.IsModelAnimationValid"/> </summary>
    public static bool IsAnimationValid(Model model, ModelAnimation anim) => Raylib.IsModelAnimationValid(model, anim);
    
    
    /// <summary> See <see cref="Raylib.CheckCollisionSpheres"/> </summary>
    public static bool CheckCollisionSpheres(Vector3 center1, float radius1, Vector3 center2, float radius2) => Raylib.CheckCollisionSpheres(center1, radius1, center2, radius2);
    
    /// <summary> See <see cref="Raylib.CheckCollisionBoxes"/> </summary>
    public static bool CheckCollisionBoxes(BoundingBox box1, BoundingBox box2) => Raylib.CheckCollisionBoxes(box1, box2);
    
    /// <summary> See <see cref="Raylib.CheckCollisionBoxSphere"/> </summary>
    public static bool CheckCollisionBoxSphere(BoundingBox box, Vector3 center, float radius) => Raylib.CheckCollisionBoxSphere(box, center, radius);
    
    /// <summary> See <see cref="Raylib.GetRayCollisionSphere"/> </summary>
    public static RayCollision GetRayCollisionSphere(Ray ray, Vector3 center, float radius) => Raylib.GetRayCollisionSphere(ray, center, radius);
    
    /// <summary> See <see cref="Raylib.GetRayCollisionBox"/> </summary>
    public static RayCollision GetRayCollisionBox(Ray ray, BoundingBox box) => Raylib.GetRayCollisionBox(ray, box);
    
    /// <summary> See <see cref="Raylib.GetRayCollisionMesh"/> </summary>
    public static RayCollision GetRayCollisionMesh(Ray ray, Mesh mesh, Matrix4x4 transform) => Raylib.GetRayCollisionMesh(ray, mesh, transform);
    
    /// <summary> See <see cref="Raylib.GetRayCollisionTriangle"/> </summary>
    public static RayCollision GetRayCollisionTriangle(Ray ray, Vector3 p1, Vector3 p2, Vector3 p3) => Raylib.GetRayCollisionTriangle(ray, p1, p2, p3);
    
    /// <summary> See <see cref="Raylib.GetRayCollisionQuad"/> </summary>
    public static RayCollision GetRayCollisionQuad(Ray ray, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) => Raylib.GetRayCollisionQuad(ray, p1, p2, p3, p4);
}