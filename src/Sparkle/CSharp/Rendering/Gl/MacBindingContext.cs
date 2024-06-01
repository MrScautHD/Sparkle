using System.Runtime.InteropServices;
using OpenTK;
using Sparkle.CSharp.Logging;

namespace Sparkle.CSharp.Rendering.Gl;

public class MacBindingContext : IBindingsContext {
    
    /// <summary>
    /// Retrieves the address of an OpenGL extension function.
    /// </summary>
    /// <param name="procName">The name of the extension function.</param>
    /// <returns>
    /// A pointer to the extension function if found; otherwise, <see cref="IntPtr.Zero"/>.
    /// </returns>
    [DllImport("/System/Library/Frameworks/OpenGL.framework/OpenGL", EntryPoint = "NSOpenGLGetProcAddress", CharSet = CharSet.Ansi)]
    private static extern IntPtr NSOpenGLGetProcAddress(string procName);
    
    public IntPtr GetProcAddress(string procName) {
        IntPtr address = NSOpenGLGetProcAddress(procName);

        if (address == IntPtr.Zero) {
            Logger.Fatal("Failed to retrieve the Procedure Address.");
        }

        return address;
    }
}