using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
#pragma warning disable CA1416
#pragma warning disable CS0649
#pragma warning disable CS0168

namespace Sparkle.CSharp;

public static class SystemInfo {
    
    /// <summary>
    /// Gets the name of the processor used in the system.
    /// </summary>
    public static readonly string Cpu = GetProcessorName();
    
    /// <summary>
    /// Gets the memory info.
    /// </summary>
    public static readonly (int Total, int Available) MemoryInfo = GetMemoryInfo();
    
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
    
    /// <summary>
    /// The total and available memory information of the current system.
    /// </summary>
    /// <returns>A tuple containing total memory (in MB) as the first value and available memory (in MB) as the second value.</returns>
    private static (int Total, int Available) GetMemoryInfo() {
        if (OperatingSystem.IsWindows()) {
            return GetMemoryInfoWindows();
        }
        else if (OperatingSystem.IsLinux()) {
            return GetMemoryInfoLinux();
        }
        else if (OperatingSystem.IsMacOS()) {
            return GetMemoryInfoMacOs();
        }
        
        return default;
    }
    
    /// <summary>
    /// The total and available memory information of the system on a Windows platform.
    /// </summary>
    /// <returns>A tuple containing the total memory (in MB) as the first value and the available memory (in MB) as the second value.</returns>
    private static (int, int) GetMemoryInfoWindows() {
        MemoryStatusEx memStatus = new MemoryStatusEx();
        
        if (GlobalMemoryStatusEx(memStatus)) {
            int totalMb = (int) (memStatus.UllTotalPhys / 1024.0F / 1024.0F);
            int availMb = (int) (memStatus.UllAvailPhys / 1024.0F / 1024.0F);
            
            return (totalMb, availMb);
        }
        
        return (0, 0);
    }
    
    /// <summary>
    /// The memory status structure used to retrieve system memory information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MemoryStatusEx {
        public uint DwLength = (uint) Marshal.SizeOf<MemoryStatusEx>();
        public uint DwMemoryLoad;
        public ulong UllTotalPhys;
        public ulong UllAvailPhys;
        public ulong UllTotalPageFile;
        public ulong UllAvailPageFile;
        public ulong UllTotalVirtual;
        public ulong UllAvailVirtual;
        public ulong UllAvailExtendedVirtual;
    }
    
    /// <summary>
    /// The global memory status of the current system.
    /// </summary>
    /// <param name="lpBuffer">A reference to a <see cref="MemoryStatusEx"/> object that will hold the memory status information.</param>
    /// <returns>True if the memory status was successfully retrieved; otherwise, false.</returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx lpBuffer);
    
    /// <summary>
    /// The total and available memory information on a Linux system.
    /// </summary>
    /// <returns>A tuple containing the total memory (in MB) as the first value and available memory (in MB) as the second value, or (0, 0) if an error occurs.</returns>
    private static (int total, int available) GetMemoryInfoLinux() {
        try {
            int total = 0;
            int available = 0;
            string[] lines = File.ReadAllLines("/proc/meminfo");
            
            foreach (string line in lines) {
                string tLine = line.Trim().ToLower();
                
                if (tLine.StartsWith("memtotal")) {
                    string[] parts = line.Split(':');
                    if (parts.Length > 1 && int.TryParse(parts[1].Trim().Split(' ')[0], out int kb)) {
                        total = kb / 1024;
                    }
                }
                else if (tLine.StartsWith("memavailable")) {
                    string[] parts = line.Split(':');
                    if (parts.Length > 1 && int.TryParse(parts[1].Trim().Split(' ')[0], out int kb)) {
                        available = kb / 1024;
                    }
                }
            }
            
            return (total, available);
        }
        catch {
            return (0, 0);
        }
    }
    
    /// <summary>
    /// The memory information for MacOS systems, including total and available memory in megabytes.
    /// </summary>
    /// <returns>A tuple containing two integers: the total memory and the available memory in megabytes. Returns (0, 0) if an error occurs during retrieval.</returns>
    private static (int, int) GetMemoryInfoMacOs() {
        try {
            string totalStr = RunBashCommand("sysctl -n hw.memsize").Trim();
            int totalMb = (int) (float.Parse(totalStr) / 1024.0F / 1024.0F);
            
            string vmStat = RunBashCommand("vm_stat");
            float freePages = 0.0F;
            float pageSize = 4096.0F;
            
            foreach (string line in vmStat.Split('\n')) {
                if (line.StartsWith("Pages free:")) {
                    freePages = float.Parse(line.Split(':')[1].Trim().TrimEnd('.'));
                }
                else if (line.StartsWith("page size of")) {
                    pageSize = float.Parse(line.Split(' ')[3]);
                }
            }
            
            int availableMb = (int) (freePages * pageSize / 1024 / 1024);
            return (totalMb, availableMb);
        }
        catch {
            return (0, 0);
        }
    }
    
    /// <summary>
    /// Executes a Bash command on the system and returns the output as a string.
    /// </summary>
    /// <param name="command">The Bash command to execute.</param>
    /// <returns>The standard output from the executed Bash command.</returns>
    private static string RunBashCommand(string command) {
        Process process = new Process() {
            StartInfo = new ProcessStartInfo {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        
        return result;
    }
}