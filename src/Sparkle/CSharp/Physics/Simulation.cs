using Bliss.CSharp;

namespace Sparkle.CSharp.Physics;

public abstract class Simulation : Disposable {
    
    /// <summary>
    /// Executes a simulation step in the physics engine with a fixed time step.
    /// </summary>
    /// <param name="fixedStep">The fixed time interval for the simulation step.</param>
    protected internal abstract void Step(double fixedStep);
}