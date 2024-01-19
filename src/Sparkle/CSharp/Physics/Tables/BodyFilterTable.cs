using JoltPhysicsSharp;

namespace Sparkle.CSharp.Physics.Tables; 

public class BodyFilterTable : BodyFilter {
    
    protected override bool ShouldCollide(BodyID bodyId) {
        return true;
    }

    protected override bool ShouldCollideLocked(Body body) {
        return !body.IsSensor;
    }
}