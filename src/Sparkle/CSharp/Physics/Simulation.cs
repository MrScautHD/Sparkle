using Bliss.CSharp;

namespace Sparkle.CSharp.Physics;

public abstract class Simulation : Disposable {

    /// <summary>
    /// Performs a single step in the physics simulation based on the given time step.
    /// </summary>
    /// <param name="timeStep">The duration of the step in seconds.</param>
    protected internal abstract void Step(float timeStep);
}