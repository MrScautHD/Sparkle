using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.entity;

namespace Sparkle.csharp.graphics.util; 

public static class ModelHelper {
    
    
    /// <inheritdoc cref="Raylib.LoadModel"/>
    public static Model Load(string path) => Raylib.LoadModel(path);
    
    /// <inheritdoc cref="Raylib.LoadModelFromMesh"/>
    public static Model LoadFromMesh(Mesh mesh) => Raylib.LoadModelFromMesh(mesh);
    
    /// <inheritdoc cref="Raylib.UnloadModel"/>
    public static void Unload(Model model) => Raylib.UnloadModel(model);
    
    
    /// <inheritdoc cref="Raylib.IsModelReady"/>
    public static bool IsReady(Model model) => Raylib.IsModelReady(model);

    /// <inheritdoc cref="Raylib.GetMaterial"/>
    public static Material GetMaterial(ref Model model, int materialIndex) => Raylib.GetMaterial(ref model, materialIndex);
    
    /// <inheritdoc cref="Raylib.GetModelBoundingBox"/>
    public static BoundingBox GetBoundingBox(Model model) => Raylib.GetModelBoundingBox(model);
    
    /// <inheritdoc cref="Raylib.SetModelMeshMaterial(ref Model, int, int)"/>
    public static void SetMeshMaterial(ref Model model, int meshId, int materialId) => Raylib.SetModelMeshMaterial(ref model, meshId, materialId);
    
    
    /// <inheritdoc cref="Raylib.DrawModel"/>
    public static void DrawModel(Model model, Vector3 pos, float scale, Color color) => Raylib.DrawModel(model, pos, scale, color);
    
    /// <inheritdoc cref="Raylib.DrawModel"/>
    public static void DrawModel(Model model, Vector3 pos, Vector3 rotationAxis, float rotationAngle, Vector3 scale, Color color) => Raylib.DrawModelEx(model, pos, rotationAxis, rotationAngle, scale, color);
    
    /// <inheritdoc cref="Raylib.DrawModelWires"/>
    public static void DrawModelWires(Model model, Vector3 pos, float scale, Color color) => Raylib.DrawModelWires(model, pos, scale, color);
    
    /// <inheritdoc cref="Raylib.DrawModelWires"/>
    public static void DrawModelWires(Model model, Vector3 pos, Vector3 rotationAxis, float rotationAngle, Vector3 scale, Color color) => Raylib.DrawModelWiresEx(model, pos, rotationAxis, rotationAngle, scale, color);
    
    /// <inheritdoc cref="Raylib.DrawBoundingBox"/>
    public static void DrawBoundingBox(BoundingBox box, Color color) => Raylib.DrawBoundingBox(box, color);
    
    /// <inheritdoc cref="Raylib.DrawBillboard"/>
    public static void DrawBillboard(Cam3D camera, Texture2D texture, Vector3 center, float size, Color color) => Raylib.DrawBillboard(camera.GetCamera3D(), texture, center, size, color);
    
    /// <inheritdoc cref="Raylib.DrawBillboardRec"/>
    public static void DrawBillboardRec(Cam3D camera, Texture2D texture, Rectangle source, Vector3 position, Vector2 size, Color color) => Raylib.DrawBillboardRec(camera.GetCamera3D(), texture, source, position, size, color);
    
    /// <inheritdoc cref="Raylib.DrawBillboardPro"/>
    public static void DrawBillboardPro(Cam3D camera, Texture2D texture, Rectangle source, Vector3 position, Vector3 up, Vector2 size, Vector2 origin, float rotation, Color color) => Raylib.DrawBillboardPro(camera.GetCamera3D(), texture, source, position, up, size, origin, rotation, color);
    
    
    /// <inheritdoc cref="Raylib.DrawLine3D"/>
    public static void DrawLine3D(Vector3 startPos, Vector3 endPos, Color color) => Raylib.DrawLine3D(startPos, endPos, color);
    
    /// <inheritdoc cref="Raylib.DrawPoint3D"/>
    public static void DrawPoint3D(Vector3 pos, Color color) => Raylib.DrawPoint3D(pos, color);
    
    /// <inheritdoc cref="Raylib.DrawCircle3D"/>
    public static void DrawCircle3D(Vector3 center, float radius, Vector3 rotationAxis, float rotationAngle, Color color) => Raylib.DrawCircle3D(center, radius, rotationAxis, rotationAngle, color);
    
    /// <inheritdoc cref="Raylib.DrawTriangle3D"/>
    public static void DrawTriangle3D(Vector3 v1, Vector3 v2, Vector3 v3, Color color) => Raylib.DrawTriangle3D(v1, v2, v3, color);
    
    /// <inheritdoc cref="Raylib.DrawTriangleStrip3D(Vector3[], int, Color)"/>
    public static void DrawTriangleStrip3D(Vector3[] points, int pointCount, Color color) => Raylib.DrawTriangleStrip3D(points, pointCount, color);
    
    /// <inheritdoc cref="Raylib.DrawCube"/>
    public static void DrawCube(Vector3 pos, float width, float height, float length, Color color) => Raylib.DrawCube(pos, width, height, length, color);
    
    /// <inheritdoc cref="Raylib.DrawCubeV"/>
    public static void DrawCube(Vector3 pos, Vector3 size, Color color) => Raylib.DrawCubeV(pos, size, color);
    
    /// <inheritdoc cref="Raylib.DrawCubeWires"/>
    public static void DrawCubeWires(Vector3 pos, float width, float height, float length, Color color) => Raylib.DrawCubeWires(pos, width, height, length, color);
    
    /// <inheritdoc cref="Raylib.DrawCubeWiresV"/>
    public static void DrawCubeWires(Vector3 pos, Vector3 size, Color color) => Raylib.DrawCubeWiresV(pos, size, color);
    
    /// <inheritdoc cref="Raylib.DrawSphere"/>
    public static void DrawSphere(Vector3 centerPos, float radius, Color color) => Raylib.DrawSphere(centerPos, radius, color);
    
    /// <inheritdoc cref="Raylib.DrawSphereEx"/>
    public static void DrawSphere(Vector3 centerPos, float radius, int rings, int slices, Color color) => Raylib.DrawSphereEx(centerPos, radius, rings, slices, color);
    
    /// <inheritdoc cref="Raylib.DrawSphereWires"/>
    public static void DrawSphereWires(Vector3 centerPos, float radius, int rings, int slices, Color color) => Raylib.DrawSphereWires(centerPos, radius, rings, slices, color);
    
    /// <inheritdoc cref="Raylib.DrawCylinder"/>
    public static void DrawCylinder(Vector3 pos, float radiusTop, float radiusBottom, float height, int slices, Color color) => Raylib.DrawCylinder(pos, radiusTop, radiusBottom, height, slices, color);
    
    /// <inheritdoc cref="Raylib.DrawCylinderEx"/>
    public static void DrawCylinder(Vector3 startPos, Vector3 endPos, float startRadius, float endRadius, int sides, Color color) => Raylib.DrawCylinderEx(startPos, endPos, startRadius, endRadius, sides, color);
    
    /// <inheritdoc cref="Raylib.DrawCylinderWires"/>
    public static void DrawCylinderWires(Vector3 pos, float radiusTop, float radiusBottom, float height, int slices, Color color) => Raylib.DrawCylinderWires(pos, radiusTop, radiusBottom, height, slices, color);
    
    /// <inheritdoc cref="Raylib.DrawCylinderWiresEx"/>
    public static void DrawCylinderWires(Vector3 startPos, Vector3 endPos, float startRadius, float endRadius, int sides, Color color) => Raylib.DrawCylinderWiresEx(startPos, endPos, startRadius, endRadius, sides, color);
    
    /// <inheritdoc cref="Raylib.DrawCapsule"/>
    public static void DrawCapsule(Vector3 startPos, Vector3 endPos, float radius, int slices, int rings, Color color) => Raylib.DrawCapsule(startPos, endPos, radius, slices, rings, color);
    
    /// <inheritdoc cref="Raylib.DrawCapsuleWires"/>
    public static void DrawCapsuleWires(Vector3 startPos, Vector3 endPos, float radius, int slices, int rings, Color color) => Raylib.DrawCapsuleWires(startPos, endPos, radius, slices, rings, color);
    
    /// <inheritdoc cref="Raylib.DrawPlane"/>
    public static void DrawPlane(Vector3 centerPos, Vector2 size, Color color) => Raylib.DrawPlane(centerPos, size, color);
    
    /// <inheritdoc cref="Raylib.DrawRay"/>
    public static void DrawRay(Ray ray, Color color) => Raylib.DrawRay(ray, color);
    
    /// <inheritdoc cref="Raylib.DrawGrid"/>
    public static void DrawGrid(int slices, float spacing) => Raylib.DrawGrid(slices, spacing);

    
    /// <inheritdoc cref="Raylib.LoadModelAnimations(string, ref uint)"/>
    public static ReadOnlySpan<ModelAnimation> LoadAnimations(string path, ref uint animCount) => Raylib.LoadModelAnimations(path, ref animCount);
    
    /// <inheritdoc cref="Raylib.UpdateModelAnimation"/>
    public static void UpdateAnimation(Model model, ModelAnimation anim, int frame) => Raylib.UpdateModelAnimation(model, anim, frame);
    
    /// <inheritdoc cref="Raylib.UnloadModelAnimation"/>
    public static void UnloadAnimation(ModelAnimation anim) => Raylib.UnloadModelAnimation(anim);
    
    /// <inheritdoc cref="Raylib.UnloadModelAnimations"/>
    public static unsafe void UnloadAnimations(ModelAnimation* animations, uint count) => Raylib.UnloadModelAnimations(animations, count);
    
    /// <inheritdoc cref="Raylib.IsModelAnimationValid"/>
    public static bool IsAnimationValid(Model model, ModelAnimation anim) => Raylib.IsModelAnimationValid(model, anim);
    
    
    /// <inheritdoc cref="Raylib.CheckCollisionSpheres"/>
    public static bool CheckCollisionSpheres(Vector3 center1, float radius1, Vector3 center2, float radius2) => Raylib.CheckCollisionSpheres(center1, radius1, center2, radius2);
    
    /// <inheritdoc cref="Raylib.CheckCollisionBoxes"/>
    public static bool CheckCollisionBoxes(BoundingBox box1, BoundingBox box2) => Raylib.CheckCollisionBoxes(box1, box2);
    
    /// <inheritdoc cref="Raylib.CheckCollisionBoxSphere"/>
    public static bool CheckCollisionBoxSphere(BoundingBox box, Vector3 center, float radius) => Raylib.CheckCollisionBoxSphere(box, center, radius);
    
    /// <inheritdoc cref="Raylib.GetRayCollisionSphere"/>
    public static RayCollision GetRayCollisionSphere(Ray ray, Vector3 center, float radius) => Raylib.GetRayCollisionSphere(ray, center, radius);
    
    /// <inheritdoc cref="Raylib.GetRayCollisionBox"/>
    public static RayCollision GetRayCollisionBox(Ray ray, BoundingBox box) => Raylib.GetRayCollisionBox(ray, box);
    
    /// <inheritdoc cref="Raylib.GetRayCollisionMesh"/>
    public static RayCollision GetRayCollisionMesh(Ray ray, Mesh mesh, Matrix4x4 transform) => Raylib.GetRayCollisionMesh(ray, mesh, transform);
    
    /// <inheritdoc cref="Raylib.GetRayCollisionTriangle"/>
    public static RayCollision GetRayCollisionTriangle(Ray ray, Vector3 p1, Vector3 p2, Vector3 p3) => Raylib.GetRayCollisionTriangle(ray, p1, p2, p3);
    
    /// <inheritdoc cref="Raylib.GetRayCollisionQuad"/>
    public static RayCollision GetRayCollisionQuad(Ray ray, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) => Raylib.GetRayCollisionQuad(ray, p1, p2, p3, p4);
}