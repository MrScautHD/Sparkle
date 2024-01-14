using System.Runtime.InteropServices;
using OpenTK;

namespace Sparkle.csharp.graphics.gl;

public class MacBindingContext : IBindingsContext {
    
    //TODO check to if it works with a MAC "/System/Library/Frameworks/OpenGL.framework/OpenGL"

    /// <summary>
    /// Retrieves the address of an OpenGL extension function.
    /// </summary>
    /// <param name="procName">The name of the extension function.</param>
    /// <returns>
    /// A pointer to the extension function if found; otherwise, <see cref="IntPtr.Zero"/>.
    /// </returns>
    [DllImport("libGL.dylib", EntryPoint = "NSOpenGLGetProcAddress", CharSet = CharSet.Ansi)]
    private static extern IntPtr NSOpenGLGetProcAddress(string procName);
    
    public IntPtr GetProcAddress(string procName) {
        return NSOpenGLGetProcAddress(procName);
    }
}