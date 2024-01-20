using System.Runtime.InteropServices;
using OpenTK;

namespace Sparkle.CSharp.Rendering.Gl;

public class LinuxBindingContext : IBindingsContext {
    
    private delegate IntPtr ProcAddressDelegate(string procName);
    
    private ProcAddressDelegate[] _procAddresses;

    /// <summary>
    /// Represents a Linux binding context. </summary>
    /// /
    public LinuxBindingContext() {
        this._procAddresses = new ProcAddressDelegate[3];
        this._procAddresses[0] = GetXProcAddress;
        this._procAddresses[1] = GetXProcAddress0;
        this._procAddresses[2] = GetXProcAddress1;
    }
    
    public IntPtr GetProcAddress(string procName) {
        foreach (ProcAddressDelegate procAddress in this._procAddresses) {
            try {
                return procAddress(procName);
            }
            catch (Exception) {
                // Continue to the next delegate method
            }
        }
        
        throw new Exception("Unable to locate one of the .SO files.");
    }

    /// <summary>
    /// Retrieves the address of an OpenGL extension function.
    /// </summary>
    /// <param name="procName">The name of the OpenGL extension function to retrieve.</param>
    /// <returns>
    /// A pointer to the address of the extension function if successful; otherwise, returns NULL.
    /// </returns>
    [DllImport("libGL.so", EntryPoint = "glXGetProcAddress", CharSet = CharSet.Ansi)]
    private static extern IntPtr GetXProcAddress(string procName);

    /// <summary>
    /// Retrieves the address of an OpenGL extension function.
    /// </summary>
    /// <param name="procName">The name of the OpenGL extension function to retrieve.</param>
    /// <returns>
    /// A pointer to the address of the extension function if successful; otherwise, returns NULL.
    /// </returns>
    [DllImport("libGL.so.0", EntryPoint = "glXGetProcAddress", CharSet = CharSet.Ansi)]
    private static extern IntPtr GetXProcAddress0(string procName);

    /// <summary>
    /// Retrieves the address of an OpenGL extension function.
    /// </summary>
    /// <param name="procName">The name of the OpenGL extension function to retrieve.</param>
    /// <returns>
    /// A pointer to the address of the extension function if successful; otherwise, returns NULL.
    /// </returns>
    [DllImport("libGL.so.1", EntryPoint = "glXGetProcAddress", CharSet = CharSet.Ansi)]
    private static extern IntPtr GetXProcAddress1(string procName);
}
