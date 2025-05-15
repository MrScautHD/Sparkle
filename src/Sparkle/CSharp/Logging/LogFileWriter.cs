using Bliss.CSharp.Logging;
using Sparkle.CSharp.IO;

namespace Sparkle.CSharp.Logging;

public class LogFileWriter {
    
    /// <summary>
    /// The full path of the log file where messages are written.
    /// </summary>
    public string FilePath { get; private set; }
    
    /// <summary>
    /// The directory where log files are stored.
    /// </summary>
    private string _directory;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LogFileWriter"/> class, creating a new log file in the specified directory.
    /// </summary>
    /// <param name="directory">The directory where log files should be saved.</param>
    public LogFileWriter(string directory) {
        this._directory = directory;
        this.FilePath = this.CreateLogFile();
    }

    /// <summary>
    /// Writes a log message to the log file with the specified log type, message, color, and source information.
    /// </summary>
    /// <param name="type">The type of the log (e.g., Debug, Info, Warn, Error, Fatal).</param>
    /// <param name="msg">The log message to write.</param>
    /// <param name="color">The console color associated with the message.</param>
    /// <param name="sourceFilePath">The file path of the source code that emitted the message.</param>
    /// <param name="memberName">The name of the method or member that emitted the message.</param>
    /// <param name="sourceLineNumber">The line number in the source where the message originated.</param>
    /// <returns>Returns <c>true</c> if the log message was successfully written, otherwise <c>false</c>.</returns>
    public bool WriteFileMsg(LogType type, string msg, ConsoleColor color, string sourceFilePath, string memberName, int sourceLineNumber) {
        string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string text = $"[{timestamp} | {fileName}::{memberName}({sourceLineNumber})] {msg}";
        
        switch (type) {
            case LogType.Debug:
                FileAccessor.WriteLine(this.FilePath, text);
                return false;
            
            case LogType.Info:
                FileAccessor.WriteLine(this.FilePath, text);
                return false;
            
            case LogType.Warn:
                FileAccessor.WriteLine(this.FilePath, text);
                return false;
            
            case LogType.Error:
                FileAccessor.WriteLine(this.FilePath, text);
                return false;
            
            case LogType.Fatal:
                FileAccessor.WriteLine(this.FilePath, text);
                return false;
        }
        
        return false;
    }

    /// <summary>
    /// Creates a new log file with a timestamp in the specified directory.
    /// </summary>
    /// <returns>The full file path of the newly created log file.</returns>
    private string CreateLogFile() {
        string path;
        int fileCounter = 1;
        
        do {
            path = Path.Combine(this._directory, $"log - {DateTime.Now:yyyy-MM-dd}-{fileCounter}.txt");
            fileCounter++;
        } while (File.Exists(path));

        
        if (!Directory.Exists(this._directory)) {
            Directory.CreateDirectory(this._directory);
        }
        
        File.Create(path).Close();
        return path;
    }
}