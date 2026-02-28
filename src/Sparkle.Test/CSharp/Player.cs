using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Transformations;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.LinearMath;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics.Animations;

namespace Sparkle.Test.CSharp;

public class Player : Entity {
    
    private ModelRenderer _modelRenderer;
    private Animator _animator;
    private RigidBody3D _rigidBody;
    
    public float Speed { get; private set; }
    public bool IsOnGround { get; private set; }
    
    public Player(Transform transform, string? tag = null) : base(transform, tag) { }

    protected override void Init() {
        
        // Renderer.
        this._modelRenderer = new ModelRenderer(ContentRegistry.PlayerModel, -Vector3.UnitY, drawBox: false, boxColor: Color.Magenta);
        this.AddComponent(this._modelRenderer);
        
        // Animation controller. // Can also get placed into a registry so you don't have to create it every time. (saves memory)
        AnimatorController animatorController = new AnimatorController();
        
        AnimatorState idleState = new AnimatorState("Idle", this._modelRenderer.Model.Animations[0], true);
        AnimatorState walkState = new AnimatorState("Walk", this._modelRenderer.Model.Animations[1], true);
        AnimatorState runState = new AnimatorState("Run", this._modelRenderer.Model.Animations[2], true);
        AnimatorState jumpState = new AnimatorState("Jump", this._modelRenderer.Model.Animations[3], false, 0.8F);
        
        // Idle => Walk (Only if speed is between 0.1 and 5.0)
        idleState.AddTransition(new AnimatorTransition("Walk", 0.2F).AddCondition(animator => ((Player) animator.Entity).Speed > 0.1F && ((Player) animator.Entity).Speed <= 5.0F));
        
        // Idle => Run
        idleState.AddTransition(new AnimatorTransition("Run", 0.2F).AddCondition(animator => ((Player) animator.Entity).Speed > 5.0F));
        
        // Walk => Idle
        walkState.AddTransition(new AnimatorTransition("Idle", 0.2F).AddCondition(animator => ((Player) animator.Entity).Speed <= 0.1F));
        
        // Walk => Run (Fixed condition to > 5.0f)
        walkState.AddTransition(new AnimatorTransition("Run", 0.4F).AddCondition(animator => ((Player) animator.Entity).Speed > 5.0F));
        
        // Run => Idle
        runState.AddTransition(new AnimatorTransition("Idle", 0.2F).AddCondition(animator => ((Player) animator.Entity).Speed <= 0.1F));
        
        // Run => Walk (Added missing transition)
        runState.AddTransition(new AnimatorTransition("Walk", 0.4F).AddCondition(animator => ((Player) animator.Entity).Speed > 0.1F && ((Player) animator.Entity).Speed <= 5.0F));
        
        // Any => Jump.
        AnimatorTransition jumpTransition = new AnimatorTransition("Jump", 0.1F).AddCondition(animator => !((Player) animator.Entity).IsOnGround);
        idleState.AddTransition(jumpTransition);
        walkState.AddTransition(jumpTransition);
        runState.AddTransition(jumpTransition);
        
        // Jump => Idle.
        jumpState.AddTransition(new AnimatorTransition("Idle", 0.4F).AddCondition(animator => ((Player) animator.Entity).IsOnGround && ((Player) animator.Entity).Speed <= 0.1F));
        
        // Jump => Walk
        jumpState.AddTransition(new AnimatorTransition("Walk", 0.4F).AddCondition(animator => ((Player) animator.Entity).IsOnGround && ((Player) animator.Entity).Speed > 0.1F && ((Player) animator.Entity).Speed <= 5.0F));

        // Jump => Run
        jumpState.AddTransition(new AnimatorTransition("Run", 0.4F).AddCondition(animator => ((Player) animator.Entity).IsOnGround && ((Player) animator.Entity).Speed > 5.0F));
        
        animatorController.AddState(idleState);
        animatorController.AddState(walkState);
        animatorController.AddState(runState);
        animatorController.AddState(jumpState);
        
        this._animator = new Animator(animatorController);
        this.AddComponent(this._animator);
        
        // Play default animation.
        this._animator.Play("Idle");
        
        // Physics body.
        this._rigidBody = new RigidBody3D(new TransformedShape(new CapsuleShape(0.5F, 2), new Vector3(0, 0.5F, 0)), motionType: MotionType.Dynamic) {
            DrawDebug = false,
            DebugDrawColor = Color.Red
        };
        this.AddComponent(this._rigidBody);
        
        // Player lock rotation (Cannot fall over).
        HingeAngle angleConstraint = this._rigidBody.World.CreateConstraint<HingeAngle>(this._rigidBody.Body, this._rigidBody.World.NullBody);
        angleConstraint.Initialize(JVector.UnitY, AngularLimit.Full);
    }
    
    protected override void Update(double delta) {
        base.Update(delta);
        
        bool isMovingForward = Input.IsKeyDown(KeyboardKey.F);
        
        if (isMovingForward) {
            this.Speed = 5.0f;

            if (Input.IsKeyDown(KeyboardKey.ControlLeft)) {
                this.Speed = 10.0F;
            }
        }
        else {
            this.Speed = 0.0f;
        }
        
        this.IsOnGround = this._rigidBody.World.DynamicTree.RayCast(
            this.GlobalTransform.Translation,
            new Vector3(0, -0.9F, 0),
            shape => {
                // Ignore the player's own shapes.
                if (shape is Shape jShape && jShape.ShapeId == this._rigidBody.Body.Shapes.First().ShapeId) {
                    return false;
                }
                return true;
            },
            null,
            out _,
            out _,
            out float fraction
        ) && fraction < 1.2f; // Adjust fraction if needed based on capsule offset
        
        if (Input.IsKeyPressed(KeyboardKey.X) && this.IsOnGround) {
            this._rigidBody.SetActivationState(true);
            this._rigidBody.AddForce(new Vector3(0, 600, 0)); // Adjust the 300 value if you want to jump higher/lower
        }
    }
}