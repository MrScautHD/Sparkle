using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class ImageHelper {
    
    /// <summary> See <see cref="Raylib.LoadImage(string)"/> </summary>
    public static Image Load(string path) => Raylib.LoadImage(path);
    
    /// <summary> See <see cref="Raylib.LoadImageRaw(string, int, int, PixelFormat, int)"/> </summary>
    public static Image Load(string path, int width, int height, PixelFormat format, int headerSize) => Raylib.LoadImageRaw(path, width, height, format, headerSize);
    
    /// <summary> See <see cref="Raylib.LoadImageAnim(string, out int)"/> </summary>
    public static Image Load(string path, out int frames) => Raylib.LoadImageAnim(path, out frames);
    
    /// <summary> See <see cref="Raylib.LoadImageFromMemory(string, byte[])"/> </summary>
    public static Image Load(string fileType, byte[] fileData) => Raylib.LoadImageFromMemory(fileType, fileData);
    
    /// <summary> See <see cref="Raylib.LoadImageFromTexture"/> </summary>
    public static Image Load(Texture2D texture) => Raylib.LoadImageFromTexture(texture);
    
    /// <summary> See <see cref="Raylib.LoadImageFromScreen"/> </summary>
    public static Image LoadFromScreen() => Raylib.LoadImageFromScreen();
    
    /// <summary> See <see cref="Raylib.UnloadImage"/> </summary>
    public static void Unload(Image image) => Raylib.UnloadImage(image);
    
    
    /// <summary> See <see cref="Raylib.IsImageReady"/> </summary>
    public static bool IsReady(Image image) => Raylib.IsImageReady(image);
    
    /// <summary> See <see cref="Raylib.ExportImage(Image, string)"/> </summary>
    public static bool Export(Image image, string path) => Raylib.ExportImage(image, path);
    
    /// <summary> See <see cref="Raylib.ExportImageAsCode(Image, string)"/> </summary>
    public static bool ExportAsCode(Image image, string path) => Raylib.ExportImageAsCode(image, path);
    
    
    /// <summary> See <see cref="Raylib.LoadImageColors"/> </summary>
    public static unsafe Color* LoadColors(Image image) => Raylib.LoadImageColors(image);
    
    /// <summary> See <see cref="Raylib.LoadImagePalette"/> </summary>
    public static unsafe Color* LoadPalette(Image image, int maxPaletteSize, int* colorCount) => Raylib.LoadImagePalette(image, maxPaletteSize, colorCount);
    
    /// <summary> See <see cref="Raylib.UnloadImageColors"/> </summary>
    public static unsafe void UnloadColors(Color* color) => Raylib.UnloadImageColors(color);
    
    /// <summary> See <see cref="Raylib.UnloadImagePalette"/> </summary>
    public static unsafe void UnloadPalette(Color* color) => Raylib.UnloadImagePalette(color);

    
    /// <summary> See <see cref="Raylib.GenImageColor"/> </summary>
    public static Image GenColor(int width, int height, Color color) => Raylib.GenImageColor(width, height, color);
    
    /// <summary> See <see cref="Raylib.GenImageGradientH"/> </summary>
    public static Image GenGradientH(int width, int height, Color left, Color right) => Raylib.GenImageGradientH(width, height, left, right);
    
    /// <summary> See <see cref="Raylib.GenImageGradientV"/> </summary>
    public static Image GenGradientV(int width, int height, Color top, Color bottom) => Raylib.GenImageGradientV(width, height, top, bottom);
    
    /// <summary> See <see cref="Raylib.GenImageGradientRadial"/> </summary>
    public static Image GenGradientRadial(int width, int height, float density, Color inner, Color outer) => Raylib.GenImageGradientRadial(width, height, density, inner, outer);
    
    /// <summary> See <see cref="Raylib.GenImageChecked"/> </summary>
    public static Image GenChecked(int width, int height, int checksX, int checksY, Color col1, Color col2) => Raylib.GenImageChecked(width, height, checksX, checksY, col1, col2);
    
    /// <summary> See <see cref="Raylib.GenImageWhiteNoise"/> </summary>
    public static Image GenWhiteNoise(int width, int height, float factor) => Raylib.GenImageWhiteNoise(width, height, factor);
    
    /// <summary> See <see cref="Raylib.GenImagePerlinNoise"/> </summary>
    public static Image GenPerlinNoise(int width, int height, int offsetX, int offsetY, float scale) => Raylib.GenImagePerlinNoise(width, height, offsetX, offsetY, scale);
    
    /// <summary> See <see cref="Raylib.GenImageCellular"/> </summary>
    public static Image GenCellular(int width, int height, int tileSize) => Raylib.GenImageCellular(width, height, tileSize);
    
    /// <summary> See <see cref="Raylib.GenImageText"/> </summary>
    public static Image GenText(int width, int height, int tileSize) => Raylib.GenImageText(width, height, tileSize);
    
    
    /// <summary> See <see cref="Raylib.ImageCopy"/> </summary>
    public static Image Copy(Image image) => Raylib.ImageCopy(image);
    
    /// <summary> See <see cref="Raylib.ImageFromImage"/> </summary>
    public static Image FromImage(Image image, Rectangle rec) => Raylib.ImageFromImage(image, rec);
    
    /// <summary> See <see cref="Raylib.ImageText(string, int, Color)"/> </summary>
    public static Image Text(string text, int fontSize, Color color) => Raylib.ImageText(text, fontSize, color);
    
    /// <summary> See <see cref="Raylib.ImageTextEx(Font, string, float, float, Color)"/> </summary>
    public static Image Text(Font font, string text, float fontSize, float spacing, Color color) => Raylib.ImageTextEx(font, text, fontSize, spacing, color);
    
    /// <summary> See <see cref="Raylib.ImageFormat(ref Image, PixelFormat)"/> </summary>
    public static void Format(ref Image image, PixelFormat newFormat) => Raylib.ImageFormat(ref image, newFormat);
    
    /// <summary> See <see cref="Raylib.ImageToPOT(ref Image, Color)"/> </summary>
    public static void ToPOT(ref Image image, Color fill) => Raylib.ImageToPOT(ref image, fill);
    
    /// <summary> See <see cref="Raylib.ImageCrop(ref Image, Rectangle)"/> </summary>
    public static void Crop(ref Image image, Rectangle crop) => Raylib.ImageCrop(ref image, crop);
    
    /// <summary> See <see cref="Raylib.ImageAlphaCrop(ref Image, float)"/> </summary>
    public static void AlphaCrop(ref Image image, float threshold) => Raylib.ImageAlphaCrop(ref image, threshold);
    
    /// <summary> See <see cref="Raylib.ImageAlphaClear(ref Image, Color, float)"/> </summary>
    public static void AlphaClear(ref Image image, Color color, float threshold) => Raylib.ImageAlphaClear(ref image, color, threshold);
    
    /// <summary> See <see cref="Raylib.ImageAlphaMask(ref Image, Image)"/> </summary>
    public static void AlphaMask(ref Image image, Image alphaMask) => Raylib.ImageAlphaMask(ref image, alphaMask);
    
    /// <summary> See <see cref="Raylib.ImageAlphaPremultiply(ref Image)"/> </summary>
    public static void AlphaPremultiply(ref Image image) => Raylib.ImageAlphaPremultiply(ref image);
    
    /// <summary> See <see cref="Raylib.ImageBlurGaussian(ref Image, int)"/> </summary>
    public static void BlurGaussian(ref Image image, int blurSize) => Raylib.ImageBlurGaussian(ref image, blurSize);
    
    /// <summary> See <see cref="Raylib.ImageResize(ref Image, int, int)"/> </summary>
    public static void Resize(ref Image image, int newWidth, int newHeight) => Raylib.ImageResize(ref image, newWidth, newHeight);
    
    /// <summary> See <see cref="Raylib.ImageResizeNN(ref Image, int, int)"/> </summary>
    public static void ResizeNN(ref Image image, int newWidth, int newHeight) => Raylib.ImageResizeNN(ref image, newWidth, newHeight);
    
    /// <summary> See <see cref="Raylib.ImageResizeCanvas(ref Image, int, int, int, int, Color)"/> </summary>
    public static void ResizeCanvas(ref Image image, int newWidth, int newHeight, int offsetX, int offsetY, Color color) => Raylib.ImageResizeCanvas(ref image, newWidth, newHeight, offsetX, offsetY, color);
    
    /// <summary> See <see cref="Raylib.ImageMipmaps(ref Image)"/> </summary>
    public static void Mipmaps(ref Image image) => Raylib.ImageMipmaps(ref image);
    
    /// <summary> See <see cref="Raylib.ImageDither(ref Image, int, int, int, int)"/> </summary>
    public static void Dither(ref Image image, int rBpp, int gBpp, int bBpp, int aBpp) => Raylib.ImageDither(ref image, rBpp, gBpp, bBpp, aBpp);
    
    /// <summary> See <see cref="Raylib.ImageFlipVertical(ref Image)"/> </summary>
    public static void FlipVertical(ref Image image) => Raylib.ImageFlipVertical(ref image);
    
    /// <summary> See <see cref="Raylib.ImageFlipHorizontal(ref Image)"/> </summary>
    public static void FlipHorizontal(ref Image image) => Raylib.ImageFlipHorizontal(ref image);
    
    /// <summary> See <see cref="Raylib.ImageRotateCW(ref Image)"/> </summary>
    public static void RotateCW(ref Image image) => Raylib.ImageRotateCW(ref image);
    
    /// <summary> See <see cref="Raylib.ImageRotateCCW(ref Image)"/> </summary>
    public static void RotateCCW(ref Image image) => Raylib.ImageRotateCCW(ref image);
    
    /// <summary> See <see cref="Raylib.ImageColorTint(ref Image, Color)"/> </summary>
    public static void ColorTint(ref Image image, Color color) => Raylib.ImageColorTint(ref image, color);
    
    /// <summary> See <see cref="Raylib.ImageColorInvert(ref Image)"/> </summary>
    public static void ColorInvert(ref Image image) => Raylib.ImageColorInvert(ref image);
    
    /// <summary> See <see cref="Raylib.ImageColorGrayscale(ref Image)"/> </summary>
    public static void ColorGrayscale(ref Image image) => Raylib.ImageColorGrayscale(ref image);
    
    /// <summary> See <see cref="Raylib.ImageColorContrast(ref Image, float)"/> </summary>
    public static void ColorContrast(ref Image image, float contrast) => Raylib.ImageColorContrast(ref image, contrast);
    
    /// <summary> See <see cref="Raylib.ImageColorBrightness(ref Image, int)"/> </summary>
    public static void ColorBrightness(ref Image image, int brightness) => Raylib.ImageColorBrightness(ref image, brightness);
    
    /// <summary> See <see cref="Raylib.ImageColorReplace(ref Image, Color, Color)"/> </summary>
    public static void ColorReplace(ref Image image, Color color, Color replace) => Raylib.ImageColorReplace(ref image, color, replace);
    
    /// <summary> See <see cref="Raylib.GetImageAlphaBorder(Image, float)"/> </summary>
    public static Rectangle GetAlphaBorder(Image image, float threshold) => Raylib.GetImageAlphaBorder(image, threshold);
    
    /// <summary> See <see cref="Raylib.GetImageColor(Image, int, int)"/> </summary>
    public static Color GetColor(Image image, int x, int y) => Raylib.GetImageColor(image, x, y);
    
    
    /// <summary> See <see cref="Raylib.ImageClearBackground(ref Image, Color)"/> </summary>
    public static void ClearBackground(ref Image dst, Color color) => Raylib.ImageClearBackground(ref dst, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawPixel(ref Image, int, int, Color)"/> </summary>
    public static void DrawPixel(ref Image dst, int posX, int posY, Color color) => Raylib.ImageDrawPixel(ref dst, posX, posY, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawPixelV(ref Image, Vector2, Color)"/> </summary>
    public static void DrawPixel(ref Image dst, Vector2 pos, Color color) => Raylib.ImageDrawPixelV(ref dst, pos, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawLine(ref Image, int, int, int, int, Color)"/> </summary>
    public static void DrawLine(ref Image dst, int startPosX, int startPosY, int endPosX, int endPosY, Color color) => Raylib.ImageDrawLine(ref dst, startPosX, startPosY, endPosX, endPosY, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawLineV(ref Image, Vector2, Vector2, Color)"/> </summary>
    public static void DrawLine(ref Image dst, Vector2 start, Vector2 end, Color color) => Raylib.ImageDrawLineV(ref dst, start, end, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawCircle(ref Image, int, int, int, Color)"/> </summary>
    public static void DrawCircle(ref Image dst, int centerX, int centerY, int radius, Color color) => Raylib.ImageDrawCircle(ref dst, centerX, centerY, radius, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawCircleV(ref Image, Vector2, int, Color)"/> </summary>
    public static void DrawCircle(ref Image dst, Vector2 center, int radius, Color color) => Raylib.ImageDrawCircleV(ref dst, center, radius, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawCircleLines"/> </summary>
    public static unsafe void DrawCircleLines(Image* dst, int centerX, int centerY, int radius, Color color) => Raylib.ImageDrawCircleLines(dst, centerX, centerY, radius, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawCircleLinesV"/> </summary>
    public static unsafe void DrawCircleLines(Image* dst, Vector2 center, int radius, Color color) => Raylib.ImageDrawCircleLinesV(dst, center, radius, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawRectangle(ref Image, int, int, int, int, Color)"/> </summary>
    public static void DrawRectangle(ref Image dst, int posX, int posY, int width, int height, Color color) => Raylib.ImageDrawRectangle(ref dst, posX, posY, width, height, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawRectangleV(ref Image, Vector2, Vector2, Color)"/> </summary>
    public static void DrawRectangle(ref Image dst, Vector2 pos, Vector2 size, Color color) => Raylib.ImageDrawRectangleV(ref dst, pos, size, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawRectangleRec(ref Image, Rectangle, Color)"/> </summary>
    public static void DrawRectangleRec(ref Image dst, Rectangle rec, Color color) => Raylib.ImageDrawRectangleRec(ref dst, rec, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawRectangleLines(ref Image, Rectangle, int, Color)"/> </summary>
    public static void DrawRectangleLines(ref Image dst, Rectangle rec, int thick, Color color) => Raylib.ImageDrawRectangleLines(ref dst, rec, thick, color);
    
    /// <summary> See <see cref="Raylib.ImageDraw(ref Image, Image, Rectangle, Rectangle, Color)"/> </summary>
    public static void Draw(ref Image dst, Image src, Rectangle srcRec, Rectangle dstRec, Color color) => Raylib.ImageDraw(ref dst, src, srcRec, dstRec, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawText(ref Image, string, int, int, int, Color)"/> </summary>
    public static void DrawText(ref Image dst, string text, int x, int y, int fontSize, Color color) => Raylib.ImageDrawText(ref dst, text, x, y, fontSize, color);
    
    /// <summary> See <see cref="Raylib.ImageDrawTextEx(ref Image, Font, string, Vector2, int, float, Color)"/> </summary>
    public static void DrawText(ref Image dst, Font font, string text, Vector2 pos, int fontSize, float spacing, Color color) => Raylib.ImageDrawTextEx(ref dst, font, text, pos, fontSize, spacing, color);
}