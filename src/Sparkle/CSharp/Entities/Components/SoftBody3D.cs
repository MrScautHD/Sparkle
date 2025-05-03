using System.Numerics;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Transformations;
using Jitter2;
using Jitter2.Dynamics;
using Jitter2.Dynamics.Constraints;
using Jitter2.SoftBodies;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Physics.Dim3;
using Sparkle.CSharp.Physics.Dim3.SoftBodies;
using Sparkle.CSharp.Physics.Dim3.SoftBodies.Factories;
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
    /// The physics simulation world this soft body belongs to.
    /// </summary>
    public World World => ((Simulation3D) SceneManager.Simulation!).World;
    
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
    /// Optional rendering configuration for the soft body.
    /// </summary>
    public SoftBodyRenderInfo? RenderInfo;
    
    /// <summary>
    /// The positional offset from the entity's origin. Always returns zero for this component.
    /// </summary>
    public new Vector3 OffsetPosition => Vector3.Zero;

    /// <summary>
    /// The factory responsible for creating a soft body instance.
    /// </summary>
    private ISoftBodyFactory _factory;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftBody3D"/> component.
    /// </summary>
    /// <param name="factory">The factory used to create the soft body instance.</param>
    /// <param name="renderInfo">Optional rendering configuration.</param>
    public SoftBody3D(ISoftBodyFactory factory, SoftBodyRenderInfo? renderInfo = null) : base(Vector3.Zero) {
        this._factory = factory;
        this.RenderInfo = renderInfo;
    }
    
    /// <summary>
    /// Initializes the component and creates the soft body instance.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        this.CreateSoftBody();
    }

    /// <summary>
    /// Called after the simulation update step to synchronize the soft body data with the entity position and rotation.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update step, in seconds.</param>
    protected internal override void AfterUpdate(double delta) {
        base.AfterUpdate(delta);
        this.UpdateBodyPosition();
        this.UpdateBodyRotation();
    }

    /// <summary>
    /// Called during the fixed simulation step to update physics-driven transformations
    /// for the entity and its components.
    /// </summary>
    /// <param name="fixedStep">The fixed time step duration, typically used for consistent physics calculations.</param>
    protected internal override void FixedUpdate(double fixedStep) {
        base.FixedUpdate(fixedStep);
        this.UpdateEntityPosition();
        this.UpdateEntityRotation();
    }

    /// <summary>
    /// Renders the soft body mesh if it falls within the active camera's frustum, applying optional visual configurations.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer to which the rendering output is written.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        
        if (this.RenderInfo != null) {
            Camera3D? cam3D = SceneManager.ActiveCam3D;
        
            if (cam3D == null) {
                return;
            }
            
            // Update mesh data.
            this.SoftBody.UpdateMesh(context.CommandList);
            
            // Draw the mesh.
            if (this.Vertices.Any(v => cam3D.GetFrustum().ContainsPoint(this.GetLerpedVertexPos(this.Vertices.IndexOf(v))))) {
                this.Mesh.Draw(context.CommandList, new Transform(), framebuffer.OutputDescription, this.RenderInfo.Sampler, this.RenderInfo.DepthStencilState, this.RenderInfo.RasterizerState, this.RenderInfo.Color);
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
        Transform transform = this.Entity.Transform;
        this.SoftBody = this._factory.CreateSoftBody(this.GraphicsDevice, this.World, this.GlobalPosition, transform.Rotation);
    }
    
    /// <summary>
    /// Synchronizes the entity's position with the soft body center when active.
    /// </summary>
    private void UpdateEntityPosition() {
        if (this.IsActive && (Vector3) this.Center.Position != this.Entity.Transform.Translation) {
            this.Entity.Transform.Translation = this.Center.Position;
        }
    }
    
    /// <summary>
    /// Synchronizes the soft body center position with the entity when inactive.
    /// </summary>
    private void UpdateBodyPosition() {
        if (!this.IsActive && (Vector3) this.Center.Position != this.Entity.Transform.Translation) {
            this.Center.Position = this.Entity.Transform.Translation;
        }
    }
    
    /// <summary>
    /// Synchronizes the entity's rotation with the soft body center when active.
    /// </summary>
    private void UpdateEntityRotation() {
        Quaternion bodyRot = this.Center.Orientation;
        
        if (this.IsActive && this.Entity.Transform.Rotation != bodyRot) {
            this.Entity.Transform.Rotation = bodyRot;
        }
    }
    
    /// <summary>
    /// Synchronizes the soft body center rotation with the entity when inactive.
    /// </summary>
    private void UpdateBodyRotation() {
        Quaternion entityRot = this.Entity.Transform.Rotation;

        if (!this.IsActive && (Quaternion) this.Center.Orientation != entityRot) {
            this.Center.Orientation = entityRot;
        }
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            this.SoftBody.Destroy();
        }
    }
}