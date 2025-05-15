using Bliss.CSharp.Logging;
using JLogger = Jitter2.Logger;

namespace Sparkle.CSharp.Logging;

public class LogJitter {
    
    /// <summary>
    /// Logs a message with the specified log level.
    /// </summary>
    /// <param name="level">The log level used to classify the log message (Information, Warning, or Error).</param>
    /// <param name="msg">The message to log.</param>
    /// <returns>A boolean value indicating the success of the logging operation. Always returns false.</returns>
    public void Log(JLogger.LogLevel level, string msg) {
        switch (level) {
            case JLogger.LogLevel.Information:
                Logger.Info(msg);
                break;
            
            case JLogger.LogLevel.Warning:
                Logger.Warn(msg);
                break;
            
            case JLogger.LogLevel.Error:
                Logger.Error(msg);
                break;
        }
    }
}