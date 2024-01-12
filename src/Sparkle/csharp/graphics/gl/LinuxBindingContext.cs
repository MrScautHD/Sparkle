using System.Runtime.InteropServices;
using OpenTK;

namespace Sparkle.csharp.graphics.gl;

public class LinuxBindingContext : IBindingsContext {
    
    public IntPtr GetProcAddress(string procName) {
        return GetXProcAddress(procName);
    }
    
    [DllImport("libGL", EntryPoint = "glXGetProcAddress", CharSet = CharSet.Ansi)]
    private static extern IntPtr GetXProcAddress(string procName);
}