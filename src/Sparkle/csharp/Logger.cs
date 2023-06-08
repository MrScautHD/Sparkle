using System.Diagnostics;
using System.Reflection;

namespace Sparkle.csharp; 

public static class Logger {

    public static void Debug(string text) {
        Log(text, ConsoleColor.White);
    }

    public static void Info(string text) {
        Log(text, ConsoleColor.Cyan);
    }

    public static void Warn(string text) {
        Log(text, ConsoleColor.Yellow);
    }

    public static void Error(string text) {
        Log(text, ConsoleColor.Red);
    }

    public static void Fatal(string text) {
        Log(text, ConsoleColor.DarkRed);
        throw new Exception(text);
    }

    private static void Log(string text, ConsoleColor color) {
        MethodBase? info = new StackFrame(2).GetMethod();
        Console.ForegroundColor = color;
        Console.WriteLine($"[{info.DeclaringType.FullName} :: {info.Name}] {text}");
        Console.ResetColor();
    }
    
    //TODO Add LOG file.
}