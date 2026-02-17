namespace Sparkle.CSharp.Utils.Async;

public class AsyncOperation {
    
    /// <summary>
    /// Gets a value indicating whether the operation has completed.
    /// </summary>
    public bool IsDone => this._state.IsDone;
    
    /// <summary>
    /// Gets the current progress of the operation.
    /// </summary>
    public float Progress => this._state.Progress;
    
    /// <summary>
    /// Occurs when the asynchronous operation completes.
    /// </summary>
    public event Action<bool>? Completed {
        add => this._state.OnComplete += value;
        remove => this._state.OnComplete -= value;
    }
    
    /// <summary>
    /// Internal state backing this asynchronous operation.
    /// </summary>
    private AsyncState _state;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncOperation"/> class.
    /// </summary>
    /// <param name="state">The internal state used to track progress and completion.</param>
    public AsyncOperation(AsyncState state) {
        this._state = state;
    }
}