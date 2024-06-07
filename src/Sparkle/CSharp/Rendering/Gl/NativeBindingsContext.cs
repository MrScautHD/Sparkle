using OpenTK;
using Raylib_CSharp.Rendering.Gl.Contexts;

namespace Sparkle.CSharp.Rendering.Gl;

public class NativeBindingsContext : Disposable, IBindingsContext {
    
    private NativeGlContext _context;

    /// <summary>
    /// Represents a context for native bindings in OpenGL.
    /// </summary>
    public NativeBindingsContext() {
        this._context = new NativeGlContext();
    }
    
    public nint GetProcAddress(string procName) {
        return this._context.GetProcAddress(procName);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._context.Dispose();
        }
    }
}