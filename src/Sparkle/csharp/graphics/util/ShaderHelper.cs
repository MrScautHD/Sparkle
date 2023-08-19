using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class ShaderHelper {
    
    /// <summary> See <see cref="Raylib.LoadShader(string, string)"/> </summary>
    public static Shader Load(string vsFileName, string fsFileName) => Raylib.LoadShader(vsFileName, fsFileName);
    
    /// <summary> See <see cref="Raylib.LoadShaderFromMemory(string, string)"/> </summary>
    public static Shader LoadFromMemory(string vsFileName, string fsFileName) => Raylib.LoadShaderFromMemory(vsFileName, fsFileName);
    
    /// <summary> See <see cref="Raylib.UnloadShader"/> </summary>
    public static void Unload(Shader shader) => Raylib.UnloadShader(shader);

    
    /// <summary> See <see cref="Raylib.BeginShaderMode"/> </summary>
    public static void BeginMode(Shader shader) => Raylib.BeginShaderMode(shader);
    
    /// <summary> See <see cref="Raylib.EndShaderMode"/> </summary>
    public static void EndMode() => Raylib.EndShaderMode();

    
    /// <summary> See <see cref="Raylib.IsShaderReady"/> </summary>
    public static bool IsReady(Shader shader) => Raylib.IsShaderReady(shader);
    
    /// <summary> See <see cref="Raylib.GetShaderLocationAttrib(Shader, string)"/> </summary>
    public static int GetLocationAttribute(Shader shader, string attributeName) => Raylib.GetShaderLocationAttrib(shader, attributeName);
    
    
    /// <summary> See <see cref="Raylib.SetShaderValue"/> </summary>
    public static void SetValue<T>(Shader shader, int locIndex, T value, ShaderUniformDataType uniformType) where T : unmanaged => Raylib.SetShaderValue(shader, locIndex, value, uniformType);
    
    /// <summary> See <see cref="Raylib.SetShaderValueMatrix"/> </summary>
    public static void SetValueMatrix(Shader shader, int locIndex, Matrix4x4 mat) => Raylib.SetShaderValueMatrix(shader, locIndex, mat);
    
    /// <summary> See <see cref="Raylib.SetShaderValueTexture"/> </summary>
    public static void SetValueTexture(Shader shader, int locIndex, Texture2D texture) => Raylib.SetShaderValueTexture(shader, locIndex, texture);
}