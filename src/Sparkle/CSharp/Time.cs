using Raylib_cs;

namespace Sparkle.CSharp; 

public static class Time {
    
    /// <inheritdoc cref="Raylib.GetFrameTime"/>
    public static float Delta => Raylib.GetFrameTime();
    
    /// <inheritdoc cref="Raylib.GetTime"/>
    public static double Total => Raylib.GetTime();

    /// <inheritdoc cref="Raylib.WaitTime"/>
    public static void Wait(double seconds) => Raylib.WaitTime(seconds);
}