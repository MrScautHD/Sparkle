using JoltPhysicsSharp;

namespace Sparkle.csharp.physics.layers; 

public class ObjectLayerFilterImpl : ObjectLayerFilter {

    protected override bool ShouldCollide(ObjectLayer layer) {
        return true;
    }
}