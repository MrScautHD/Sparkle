using JoltPhysicsSharp;

namespace Sparkle.csharp.physics.tables; 

public class BodyFilterTable : BodyFilter {
    
    protected override bool ShouldCollide(BodyID bodyId) {
        return true;
    }

    protected override bool ShouldCollideLocked(Body body) {
        return !body.IsSensor;
    }
}