using Jitter2.Collision;
using Jitter2.LinearMath;

namespace Sparkle.CSharp.Physics.Dim3.Mappables;

public readonly struct TriangleMappable : ISupportMappable {
    
    /// <summary>
    /// The first corner of the triangle.
    /// </summary>
    private readonly JVector _a;
    
    /// <summary>
    /// The second corner of the triangle.
    /// </summary>
    private readonly JVector _b;
    
    /// <summary>
    /// The third corner of the triangle.
    /// </summary>
    private readonly JVector _c;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleMappable"/> struct.
    /// </summary>
    /// <param name="a">The first corner of the triangle.</param>
    /// <param name="b">The second corner of the triangle.</param>
    /// <param name="c">The third corner of the triangle.</param>
    public TriangleMappable(JVector a, JVector b, JVector c) {
        this._a = a;
        this._b = b;
        this._c = c;
    }
    
    /// <summary>
    /// Finds the point of the triangle that lies farthest in the given direction (the support point).
    /// </summary>
    /// <param name="direction">The direction to search in. It does not have to be normalized.</param>
    /// <param name="result">The corner with the largest projection onto the direction.</param>
    public void SupportMap(in JVector direction, out JVector result) {
        float dotA = JVector.Dot(this._a, direction);
        float dotB = JVector.Dot(this._b, direction);
        float dotC = JVector.Dot(this._c, direction);
        
        // The corner with the largest projection is the support point.
        if (dotA >= dotB && dotA >= dotC) {
            result = this._a;
        }
        else if (dotB >= dotC) {
            result = this._b;
        }
        else {
            result = this._c;
        }
    }
    
    /// <summary>
    /// Calculates a point inside of the triangle, which is its center of mass (the centroid).
    /// </summary>
    /// <param name="point">The centroid of the triangle.</param>
    public void GetCenter(out JVector point) {
        JVector sum = this._a + this._b + this._c;
        point = new JVector(sum.X * (1.0F / 3.0F), sum.Y * (1.0F / 3.0F), sum.Z * (1.0F / 3.0F));
    }
}