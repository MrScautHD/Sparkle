namespace Sparkle.csharp; 

public static class SystemInfo {

    public static string Cpu => Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")!; //TODO FIX IT

    public static long Memory => GC.GetTotalMemory(true); //TODO FIX IT

    public static int Threads => Environment.ProcessorCount;

    public static string Os => Environment.OSVersion.VersionString;
}