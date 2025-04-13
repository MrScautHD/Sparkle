using Bliss.CSharp.Geometry;
using Jitter2;
using Jitter2.Dynamics;
using Jitter2.SoftBodies;
using Veldrid;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies;

public abstract class SimpleSoftBody : SoftBody, IDebugDrawable {
    
    /// <summary>
    /// The graphics device used to create and manage GPU resources.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// The central rigid body representing the core of the soft body structure.
    /// </summary>
    public RigidBody Center { get; protected set; }
    
    /// <summary>
    /// The GPU mesh associated with the soft body. Created lazily on first access.
    /// </summary>
    public Mesh Mesh => this._mesh ??= this.CreateMesh(this.GraphicsDevice);
    
    /// <summary>
    /// Internal cached mesh instance.
    /// </summary>
    private Mesh? _mesh;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleSoftBody"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering the mesh.</param>
    /// <param name="world">The physics world this soft body is part of.</param>
    protected SimpleSoftBody(GraphicsDevice graphicsDevice, World world) : base(world) {
        this.GraphicsDevice = graphicsDevice;
    }
    
    /// <summary>
    /// Creates the mesh representation of this soft body using the specified graphics device.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create the mesh.</param>
    /// <returns>A new <see cref="Mesh"/> instance representing the soft body.</returns>
    protected abstract Mesh CreateMesh(GraphicsDevice graphicsDevice);
    
    /// <summary>
    /// Updates the mesh with the latest simulation data.
    /// </summary>
    /// <param name="commandList">The command list used to submit rendering commands.</param>
    protected internal abstract void UpdateMesh(CommandList commandList);

    /// <summary>
    /// Draws debug visuals for this soft body using the specified debug drawer.
    /// </summary>
    /// <param name="drawer">The debug drawer to use for rendering visual helpers.</param>
    public abstract void DebugDraw(IDebugDrawer drawer);

    public new virtual void Destroy() {
        base.Destroy();
        this.world.Remove(this.Center);
        this._mesh?.Dispose();
    }
}