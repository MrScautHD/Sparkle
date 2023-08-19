using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class TextureHelper {
    
    /// <summary> See <see cref="Raylib.LoadTexture(string)"/> </summary>
    public static Texture2D Load(string path) => Raylib.LoadTexture(path);
    
    /// <summary> See <see cref="Raylib.LoadTextureFromImage"/> </summary>
    public static Texture2D LoadFromImage(Image image) => Raylib.LoadTextureFromImage(image);
    
    /// <summary> See <see cref="Raylib.LoadTextureCubemap"/> </summary>
    public static Texture2D LoadCubemap(Image image, CubemapLayout layout) => Raylib.LoadTextureCubemap(image, layout);
    
    /// <summary> See <see cref="Raylib.LoadRenderTexture"/> </summary>
    public static RenderTexture2D LoadRenderTexture(int width, int height) => Raylib.LoadRenderTexture(width, height);
    
    /// <summary> See <see cref="Raylib.UnloadTexture"/> </summary>
    public static void Unload(Texture2D texture) => Raylib.UnloadTexture(texture);
    
    /// <summary> See <see cref="Raylib.UnloadRenderTexture"/> </summary>
    public static void UnloadRenderTexture(RenderTexture2D target) => Raylib.UnloadRenderTexture(target);
    
    
    /// <summary> See <see cref="Raylib.IsTextureReady"/> </summary>
    public static bool IsReady(Texture2D texture) => Raylib.IsTextureReady(texture);
    
    /// <summary> See <see cref="Raylib.IsRenderTextureReady"/> </summary>
    public static bool IsRenderTextureReady(RenderTexture2D target) => Raylib.IsRenderTextureReady(target);
    
    /// <summary> See <see cref="Raylib.UpdateTexture"/> </summary>
    public static void Update<T>(Texture2D texture, ReadOnlySpan<T> pixels) where T : unmanaged => Raylib.UpdateTexture(texture, pixels);
    
    /// <summary> See <see cref="Raylib.UpdateTexture"/> </summary>
    public static void Update<T>(Texture2D texture, T[] pixels) where T : unmanaged => Raylib.UpdateTexture(texture, pixels);
    
    /// <summary> See <see cref="Raylib.UpdateTextureRec"/> </summary>
    public static void UpdateRec<T>(Texture2D texture, Rectangle rec, ReadOnlySpan<T> pixels) where T : unmanaged => Raylib.UpdateTextureRec(texture, rec, pixels);
    
    /// <summary> See <see cref="Raylib.UpdateTextureRec"/> </summary>
    public static void UpdateRec<T>(Texture2D texture, Rectangle rec, T[] pixels) where T : unmanaged => Raylib.UpdateTextureRec(texture, rec, pixels);

    
    /// <summary> See <see cref="Raylib.GenTextureMipmaps(ref Texture2D)"/> </summary>
    public static void GenMipmaps(ref Texture2D texture) => Raylib.GenTextureMipmaps(ref texture);
    
    /// <summary> See <see cref="Raylib.SetTextureFilter"/> </summary>
    public static void SetFilter(Texture2D texture, TextureFilter filter) => Raylib.SetTextureFilter(texture, filter);
    
    /// <summary> See <see cref="Raylib.SetTextureWrap"/> </summary>
    public static void SetWrap(Texture2D texture, TextureWrap wrap) => Raylib.SetTextureWrap(texture, wrap);

    
    /// <summary> See <see cref="Raylib.DrawTexture"/> </summary>
    public static void Draw(Texture2D texture, int posX, int posY, Color color) => Raylib.DrawTexture(texture, posX, posY, color);
    
    /// <summary> See <see cref="Raylib.DrawTextureV"/> </summary>
    public static void Draw(Texture2D texture, Vector2 pos, Color color) => Raylib.DrawTextureV(texture, pos, color);
    
    /// <summary> See <see cref="Raylib.DrawTextureEx"/> </summary>
    public static void Draw(Texture2D texture, Vector2 pos, float rotation, float scale, Color color) => Raylib.DrawTextureEx(texture, pos, rotation, scale, color);
    
    /// <summary> See <see cref="Raylib.DrawTextureRec"/> </summary>
    public static void DrawRec(Texture2D texture, Rectangle source, Vector2 pos, Color color) => Raylib.DrawTextureRec(texture, source, pos, color);
    
    /// <summary> See <see cref="Raylib.DrawTexturePro"/> </summary>
    public static void DrawPro(Texture2D texture, Rectangle source, Rectangle dest, Vector2 origin, float rotation, Color color) => Raylib.DrawTexturePro(texture, source, dest, origin, rotation, color);
    
    /// <summary> See <see cref="Raylib.DrawTextureNPatch"/> </summary>
    public static void DrawNPatch(Texture2D texture, NPatchInfo nPatchInfo, Rectangle dest, Vector2 origin, float rotation, Color color) => Raylib.DrawTextureNPatch(texture, nPatchInfo, dest, origin, rotation, color);
}