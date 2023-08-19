using Raylib_cs;

namespace Sparkle.csharp.graphics; 

public class Graphics {
    
    // TODO Create for 2D a own Camera Entity! (Method already removed here)

    /// <summary> See <see cref="Raylib.ClearBackground"/> </summary>
    public void ClearBackground(Color color) => Raylib.ClearBackground(color);
    
    
    /// <summary> See <see cref="Raylib.BeginDrawing"/> </summary>
    public void BeginDrawing() => Raylib.BeginDrawing();
    
    /// <summary> See <see cref="Raylib.EndDrawing"/> </summary>
    public void EndDrawing() => Raylib.EndDrawing();
    
    
    /// <summary> See <see cref="Raylib.BeginTextureMode"/> </summary>
    public void BeginTextureMode(RenderTexture2D target) => Raylib.BeginTextureMode(target);
    
    /// <summary> See <see cref="Raylib.EndTextureMode"/> </summary>
    public void EndTextureMode() => Raylib.EndTextureMode();
    
    
    /// <summary> See <see cref="Raylib.BeginShaderMode"/> </summary>
    public void BeginShaderMode(Shader shader) => Raylib.BeginShaderMode(shader);
    
    /// <summary> See <see cref="Raylib.EndShaderMode"/> </summary>
    public void EndShaderMode() => Raylib.EndShaderMode();
    
    
    /// <summary> See <see cref="Raylib.BeginBlendMode"/> </summary>
    public void BeginBlendMode(BlendMode mode) => Raylib.BeginBlendMode(mode);
    
    /// <summary> See <see cref="Raylib.EndBlendMode"/> </summary>
    public void EndBlendMode() => Raylib.EndBlendMode();
    
    
    /// <summary> See <see cref="Raylib.BeginScissorMode"/> </summary>
    public void BeginScissorMode(int x, int y, int width, int height) => Raylib.BeginScissorMode(x, y, width, height);
    
    /// <summary> See <see cref="Raylib.EndScissorMode"/> </summary>
    public void EndScissorMode() => Raylib.EndScissorMode();
}