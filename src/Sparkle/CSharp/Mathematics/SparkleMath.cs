using System.Numerics;

namespace Sparkle.CSharp.Mathematics;

public static class SparkleMath {
    
    /// <summary>
    /// Converts a quaternion to Euler angles (yaw, pitch, roll).
    /// </summary>
    /// <param name="quaternion">The quaternion to be converted.</param>
    /// <returns>A <see cref="Vector3"/> containing the Euler angles in radians.</returns>
    public static Vector3 QuaternionToEuler(Quaternion quaternion) {
        // Roll (x-axis rotation)
        double sinrCosp = 2.0F * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
        double cosrCosp = 1.0F - 2.0F * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
        double roll = Math.Atan2(sinrCosp, cosrCosp);

        // Pitch (y-axis rotation)
        double sinp = 2.0F * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
        double pitch = Math.Abs(sinp) >= 1.0F ? Math.CopySign(Math.PI / 2.0F, sinp) : Math.Asin(sinp);

        // Yaw (z-axis rotation)
        double sinyCosp = 2.0F * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
        double cosyCosp = 1.0F - 2.0F * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
        double yaw = Math.Atan2(sinyCosp, cosyCosp);

        return new Vector3((float) yaw, (float) pitch, (float) roll);
    }
}