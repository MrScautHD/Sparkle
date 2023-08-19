using Raylib_cs;

namespace Sparkle.csharp; 

public static class Time {

    /// <summary>
    /// Gets the time elapsed since the last frame in seconds.
    /// </summary>
    public static float Delta => Raylib.GetFrameTime();
    
    /// <summary>
    /// Gets the total elapsed time since the application started in seconds.
    /// </summary>
    public static double Total => Raylib.GetTime();

    /// <summary>
    /// Pauses the execution for the specified amount of time in seconds.
    /// </summary>
    /// <param name="seconds">The duration to wait in seconds.</param>
    public static void Wait(double seconds) => Raylib.WaitTime(seconds);
}