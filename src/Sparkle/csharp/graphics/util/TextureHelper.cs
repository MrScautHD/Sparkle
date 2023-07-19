using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class TextureHelper {
    
    public static Texture2D Load(string path) => Raylib.LoadTexture(path);
    public static Texture2D LoadFromImage(Image image) => Raylib.LoadTextureFromImage(image);
    public static Texture2D LoadCubemap(Image image, CubemapLayout layout) => Raylib.LoadTextureCubemap(image, layout);
    public static RenderTexture2D LoadRenderTexture(int width, int height) => Raylib.LoadRenderTexture(width, height);
    public static void Unload(Texture2D texture) => Raylib.UnloadTexture(texture);
    public static void UnloadRenderTexture(RenderTexture2D target) => Raylib.UnloadRenderTexture(target);

    public static bool IsReady(Texture2D texture) => Raylib.IsTextureReady(texture);
    public static bool IsRenderTextureReady(RenderTexture2D target) => Raylib.IsRenderTextureReady(target);
    public static unsafe void Update(Texture2D texture, void* pixels) => Raylib.UpdateTexture(texture, pixels);
    public static unsafe void UpdateRec(Texture2D texture, Rectangle rec, void* pixels) => Raylib.UpdateTextureRec(texture, rec, pixels);
    
    public static void GenMipmaps(ref Texture2D texture) => Raylib.GenTextureMipmaps(ref texture);
    public static void SetFilter(Texture2D texture, TextureFilter filter) => Raylib.SetTextureFilter(texture, filter);
    public static void SetWrap(Texture2D texture, TextureWrap wrap) => Raylib.SetTextureWrap(texture, wrap);

    public static void Draw(Texture2D texture, int posX, int posY, Color color) => Raylib.DrawTexture(texture, posX, posY, color);
    public static void Draw(Texture2D texture, Vector2 position, Color color) => Raylib.DrawTextureV(texture, position, color);
    public static void Draw(Texture2D texture, Vector2 position, float rotation, float scale, Color color) => Raylib.DrawTextureEx(texture, position, rotation, scale, color);
    public static void DrawRec(Texture2D texture, Rectangle source, Vector2 position, Color color) => Raylib.DrawTextureRec(texture, source, position, color);
    public static void DrawPro(Texture2D texture, Rectangle source, Rectangle dest, Vector2 origin, float rotation, Color color) => Raylib.DrawTexturePro(texture, source, dest, origin, rotation, color);
    public static void DrawNPatch(Texture2D texture, NPatchInfo nPatchInfo, Rectangle dest, Vector2 origin, float rotation, Color color) => Raylib.DrawTextureNPatch(texture, nPatchInfo, dest, origin, rotation, color);
}