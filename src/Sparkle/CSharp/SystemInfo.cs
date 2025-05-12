using System.Diagnostics;
using Microsoft.Win32;
#pragma warning disable CA1416

namespace Sparkle.CSharp;

public static class SystemInfo {
    
    /// <summary>
    /// Gets the name of the processor used in the system.
    /// </summary>
    public static readonly string Cpu = GetProcessorName();

    /// <summary>
    /// Gets the total available memory size in gigabytes (GB).
    /// </summary>
    public static readonly int MemorySize = (int) Math.Ceiling(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024.0F * 1024.0F * 1024.0F));
    
    /// <summary>
    /// Gets the number of available processor threads.
    /// </summary>
    public static readonly int Threads = Environment.ProcessorCount;

    /// <summary>
    /// Gets the operating system version string.
    /// </summary>
    public static readonly string Os = Environment.OSVersion.VersionString;

    /// <summary>
    /// Retrieves the processor name of the current system.
    /// </summary>
    /// <returns>A string representing the processor name if available, otherwise "Unknown".</returns>
    private static string GetProcessorName() {
        if (OperatingSystem.IsWindows()) {
            return GetProcessorNameWindows();
        }
        else if (OperatingSystem.IsLinux()) {
            return GetProcessorNameLinux();
        }
        else if (OperatingSystem.IsMacOS()) {
            return GetProcessorNameMacOs();
        }
        
        return "Unknown";
    }

    /// <summary>
    /// Retrieves the processor name for the current system running on Windows.
    /// </summary>
    /// <returns>A string containing the processor name if successfully retrieved, otherwise "Unknown".</returns>
    private static string GetProcessorNameWindows() {
        try {
            using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0")) {
                if (key != null) {
                    object? val = key.GetValue("ProcessorNameString");
                    
                    if (val != null) {
                        return val.ToString() ?? "Unknown";
                    }
                }
            }
        }
        catch (Exception ex) {
            // ignored
        }

        return "Unknown";
    }

    /// <summary>
    /// Retrieves the processor name for the current system running on Linux.
    /// </summary>
    /// <returns>A string containing the processor name if successfully retrieved, otherwise "Unknown".</returns>
    private static string GetProcessorNameLinux() {
        try {
            using (StreamReader reader = new StreamReader("/proc/cpuinfo")) {
                string? line;
                
                while ((line = reader.ReadLine()) != null) {
                    if (line.StartsWith("model name")) {
                        return line.Split(':')[1].Trim();
                    }
                }
            }
        }
        catch (Exception ex) {
            // Ignored.
        }

        return "Unknown";
    }

    /// <summary>
    /// Retrieves the processor name for the current system running on macOS.
    /// </summary>
    /// <returns>A string containing the processor name if successfully retrieved, otherwise "Unknown".</returns>
    private static string GetProcessorNameMacOs() {
        try {
            ProcessStartInfo psi = new ProcessStartInfo {
                FileName = "sysctl",
                Arguments = "-n machdep.cpu.brand_string",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using (Process? process = Process.Start(psi)) {
                if (process != null) {
                    using (StreamReader reader = process.StandardOutput) {
                        return reader.ReadToEnd().Trim();
                    }
                }
            }
        }
        catch (Exception ex) {
            // Ignored.
        }
        
        return "Unknown";
    }
}