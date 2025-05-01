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

public class SoftBody3D : InterpolatedComponent {
    
    /// <summary>
    /// Gets a list of component types that are incompatible with this component.
    /// </summary>
    public override IReadOnlyList<Type> InCompatibleTypes => [
        typeof(RigidBody2D),
        typeof(RigidBody3D)
    ];
    
    public World World => ((Simulation3D) SceneManager.Simulation!).World;
    
    public SimpleSoftBody SoftBody { get; private set; }

    public RigidBody Center => this.SoftBody.Center;
    
    public List<RigidBody> Vertices => this.SoftBody.Vertices;
    
    public List<Constraint> Springs => this.SoftBody.Springs;

    public List<SoftBodyShape> Shapes => this.SoftBody.Shapes;

    public bool IsActive => this.SoftBody.IsActive;
    
    public Mesh Mesh => this.SoftBody.Mesh;

    public BoundingBox Box { get; private set; }
    
    public SoftBodyRenderInfo? RenderInfo;
    
    public new Vector3 OffsetPos => Vector3.Zero;
    
    private ISoftBodyFactory _factory;
    
    public SoftBody3D(ISoftBodyFactory factory, Vector3 offsetPos, SoftBodyRenderInfo? renderInfo = null) : base(offsetPos) { // TODO: Add Offset Pos
        this._factory = factory;
        this.RenderInfo = renderInfo;
    }
    
    protected internal override void Init() {
        base.Init();
        this.CreateSoftBody();
    }
    
    private void CreateSoftBody() {
        Transform transform = this.Entity.Transform;
        this.SoftBody = this._factory.CreateSoftBody(this.GraphicsDevice, this.World, this.GlobalPos, Quaternion.Conjugate(transform.Rotation), transform.Scale);
    }

    protected internal override void AfterUpdate(double delta) {
        base.AfterUpdate(delta);
        this.UpdateBodyPosition();
        this.UpdateBodyRotation();
    }

    protected internal override void FixedUpdate(double fixedStep) {
        base.FixedUpdate(fixedStep);
        this.UpdateEntityPosition();
        this.UpdateEntityRotation();
    }

    // TODO: Done this! also make the Boundingbox working!
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        
        if (this.RenderInfo != null) {
            Camera3D? cam3D = SceneManager.ActiveCam3D;
        
            if (cam3D == null) {
                return;
            }
            
            // Update mesh data.
            this.SoftBody.UpdateMesh(context.CommandList);

            //if (cam3D.GetFrustum().ContainsOrientedBox(this.Box, this.LerpedGlobalPosition, this.LerpedRotation)) {
                RasterizerStateDescription? rasterizerState = null;
                
                if (this.RenderInfo.DrawWires) {
                    rasterizerState = RasterizerStateDescription.DEFAULT with {
                        CullMode = FaceCullMode.None,
                        FillMode = PolygonFillMode.Wireframe
                    };
                }
                
                // Draw mesh.
                this.Mesh.Draw(context.CommandList, new Transform(), framebuffer.OutputDescription, this.RenderInfo.Sampler, null, rasterizerState, this.RenderInfo.MeshColor);
                
                // Draw bounding box.
                if (this.RenderInfo.DrawBoundingBox) {
                    context.ImmediateRenderer.DrawBoundingBox(context.CommandList, framebuffer.OutputDescription, new Transform(), this.Box, this.RenderInfo.BoxColor);
                }
            //}
        }
    }
    
    public void DebugDraw(IDebugDrawer drawer) {
        this.SoftBody.DebugDraw(drawer);
    }
    
    // TODO: Take a look if it really works!
    private void UpdateEntityPosition() {
        if (this.Center.IsActive && (Vector3) this.Center.Position != this.Entity.Transform.Translation) {
            this.Entity.Transform.Translation = this.Center.Position;
        }
    }
    
    private void UpdateBodyPosition() {
        if (!this.Center.IsActive && (Vector3) this.Center.Position != this.Entity.Transform.Translation) {
            this.Center.Position = this.Entity.Transform.Translation;
        }
    }
    
    private void UpdateEntityRotation() {
        Quaternion bodyRot = Quaternion.Conjugate(this.Center.Orientation);
        
        if (this.Center.IsActive && this.Entity.Transform.Rotation != bodyRot) {
            this.Entity.Transform.Rotation = bodyRot;
        }
    }
    
    private void UpdateBodyRotation() {
        Quaternion entityRot = Quaternion.Conjugate(this.Entity.Transform.Rotation);

        if (!this.Center.IsActive && (Quaternion) this.Center.Orientation != entityRot) {
            this.Center.Orientation = entityRot;
        }
    }
    
     // TODO: ADD Scale also for Rigidbody.

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            this.SoftBody.Destroy();
        }
    }
}