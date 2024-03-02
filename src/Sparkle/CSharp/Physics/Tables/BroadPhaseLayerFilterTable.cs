using JoltPhysicsSharp;

namespace Sparkle.CSharp.Physics.Tables;

public class BroadPhaseLayerFilterTable : BroadPhaseLayerFilter {
    
    protected override bool ShouldCollide(BroadPhaseLayer layer) {
        return true;
    }
}