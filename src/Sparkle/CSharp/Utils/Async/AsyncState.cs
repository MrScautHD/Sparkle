namespace Sparkle.CSharp.Utils.Async;

public class AsyncState {
    
    /// <summary>
    /// Indicates whether the asynchronous operation has completed.
    /// </summary>
    public bool IsDone;
    
    /// <summary>
    /// The progress of the asynchronous operation, with a value between 0.0 and 1.0.
    /// </summary>
    public float Progress;
    
    /// <summary>
    /// A callback action invoked upon the completion of the asynchronous operation, providing a success flag.
    /// </summary>
    internal Action<bool>? OnComplete;
    
    /// <summary>
    /// Marks the asynchronous operation as completed, updates its progress to 100%, and triggers the completion callback with the success status.
    /// </summary>
    /// <param name="success">Indicates whether the asynchronous operation completed successfully.</param>
    public void Invoke(bool success) {
        this.IsDone = true;
        this.Progress = 1.0F;
        this.OnComplete?.Invoke(success);
    }
}