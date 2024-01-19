using Raylib_cs;

namespace Sparkle.CSharp.Rendering; 

public static class Graphics {
    
    /// <inheritdoc cref="Raylib.ClearBackground"/>
    public static void ClearBackground(Color color) => Raylib.ClearBackground(color);
    
    
    /// <inheritdoc cref="Raylib.BeginDrawing"/>
    public static void BeginDrawing() => Raylib.BeginDrawing();
    
    /// <inheritdoc cref="Raylib.EndDrawing"/>
    public static void EndDrawing() => Raylib.EndDrawing();
    
    
    /// <inheritdoc cref="Raylib.BeginTextureMode"/>
    public static void BeginTextureMode(RenderTexture2D target) => Raylib.BeginTextureMode(target);
    
    /// <inheritdoc cref="Raylib.EndTextureMode"/>
    public static void EndTextureMode() => Raylib.EndTextureMode();
    
    
    /// <inheritdoc cref="Raylib.BeginShaderMode"/>
    public static void BeginShaderMode(Shader shader) => Raylib.BeginShaderMode(shader);
    
    /// <inheritdoc cref="Raylib.EndShaderMode"/>
    public static void EndShaderMode() => Raylib.EndShaderMode();
    
    
    /// <inheritdoc cref="Raylib.BeginBlendMode"/>
    public static void BeginBlendMode(BlendMode mode) => Raylib.BeginBlendMode(mode);
    
    /// <inheritdoc cref="Raylib.EndBlendMode"/>
    public static void EndBlendMode() => Raylib.EndBlendMode();
    
    
    /// <inheritdoc cref="Raylib.BeginScissorMode"/>
    public static void BeginScissorMode(int x, int y, int width, int height) => Raylib.BeginScissorMode(x, y, width, height);
    
    /// <inheritdoc cref="Raylib.EndScissorMode"/>
    public static void EndScissorMode() => Raylib.EndScissorMode();
}