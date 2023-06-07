using System.Diagnostics;

namespace Sparkle.csharp; 

public class Time {

    public static float DeltaTime { get; private set; }
    public static float TotalTime => (float) _watch.Elapsed.TotalSeconds;

    private static Stopwatch _timer;
    private static Stopwatch _watch;

    internal static void Init() {
        _timer = Stopwatch.StartNew();
        _watch = Stopwatch.StartNew();
    }

    internal static void Update() {
        DeltaTime = (float) _timer.Elapsed.TotalSeconds;
        _timer.Restart();
    }
}