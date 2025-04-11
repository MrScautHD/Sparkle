using System.Numerics;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Transformations;
using Jitter2;
using Sparkle.CSharp.Physics.Dim3;
using Sparkle.CSharp.Physics.Dim3.SoftBodies;
using Sparkle.CSharp.Physics.Dim3.SoftBodies.Factories;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities.Components;

// TODO: Done entity sync!
public class SoftBody3D : Component {
    
    /// <summary>
    /// Gets the physics world from the current 3D simulation.
    /// </summary>
    public World World => ((Simulation3D) SceneManager.Simulation!).World;
    
    /// <summary>
    /// The soft body instance created and managed by this component.
    /// </summary>
    public SimpleSoftBody SoftBody { get; private set; }

    /// <summary>
    /// Gets the mesh associated with the soft body, if available.
    /// </summary>
    public Mesh Mesh => this.SoftBody.Mesh;
    
    /// <summary>
    /// Overrides the local offset position. Always returns zero for this component.
    /// </summary>
    public new Vector3 OffsetPos => Vector3.Zero;

    /// <summary>
    /// The factory used to create the soft body instance.
    /// </summary>
    private ISoftBodyFactory _factory;
    
    public SoftBody3D(ISoftBodyFactory factory) : base(Vector3.Zero) {
        this._factory = factory;
    }

    /// <summary>
    /// Initializes the component and creates the associated soft body.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        this.CreateSoftBody();
    }

    /// <summary>
    /// Instantiates the soft body using the entity's current transform and the factory.
    /// </summary>
    private void CreateSoftBody() {
        Transform transform = this.Entity.Transform;
        this.SoftBody = this._factory.CreateSoftBody(this.GraphicsDevice, this.World, transform.Translation, Quaternion.Conjugate(transform.Rotation), transform.Scale);
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            if (this.SoftBody is IDisposable softBody) {
                softBody.Dispose();
            }
            else {
                this.SoftBody.Destroy();
            }
        }
    }
}