using Jitter2.Collision;
using Jitter2.LinearMath;

namespace Sparkle.CSharp.Physics.Dim3.Mappables;

public class UnitSphere : ISupportMappable {
    
    /// <summary>
    /// Provides the support mapping for the unit sphere in a specified direction.
    /// </summary>
    /// <param name="direction">The direction vector used for the support mapping calculation.</param>
    /// <param name="result">The output parameter that will store the support point on the unit sphere.</param>
    public void SupportMap(in JVector direction, out JVector result) {
        result = JVector.Normalize(direction);
    }

    /// <summary>
    /// Gets the center point of the unit sphere.
    /// </summary>
    /// <param name="point">The output parameter that will store the center point of the unit sphere.</param>
    public void GetCenter(out JVector point) {
        point = JVector.Zero;
    }
}