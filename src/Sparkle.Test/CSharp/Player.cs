using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Transformations;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics.Animations;
using Sparkle.CSharp.Scenes;

namespace Sparkle.Test.CSharp;

/// <summary>
/// Represents a controllable player entity in the 3D world, featuring physics-based movement,
/// animations, and procedural head tracking towards the camera direction.
/// <para>Note: Some physics logic and movement concepts in this class are adapted from Jitter2 Demo05 
/// (<see href="https://github.com/notgiven688/jitterphysics2/blob/main/src/JitterDemo/Demos/Demo05.cs"/>).</para>
/// </summary>
public class Player : Entity {
    
    /// <summary>
    /// Gets a value indicating whether the player is currently standing on a valid surface.
    /// </summary>
    public bool IsOnGround { get; private set; }
    
    /// <summary>
    /// Gets the player's current horizontal movement speed.
    /// </summary>
    public float Speed { get; private set; }
    
    /// <summary>
    /// The model renderer responsible for drawing.
    /// </summary>
    private ModelRenderer _modelRenderer;
    
    /// <summary>
    /// Controls animations.
    /// </summary>
    private Animator _animator;
    
    /// <summary>
    /// The physics rigid body.
    /// </summary>
    private RigidBody3D _rigidBody;
    
    /// <summary>
    /// Stores the current head rotation.
    /// </summary>
    private Quaternion _currentHeadRotation;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    /// <param name="transform">The initial world transform of the player.</param>
    public Player(Transform transform) : base(transform, "player") {
        this._currentHeadRotation = Quaternion.Identity;
    }
    
    /// <summary>
    /// Initializes the components and properties of the <see cref="Player"/> class,
    /// setting up rendering, animations, physics, and player-specific configurations.
    /// </summary>
    protected override void Init() {
        
        // Renderer.
        this._modelRenderer = new ModelRenderer(ContentRegistry.PlayerModel, -Vector3.UnitY, drawBox: false, boxColor: Color.Magenta);
        this.AddComponent(this._modelRenderer);
        
        // Animator.
        this._animator = new Animator(ContentRegistry.PlayerBasicAnimatorController);
        
        // Apply head rotation.
        this._animator.OnBoneTransformsReady += boneMatrices => {
            if (boneMatrices.TryGetValue("head", out Matrix4x4 headMatrix)) {
                if (Matrix4x4.Decompose(headMatrix, out Vector3 scale, out Quaternion animRot, out Vector3 translation)) {
                    
                    // Multiply the animation's rotation by your custom rotation or completely overwrite it by ignoring animRot.
                    Quaternion finalRotation = animRot * this._currentHeadRotation;
                    
                    // Reconstruct and override the matrix in the dictionary.
                    boneMatrices["head"] = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(finalRotation) * Matrix4x4.CreateTranslation(translation);
                }
            }
        };
        
        // Add upper body animation layer (Right now this is just a test layer and just swings the arms, you can use it for shooters games for holding the weapon).
        AnimationLayer upperBodyLayer = new AnimationLayer("ActionLayer", ContentRegistry.PlayerUpperBodyAnimatorController, 0.0F, [
            "arm_l",
            "arm_r",
            "forarm_l",
            "forarm_r",
            "hand_l",
            "hand_r"
        ]);
        
        this._animator.AddLayer(upperBodyLayer);
        this.AddComponent(this._animator);
        
        // Play default animation.
        this._animator.Play("Idle"); 
        
        // Physics body.
        this._rigidBody = new RigidBody3D(new TransformedShape(new CapsuleShape(0.5F, 2.0F), new Vector3(0.0F, 0.5F, 0.0F)), motionType: MotionType.Dynamic, friction: 0.8F) {
            DrawDebug = false,
            DebugDrawColor = Color.Red
        };
        this.AddComponent(this._rigidBody);
        
        // Disable velocity damping.
        this._rigidBody.Damping = (0.0F, 0.0F);
        
        // Disable deactivation.
        this._rigidBody.DeactivationTime = TimeSpan.MaxValue;
        
        // Player lock rotation (Cannot fall over).
        HingeAngle angleConstraint = this._rigidBody.World.CreateConstraint<HingeAngle>(this._rigidBody.Body, this._rigidBody.World.NullBody);
        angleConstraint.Initialize(Vector3.UnitY, AngularLimit.Full);
    }
    
    /// <summary>
    /// Updates the state of the player.
    /// </summary>
    /// <param name="delta">The time in seconds that has passed since the last frame.</param>
    protected override void Update(double delta) {
        base.Update(delta);
        
        // Check if on the ground.
        this.IsOnGround = this.CanJump(out _, out _);
        
        // Set the speed value.
        Vector3 horizontalVelocity = this._rigidBody.Velocity with {
            Y = 0.0F
        };
        
        this.Speed = horizontalVelocity.Length();
        
        // Handle debug drawing.
        if (Input.IsKeyPressed(KeyboardKey.O)) {
            this._rigidBody.DrawDebug = !this._rigidBody.DrawDebug;
            this._modelRenderer.DrawBox = !this._modelRenderer.DrawBox;
        }
        
        // Handle input direction.
        Vector3 inputDir = Vector3.Zero;
        
        if (Input.IsKeyDown(KeyboardKey.W)) {
            inputDir.Z -= 1.0F;
        }
        
        if (Input.IsKeyDown(KeyboardKey.S)) {
            inputDir.Z += 1.0F;
        }
        
        if (Input.IsKeyDown(KeyboardKey.A)) {
            inputDir.X -= 1.0F;
        }
        
        if (Input.IsKeyDown(KeyboardKey.D)) {
            inputDir.X += 1.0F;
        }
        
        Vector3 camForward = Vector3.UnitZ;
        Vector3 camRight = Vector3.UnitX;
        
        float maxYawAngle = float.DegreesToRadians(60.0F);
        float maxPitchAngle = float.DegreesToRadians(45.0F);
        
        // Handle head rotation.
        if (SceneManager.ActiveCam3D != null) {
            camForward = SceneManager.ActiveCam3D.GetForward();
            
            // Calculate pitch.
            float pitch = MathF.Asin(camForward.Y);
            
            // Clamp the pitch so the neck doesn't break when looking straight up/down.
            float clampedPitch = Math.Clamp(pitch, -maxPitchAngle, maxPitchAngle);
            
            // Match the body's flipped coordinate system by using -X and -Z.
            float camYaw = MathF.Atan2(-camForward.X, -camForward.Z);
            
            // Convert body orientation to yaw.
            Vector3 bodyForward = Vector3.Transform(Vector3.UnitZ, this._rigidBody.Orientation);
            float bodyYaw = MathF.Atan2(bodyForward.X, bodyForward.Z);
            
            // Calculate the difference between camera yaw and body yaw.
            float headYaw = camYaw - bodyYaw;
            
            // Normalize the angle difference to prevent snapping issues.
            while (headYaw > MathF.PI) {
                headYaw -= MathF.PI * 2.0F;
            }
            while (headYaw < -MathF.PI) {
                headYaw += MathF.PI * 2.0F;
            }
            
            // Clamp the head rotation so it doesn't spin like an owl.
            float clampedHeadYaw = Math.Clamp(headYaw, -maxYawAngle, maxYawAngle);
            
            // Set current head rotation and lerp it.
            Quaternion targetHeadRotation = Quaternion.CreateFromYawPitchRoll(clampedHeadYaw, clampedPitch, 0.0F);
            this._currentHeadRotation = Quaternion.Slerp(this._currentHeadRotation, targetHeadRotation, (float) delta * 25.0F);
            
            // Flatten the camera forward vector so the movement is strictly horizontal.
            camForward.Y = 0.0F; 
            camForward = Vector3.Normalize(camForward);
            
            // Set the right vector.
            camRight = Vector3.Normalize(Vector3.Cross(camForward, Vector3.UnitY));
        }
        
        // Calculate target movement direction relative to camera.
        Vector3 moveDir = Vector3.Normalize(camForward * -inputDir.Z + camRight * inputDir.X);
        
        // Get the current body forward to calculate current yaw.
        Vector3 currentBodyForward = Vector3.Transform(Vector3.UnitZ, this._rigidBody.Orientation);
        float currentYaw = MathF.Atan2(currentBodyForward.X, currentBodyForward.Z);
        
        // Handle player rotation towards movement direction or camera view.
        if (moveDir.LengthSquared() > 0.0F) {
            float targetYaw = MathF.Atan2(-moveDir.X, -moveDir.Z);
            
            // Calculate the difference between the target and current yaw.
            float angleDiff = targetYaw - currentYaw;
            
            // This ensures the player always rotates taking the shortest path.
            while (angleDiff > MathF.PI) {
                angleDiff -= MathF.PI * 2.0F;
            }
            while (angleDiff < -MathF.PI) {
                angleDiff += MathF.PI * 2.0F;
            }
            
            this.SetAngularInput(angleDiff * 15.0F);
        }
        else {
            float camYaw = MathF.Atan2(-camForward.X, -camForward.Z);
            
            // Calculate the difference between the camera and current yaw.
            float angleDiff = camYaw - currentYaw;
            
            // This ensures the player always rotates taking the shortest path.
            while (angleDiff > MathF.PI) {
                angleDiff -= MathF.PI * 2.0F;
            }
            while (angleDiff < -MathF.PI) {
                angleDiff += MathF.PI * 2.0F;
            }
            
            // If the camera is outside the max neck angle, drag the body along with it.
            if (angleDiff > maxYawAngle) {
                this.SetAngularInput((angleDiff - maxYawAngle) * 15.0F);
            }
            else if (angleDiff < -maxYawAngle) {
                this.SetAngularInput((angleDiff + maxYawAngle) * 15.0F);
            }
            else {
                
                // Camera is within comfortable neck bounds, keep body perfectly still.
                this.SetAngularInput(0.0F);
            }
        }
        
        // Set linear input (movement).
        this.SetLinearInput(new Vector3(moveDir.X, 0.0F, moveDir.Z), Input.IsKeyDown(KeyboardKey.ShiftLeft));
        
        // Let the player jump.
        if (Input.IsKeyPressed(KeyboardKey.Space)) {
            this.Jump();
        }
        
        // Handle ActionLayer (Play as test just arm Swinging).
        AnimationLayer actionLayer = this._animator.GetLayer("ActionLayer")!;
        
        if (Input.IsKeyPressed(KeyboardKey.F)) {
            this._animator.Play("SwingArms", layerName: "ActionLayer");
        }
        
        float targetWeight = Input.IsKeyDown(KeyboardKey.F) ? 1.0F : 0.0F;
        actionLayer.Weight = float.Lerp(actionLayer.Weight, targetWeight, (float) delta * 10.0F);
        
        if (actionLayer.Weight < 0.01F) {
            actionLayer.Weight = 0.0F;
            this._animator.Stop("ActionLayer");
        }
    }
    
    /// <summary>
    /// Applies horizontal movement input to the player by directly adjusting rigid body velocity while preserving vertical physics behavior.
    /// </summary>
    /// <param name="deltaMove">The desired movement direction vector.</param>
    /// <param name="isSprinting">If true, applies sprint movement speed.</param>
    public void SetLinearInput(Vector3 deltaMove, bool isSprinting = false) {
        if (!this.CanJump(out RigidBody? floor, out Vector3 hitPoint)) {
            return;
        }
        
        float deltaMoveLength = deltaMove.Length();
        Vector3 bodyVelocity = this._rigidBody.Velocity;
        Vector3 horizontalBodyVelocity = new Vector3(bodyVelocity.X, 0.0F, bodyVelocity.Z);
        
        if (deltaMoveLength > 0.01F) {
            float targetSpeed = isSprinting ? 8.0F : 3.0F; 
            
            // Normalize the direction and calculate our desired horizontal velocity.
            deltaMove *= 1.0F / deltaMoveLength; 
            Vector3 targetVelocity = deltaMove * targetSpeed;
            
            // Directly set the velocity for crisp, instant movement.
            // We keep bodyVel.Y so gravity and jumping still work perfectly.
            this._rigidBody.Velocity = new Vector3(targetVelocity.X, bodyVelocity.Y, targetVelocity.Z);
            
            // Apply equal opposite force to dynamic floors (like standing on a moving box).
            if (floor is { MotionType: MotionType.Dynamic }) {
                Vector3 velocityDifference = targetVelocity - horizontalBodyVelocity;
                Vector3 pushForce = velocityDifference * this._rigidBody.Mass * 10.0F;
                
                floor.AddForce(-pushForce, this._rigidBody.Position + hitPoint);
            }
        }
        else {
            // Apply strong damping to X and Z when there's no input, so the player stops sliding.
            this._rigidBody.Velocity = new Vector3(bodyVelocity.X * 0.8F, bodyVelocity.Y, bodyVelocity.Z * 0.8F);
            
            // Completely zero out the velocity if it's tiny to force the Idle animation.
            if (horizontalBodyVelocity.Length() < 0.15F) {
                this._rigidBody.Velocity = new Vector3(0.0F, bodyVelocity.Y, 0.0F);
            }
        }
    }
    
    /// <summary>
    /// Applies rotational input to the player by modifying angular velocity around the vertical axis.
    /// </summary>
    /// <param name="rotate">The desired yaw rotation velocity.</param>
    public void SetAngularInput(float rotate) {
        Vector3 currentVel = this._rigidBody.AngularVelocity;
        this._rigidBody.AngularVelocity = new Vector3(currentVel.X, rotate, currentVel.Z);
    }
    
    /// <summary>
    /// Makes the player jump if grounded by applying an upward velocity impulse.
    /// </summary>
    public void Jump() {
        if (this.CanJump(out RigidBody? floorBody, out Vector3 hitPoint)) {
            float newYVel = 5.0F;
            
            if (floorBody != null) {
                newYVel += floorBody.Velocity.Y;
            }
            
            float deltaVel = this._rigidBody.Velocity.Y - newYVel;
            
            this._rigidBody.Velocity = new Vector3(this._rigidBody.Velocity.X, newYVel, this._rigidBody.Velocity.Z);
            
            if (floorBody is { MotionType: MotionType.Dynamic }) {
                float force = this._rigidBody.Mass * deltaVel * 100.0F;
                
                floorBody.SetActivationState(true);
                floorBody.AddForce(Vector3.UnitY * force, floorBody.Position + hitPoint);
            }
        }
    }
    
    /// <summary>
    /// Determines whether the player can jump based on contact points with the ground.
    /// </summary>
    /// <param name="floor">The rigid body representing the floor the player is in contact with, if any.</param>
    /// <param name="hitPoint">The contact point where the player is touching the ground.</param>
    /// <returns>Returns <c>true</c> if the player can jump, otherwise <c>false</c>.</returns>
    private bool CanJump(out RigidBody? floor, out Vector3 hitPoint) {
        foreach (Arbiter contact in this._rigidBody.Body.Contacts) {
            ref ContactData contactData = ref contact.Handle.Data;
            int numContacts = 0;
            hitPoint = Vector3.Zero;
            
            // A contact may contain up to four contact points, see which ones were used during the last step.
            uint mask = contactData.UsageMask >> 4;
            
            bool isBody1 = contact.Body1 == this._rigidBody.Body;
            floor = isBody1 ? contact.Body2 : contact.Body1;
            
            if ((mask & ContactData.MaskContact0) != 0) { hitPoint += isBody1 ? contactData.Contact0.RelativePosition1 : contactData.Contact0.RelativePosition2; numContacts++; }
            if ((mask & ContactData.MaskContact1) != 0) { hitPoint += isBody1 ? contactData.Contact1.RelativePosition1 : contactData.Contact1.RelativePosition2; numContacts++; }
            if ((mask & ContactData.MaskContact2) != 0) { hitPoint += isBody1 ? contactData.Contact2.RelativePosition1 : contactData.Contact2.RelativePosition2; numContacts++; }
            if ((mask & ContactData.MaskContact3) != 0) { hitPoint += isBody1 ? contactData.Contact3.RelativePosition1 : contactData.Contact3.RelativePosition2; numContacts++; }
            
            if (numContacts == 0) {
                continue;
            }
            
            // Divide the result by the number of contact points to get the "center" of the contact.
            hitPoint *= 1.0F / numContacts;
            
            // Check if the hit point is on the players base.
            if (hitPoint.Y <= -0.8F) {
                return true;
            }
        }
        
        hitPoint = Vector3.Zero;
        floor = null;
        
        return false;
    }
}