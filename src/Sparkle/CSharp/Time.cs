using System.Diagnostics;

namespace Sparkle.CSharp;

public static class Time {

    /// <summary>
    /// Gets the time elapsed between the current and the previous frame.
    /// </summary>
    public static double Delta { get; private set; }

    /// <summary>
    /// Represents an accumulator used to track fixed time steps for the physics or fixed update loop.
    /// </summary>
    public static double FixedAccumulator { get; private set; }

    /// <summary>
    /// Gets the fixed time step interval defined in the current game settings.
    /// </summary>
    public static double FixedStep => Game.Instance?.Settings.FixedTimeStep ?? throw new Exception("Something went wrong!");

    /// <summary>
    /// Gets the total elapsed time since the start of the application.
    /// </summary>
    public static double Total => _totalTimeWatch.Elapsed.TotalSeconds;
    
    /// <summary>
    /// A static stopwatch used for measuring elapsed time. This is intended for internal use only.
    /// </summary>
    internal static Stopwatch DeltaTimer;
    
    /// <summary>
    /// A private static stopwatch used for measuring the total elapsed time since the start of the application.
    /// </summary>
    private static Stopwatch _totalTimeWatch;

    /// <summary>
    /// Initializes the Time class.
    /// </summary>
    internal static void Init() {
        DeltaTimer = Stopwatch.StartNew();
        _totalTimeWatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Updates the Time class by calculating the time delta and restarting the timer.
    /// </summary>
    internal static void Update() {
        
        // Calculate delta time.
        Delta = DeltaTimer.Elapsed.TotalSeconds;
        DeltaTimer.Restart();
        
        // Calculate fixed accumulator.
        FixedAccumulator += Delta;

        while (FixedAccumulator >= FixedStep) {
            FixedAccumulator -= FixedStep;
        }
    }
}