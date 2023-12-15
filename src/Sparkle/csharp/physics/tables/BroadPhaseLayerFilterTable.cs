using JoltPhysicsSharp;

namespace Sparkle.csharp.physics.tables; 

public class BroadPhaseLayerFilterTable : BroadPhaseLayerFilter {
    
    protected override bool ShouldCollide(BroadPhaseLayer layer) {
        return true;
    }
}