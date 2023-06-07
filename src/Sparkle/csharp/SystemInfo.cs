namespace Sparkle.csharp; 

public class SystemInfo {

    public static string Cpu => Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")!;

    public static long Memory => GC.GetTotalMemory(true);

    public static int Threads => Environment.ProcessorCount;

    public static string Os => Environment.OSVersion.VersionString;
}