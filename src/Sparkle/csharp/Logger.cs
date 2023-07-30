using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs;
using Sparkle.csharp.file;

namespace Sparkle.csharp; 

public static class Logger {
    
    public static string? LogPath { get; private set; }
    public static bool LogFile { get; private set; }

    public static void Debug(string msg, int skipFrames = 2) {
        Log(msg, skipFrames, ConsoleColor.Gray);
    }

    public static void Info(string msg, int skipFrames = 2) {
        Log(msg, skipFrames, ConsoleColor.Cyan);
    }

    public static void Warn(string msg, int skipFrames = 2) {
        Log(msg, skipFrames, ConsoleColor.Yellow);
    }

    public static void Error(string msg, int skipFrames = 2) {
        Log(msg, skipFrames, ConsoleColor.Red);
    }

    private static void Log(string msg, int skipFrames, ConsoleColor color) {
        MethodBase? info = new StackFrame(skipFrames).GetMethod();
        string text = $"[{info!.DeclaringType!.FullName} :: {info.Name}] {msg}";

        if (LogFile) {
            FileManager.WriteLine(text, LogPath!);
        }

        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }
    
    internal static void CreateLogFile(string directory) {
        LogPath = Path.Combine(directory, $"log - {DateTime.Now:yyyy-MM-dd--HH-mm-ss}.txt");
        LogFile = true;
        
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }
        
        File.Create(LogPath).Close();
    }

    internal static unsafe void SetupRayLibLogger() {
        Raylib.SetTraceLogCallback(&RayLibLogger);
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] {typeof(CallConvCdecl)})]
    private static unsafe void RayLibLogger(int logLevel, sbyte* text, sbyte* args) {
        string message = Logging.GetLogMessage(new IntPtr(text), new IntPtr(args));

        switch ((TraceLogLevel) logLevel) {
            case TraceLogLevel.LOG_DEBUG:
                Debug(message, 3);
                break;

            case TraceLogLevel.LOG_INFO:
                Info(message, 3);
                break;
            
            case TraceLogLevel.LOG_WARNING:
                Warn(message, 3);
                break;
            
            case TraceLogLevel.LOG_ERROR:
                Error(message, 3);
                break;
        }
    }
}