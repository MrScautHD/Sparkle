using System.Runtime.InteropServices;
using OpenTK;
using Sparkle.CSharp.Logging;

namespace Sparkle.CSharp.Rendering.Gl;

public class WinBindingContext : IBindingsContext, IDisposable {
    
    public bool HasDisposed { get; private set; }

    private IntPtr _openGlHandle;

    /// <summary>
    /// Represents a context for managing WGL bindings.
    /// </summary>
    public WinBindingContext() {
        this._openGlHandle = LoadLibrary("opengl32.dll");
    }
    
    public IntPtr GetProcAddress(string procName) {
        IntPtr wglAddress = GetWglProcAddress(procName);
        
        if (wglAddress == IntPtr.Zero) {
            IntPtr procAddress = GetProcAddress(this._openGlHandle, procName);

            if (procAddress == IntPtr.Zero) {
                Logger.Fatal("Failed to retrieve the Procedure Address.");
            }

            return procAddress;
        }

        return wglAddress;
    }

    /// <summary>
    /// Retrieves the address of an OpenGL extension function.
    /// </summary>
    /// <param name="procName">The name of the extension function.</param>
    /// <returns>
    /// A pointer to the extension function if successful; otherwise, <see cref="IntPtr.Zero"/>.
    /// </returns>
    [DllImport("opengl32.dll", EntryPoint = "wglGetProcAddress", CharSet = CharSet.Ansi)]
    private static extern IntPtr GetWglProcAddress(string procName);

    /// <summary>
    /// Loads the specified dynamic-link library (DLL) into the address space of the calling process.
    /// </summary>
    /// <param name="fileName">The name of the DLL to be loaded.</param>
    /// <returns>
    /// If the function succeeds, the return value is a handle to the loaded module.
    /// If the function fails, the return value is <see cref="IntPtr.Zero"/>. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
    /// </returns>
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string fileName);

    /// <summary>
    /// Retrieves the address of an exported function or variable from the specified dynamic-link library (DLL).
    /// </summary>
    /// <param name="module">A handle to the DLL module that contains the function or variable.</param>
    /// <param name="procName">The name of the function or variable.</param>
    /// <returns>
    /// If the function succeeds, the return value is the address of the exported function or variable.
    /// If the function fails, the return value is <see cref="IntPtr.Zero"/>.
    /// To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
    /// </returns>
    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr module, string procName);

    /// <summary>
    /// Frees the loaded dynamic-link library (DLL) module.
    /// </summary>
    /// <param name="module">A handle to the loaded DLL module obtained from the LoadLibrary or LoadLibraryEx function.</param>
    /// <returns>
    /// If the function succeeds, the return value is true.
    /// If the function fails, the return value is false.
    /// </returns>
    [DllImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeLibrary(IntPtr module);

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
            FreeLibrary(this._openGlHandle);
        }
    }
}