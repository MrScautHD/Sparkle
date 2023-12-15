using JoltPhysicsSharp;

namespace Sparkle.csharp.physics.tables; 

public class ObjectLayerFilterTable : ObjectLayerFilter {

    protected override bool ShouldCollide(ObjectLayer layer) {
        return true;
    }
}