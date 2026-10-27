using System.Numerics;
using Jitter2;
using Jitter2.Collision;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.LinearMath;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Physics.Dim3.Mappables;
using Sparkle.CSharp.Terrain.Chunks;

namespace Sparkle.CSharp.Terrain.Physics;

public class HeightmapTerrainPhysicsProvider<T> : ITerrainPhysicsProvider<T> where T : class, IChunk<T> {
    
    /// <summary>
    /// Gets the world-space bounds of the terrain.
    /// </summary>
    /// <param name="terrainPhysics">The owning terrain physics component.</param>
    /// <returns>The terrain broadphase bounds.</returns>
    public JBoundingBox GetWorldBounds(TerrainPhysics3D<T> terrainPhysics) {
        int width = terrainPhysics.Terrain3D.Terrain.Width;
        int height = terrainPhysics.Terrain3D.Terrain.Height;
        int depth = terrainPhysics.Terrain3D.Terrain.Depth;
        
        // Start with an empty box, so the first corner defines both limits.
        Vector3 worldMin = new Vector3(float.MaxValue);
        Vector3 worldMax = new Vector3(float.MinValue);
        
        // Grow the box by all eight corners of the terrain.
        Expand(terrainPhysics, new Vector3(0, 0, 0), ref worldMin, ref worldMax);
        Expand(terrainPhysics, new Vector3(width, 0, 0), ref worldMin, ref worldMax);
        Expand(terrainPhysics, new Vector3(0, height, 0), ref worldMin, ref worldMax);
        Expand(terrainPhysics, new Vector3(width, height, 0), ref worldMin, ref worldMax);
        Expand(terrainPhysics, new Vector3(0, 0, depth), ref worldMin, ref worldMax);
        Expand(terrainPhysics, new Vector3(width, 0, depth), ref worldMin, ref worldMax);
        Expand(terrainPhysics, new Vector3(0, height, depth), ref worldMin, ref worldMax);
        Expand(terrainPhysics, new Vector3(width, height, depth), ref worldMin, ref worldMax);
        
        return new JBoundingBox(
            new JVector(worldMin.X, worldMin.Y, worldMin.Z),
            new JVector(worldMax.X, worldMax.Y, worldMax.Z)
        );
    }
    
    /// <summary>
    /// Generates contacts between the heightmap terrain and the provided rigid body shape.
    /// </summary>
    /// <param name="terrainPhysics">The owning terrain physics component.</param>
    /// <param name="world">The physics world that receives the contacts.</param>
    /// <param name="bodyShape">The rigid body shape that is tested against the terrain.</param>
    /// <param name="contactIdBase">The first id of the range of contact ids that is reserved for the terrain.</param>
    /// <param name="contactIdCount">The number of reserved contact ids.</param>
    public void GenerateContacts(TerrainPhysics3D<T> terrainPhysics, World world, RigidBodyShape bodyShape, ulong contactIdBase, ulong contactIdCount) {
        JBoundingBox worldBox = bodyShape.WorldBoundingBox;
        
        // Convert the corners of the shape bounds into terrain space. The terrain transform can flip an axis, so the limits are sorted again.
        Vector3 terrainMin = terrainPhysics.WorldToTerrainPosition(new Vector3(worldBox.Min.X, worldBox.Min.Y, worldBox.Min.Z));
        Vector3 terrainMax = terrainPhysics.WorldToTerrainPosition(new Vector3(worldBox.Max.X, worldBox.Max.Y, worldBox.Max.Z));
        
        Vector3 min = Vector3.Min(terrainMin, terrainMax);
        Vector3 max = Vector3.Max(terrainMin, terrainMax);
        
        int width = terrainPhysics.Terrain3D.Terrain.Width;
        int depth = terrainPhysics.Terrain3D.Terrain.Depth;
        int margin = Math.Max(0, terrainPhysics.ContactSampleMargin);
        
        // The cell range below the shape, widened by the margin and kept inside of the terrain. The last cell starts at width - 1, because it reaches up to the last height value.
        int minX = Math.Clamp((int) MathF.Floor(min.X) - margin, 0, Math.Max(0, width - 1));
        int maxX = Math.Clamp((int) MathF.Ceiling(max.X) + margin, 0, Math.Max(0, width - 1));
        int minZ = Math.Clamp((int) MathF.Floor(min.Z) - margin, 0, Math.Max(0, depth - 1));
        int maxZ = Math.Clamp((int) MathF.Ceiling(max.Z) + margin, 0, Math.Max(0, depth - 1));
        
        // A very large shape must not test an unlimited number of cells.
        ClampCellArea(terrainPhysics, width, depth, ref minX, ref maxX, ref minZ, ref maxZ);
        
        for (int z = minZ; z <= maxZ; z++) {
            for (int x = minX; x <= maxX; x++) {
                TestCell(terrainPhysics, world, bodyShape, contactIdBase, contactIdCount, x, z);
            }
        }
    }
    
    /// <summary>
    /// Expands world bounds by a terrain-local point.
    /// </summary>
    /// <param name="terrainPhysics">The owning terrain physics component, which converts the point into world space.</param>
    /// <param name="terrainPoint">The point in terrain space.</param>
    /// <param name="worldMin">The lower corner of the world bounds. It is lowered if the point lies below it.</param>
    /// <param name="worldMax">The upper corner of the world bounds. It is raised if the point lies above it.</param>
    private static void Expand(TerrainPhysics3D<T> terrainPhysics, Vector3 terrainPoint, ref Vector3 worldMin, ref Vector3 worldMax) {
        Vector3 worldPoint = terrainPhysics.TerrainToWorldPosition(terrainPoint);
        worldMin = Vector3.Min(worldMin, worldPoint);
        worldMax = Vector3.Max(worldMax, worldPoint);
    }
    
    /// <summary>
    /// Gets the terrain height at an integer terrain coordinate.
    /// </summary>
    /// <param name="terrainPhysics">The owning terrain physics component.</param>
    /// <param name="x">The X coordinate in terrain space.</param>
    /// <param name="z">The Z coordinate in terrain space.</param>
    /// <returns>The height at the coordinate, in terrain space.</returns>
    private static float GetHeight(TerrainPhysics3D<T> terrainPhysics, int x, int z) {
        x = Math.Clamp(x, 0, terrainPhysics.Terrain3D.Terrain.Width);
        z = Math.Clamp(z, 0, terrainPhysics.Terrain3D.Terrain.Depth);
        return terrainPhysics.Terrain3D.Terrain.GetRawDensityAt(x, 0, z);
    }
    
    /// <summary>
    /// Clamps a contact test area so large shapes cannot test too many terrain cells.
    /// </summary>
    /// <param name="terrainPhysics">The owning terrain physics component, which defines the maximum number of cells.</param>
    /// <param name="width">The width of the terrain in cells.</param>
    /// <param name="depth">The depth of the terrain in cells.</param>
    /// <param name="minX">The first cell on the X axis. It is changed if the area is reduced.</param>
    /// <param name="maxX">The last cell on the X axis. It is changed if the area is reduced.</param>
    /// <param name="minZ">The first cell on the Z axis. It is changed if the area is reduced.</param>
    /// <param name="maxZ">The last cell on the Z axis. It is changed if the area is reduced.</param>
    private static void ClampCellArea(TerrainPhysics3D<T> terrainPhysics, int width, int depth, ref int minX, ref int maxX, ref int minZ, ref int maxZ) {
        int countX = Math.Max(0, maxX - minX + 1);
        int countZ = Math.Max(0, maxZ - minZ + 1);
        int total = countX * countZ;
        int maxCells = Math.Max(1, terrainPhysics.MaxCellsPerShape);
        
        // The area is small enough, so nothing changes.
        if (total <= maxCells) {
            return;
        }
        
        // The area scales with the square of the side length, so the sides shrink by the square root of the ratio.
        float scale = MathF.Sqrt(maxCells / (float) total);
        int reducedX = Math.Max(1, (int) MathF.Floor(countX * scale));
        int reducedZ = Math.Max(1, (int) MathF.Floor(countZ * scale));
        
        // Keep the reduced area around the center of the original one, inside of the terrain.
        int centerX = (minX + maxX) / 2;
        int centerZ = (minZ + maxZ) / 2;
        
        minX = Math.Clamp(centerX - reducedX / 2, 0, Math.Max(0, width - 1));
        maxX = Math.Clamp(minX + reducedX, 0, Math.Max(0, width - 1));
        minZ = Math.Clamp(centerZ - reducedZ / 2, 0, Math.Max(0, depth - 1));
        maxZ = Math.Clamp(minZ + reducedZ, 0, Math.Max(0, depth - 1));
    }
    
    /// <summary>
    /// Tests one heightmap cell against a rigid body shape.
    /// </summary>
    /// <param name="terrainPhysics">The owning terrain physics component.</param>
    /// <param name="world">The physics world that receives the contacts.</param>
    /// <param name="bodyShape">The rigid body shape that is tested.</param>
    /// <param name="contactIdBase">The first id of the range of contact ids that is reserved for the terrain.</param>
    /// <param name="contactIdCount">The number of reserved contact ids.</param>
    /// <param name="x">The X index of the cell.</param>
    /// <param name="z">The Z index of the cell.</param>
    private static void TestCell(TerrainPhysics3D<T> terrainPhysics, World world, RigidBodyShape bodyShape, ulong contactIdBase, ulong contactIdCount, int x, int z) {
        
        // Heights of the four corners of the cell. h10 is the corner at (x + 1, z), and h01 the corner at (x, z + 1).
        float h00 = GetHeight(terrainPhysics, x, z);
        float h10 = GetHeight(terrainPhysics, x + 1, z);
        float h01 = GetHeight(terrainPhysics, x, z + 1);
        float h11 = GetHeight(terrainPhysics, x + 1, z + 1);
        
        // The corners in world space, because the body shape lives in world space.
        Vector3 p00 = terrainPhysics.TerrainToWorldPosition(new Vector3(x, h00, z));
        Vector3 p10 = terrainPhysics.TerrainToWorldPosition(new Vector3(x + 1, h10, z));
        Vector3 p01 = terrainPhysics.TerrainToWorldPosition(new Vector3(x, h01, z + 1));
        Vector3 p11 = terrainPhysics.TerrainToWorldPosition(new Vector3(x + 1, h11, z + 1));
        
        JVector a = new JVector(p00.X, p00.Y, p00.Z);
        JVector b = new JVector(p10.X, p10.Y, p10.Z);
        JVector c = new JVector(p01.X, p01.Y, p01.Z);
        JVector d = new JVector(p11.X, p11.Y, p11.Z);
        
        TestTriangle(terrainPhysics, world, bodyShape, contactIdBase, contactIdCount, x, z, 0, new TriangleMappable(a, c, b));
        TestTriangle(terrainPhysics, world, bodyShape, contactIdBase, contactIdCount, x, z, 1, new TriangleMappable(b, c, d));
    }
    
    /// <summary>
    /// Tests one temporary terrain triangle against a rigid body shape.
    /// </summary>
    /// <param name="terrainPhysics">The owning terrain physics component.</param>
    /// <param name="world">The physics world that receives the contact.</param>
    /// <param name="bodyShape">The rigid body shape that is tested.</param>
    /// <param name="contactIdBase">The first id of the range of contact ids that is reserved for the terrain.</param>
    /// <param name="contactIdCount">The number of reserved contact ids.</param>
    /// <param name="x">The X index of the cell that the triangle belongs to.</param>
    /// <param name="z">The Z index of the cell that the triangle belongs to.</param>
    /// <param name="triangleIndex">The index of the triangle inside of the cell (0 or 1).</param>
    /// <param name="triangle">The triangle in world space.</param>
    private static void TestTriangle(TerrainPhysics3D<T> terrainPhysics, World world, RigidBodyShape bodyShape, ulong contactIdBase, ulong contactIdCount, int x, int z, int triangleIndex, TriangleMappable triangle) {
        RigidBody body = bodyShape.RigidBody;
        
        // The triangle is already in world space, so it needs no orientation or position. The body shape is placed with the transform of its body.
        bool hit = NarrowPhase.MprEpa(triangle, bodyShape, body.Orientation, body.Position, out JVector pointA, out JVector pointB, out JVector normal, out _);
        
        if (!hit) {
            return;
        }
        
        // A contact at an inner edge is also found by the neighboring cell, where it has the correct normal, so this one is dropped.
        if (terrainPhysics.FilterInternalEdges && IsInternalEdgeContact(terrainPhysics, x, z, pointA, normal)) {
            return;
        }
        
        // The id depends on the triangle only, so the same triangle always gets the same id. It stays inside of the reserved range.
        ulong terrainContactId = contactIdBase + (HashTriangle(x, z, triangleIndex) % contactIdCount);
        
        world.RegisterContact(terrainContactId, bodyShape.ShapeId, world.NullBody, body, pointA, pointB, normal);
    }
    
    /// <summary>
    /// Determines whether a terrain contact belongs to an internal/shared edge.
    /// </summary>
    /// <param name="terrainPhysics">The owning terrain physics component, which defines the thresholds.</param>
    /// <param name="x">The X index of the cell.</param>
    /// <param name="z">The Z index of the cell.</param>
    /// <param name="worldContactPoint">The contact point on the terrain triangle, in world space.</param>
    /// <param name="normal">The contact normal in world space.</param>
    /// <returns><see langword="true"/> if the contact should be dropped; otherwise, <see langword="false"/>.</returns>
    private static bool IsInternalEdgeContact(TerrainPhysics3D<T> terrainPhysics, int x, int z, JVector worldContactPoint, JVector normal) {
        Vector3 terrainPoint = terrainPhysics.WorldToTerrainPosition(new Vector3(worldContactPoint.X, worldContactPoint.Y, worldContactPoint.Z));
        
        // The position of the contact inside of the cell, from 0 to 1 on both axes.
        float localX = terrainPoint.X - x;
        float localZ = terrainPoint.Z - z;
        
        float min = terrainPhysics.InternalEdgeDistanceThreshold;
        float max = 1.0F - terrainPhysics.InternalEdgeDistanceThreshold;
        float normalThreshold = terrainPhysics.InternalEdgeNormalThreshold;
        
        // Near the left border, with the normal pointing out of the cell, and a neighbor on that side.
        if (localX <= min && normal.X < -normalThreshold && x > 0) {
            return true;
        }
        
        // Near the right border.
        if (localX >= max && normal.X > normalThreshold && x < terrainPhysics.Terrain3D.Terrain.Width - 1) {
            return true;
        }
        
        // Near the front border.
        if (localZ <= min && normal.Z < -normalThreshold && z > 0) {
            return true;
        }
        
        // Near the back border.
        if (localZ >= max && normal.Z > normalThreshold && z < terrainPhysics.Terrain3D.Terrain.Depth - 1) {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Creates a deterministic hash for a terrain triangle.
    /// </summary>
    /// <param name="x">The X index of the cell.</param>
    /// <param name="z">The Z index of the cell.</param>
    /// <param name="triangleIndex">The index of the triangle inside of the cell (0 or 1).</param>
    /// <returns>The hash of the triangle.</returns>
    private static ulong HashTriangle(int x, int z, int triangleIndex) {
        return (ulong) ((x * 73856093) ^ (z * 83492791) ^ (triangleIndex * 19349663));
    }
}