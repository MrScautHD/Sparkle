using System.Numerics;
using Bliss.CSharp.Transformations;
using Jitter2;
using Jitter2.SoftBodies;
using Sparkle.CSharp.Physics.Dim3;
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
    public SoftBody SoftBody { get; private set; }
    
    /// <summary>
    /// Overrides the local offset position. Always returns zero for this component.
    /// </summary>
    public new Vector3 OffsetPos => Vector3.Zero;

    /// <summary>
    /// The factory used to create the soft body instance.
    /// </summary>
    private ISoftBodyFactory _factory;

    /// <summary>
    /// Constructs a new <see cref="SoftBody3D"/> using the provided soft body factory.
    /// </summary>
    /// <param name="factory">The soft body factory used to generate the soft body instance.</param>
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
        this.SoftBody = this._factory.CreateSoftBody(this.World, transform.Translation, transform.Rotation, transform.Scale);
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