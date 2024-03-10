using System.Numerics;
using Jitter2.LinearMath;

namespace Sparkle.CSharp.Physics.Conversions;

public static class PhysicsConversion {
    
    /// <summary>
    /// Converts a Vector3 to a JVector.
    /// </summary>
    /// <param name="vector3">The Vector3 to convert.</param>
    /// <returns>A JVector representing the converted Vector3.</returns>
    public static JVector ToJVector(Vector3 vector3) {
        return new JVector(vector3.X, vector3.Y, vector3.Z);
    }

    /// <summary>
    /// Converts a Quaternion to a JQuaternion.
    /// </summary>
    /// <param name="quaternion">The Quaternion to convert.</param>
    /// <returns>A JQuaternion representing the converted Quaternion.</returns>
    public static JQuaternion ToJQuaternion(Quaternion quaternion) {
        return new JQuaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
    }
    
    /// <summary>
    /// Converts a Matrix4x4 to a JMatrix.
    /// </summary>
    /// <param name="matrix4X4">The Matrix4x4 to convert.</param>
    /// <returns>A JMatrix representing the converted Matrix4x4.</returns>
    public static JMatrix ToJMatrix(Matrix4x4 matrix4X4) {
        JMatrix jMatrix = JMatrix.Identity;
        jMatrix.M11 = matrix4X4.M11;
        jMatrix.M12 = matrix4X4.M12;
        jMatrix.M13 = matrix4X4.M13;

        jMatrix.M21 = matrix4X4.M21;
        jMatrix.M22 = matrix4X4.M22;
        jMatrix.M23 = matrix4X4.M23;

        jMatrix.M31 = matrix4X4.M31;
        jMatrix.M32 = matrix4X4.M32;
        jMatrix.M33 = matrix4X4.M33;

        return jMatrix;
    }

    /// <summary>
    /// Converts a JVector to a Vector3.
    /// </summary>
    /// <param name="jVector">The JVector to convert.</param>
    /// <returns>A Vector3 representing the converted JVector.</returns>
    public static Vector3 FromJVector(JVector jVector) {
        return new Vector3(jVector.X, jVector.Y, jVector.Z);
    }

    /// <summary>
    /// Converts a JQuaternion to a Quaternion.
    /// </summary>
    /// <param name="jQuaternion">The JQuaternion to convert.</param>
    /// <returns>A Quaternion representing the converted JQuaternion.</returns>
    public static Quaternion FromJQuaternion(JQuaternion jQuaternion) {
        return new Quaternion(jQuaternion.X, jQuaternion.Y, jQuaternion.Z, jQuaternion.W);
    }

    /// <summary>
    /// Converts a JMatrix to a Matrix4x4.
    /// </summary>
    /// <param name="jMatrix">The JMatrix to convert.</param>
    /// <returns>A Matrix4x4 representing the converted JMatrix.</returns>
    public static Matrix4x4 FromJMatrix(JMatrix jMatrix) {
        Matrix4x4 matrix4X4 = Matrix4x4.Identity;
        
        matrix4X4.M11 = jMatrix.M11;
        matrix4X4.M12 = jMatrix.M12;
        matrix4X4.M13 = jMatrix.M13;
        matrix4X4.M14 = 0;

        matrix4X4.M21 = jMatrix.M21;
        matrix4X4.M22 = jMatrix.M22;
        matrix4X4.M23 = jMatrix.M23;
        matrix4X4.M24 = 0;

        matrix4X4.M31 = jMatrix.M31;
        matrix4X4.M32 = jMatrix.M32;
        matrix4X4.M33 = jMatrix.M33;
        matrix4X4.M34 = 0;
        
        matrix4X4.M41 = 0;
        matrix4X4.M42 = 0;
        matrix4X4.M43 = 0;
        matrix4X4.M44 = 1;

        return matrix4X4;
    }
}