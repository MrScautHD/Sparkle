using Raylib_cs;

namespace Sparkle.csharp; 

public static class Time {

    public static float DeltaTime => Raylib.GetFrameTime();
    
    public static double TotalTime => Raylib.GetTime();

    public static void WaitTime(double seconds) => Raylib.WaitTime(seconds);
}