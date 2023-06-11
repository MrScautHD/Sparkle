using System.Diagnostics;
using System.Reflection;
using Raylib_cs;
using Sparkle.csharp.file;

namespace Sparkle.csharp; 

public static class Logger {
    
    public static string LogPath { get; private set; }
    public static bool LogFile { get; private set; }

    public static void Debug(string msg) {
        Log(msg, ConsoleColor.Gray);
    }

    public static void Info(string msg) {
        Log(msg, ConsoleColor.Cyan);
    }

    public static void Warn(string msg) {
        Log(msg, ConsoleColor.Yellow);
    }

    public static void Error(string msg) {
        Log(msg, ConsoleColor.Red);
    }

    private static void Log(string msg, ConsoleColor color) {
        MethodBase? info = new StackFrame(2).GetMethod();
        string text = $"[{info.DeclaringType.FullName} :: {info.Name}] {msg}";

        if (LogFile) {
            FileManager.WriteLine(text, LogPath);
        }

        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }
    
    public static void CreateLogFile(string directory, string name) {
        LogPath = Path.Combine(directory, name + "-" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss") + ".txt");
        LogFile = true;
        
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }
        
        File.Create(LogPath).Close();
    }
}