using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Jitter2;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.SoftBodies;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Physics.Dim3;
using Sparkle.CSharp.Physics.Dim3.SoftBodies.Factories;
using Sparkle.CSharp.Physics.Dim3.SoftBodies.Types;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

public class SoftBody3D : Component {
    
    /// <summary>
    /// Gets a list of component types that are incompatible with this component.
    /// </summary>
    public override IReadOnlyList<Type> InCompatibleTypes => [
        typeof(RigidBody2D),
        typeof(RigidBody3D)
    ];
    
    /// <summary>
    /// Gets the current 3D simulation instance used by the scene.
    /// Throws an <see cref="InvalidOperationException"/> if the simulation is not of type <see cref="Simulation3D"/>.
    /// </summary>
    public Simulation3D Simulation => SceneManager.Simulation as Simulation3D ?? throw new InvalidOperationException("The current simulation must be of type Simulation3D.");
    
    /// <summary>
    /// Gets the physics world from the current 3D simulation.
    /// </summary>
    public World World => this.Simulation.World;
    
    /// <summary>
    /// The internal soft body implementation.
    /// </summary>
    public SimpleSoftBody SoftBody { get; private set; }

    /// <summary>
    /// The center rigid body used to constrain the soft body structure.
    /// </summary>
    public RigidBody Center => this.SoftBody.Center;
    
    /// <summary>
    /// The vertex rigid bodies that make up the soft body.
    /// </summary>
    public List<RigidBody> Vertices => this.SoftBody.Vertices;
    
    /// <summary>
    /// The constraints (springs) connecting the soft body vertices.
    /// </summary>
    public List<Constraint> Springs => this.SoftBody.Springs;

    /// <summary>
    /// The geometric shapes representing the volume of the soft body.
    /// </summary>
    public List<SoftBodyShape> Shapes => this.SoftBody.Shapes;

    /// <summary>
    /// Indicates whether the soft body is currently active in the simulation.
    /// </summary>
    public bool IsActive => this.SoftBody.IsActive;
    
    /// <summary>
    /// The mesh used to visually represent the soft body.
    /// </summary>
    public Mesh Mesh => this.SoftBody.Mesh;
    
    /// <summary>
    /// A reference to the material used for rendering the mesh.
    /// </summary>
    public ref Material Material => ref this._renderable.Material;
    
    /// <summary>
    /// A reference to the bone matrices for skeletal animation, if applicable.
    /// </summary>
    public Matrix4x4[]? BoneMatrics => this._renderable.BoneMatrices;
    
    /// <summary>
    /// Indicates whether the mesh associated with the soft body should be drawn.
    /// </summary>
    public bool DrawMesh;
    
    /// <summary>
    /// Determines whether debug information, such as visual representations of physics-related elements, should be displayed for the soft body.
    /// </summary>
    public bool DrawDebug;
    
    /// <summary>
    /// The color used for debugging visualization of the 3D soft body's properties and behavior.
    /// </summary>
    public Color DebugDrawColor;
    
    /// <summary>
    /// The positional offset from the entity's origin. Always returns zero for this component.
    /// </summary>
    public new Vector3 OffsetPosition => Vector3.Zero;

    /// <summary>
    /// The factory responsible for creating a soft body instance.
    /// </summary>
    private ISoftBodyFactory _factory;

    /// <summary>
    /// The underlying renderable object associated with the soft body.
    /// </summary>
    private Renderable _renderable;
    
    /// <summary>
    /// Indicates whether the synchronization process is currently active (When sync entity with body).
    /// </summary>
    private bool _isSyncing;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftBody3D"/> component.
    /// </summary>
    /// <param name="factory">The factory used to create the soft body instance.</param>
    /// <param name="drawMesh">Whether the soft body’s mesh should be rendered. Defaults to <c>true</c>.</param>
    /// <param name="drawDebug">Indicates whether debug visualization for the soft body should be enabled.</param>
    public SoftBody3D(ISoftBodyFactory factory, bool drawMesh = true, bool drawDebug = false) : base(Vector3.Zero) {
        this._factory = factory;
        this.DrawMesh = drawMesh;
        this.DrawDebug = drawDebug;
        this.DebugDrawColor = Color.White;
    }
    
    /// <summary>
    /// Initializes the component and creates the soft body instance.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        this.CreateSoftBody();
        this._renderable = new Renderable(this.Mesh, new Transform());
        this.Simulation.BodyMoved += this.SyncEntityToBodyTransform;
    }
    
    /// <summary>
    /// Updates the state of the <see cref="SoftBody3D"/> component during the simulation step.
    /// </summary>
    /// <param name="delta">The time elapsed since the last frame, in seconds.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);
        this.SyncBodyToEntityTransform(this.Entity.GlobalTransform);
    }
    
    /// <summary>
    /// Renders the soft body mesh if it falls within the active camera's frustum, applying optional visual configurations.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer to which the rendering output is written.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        
        if (this.DrawMesh) {
            Camera3D? cam3D = SceneManager.ActiveCam3D;
        
            if (cam3D == null) {
                return;
            }
            
            // Update mesh data.
            this.SoftBody.UpdateMesh(context.CommandList);
            
            // Draw the mesh.
            if (this.Vertices.Any(v => cam3D.GetFrustum().ContainsPoint(this.GetLerpedVertexPos(this.Vertices.IndexOf(v))))) {
                this.Entity.Scene.Renderer.DrawRenderable(this._renderable);
            }
        }
    }
    
    /// <summary>
    /// Retrieves the interpolated position of a vertex at the specified index within the soft body.
    /// </summary>
    /// <param name="index">The index of the vertex within the soft body.</param>
    /// <returns>The interpolated position of the vertex. If interpolation data is unavailable, the current position of the vertex is returned.</returns>
    public Vector3 GetLerpedVertexPos(int index) {
        return this.SoftBody.GetLerpedVertexPos(index);
    }

    /// <summary>
    /// Renders debug information for the soft body component using the provided debug drawer.
    /// </summary>
    /// <param name="drawer">The debug drawer used to render the debug information.</param>
    public void DebugDraw(IDebugDrawer drawer) {
        this.SoftBody.DebugDraw(drawer);
    }

    /// <summary>
    /// Creates a soft body instance associated with the current <see cref="SoftBody3D"/> component.
    /// </summary>
    private void CreateSoftBody() {
        Transform transform = this.Entity.GlobalTransform;
        this.SoftBody = this._factory.CreateSoftBody(this.GraphicsDevice, this.World, transform.Translation, transform.Rotation);
    }
    
    private void SyncEntityToBodyTransform(RigidBody body) {
        if (body == this.Center) {
            if (!this._isSyncing) {
                this._isSyncing = true;
                try {
                    Transform globalTransform = this.Entity.GlobalTransform;
                    
                    Vector3 entityPos = globalTransform.Translation;
                    Vector3 bodyPos = body.Position;
                    
                    Quaternion entityRot = globalTransform.Rotation;
                    Quaternion bodyRot = body.Orientation;
                    
                    if (bodyPos != entityPos || bodyRot != entityRot) {
                        if (this.Entity.Parent == null) {
                            if (bodyPos != entityPos) {
                                this.Entity.LocalTransform.Translation = bodyPos;
                            }
                            
                            if (bodyRot != entityRot) {
                                this.Entity.LocalTransform.Rotation = bodyRot;
                            }
                        }
                        else {
                            Transform parentGlobal = this.Entity.Parent.GlobalTransform;
                            Quaternion invParentRot = Quaternion.Inverse(parentGlobal.Rotation);
                            
                            if (bodyPos != entityPos) {
                                this.Entity.LocalTransform.Translation = Vector3.Transform(bodyPos - parentGlobal.Translation, invParentRot) / parentGlobal.Scale;
                            }
                            
                            if (bodyRot != entityRot) {
                                this.Entity.LocalTransform.Rotation = invParentRot * bodyRot;
                            }
                        }
                        
                        // Restore children's global transforms using inline math.
                        IReadOnlyCollection<Entity> children = this.Entity.GetChildren();
                        
                        if (children.Count > 0) {
                            Transform newGlobalTransform = this.Entity.GlobalTransform;
                            Quaternion invNewParentRot = Quaternion.Inverse(newGlobalTransform.Rotation);
                            
                            foreach (Entity child in children) {
                                bool hasPhysics = false;
                                bool isKinematic = false;
                                
                                if (child.TryGetComponent(out RigidBody3D? childRb)) {
                                    hasPhysics = true;
                                    isKinematic = childRb.MotionType == MotionType.Kinematic;
                                    
                                    if (isKinematic) {
                                        childRb.SyncBodyToEntityTransform(child.GlobalTransform);
                                    }
                                }
                                else if (child.TryGetComponent(out SoftBody3D? childSb)) {
                                    hasPhysics = true;
                                    isKinematic = childSb.Center.MotionType == MotionType.Kinematic;
                                    
                                    if (isKinematic) {
                                        childSb.SyncBodyToEntityTransform(child.GlobalTransform);
                                    }
                                }
                                
                                if (hasPhysics && !isKinematic) {
                                    
                                    // Calculate what the child's global transform was.
                                    Vector3 oldChildGlobalPos = globalTransform.Translation + Vector3.Transform(child.LocalTransform.Translation * globalTransform.Scale, globalTransform.Rotation);
                                    Quaternion oldChildGlobalRot = globalTransform.Rotation * child.LocalTransform.Rotation;
                                    
                                    // Apply it as the new LocalTransform relative to the parent's new position.
                                    child.LocalTransform.Translation = Vector3.Transform(oldChildGlobalPos - newGlobalTransform.Translation, invNewParentRot) / newGlobalTransform.Scale;
                                    child.LocalTransform.Rotation = invNewParentRot * oldChildGlobalRot;
                                }
                            }
                        }
                    }
                }
                finally {
                    this._isSyncing = false;
                }
            }
        }
    }
    
    /// <summary>
    /// Handles the logic for sync the body's position and rotation with the associated entity movement.
    /// </summary>
    /// <param name="transform">The transform of the entity.</param>
    internal void SyncBodyToEntityTransform(Transform transform) {
        if (!this._isSyncing) {
            this._isSyncing = true;
            try {
                Vector3 entityPos = transform.Translation;
                Vector3 bodyPos = this.Center.Position;
                Vector3 posDiff = entityPos - bodyPos;
                
                Quaternion entityRot = transform.Rotation;
                Quaternion bodyRot = this.Center.Orientation;
                Quaternion rotDiff = Quaternion.Inverse(bodyRot) * entityRot;
                
                // Only move if the position or rotation actually changed.
                if (posDiff != Vector3.Zero || rotDiff != Quaternion.Identity) {
                    
                    // Move and rotate the Center body
                    this.Center.Position = entityPos;
                    this.Center.Orientation = entityRot;
                    
                    // Move and rotate all the Vertices relative to the Center.
                    foreach (RigidBody vertex in this.Vertices) {
                        Vector3 localVertexPos = vertex.Position - bodyPos;
                        Vector3 rotatedLocalPos = Vector3.Transform(localVertexPos, rotDiff);
                        
                        vertex.Position = entityPos + rotatedLocalPos;
                        vertex.Orientation = vertex.Orientation * rotDiff;
                    }
                }
            }
            finally {
                this._isSyncing = false;
            }
        }
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        
        if (disposing) {
            this.Simulation.BodyMoved -= this.SyncEntityToBodyTransform;
            this.SoftBody.Destroy();
        }
    }
}