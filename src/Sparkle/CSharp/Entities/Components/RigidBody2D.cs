using System.Numerics;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.World;
using Sparkle.CSharp.Physics.Dim2;
using Sparkle.CSharp.Physics.Dim2.Def;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities.Components;

public class RigidBody2D : Component {

    public World World => ((Simulation2D) SceneManager.Simulation!).World;
    public Body Body { get; private set; }
    
    private BodyDefinition _bodyDefinition;
    private FixtureDefinition _fixtureDefinition;
    
    public RigidBody2D(Vector3 offsetPos, BodyDefinition bodyDefinition, FixtureDefinition fixtureDefinition, Body body) : base(offsetPos) {
        _bodyDefinition = bodyDefinition;
        _fixtureDefinition = fixtureDefinition;
        Body = body;
    }
}