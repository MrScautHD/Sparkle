using System.Numerics;
using JoltPhysicsSharp;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;

namespace Test; 

public class GroundEntity : Entity {

    public GroundEntity(Vector3 position) : base(position) {
        this.AddComponent(new Rigidbody(new BoxShape(new Vector3(100000, 1, 100000)), MotionType.Static));
    }
}