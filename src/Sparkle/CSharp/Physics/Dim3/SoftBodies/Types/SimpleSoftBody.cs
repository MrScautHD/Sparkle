using System.Numerics;
using Bliss.CSharp.Geometry;
using Jitter2;
using Jitter2.Dynamics;
using Jitter2.SoftBodies;
using Veldrid;

namespace Sparkle.CSharp.Physics.Dim3.SoftBodies.Types;

public abstract class SimpleSoftBody : SoftBody, IDebugDrawable {
    
    /// <summary>
    /// The graphics device used to create and manage GPU resources.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }

    /// <summary>
    /// The central rigid body representing the core of the soft body structure.
    /// </summary>
    public abstract RigidBody Center { get; protected set; }
    
    /// <summary>
    /// The GPU mesh associated with the soft body. Created lazily on first access.
    /// </summary>
    public Mesh Mesh => this._mesh ??= this.CreateMesh(this.GraphicsDevice);
    
    /// <summary>
    /// Internal cached mesh instance.
    /// </summary>
    private Mesh? _mesh;
    
    /// <summary>
    /// Maintains a history of vertex positions for interpolation.
    /// </summary>
    private Dictionary<RigidBody, (Vector3 Previous, Vector3 Current)> _vertexPositonsHistory;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleSoftBody"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering the mesh.</param>
    /// <param name="world">The physics world this soft body is part of.</param>
    protected SimpleSoftBody(GraphicsDevice graphicsDevice, World world) : base(world) {
        this.GraphicsDevice = graphicsDevice;
        this._vertexPositonsHistory = new Dictionary<RigidBody, (Vector3 Previous, Vector3 Current)>();
    }

    /// <summary>
    /// Executes actions required after the simulation step has completed.
    /// </summary>
    /// <param name="dt">The time step duration since the last simulation update.</param>
    protected override void WorldOnPostStep(float dt) {
        base.WorldOnPostStep(dt);

        if (this.IsActive) {
            foreach (RigidBody body in this.Vertices) {
                if (!this._vertexPositonsHistory.TryGetValue(body, out var positions)) {
                    positions = (body.Position, body.Position);
                }

                this._vertexPositonsHistory[body] = (positions.Current, body.Position);
            }
        }
    }

    /// <summary>
    /// Gets the interpolated position of a vertex at the specified index based on its history.
    /// </summary>
    /// <param name="index">The index of the vertex.</param>
    /// <returns>The interpolated position of the vertex. If interpolation data is not available, the current position of the vertex is returned.</returns>
    public Vector3 GetLerpedVertexPos(int index) {
        if (this.IsActive) {
            if (this._vertexPositonsHistory.TryGetValue(this.Vertices[index], out var positions)) {
                return Vector3.Lerp(positions.Previous, positions.Current, (float) (Time.FixedAccumulator / Time.FixedStep));
            }
        }
        
        return this.Vertices[index].Position;
    }

    /// <summary>
    /// Creates the mesh representation of this soft body using the specified graphics device.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used to create the mesh.</param>
    /// <returns>A new <see cref="Mesh"/> instance representing the soft body.</returns>
    protected abstract Mesh CreateMesh(GraphicsDevice graphicsDevice);

    /// <summary>
    /// Updates the mesh with the latest simulation and rendering data.
    /// </summary>
    /// <param name="commandList">The command list used to issue rendering commands to the GPU.</param>
    protected internal abstract void UpdateMesh(CommandList commandList);

    /// <summary>
    /// Draws debug visuals for this soft body using the specified debug drawer.
    /// </summary>
    /// <param name="drawer">The debug drawer to use for rendering visual helpers.</param>
    public abstract void DebugDraw(IDebugDrawer drawer);

    public override void Destroy() {
        base.Destroy();
        this.World.Remove(this.Center);
        this._mesh?.Dispose();
    }
}