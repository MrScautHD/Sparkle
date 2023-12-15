using System.Numerics;
using JoltPhysicsSharp;
using Sparkle.csharp.physics.tables;
using Sparkle.csharp.physics.layers;

namespace Sparkle.csharp.physics; 

public class Simulation : Disposable {

    public PhysicsSystem PhysicsSystem { get; private set; }
    public PhysicsSettings Settings { get; private set; }

    private ObjectLayerPairFilterTable _objectLayerPairFilterTable;
    private BroadPhaseLayerInterfaceTable _broadPhaseLayerInterfaceTable;
    private ObjectVsBroadPhaseLayerFilterTable _objectVsBroadPhaseLayerFilterTable;

    private BroadPhaseLayerFilterTable _broadPhaseLayerFilterTable;
    private ObjectLayerFilterTable _objectLayerFilterTable;
    private BodyFilterTable _bodyFilterTable;

    /// <summary>
    /// Initializes a new instance of the Simulation class with the specified physics settings.
    /// </summary>
    /// <param name="settings">The physics settings to configure the simulation.</param>
    public Simulation(PhysicsSettings settings) {
        if (!Foundation.Init()) {
            return;
        }
        
        this.Settings = settings;
        
        this._objectLayerPairFilterTable = new ObjectLayerPairFilterTable(2);
        this._objectLayerPairFilterTable.EnableCollision(Layers.NonMoving, Layers.Moving);
        this._objectLayerPairFilterTable.EnableCollision(Layers.Moving, Layers.Moving);
        
        this._broadPhaseLayerInterfaceTable = new BroadPhaseLayerInterfaceTable(2, 2);
        this._broadPhaseLayerInterfaceTable.MapObjectToBroadPhaseLayer(Layers.NonMoving, BroadPhaseLayers.NonMoving);
        this._broadPhaseLayerInterfaceTable.MapObjectToBroadPhaseLayer(Layers.Moving, BroadPhaseLayers.Moving);
        
        this._objectVsBroadPhaseLayerFilterTable = new ObjectVsBroadPhaseLayerFilterTable(this._broadPhaseLayerInterfaceTable, 2, this._objectLayerPairFilterTable, 2);
        
        this._broadPhaseLayerFilterTable = new BroadPhaseLayerFilterTable();
        this._objectLayerFilterTable = new ObjectLayerFilterTable();
        this._bodyFilterTable = new BodyFilterTable();
        
        this.PhysicsSystem = new PhysicsSystem(new PhysicsSystemSettings() {
            MaxBodies = this.Settings.MaxBodies,
            MaxBodyPairs = this.Settings.MaxBodyPairs,
            MaxContactConstraints = this.Settings.MaxContactConstraints,
            ObjectLayerPairFilter = this._objectLayerPairFilterTable,
            BroadPhaseLayerInterface = this._broadPhaseLayerInterfaceTable,
            ObjectVsBroadPhaseLayerFilter = this._objectVsBroadPhaseLayerFilterTable
        });
        
        this.PhysicsSystem.OptimizeBroadPhase();
    }

    /// <summary>
    /// Updates the simulation for a given time step with a specified number of collision resolution steps.
    /// </summary>
    /// <param name="timeStep">The time step to advance the simulation.</param>
    /// <param name="collisionSteps">The number of collision resolution steps to perform.</param>
    protected internal void Step(float timeStep, int collisionSteps) {
        this.PhysicsSystem.Step(timeStep, collisionSteps);
    }

    /// <summary>
    /// Performs a raycast in the simulation to detect intersections between a ray and physical objects.
    /// </summary>
    /// <param name="origin">The origin of the ray.</param>
    /// <param name="result">The result of the raycast, including intersection details.</param>
    /// <param name="direction">The direction of the ray.</param>
    /// <param name="distance">The maximum distance to check for intersections.</param>
    /// <returns>True if an intersection was found, false otherwise.</returns>
    public bool RayCast(Vector3 origin, out RayCastResult result, Vector3 direction, float distance) {
        result = RayCastResult.Default;
        
        return this.PhysicsSystem.NarrowPhaseQuery.CastRay((Double3) origin, direction * distance, ref result, this._broadPhaseLayerFilterTable, this._objectLayerFilterTable, this._bodyFilterTable);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            Foundation.Shutdown();
            this._broadPhaseLayerFilterTable.Dispose();
            this._objectLayerFilterTable.Dispose();
            this._bodyFilterTable.Dispose();
            this.PhysicsSystem.Dispose();
        }
    }
}