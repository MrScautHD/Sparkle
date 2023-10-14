namespace Sparkle.csharp; 

public abstract class Disposable : IDisposable {
    
    public bool HasDisposed { get; private set; }
    
    public void Dispose() {
        if (this.HasDisposed) return;
        
        this.Dispose(true);
        GC.SuppressFinalize(this);
        this.HasDisposed = true;
    }

    protected abstract void Dispose(bool disposing);
    
    protected void ThrowIfDisposed() {
        if (this.HasDisposed) {
            throw new ObjectDisposedException(this.GetType().Name);
        }
    }
}