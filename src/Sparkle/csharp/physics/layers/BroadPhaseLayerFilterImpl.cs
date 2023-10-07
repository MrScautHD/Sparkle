using JoltPhysicsSharp;

namespace Sparkle.csharp.physics.layers; 

public class BroadPhaseLayerFilterImpl : BroadPhaseLayerFilter {
    
    protected override bool ShouldCollide(BroadPhaseLayer layer) {
        return true;
    }
}