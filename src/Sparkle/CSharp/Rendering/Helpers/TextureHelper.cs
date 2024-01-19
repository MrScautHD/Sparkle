using System.Numerics;
using Raylib_cs;

namespace Sparkle.CSharp.Rendering.Helpers; 

public static class TextureHelper {
    
    /// <inheritdoc cref="Raylib.LoadTexture(string)"/>
    public static Texture2D Load(string path) => Raylib.LoadTexture(path);
    
    /// <inheritdoc cref="Raylib.LoadTextureFromImage"/>
    public static Texture2D LoadFromImage(Image image) => Raylib.LoadTextureFromImage(image);
    
    /// <inheritdoc cref="Raylib.LoadTextureCubemap"/>
    public static Texture2D LoadCubemap(Image image, CubemapLayout layout) => Raylib.LoadTextureCubemap(image, layout);
    
    /// <inheritdoc cref="Raylib.LoadRenderTexture"/>
    public static RenderTexture2D LoadRenderTexture(int width, int height) => Raylib.LoadRenderTexture(width, height);
    
    /// <inheritdoc cref="Raylib.UnloadTexture"/>
    public static void Unload(Texture2D texture) => Raylib.UnloadTexture(texture);
    
    /// <inheritdoc cref="Raylib.UnloadRenderTexture"/>
    public static void UnloadRenderTexture(RenderTexture2D target) => Raylib.UnloadRenderTexture(target);
    
    
    /// <inheritdoc cref="Raylib.IsTextureReady"/>
    public static bool IsReady(Texture2D texture) => Raylib.IsTextureReady(texture);
    
    /// <inheritdoc cref="Raylib.IsRenderTextureReady"/>
    public static bool IsRenderTextureReady(RenderTexture2D target) => Raylib.IsRenderTextureReady(target);
    
    /// <inheritdoc cref="Raylib.UpdateTexture"/>
    public static void Update<T>(Texture2D texture, ReadOnlySpan<T> pixels) where T : unmanaged => Raylib.UpdateTexture(texture, pixels);
    
    /// <inheritdoc cref="Raylib.UpdateTexture"/>
    public static void Update<T>(Texture2D texture, T[] pixels) where T : unmanaged => Raylib.UpdateTexture(texture, pixels);
    
    /// <inheritdoc cref="Raylib.UpdateTextureRec"/>
    public static void UpdateRec<T>(Texture2D texture, Rectangle rec, ReadOnlySpan<T> pixels) where T : unmanaged => Raylib.UpdateTextureRec(texture, rec, pixels);
    
    /// <inheritdoc cref="Raylib.UpdateTextureRec"/>
    public static void UpdateRec<T>(Texture2D texture, Rectangle rec, T[] pixels) where T : unmanaged => Raylib.UpdateTextureRec(texture, rec, pixels);

    
    /// <inheritdoc cref="Raylib.GenTextureMipmaps(ref Texture2D)"/>
    public static void GenMipmaps(ref Texture2D texture) => Raylib.GenTextureMipmaps(ref texture);
    
    /// <inheritdoc cref="Raylib.SetTextureFilter"/>
    public static void SetFilter(Texture2D texture, TextureFilter filter) => Raylib.SetTextureFilter(texture, filter);
    
    /// <inheritdoc cref="Raylib.SetTextureWrap"/>
    public static void SetWrap(Texture2D texture, TextureWrap wrap) => Raylib.SetTextureWrap(texture, wrap);

    
    /// <inheritdoc cref="Raylib.DrawTexture"/>
    public static void Draw(Texture2D texture, int posX, int posY, Color color) => Raylib.DrawTexture(texture, posX, posY, color);
    
    /// <inheritdoc cref="Raylib.DrawTextureV"/>
    public static void Draw(Texture2D texture, Vector2 pos, Color color) => Raylib.DrawTextureV(texture, pos, color);
    
    /// <inheritdoc cref="Raylib.DrawTextureEx"/>
    public static void Draw(Texture2D texture, Vector2 pos, float rotation, float scale, Color color) => Raylib.DrawTextureEx(texture, pos, rotation, scale, color);
    
    /// <inheritdoc cref="Raylib.DrawTextureRec"/>
    public static void DrawRec(Texture2D texture, Rectangle source, Vector2 pos, Color color) => Raylib.DrawTextureRec(texture, source, pos, color);
    
    /// <inheritdoc cref="Raylib.DrawTexturePro"/>
    public static void DrawPro(Texture2D texture, Rectangle source, Rectangle dest, Vector2 origin, float rotation, Color color) => Raylib.DrawTexturePro(texture, source, dest, origin, rotation, color);
    
    /// <inheritdoc cref="Raylib.DrawTextureNPatch"/>
    public static void DrawNPatch(Texture2D texture, NPatchInfo nPatchInfo, Rectangle dest, Vector2 origin, float rotation, Color color) => Raylib.DrawTextureNPatch(texture, nPatchInfo, dest, origin, rotation, color);
}