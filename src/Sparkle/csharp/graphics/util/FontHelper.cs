using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class FontHelper {

    /// <inheritdoc cref="Raylib.GetFontDefault"/>
    public static Font GetDefault() => Raylib.GetFontDefault();
    
    /// <inheritdoc cref="Raylib.IsFontReady"/>
    public static bool IsReady(Font font) => Raylib.IsFontReady(font);
    
    
    /// <inheritdoc cref="Raylib.LoadFont(string)"/>
    public static Font Load(string path) => Raylib.LoadFont(path);
    
    /// <inheritdoc cref="Raylib.LoadFontFromImage"/>
    public static Font LoadFromImage(Image image, Color key, int firstChar) => Raylib.LoadFontFromImage(image, key, firstChar);
    
    /// <inheritdoc cref="Raylib.LoadFontFromMemory(string, byte[], int, int[], int)"/>
    public static Font LoadFromMemory(string fileType, byte[] fileData, int fontSize, int[] fontChars, int glyphCount) => Raylib.LoadFontFromMemory(fileType, fileData, fontSize, fontChars, glyphCount);
    
    /// <inheritdoc cref="Raylib.UnloadFont"/>
    public static void Unload(Font font) => Raylib.UnloadFont(font);
    
    
    /// <inheritdoc cref="Raylib.DrawFPS"/>
    public static void DrawFps(int x, int y) => Raylib.DrawFPS(x, y);
    
    /// <inheritdoc cref="Raylib.DrawText(string, int, int, int, Color)"/>
    public static void DrawText(string text, int posX, int posY, int fontSize, Color color) => Raylib.DrawText(text, posX, posY, fontSize, color);
    
    /// <inheritdoc cref="Raylib.DrawTextEx(Font, string, Vector2, float, float, Color)"/>
    public static void DrawText(Font font, string text, Vector2 pos, float fontSize, float spacing, Color color) => Raylib.DrawTextEx(font, text, pos, fontSize, spacing, color);
    
    /// <inheritdoc cref="Raylib.DrawTextPro(Font, string, Vector2, Vector2, float, float, float, Color)"/>
    public static void DrawText(Font font, string text, Vector2 pos, Vector2 origin, float rotation, float fontSize, float spacing, Color color) => Raylib.DrawTextPro(font, text, pos, origin, rotation, fontSize, spacing, color);
    
    /// <inheritdoc cref="Raylib.DrawTextCodepoint"/>
    public static void DrawTextCodepoint(Font font, int codepoint, Vector2 pos, float fontSize, Color color) => Raylib.DrawTextCodepoint(font, codepoint, pos, fontSize, color);
    
    
    /// <inheritdoc cref="Raylib.MeasureText(string, int)"/>
    public static int MeasureText(string text, int fontSize) => Raylib.MeasureText(text, fontSize);
    
    /// <inheritdoc cref="Raylib.MeasureTextEx(Font, string, float, float)"/>
    public static Vector2 MeasureText(Font font, string text, float fontSize, float spacing) => Raylib.MeasureTextEx(font, text, fontSize, spacing);

    
    /// <inheritdoc cref="Raylib.GetGlyphIndex"/>
    public static int GetGlyphIndex(Font font, int character) => Raylib.GetGlyphIndex(font, character);
    
    /// <inheritdoc cref="Raylib.GetGlyphInfo"/>
    public static GlyphInfo GetGlyphInfo(Font font, int character) => Raylib.GetGlyphInfo(font, character);
    
    /// <inheritdoc cref="Raylib.GetGlyphAtlasRec"/>
    public static Rectangle GetGlyphAtlasRec(Font font, int character) => Raylib.GetGlyphAtlasRec(font, character);
}