using System.Diagnostics;

namespace Sparkle.csharp; 

public static class Time {

    public static double DeltaTime => Application.Instance.Win.Time;
    public static double TotalTime => _watch.Elapsed.TotalSeconds;

    private static Stopwatch _watch;

    internal static void Init() {
        _watch = Stopwatch.StartNew();
    }
}