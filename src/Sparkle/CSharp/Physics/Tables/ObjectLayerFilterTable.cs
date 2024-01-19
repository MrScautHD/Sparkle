using JoltPhysicsSharp;

namespace Sparkle.CSharp.Physics.Tables; 

public class ObjectLayerFilterTable : ObjectLayerFilter {

    protected override bool ShouldCollide(ObjectLayer layer) {
        return true;
    }
}