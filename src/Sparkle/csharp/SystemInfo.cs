using System.Diagnostics;

namespace Sparkle.csharp; 

public static class SystemInfo {

    public static string Cpu => Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")!; //TODO FIX IT
    
    public static long VirtualMemorySize => Process.GetCurrentProcess().VirtualMemorySize64 / 1000000;

    public static int Threads => Environment.ProcessorCount;

    public static string Os => Environment.OSVersion.VersionString;
}