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
    /// Writes a formatted log message to the log file based on the log type.
    /// </summary>
    /// <param name="type">The severity level of the log message.</param>
    /// <param name="msg">The log message to write.</param>
    /// <param name="skipFrames">Unused parameter for potential stack tracing.</param>
    /// <param name="color">The console color associated with the log message.</param>
    /// <returns>Returns true if the message was successfully written; otherwise, false.</returns>
    public bool WriteFileMsg(LogType type, string msg, int skipFrames, ConsoleColor color) {
        switch (type) {
            case LogType.Debug:
                FileAccessor.WriteLine(this.FilePath, $"[DEBUG]: {msg}");
                return true;
            
            case LogType.Info:
                FileAccessor.WriteLine(this.FilePath, $"[INFO]: {msg}");
                return true;
            
            case LogType.Warn:
                FileAccessor.WriteLine(this.FilePath, $"[WARN]: {msg}");
                return true;
            
            case LogType.Error:
                FileAccessor.WriteLine(this.FilePath, $"[ERROR]: {msg}");
                return true;
            
            case LogType.Fatal:
                FileAccessor.WriteLine(this.FilePath, $"[FATAL]: {msg}");
                return true;
        }

        return false;
    }

    /// <summary>
    /// Creates a new log file with a timestamp in the specified directory.
    /// </summary>
    /// <returns>The full file path of the newly created log file.</returns>
    private string CreateLogFile() {
        string path = Path.Combine(this._directory, $"log - {DateTime.Now:yyyy-MM-dd--HH-mm-ss}.txt");
        
        if (!Directory.Exists(this._directory)) {
            Directory.CreateDirectory(this._directory);
        }

        File.Create(path).Close();
        return path;
    }
}