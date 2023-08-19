using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class FontHelper {

    /// <summary> See <see cref="Raylib.GetFontDefault"/> </summary>
    public static Font GetDefault() => Raylib.GetFontDefault();
    
    /// <summary> See <see cref="Raylib.IsFontReady"/> </summary>
    public static bool IsReady(Font font) => Raylib.IsFontReady(font);
    
    
    /// <summary> See <see cref="Raylib.LoadFont(string)"/> </summary>
    public static Font Load(string path) => Raylib.LoadFont(path);
    
    /// <summary> See <see cref="Raylib.LoadFontFromImage"/> </summary>
    public static Font LoadFromImage(Image image, Color key, int firstChar) => Raylib.LoadFontFromImage(image, key, firstChar);
    
    /// <summary> See <see cref="Raylib.LoadFontFromMemory(string, byte[], int, int[], int)"/> </summary>
    public static Font LoadFromMemory(string fileType, byte[] fileData, int fontSize, int[] fontChars, int glyphCount) => Raylib.LoadFontFromMemory(fileType, fileData, fontSize, fontChars, glyphCount);
    
    /// <summary> See <see cref="Raylib.UnloadFont"/> </summary>
    public static void Unload(Font font) => Raylib.UnloadFont(font);
    
    
    /// <summary> See <see cref="Raylib.DrawFPS"/> </summary>
    public static void DrawFps(int x, int y) => Raylib.DrawFPS(x, y);
    
    /// <summary> See <see cref="Raylib.DrawText(string, int, int, int, Color)"/> </summary>
    public static void DrawText(string text, int posX, int posY, int fontSize, Color color) => Raylib.DrawText(text, posX, posY, fontSize, color);
    
    /// <summary> See <see cref="Raylib.DrawTextEx(Font, string, Vector2, float, float, Color)"/> </summary>
    public static void DrawText(Font font, string text, Vector2 pos, float fontSize, float spacing, Color color) => Raylib.DrawTextEx(font, text, pos, fontSize, spacing, color);
    
    /// <summary> See <see cref="Raylib.DrawTextPro(Font, string, Vector2, Vector2, float, float, float, Color)"/> </summary>
    public static void DrawText(Font font, string text, Vector2 pos, Vector2 origin, float rotation, float fontSize, float spacing, Color color) => Raylib.DrawTextPro(font, text, pos, origin, rotation, fontSize, spacing, color);
    
    /// <summary> See <see cref="Raylib.DrawTextCodepoint"/> </summary>
    public static void DrawTextCodepoint(Font font, int codepoint, Vector2 pos, float fontSize, Color color) => Raylib.DrawTextCodepoint(font, codepoint, pos, fontSize, color);
    
    
    /// <summary> See <see cref="Raylib.MeasureText(string, int)"/> </summary>
    public static int MeasureText(string text, int fontSize) => Raylib.MeasureText(text, fontSize);
    
    /// <summary> See <see cref="Raylib.MeasureTextEx(Font, string, float, float)"/> </summary>
    public static Vector2 MeasureText(Font font, string text, float fontSize, float spacing) => Raylib.MeasureTextEx(font, text, fontSize, spacing);

    
    /// <summary> See <see cref="Raylib.GetGlyphIndex"/> </summary>
    public static int GetGlyphIndex(Font font, int character) => Raylib.GetGlyphIndex(font, character);
    
    /// <summary> See <see cref="Raylib.GetGlyphInfo"/> </summary>
    public static GlyphInfo GetGlyphInfo(Font font, int character) => Raylib.GetGlyphInfo(font, character);
    
    /// <summary> See <see cref="Raylib.GetGlyphAtlasRec"/> </summary>
    public static Rectangle GetGlyphAtlasRec(Font font, int character) => Raylib.GetGlyphAtlasRec(font, character);
}