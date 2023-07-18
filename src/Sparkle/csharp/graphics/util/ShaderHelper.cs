using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class ShaderHelper {
    
    public static Shader Load(string vsFileName, string fsFileName) => Raylib.LoadShader(vsFileName, fsFileName);
    public static Shader LoadFromMemory(string vsFileName, string fsFileName) => Raylib.LoadShaderFromMemory(vsFileName, fsFileName);
    public static void Unload(Shader shader) => Raylib.UnloadShader(shader);

    public static void BeginMode(Shader shader) => Raylib.BeginShaderMode(shader);
    public static void EndMode() => Raylib.EndShaderMode();

    public static bool IsReady(Shader shader) => Raylib.IsShaderReady(shader);
    public static int GetLocationAttribute(Shader shader, string attributeName) => Raylib.GetShaderLocationAttrib(shader, attributeName);
    
    public static void SetValue<T>(Shader shader, int locIndex, T value, ShaderUniformDataType uniformType) where T : unmanaged => Raylib.SetShaderValue(shader, locIndex, value, uniformType);
    public static void SetValueMatrix(Shader shader, int locIndex, Matrix4x4 mat) => Raylib.SetShaderValueMatrix(shader, locIndex, mat);
    public static void SetValueTexture(Shader shader, int locIndex, Texture2D texture) => Raylib.SetShaderValueTexture(shader, locIndex, texture);
}