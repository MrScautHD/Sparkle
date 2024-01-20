using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Raylib_cs;
using Sparkle.CSharp.IO;

namespace Sparkle.CSharp; 

public static class Logger {
    
    public static string? LogPath { get; private set; }

    /// <summary>
    /// Logs a debug message with optional stack frame information.
    /// </summary>
    /// <param name="msg">The debug message to be logged.</param>
    /// <param name="skipFrames">The number of stack frames to skip (optional, default is 2).</param>
    public static void Debug(string msg, int skipFrames = 2) {
        Log(msg, skipFrames, ConsoleColor.Gray);
    }

    /// <summary>
    /// Logs an informational message with optional stack frame information.
    /// </summary>
    /// <param name="msg">The informational message to be logged.</param>
    /// <param name="skipFrames">The number of stack frames to skip (optional, default is 2).</param>
    public static void Info(string msg, int skipFrames = 2) {
        Log(msg, skipFrames, ConsoleColor.Cyan);
    }

    /// <summary>
    /// Logs a warning message with optional stack frame information.
    /// </summary>
    /// <param name="msg">The warning message to be logged.</param>
    /// <param name="skipFrames">The number of stack frames to skip (optional, default is 2).</param>
    public static void Warn(string msg, int skipFrames = 2) {
        Log(msg, skipFrames, ConsoleColor.Yellow);
    }

    /// <summary>
    /// Logs an error message with optional stack frame information.
    /// </summary>
    /// <param name="msg">The error message to be logged.</param>
    /// <param name="skipFrames">The number of stack frames to skip (optional, default is 2).</param>
    public static void Error(string msg, int skipFrames = 2) {
        Log(msg, skipFrames, ConsoleColor.Red);
    }

    /// <summary>
    /// Logs an error message and throws an exception with optional stack frame information.
    /// </summary>
    /// <param name="msg">The fatal message to be logged.</param>
    /// <param name="skipFrames">The number of stack frames to skip (optional, default is 2).</param>
    public static void Fatal(string msg, int skipFrames = 2) {
        Log(msg, skipFrames, ConsoleColor.Red);
        throw new Exception(msg);
    }

    /// <summary>
    /// Logs an exception message with the color red and throws the exception.
    /// </summary>
    /// <param name="exception">The exception to log and throw.</param>
    /// <param name="skipFrames">The number of frames to skip when determining the source of the log message.</param>
    public static void Fatal(Exception exception, int skipFrames = 2) {
        Log(exception.Message, skipFrames, ConsoleColor.Red);
        throw exception;
    }

    /// <summary>
    /// Logs a message with optional color formatting and stack frame information.
    /// </summary>
    /// <param name="msg">The message to be logged.</param>
    /// <param name="skipFrames">The number of stack frames to skip (optional).</param>
    /// <param name="color">The console color for the log message (optional).</param>
    private static void Log(string msg, int skipFrames, ConsoleColor color) {
        MethodBase? info = new StackFrame(skipFrames).GetMethod();
        string text = $"[{info!.DeclaringType!.FullName} :: {info.Name}] {msg}";
        
        if (LogPath != null) {
            FileManager.WriteLine(text, LogPath!);
        }
        
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }
    
    /// <summary>
    /// Creates a log file in the specified directory with a timestamped filename.
    /// </summary>
    /// <param name="directory">The directory where the log file will be created.</param>
    internal static void CreateLogFile(string directory) {
        LogPath = Path.Combine(directory, $"log - {DateTime.Now:yyyy-MM-dd--HH-mm-ss}.txt");
        
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        File.Create(LogPath).Close();
    }

    /// <summary>
    /// Configures a custom <see cref="Raylib"/> log by setting a trace log callback.
    /// </summary>
    internal static unsafe void SetupRaylibLogger() {
        Raylib.SetTraceLogCallback(&RaylibLogger);
    }
    
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void RaylibLogger(int logLevel, sbyte* text, sbyte* args) {
        string message = Logging.GetLogMessage(new IntPtr(text), new IntPtr(args));

        switch ((TraceLogLevel) logLevel) {
            case TraceLogLevel.Debug:
                Debug(message, 3);
                break;

            case TraceLogLevel.Info:
                Info(message, 3);
                break;
            
            case TraceLogLevel.Warning:
                Warn(message, 3);
                break;
            
            case TraceLogLevel.Error:
                Error(message, 3);
                break;
            
            case TraceLogLevel.Fatal:
                Fatal(message, 3);
                break;
        }
    }
}