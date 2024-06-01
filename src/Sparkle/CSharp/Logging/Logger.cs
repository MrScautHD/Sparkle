using System.Diagnostics;
using System.Reflection;
using Raylib_CSharp.Logging;
using Sparkle.CSharp.IO;
using RLogger = Raylib_CSharp.Logging.Logger;

namespace Sparkle.CSharp.Logging;

public static class Logger {
    
    public delegate bool OnMessage(LogType type, string msg, int skipFrames, ConsoleColor color);
    public static event OnMessage? Message;
    
    public static string? LogPath { get; private set; }
    
    public static bool HasInitialized { get; private set; }

    /// <summary>
    /// Initializes the logger.
    /// </summary>
    internal static void Init() {
        RLogger.Init();
        RLogger.Message += RaylibLogger;

        HasInitialized = true;
    }

    /// <summary>
    /// Logs a debug message with optional stack frame information.
    /// </summary>
    /// <param name="msg">The debug message to be logged.</param>
    /// <param name="skipFrames">The number of stack frames to skip (optional, default is 2).</param>
    public static void Debug(string msg, int skipFrames = 2) {
#if DEBUG
        Log(LogType.Debug, msg, skipFrames, ConsoleColor.Gray);
#endif
    }

    /// <summary>
    /// Logs an informational message with optional stack frame information.
    /// </summary>
    /// <param name="msg">The informational message to be logged.</param>
    /// <param name="skipFrames">The number of stack frames to skip (optional, default is 2).</param>
    public static void Info(string msg, int skipFrames = 2) {
        Log(LogType.Info, msg, skipFrames, ConsoleColor.Cyan);
    }

    /// <summary>
    /// Logs a warning message with optional stack frame information.
    /// </summary>
    /// <param name="msg">The warning message to be logged.</param>
    /// <param name="skipFrames">The number of stack frames to skip (optional, default is 2).</param>
    public static void Warn(string msg, int skipFrames = 2) {
        Log(LogType.Warn, msg, skipFrames, ConsoleColor.Yellow);
    }

    /// <summary>
    /// Logs an error message with optional stack frame information.
    /// </summary>
    /// <param name="msg">The error message to be logged.</param>
    /// <param name="skipFrames">The number of stack frames to skip (optional, default is 2).</param>
    public static void Error(string msg, int skipFrames = 2) {
        Log(LogType.Error, msg, skipFrames, ConsoleColor.Red);
    }

    /// <summary>
    /// Logs an error message and throws an exception with optional stack frame information.
    /// </summary>
    /// <param name="msg">The fatal message to be logged.</param>
    /// <param name="skipFrames">The number of stack frames to skip (optional, default is 2).</param>
    public static void Fatal(string msg, int skipFrames = 2) {
        Log(LogType.Fatal, msg, skipFrames, ConsoleColor.Red);
        throw new Exception(msg);
    }

    /// <summary>
    /// Logs an exception message with the color red and throws the exception.
    /// </summary>
    /// <param name="exception">The exception to log and throw.</param>
    /// <param name="skipFrames">The number of frames to skip when determining the source of the log message.</param>
    public static void Fatal(Exception exception, int skipFrames = 2) {
        Log(LogType.Fatal, exception.Message, skipFrames, ConsoleColor.Red);
        throw exception;
    }
    
    /// <summary>
    /// Logs a message with optional color formatting and stack frame information.
    /// </summary>
    /// <param name="type">The log type.</param>
    /// <param name="msg">The message to be logged.</param>
    /// <param name="skipFrames">The number of stack frames to skip (optional).</param>
    /// <param name="color">The console color for the log message (optional).</param>
    private static void Log(LogType type, string msg, int skipFrames, ConsoleColor color) {
        OnMessage? message = Message;

        if (message != null) {
            if (message.Invoke(type, msg, skipFrames, color)) {
                return;
            }
        }

        MethodBase? stackFrame = new StackFrame(skipFrames).GetMethod();
        string text = $"[{stackFrame!.DeclaringType!} :: {stackFrame.Name}] {msg}";
        
        if (LogPath != null) {
            FileAccessor.WriteLine(LogPath, text);
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
    /// Sends a log message to the logger with the specified log level and text.
    /// </summary>
    /// <param name="level">The level of the log message.</param>
    /// <param name="msg">The text of the log message.</param>
    private static bool RaylibLogger(TraceLogLevel level, string msg) {
        switch (level) {
            case TraceLogLevel.Debug:
                Debug(msg, 3);
                return true;

            case TraceLogLevel.Info:
                Info(msg, 3);
                return true;
            
            case TraceLogLevel.Warning:
                Warn(msg, 3);
                return true;
            
            case TraceLogLevel.Error:
                Error(msg, 3);
                return true;
            
            case TraceLogLevel.Fatal:
                Fatal(msg, 3);
                return true;
        }

        return false;
    }

    /// <summary>
    /// Destroys the logger and cleans up any resources used.
    /// </summary>
    public static void Destroy() {
        Message = null;
        RLogger.Destroy();
    }
}