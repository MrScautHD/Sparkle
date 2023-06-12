using Raylib_cs;

namespace Sparkle.csharp; 

public static class Time {

    public static double DeltaTime => Raylib.GetFrameTime();
    public static double TotalTime => Raylib.GetTime();
}