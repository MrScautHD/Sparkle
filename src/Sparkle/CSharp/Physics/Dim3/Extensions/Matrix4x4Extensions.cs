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
        matrix.M14 = 0.0F;
        
        matrix.M21 = jMatrix.M21;
        matrix.M22 = jMatrix.M22;
        matrix.M23 = jMatrix.M23;
        matrix.M24 = 0.0F;
        
        matrix.M31 = jMatrix.M31;
        matrix.M32 = jMatrix.M32;
        matrix.M33 = jMatrix.M33;
        matrix.M34 = 0.0F;
        
        matrix.M41 = 0.0F;
        matrix.M42 = 0.0F;
        matrix.M43 = 0.0F;
        matrix.M44 = 1.0F;
        
        return matrix;
    }
    
    /// <summary>
    /// Converts a <see cref="JSymmetricMatrix"/> object to a <see cref="Matrix4x4"/> object.
    /// </summary>
    /// <param name="jSymmetricMatrix">The JSymmetricMatrix object to convert.</param>
    /// <returns>A Matrix4x4 object representing the same transformation as the input JSymmetricMatrix.</returns>
    public static Matrix4x4 ToMatrix4X4(this JSymmetricMatrix jSymmetricMatrix) {
        Matrix4x4 matrix = Matrix4x4.Identity;
        
        matrix.M11 = jSymmetricMatrix.M11;
        matrix.M12 = jSymmetricMatrix.M12;
        matrix.M13 = jSymmetricMatrix.M13;
        matrix.M14 = 0.0F;
        
        matrix.M21 = jSymmetricMatrix.M12;
        matrix.M22 = jSymmetricMatrix.M22;
        matrix.M23 = jSymmetricMatrix.M23;
        matrix.M24 = 0.0F;
        
        matrix.M31 = jSymmetricMatrix.M13;
        matrix.M32 = jSymmetricMatrix.M23;
        matrix.M33 = jSymmetricMatrix.M33;
        matrix.M34 = 0.0F;
        
        matrix.M41 = 0.0F;
        matrix.M42 = 0.0F;
        matrix.M43 = 0.0F;
        matrix.M44 = 1.0F;
        
        return matrix;
    }
    
    /// <summary>
    /// Converts a <see cref="Matrix4x4"/> instance to a <see cref="JMatrix"/>.
    /// </summary>
    /// <param name="matrix4X4">The Matrix4x4 instance to convert.</param>
    /// <returns>A JMatrix containing the equivalent transformation values from the given Matrix4x4.</returns>
    public static JMatrix ToJMatrix(this Matrix4x4 matrix4X4) {
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
    /// Converts a <see cref="Matrix4x4"/> object to a <see cref="JSymmetricMatrix"/> object.
    /// </summary>
    /// <param name="matrix4X4">The Matrix4x4 object to convert.</param>
    /// <returns>A JSymmetricMatrix object representing the same transformation as the input Matrix4x4.</returns>
    public static JSymmetricMatrix ToJSymmetricMatrix(this Matrix4x4 matrix4X4) {
        JSymmetricMatrix jSymmetricMatrix = JSymmetricMatrix.Identity;
        
        jSymmetricMatrix.M11 = matrix4X4.M11;
        jSymmetricMatrix.M22 = matrix4X4.M22;
        jSymmetricMatrix.M33 = matrix4X4.M33;
        
        jSymmetricMatrix.M12 = matrix4X4.M12;
        jSymmetricMatrix.M13 = matrix4X4.M13;
        jSymmetricMatrix.M23 = matrix4X4.M23;
        
        return jSymmetricMatrix;
    }
}