using Raylib_cs;

namespace Sparkle.csharp.graphics; 

public class Graphics {
    
    // TODO Create for 2D a own Camera Entity! (Method already removed here)

    public void ClearBackground(Color color) => Raylib.ClearBackground(color);
    
    public void BeginDrawing() => Raylib.BeginDrawing();
    public void EndDrawing() => Raylib.EndDrawing();
    
    public void BeginTextureMode(RenderTexture2D target) => Raylib.BeginTextureMode(target);
    public void EndTextureMode() => Raylib.EndTextureMode();
    
    public void BeginShaderMode(Shader shader) => Raylib.BeginShaderMode(shader);
    public void EndShaderMode() => Raylib.EndShaderMode();
    
    public void BeginBlendMode(BlendMode mode) => Raylib.BeginBlendMode(mode);
    public void EndBlendMode() => Raylib.EndBlendMode();
    
    public void BeginScissorMode(int x, int y, int width, int height) => Raylib.BeginScissorMode(x, y, width, height);
    public void EndScissorMode() => Raylib.EndScissorMode();
}