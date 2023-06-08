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
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }
    
    //TODO Add LOG file.
}