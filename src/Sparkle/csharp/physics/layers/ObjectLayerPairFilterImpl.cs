using System.Diagnostics;
using JoltPhysicsSharp;

namespace Sparkle.csharp.physics.layers; 

public class ObjectLayerPairFilterImpl : ObjectLayerPairFilter {

    protected override bool ShouldCollide(ObjectLayer object1, ObjectLayer object2) {
        switch (object1) {
            case Layers.NonMoving:
                return object2 == Layers.Moving;
            
            case Layers.Moving:
                return true;
            
            default:
                Debug.Assert(false);
                return false;
        }
    }
}