using System.Diagnostics;

namespace Sparkle.csharp; 

public static class SystemInfo {

    public static string Cpu => Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")!; //TODO FIX IT
    //public static string GPU => GraphicsDevice.DeviceName;

    public static string Memory => $"{Process.GetCurrentProcess().WorkingSet64 / 1000000: #,##0} MB"; //TODO Check this out

    public static int Threads => Environment.ProcessorCount;

    public static string Os => Environment.OSVersion.VersionString;
}