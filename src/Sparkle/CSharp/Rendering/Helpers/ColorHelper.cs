using System.Numerics;
using Raylib_cs;

namespace Sparkle.CSharp.Rendering.Helpers;

public class ColorHelper {
    
    /// <inheritdoc cref="Raylib.Fade"/>
    public static Color Fade(Color color, float alpha) => Raylib.Fade(color, alpha);
    
    /// <inheritdoc cref="Raylib.ColorToInt"/>
    public static int ToInt(Color color) => Raylib.ColorToInt(color);
    
    /// <inheritdoc cref="Raylib.ColorNormalize"/>
    public static Vector4 Normalize(Color color) => Raylib.ColorNormalize(color);
    
    /// <inheritdoc cref="Raylib.ColorFromNormalized"/>
    public static Color FromNormalized(Vector4 normalized) => Raylib.ColorFromNormalized(normalized);
    
    /// <inheritdoc cref="Raylib.ColorToHSV"/>
    public static Vector3 ToHSV(Color color) => Raylib.ColorToHSV(color);
    
    /// <inheritdoc cref="Raylib.ColorFromHSV"/>
    public static Color FromHSV(float hue, float saturation, float value) => Raylib.ColorFromHSV(hue, saturation, value);
    
    /// <inheritdoc cref="Raylib.ColorTint"/>
    public static Color Tint(Color color, Color tint) => Raylib.ColorTint(color, tint);
    
    /// <inheritdoc cref="Raylib.ColorBrightness"/>
    public static Color Brightness(Color color, float factor) => Raylib.ColorBrightness(color, factor);
    
    /// <inheritdoc cref="Raylib.ColorContrast"/>
    public static Color Contrast(Color color, float contrast) => Raylib.ColorContrast(color, contrast);
    
    /// <inheritdoc cref="Raylib.ColorAlpha"/>
    public static Color Alpha(Color color, float alpha) => Raylib.ColorAlpha(color, alpha);
    
    /// <inheritdoc cref="Raylib.ColorAlphaBlend"/>
    public static Color AlphaBlend(Color dst, Color src, Color tint) => Raylib.ColorAlphaBlend(dst, src, tint);
    
    /// <inheritdoc cref="Raylib.GetColor"/>
    public static Color Get(uint hexValue) => Raylib.GetColor(hexValue);
        
    /// <inheritdoc cref="Raylib.GetPixelDataSize"/>
    public static int GetPixelDataSize(int width, int height, PixelFormat format) => Raylib.GetPixelDataSize(width, height, format);
    
    /// <inheritdoc cref="Raylib.GetPixelColor"/>
    public static unsafe Color GetPixel(void *srcPtr, PixelFormat format) => Raylib.GetPixelColor(srcPtr, format);
    
    /// <inheritdoc cref="Raylib.SetPixelColor"/>
    public static unsafe void SetPixel(void *dstPtr, Color color, PixelFormat format) => Raylib.SetPixelColor(dstPtr, color, format);
}