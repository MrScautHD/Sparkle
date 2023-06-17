using Raylib_cs;

namespace Sparkle.csharp.graphics; 

public class Graphics {

    public ShapeRenderer ShapeRenderer { get; private set; }

    public Graphics() {
        this.ShapeRenderer = new ShapeRenderer();
    }
    
    public void ClearBackground(Color color) => Raylib.ClearBackground(color);
    
    public void BeginDrawing() => Raylib.BeginDrawing();
    public void EndDrawing() => Raylib.EndDrawing();
    
    public void BeginMode2D(Camera2D camera2D) => Raylib.BeginMode2D(camera2D);
    public void EndMode2D() => Raylib.EndMode2D();
    
    public void BeginMode3D(Camera3D camera3D) => Raylib.BeginMode3D(camera3D);
    public void EndMode3D() => Raylib.EndMode3D();
    
    public void BeginTextureMode(RenderTexture2D target) => Raylib.BeginTextureMode(target);
    public void EndTextureMode() => Raylib.EndTextureMode();
    
    public void BeginShaderMode(Shader shader) => Raylib.BeginShaderMode(shader);
    public void EndShaderMode() => Raylib.EndShaderMode();
    
    public void BeginBlendMode(BlendMode mode) => Raylib.BeginBlendMode(mode);
    public void EndBlendMode() => Raylib.EndBlendMode();
    
    public void BeginScissorMode(int x, int y, int width, int height) => Raylib.BeginScissorMode(x, y, width, height);
    public void EndScissorMode() => Raylib.EndScissorMode();
}