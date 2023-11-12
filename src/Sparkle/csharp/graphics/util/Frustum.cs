using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util;

public class Frustum {
    
    private Plane[] _planes;
    
    /// <summary>
    /// Initializes a new instance of the Frustum class.
    /// </summary>
    public Frustum() {
        this._planes = new Plane[6];
    }

    /// <summary>
    /// Extracts frustum planes from the view-projection matrix.
    /// </summary>
    public void Extract() {
        Matrix4x4 viewProjection = Matrix4x4.Transpose(Rlgl.GetMatrixModelview()) * Matrix4x4.Transpose(Rlgl.GetMatrixProjection());
        
        // LEFT
        this._planes[0] = new Plane(
            viewProjection.M14 + viewProjection.M11,
            viewProjection.M24 + viewProjection.M21,
            viewProjection.M34 + viewProjection.M31,
            viewProjection.M44 + viewProjection.M41
        );

        // RIGHT
        this._planes[1] = new Plane(
            viewProjection.M14 - viewProjection.M11,
            viewProjection.M24 - viewProjection.M21,
            viewProjection.M34 - viewProjection.M31,
            viewProjection.M44 - viewProjection.M41
        );

        // BOTTOM
        this._planes[2] = new Plane(
            viewProjection.M14 + viewProjection.M12,
            viewProjection.M24 + viewProjection.M22,
            viewProjection.M34 + viewProjection.M32,
            viewProjection.M44 + viewProjection.M42
        );

        // TOP
        this._planes[3] = new Plane(
            viewProjection.M14 - viewProjection.M12,
            viewProjection.M24 - viewProjection.M22,
            viewProjection.M34 - viewProjection.M32,
            viewProjection.M44 - viewProjection.M42
        );

        // NEAR
        this._planes[4] = new Plane(
            viewProjection.M13,
            viewProjection.M23,
            viewProjection.M33,
            viewProjection.M43
        );
        
        // FAR
        this._planes[5] = new Plane(
            viewProjection.M14 - viewProjection.M13,
            viewProjection.M24 - viewProjection.M23,
            viewProjection.M34 - viewProjection.M33,
            viewProjection.M44 - viewProjection.M43
        );
        
        // NORMALIZE
        for (int i = 0; i < 6; i++) {
            this._planes[i] = Plane.Normalize(this._planes[i]);
        }
    }
    
    /// <summary>
    /// Checks if a sphere is contained within the frustum.
    /// </summary>
    /// <param name="center">The center of the sphere.</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <returns>True if the sphere is contained within the frustum, otherwise false.</returns>
    public bool ContainsSphere(Vector3 center, float radius) {
        foreach (var plane in this._planes) {
            float distance = Vector3.Dot(plane.Normal, center) + plane.D;
            
            if (distance < -radius) {
                return false;
            }
        }

        return true;
    }
    
    /// <summary>
    /// Checks if a bounding box is contained within the frustum.
    /// </summary>
    /// <param name="box">The bounding box to check.</param>
    /// <returns>True if the bounding box is contained within the frustum, otherwise false.</returns>
    public bool ContainsBox(BoundingBox box) {
        Vector3 center = (box.Min + box.Max) / 2.0f;
        float radius = Vector3.Distance(center, box.Max);
        
        foreach (var plane in this._planes) {
            float distance = Vector3.Dot(plane.Normal, center) + plane.D;
            
            if (distance < -radius) {
                return false;
            }
        }

        return true;
    }
}