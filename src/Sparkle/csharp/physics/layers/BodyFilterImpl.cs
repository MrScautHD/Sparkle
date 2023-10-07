using JoltPhysicsSharp;

namespace Sparkle.csharp.physics.layers; 

public class BodyFilterImpl : BodyFilter {
    
    protected override bool ShouldCollide(BodyID bodyId) {
        return true;
    }

    protected override bool ShouldCollideLocked(Body body) {
        return !body.IsSensor;
    }
}