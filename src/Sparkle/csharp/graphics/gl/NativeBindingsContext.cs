using System.Runtime.InteropServices;
using OpenTK;

namespace Sparkle.csharp.graphics.gl;

public class NativeBindingsContext : IBindingsContext, IDisposable {
    
    public bool HasDisposed { get; private set; }

    private IBindingsContext? _context;

    public NativeBindingsContext() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            this._context = new WinBindingsContext();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            this._context = new LinuxBindingContext(); // TODO CHECK IF IT WORKS
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            Logger.Fatal(new PlatformNotSupportedException()); // TODO FINISH IT
        }
        else {
            Logger.Fatal(new PlatformNotSupportedException());
        }
    }
    
    public IntPtr GetProcAddress(string procName) {
        return this._context!.GetProcAddress(procName);
    }

    /// <summary>
    /// Disposes of the object, allowing for proper resource cleanup and finalization.
    /// </summary>
    public void Dispose() {
        if (this.HasDisposed) return;
        
        this.Dispose(true);
        GC.SuppressFinalize(this);
        this.HasDisposed = true;
    }

    /// <summary>
    /// Releases the managed resources used by the object (disposing), and optionally releases the unmanaged
    /// resources (not disposing).
    /// </summary>
    /// <param name="disposing">A boolean value indicating whether the method is being called from
    /// Dispose method directly (true) or from the finalizer (false).</param>
    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                ((WinBindingsContext) this._context!).Dispose();
            }
        }
    }
}