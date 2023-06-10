using System.Reflection;

namespace Sparkle.csharp.graphics.vulkan.util; 

public static class ShaderUtil {
    
    public static byte[] GetShaderBytes(string filename, string renderSystemName) {
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        foreach (string item in assembly.GetManifestResourceNames()) {
            Console.Write(".");
            Console.WriteLine($"{item}");
        }
        
        string? resourceName = assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(filename));
        
        if (resourceName == null) {
            throw new ApplicationException($"In {renderSystemName}, No shader file found with name {filename}\n");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var memoryStream = new MemoryStream();
        
        if (stream == null) {
            throw new ApplicationException($"In {renderSystemName}, No shader file found at {resourceName}\n");
        }

        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}