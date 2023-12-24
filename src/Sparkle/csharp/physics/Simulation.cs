using System.Numerics;
using JoltPhysicsSharp;
using Sparkle.csharp.physics.tables;
using Sparkle.csharp.physics.layers;

namespace Sparkle.csharp.physics; 

public class Simulation : Disposable {

    public PhysicsSystem PhysicsSystem { get; private set; }
    public PhysicsSettings Settings { get; private set; }

    public ObjectLayerPairFilterTable ObjectLayerPairFilterTable { get; private set; }
    public BroadPhaseLayerInterfaceTable BroadPhaseLayerInterfaceTable { get; private set; }
    public ObjectVsBroadPhaseLayerFilterTable ObjectVsBroadPhaseLayerFilterTable { get; private set; }

    public BroadPhaseLayerFilterTable BroadPhaseLayerFilterTable { get; private set; }
    public ObjectLayerFilterTable ObjectLayerFilterTable { get; private set; }
    public BodyFilterTable BodyFilterTable { get; private set; }

    /// <summary>
    /// Initializes a new instance of the Simulation class with the specified physics settings.
    /// </summary>
    /// <param name="settings">The physics settings to configure the simulation.</param>
    public Simulation(PhysicsSettings settings) {
        if (!Foundation.Init()) {
            return;
        }
        
        this.Settings = settings;
        
        this.ObjectLayerPairFilterTable = new ObjectLayerPairFilterTable(2);
        this.ObjectLayerPairFilterTable.EnableCollision(Layers.NonMoving, Layers.Moving);
        this.ObjectLayerPairFilterTable.EnableCollision(Layers.Moving, Layers.Moving);
        
        this.BroadPhaseLayerInterfaceTable = new BroadPhaseLayerInterfaceTable(2, 2);
        this.BroadPhaseLayerInterfaceTable.MapObjectToBroadPhaseLayer(Layers.NonMoving, BroadPhaseLayers.NonMoving);
        this.BroadPhaseLayerInterfaceTable.MapObjectToBroadPhaseLayer(Layers.Moving, BroadPhaseLayers.Moving);
        
        this.ObjectVsBroadPhaseLayerFilterTable = new ObjectVsBroadPhaseLayerFilterTable(this.BroadPhaseLayerInterfaceTable, 2, this.ObjectLayerPairFilterTable, 2);
        
        this.BroadPhaseLayerFilterTable = new BroadPhaseLayerFilterTable();
        this.ObjectLayerFilterTable = new ObjectLayerFilterTable();
        this.BodyFilterTable = new BodyFilterTable();
        
        this.PhysicsSystem = new PhysicsSystem(new PhysicsSystemSettings() {
            MaxBodies = this.Settings.MaxBodies,
            MaxBodyPairs = this.Settings.MaxBodyPairs,
            MaxContactConstraints = this.Settings.MaxContactConstraints,
            ObjectLayerPairFilter = this.ObjectLayerPairFilterTable,
            BroadPhaseLayerInterface = this.BroadPhaseLayerInterfaceTable,
            ObjectVsBroadPhaseLayerFilter = this.ObjectVsBroadPhaseLayerFilterTable
        });

        this.PhysicsSystem.Gravity = this.Settings.Gravity;
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
    /// Performs a raycast from a given origin in a specified direction for a given distance.
    /// </summary>
    /// <param name="origin">The starting point of the raycast.</param>
    /// <param name="result">Output parameter to store the result of the raycast.</param>
    /// <param name="direction">The direction in which the ray is casted.</param>
    /// <param name="distance">The maximum distance for the raycast.</param>
    /// <returns>Returns a boolean indicating whether the raycast hit something or not.</returns>
    public bool RayCast(Vector3 origin, out RayCastResult result, Vector3 direction, float distance) {
        result = RayCastResult.Default;
        
        return this.PhysicsSystem.NarrowPhaseQuery.CastRay((Double3) origin, direction * distance, ref result, this.BroadPhaseLayerFilterTable, this.ObjectLayerFilterTable, this.BodyFilterTable);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.ObjectLayerPairFilterTable.Dispose();
            this.BroadPhaseLayerInterfaceTable.Dispose();
            this.ObjectVsBroadPhaseLayerFilterTable.Dispose();
            this.BroadPhaseLayerFilterTable.Dispose();
            this.ObjectLayerFilterTable.Dispose();
            this.BodyFilterTable.Dispose();
            this.PhysicsSystem.Dispose();
            Foundation.Shutdown();
        }
    }
}