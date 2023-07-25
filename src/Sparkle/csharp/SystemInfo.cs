namespace Sparkle.csharp; 

public static class SystemInfo {

    public static string Cpu => Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")!;

    public static int MemorySize => (int) (GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1048576.0F) / 1000;

    public static int Threads => Environment.ProcessorCount;

    public static string Os => Environment.OSVersion.VersionString;
}