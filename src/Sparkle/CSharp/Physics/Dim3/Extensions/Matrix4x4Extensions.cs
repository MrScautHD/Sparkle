using System.Numerics;
using Jitter2.LinearMath;

namespace Sparkle.CSharp.Physics.Dim3.Extensions;

public static class Matrix4X4Extensions {
    
    /// <summary>
    /// Converts a <see cref="JMatrix"/> object to a <see cref="Matrix4x4"/> object.
    /// </summary>
    /// <param name="jMatrix">The JMatrix object to convert.</param>
    /// <returns>A Matrix4x4 object representing the same transformation as the input JMatrix.</returns>
    public static Matrix4x4 ToMatrix4X4(this JMatrix jMatrix) {
        Matrix4x4 matrix = Matrix4x4.Identity;
        
        matrix.M11 = jMatrix.M11;
        matrix.M12 = jMatrix.M12;
        matrix.M13 = jMatrix.M13;
        matrix.M14 = 0;

        matrix.M21 = jMatrix.M21;
        matrix.M22 = jMatrix.M22;
        matrix.M23 = jMatrix.M23;
        matrix.M24 = 0;

        matrix.M31 = jMatrix.M31;
        matrix.M32 = jMatrix.M32;
        matrix.M33 = jMatrix.M33;
        matrix.M34 = 0;
        
        matrix.M41 = 0;
        matrix.M42 = 0;
        matrix.M43 = 0;
        matrix.M44 = 1;

        return matrix;
    }

    /// <summary>
    /// Converts a <see cref="Matrix4x4"/> instance to a <see cref="JMatrix"/>.
    /// </summary>
    /// <param name="matrix">The Matrix4x4 instance to convert.</param>
    /// <returns>A JMatrix containing the equivalent transformation values from the given Matrix4x4.</returns>
    public static JMatrix ToJMatrix(this Matrix4x4 matrix) {
        JMatrix jMatrix = JMatrix.Identity;
        jMatrix.M11 = matrix.M11;
        jMatrix.M12 = matrix.M12;
        jMatrix.M13 = matrix.M13;

        jMatrix.M21 = matrix.M21;
        jMatrix.M22 = matrix.M22;
        jMatrix.M23 = matrix.M23;

        jMatrix.M31 = matrix.M31;
        jMatrix.M32 = matrix.M32;
        jMatrix.M33 = matrix.M33;

        return jMatrix;
    }
}