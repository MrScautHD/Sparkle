using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

#if !HEADLESS
public static class ImageHelper {
    
    /// <inheritdoc cref="Raylib.LoadImage(string)"/>
    public static Image Load(string path) => Raylib.LoadImage(path);
    
    /// <inheritdoc cref="Raylib.LoadImageRaw(string, int, int, PixelFormat, int)"/>
    public static Image Load(string path, int width, int height, PixelFormat format, int headerSize) => Raylib.LoadImageRaw(path, width, height, format, headerSize);
    
    /// <inheritdoc cref="Raylib.LoadImageAnim(string, out int)"/>
    public static Image Load(string path, out int frames) => Raylib.LoadImageAnim(path, out frames);
    
    /// <inheritdoc cref="Raylib.LoadImageFromMemory(string, byte[])"/>
    public static Image Load(string fileType, byte[] fileData) => Raylib.LoadImageFromMemory(fileType, fileData);
    
    /// <inheritdoc cref="Raylib.LoadImageFromTexture"/>
    public static Image Load(Texture2D texture) => Raylib.LoadImageFromTexture(texture);
    
    /// <inheritdoc cref="Raylib.LoadImageFromScreen"/>
    public static Image LoadFromScreen() => Raylib.LoadImageFromScreen();
    
    /// <inheritdoc cref="Raylib.UnloadImage"/>
    public static void Unload(Image image) => Raylib.UnloadImage(image);
    
    
    /// <inheritdoc cref="Raylib.IsImageReady"/>
    public static bool IsReady(Image image) => Raylib.IsImageReady(image);
    
    /// <inheritdoc cref="Raylib.ExportImage(Image, string)"/>
    public static bool Export(Image image, string path) => Raylib.ExportImage(image, path);
    
    /// <inheritdoc cref="Raylib.ExportImageAsCode(Image, string)"/>
    public static bool ExportAsCode(Image image, string path) => Raylib.ExportImageAsCode(image, path);
    
    
    /// <inheritdoc cref="Raylib.LoadImageColors"/>
    public static unsafe Color* LoadColors(Image image) => Raylib.LoadImageColors(image);
    
    /// <inheritdoc cref="Raylib.LoadImagePalette"/>
    public static unsafe Color* LoadPalette(Image image, int maxPaletteSize, int* colorCount) => Raylib.LoadImagePalette(image, maxPaletteSize, colorCount);
    
    /// <inheritdoc cref="Raylib.UnloadImageColors"/>
    public static unsafe void UnloadColors(Color* color) => Raylib.UnloadImageColors(color);
    
    /// <inheritdoc cref="Raylib.UnloadImagePalette"/>
    public static unsafe void UnloadPalette(Color* color) => Raylib.UnloadImagePalette(color);

    
    /// <inheritdoc cref="Raylib.GenImageColor"/>
    public static Image GenColor(int width, int height, Color color) => Raylib.GenImageColor(width, height, color);
    
    /// <inheritdoc cref="Raylib.GenImageGradientH"/>
    public static Image GenGradientH(int width, int height, Color left, Color right) => Raylib.GenImageGradientH(width, height, left, right);
    
    /// <inheritdoc cref="Raylib.GenImageGradientV"/>
    public static Image GenGradientV(int width, int height, Color top, Color bottom) => Raylib.GenImageGradientV(width, height, top, bottom);
    
    /// <inheritdoc cref="Raylib.GenImageGradientRadial"/>
    public static Image GenGradientRadial(int width, int height, float density, Color inner, Color outer) => Raylib.GenImageGradientRadial(width, height, density, inner, outer);
    
    /// <inheritdoc cref="Raylib.GenImageChecked"/>
    public static Image GenChecked(int width, int height, int checksX, int checksY, Color col1, Color col2) => Raylib.GenImageChecked(width, height, checksX, checksY, col1, col2);
    
    /// <inheritdoc cref="Raylib.GenImageWhiteNoise"/>
    public static Image GenWhiteNoise(int width, int height, float factor) => Raylib.GenImageWhiteNoise(width, height, factor);
    
    /// <inheritdoc cref="Raylib.GenImagePerlinNoise"/>
    public static Image GenPerlinNoise(int width, int height, int offsetX, int offsetY, float scale) => Raylib.GenImagePerlinNoise(width, height, offsetX, offsetY, scale);
    
    /// <inheritdoc cref="Raylib.GenImageCellular"/>
    public static Image GenCellular(int width, int height, int tileSize) => Raylib.GenImageCellular(width, height, tileSize);
    
    /// <inheritdoc cref="Raylib.GenImageText"/>
    public static Image GenText(int width, int height, int tileSize) => Raylib.GenImageText(width, height, tileSize);
    
    
    /// <inheritdoc cref="Raylib.ImageCopy"/>
    public static Image Copy(Image image) => Raylib.ImageCopy(image);
    
    /// <inheritdoc cref="Raylib.ImageFromImage"/>
    public static Image FromImage(Image image, Rectangle rec) => Raylib.ImageFromImage(image, rec);
    
    /// <inheritdoc cref="Raylib.ImageText(string, int, Color)"/>
    public static Image Text(string text, int fontSize, Color color) => Raylib.ImageText(text, fontSize, color);
    
    /// <inheritdoc cref="Raylib.ImageTextEx(Font, string, float, float, Color)"/>
    public static Image Text(Font font, string text, float fontSize, float spacing, Color color) => Raylib.ImageTextEx(font, text, fontSize, spacing, color);
    
    /// <inheritdoc cref="Raylib.ImageFormat(ref Image, PixelFormat)"/>
    public static void Format(ref Image image, PixelFormat newFormat) => Raylib.ImageFormat(ref image, newFormat);
    
    /// <inheritdoc cref="Raylib.ImageToPOT(ref Image, Color)"/>
    public static void ToPOT(ref Image image, Color fill) => Raylib.ImageToPOT(ref image, fill);
    
    /// <inheritdoc cref="Raylib.ImageCrop(ref Image, Rectangle)"/>
    public static void Crop(ref Image image, Rectangle crop) => Raylib.ImageCrop(ref image, crop);
    
    /// <inheritdoc cref="Raylib.ImageAlphaCrop(ref Image, float)"/>
    public static void AlphaCrop(ref Image image, float threshold) => Raylib.ImageAlphaCrop(ref image, threshold);
    
    /// <inheritdoc cref="Raylib.ImageAlphaClear(ref Image, Color, float)"/>
    public static void AlphaClear(ref Image image, Color color, float threshold) => Raylib.ImageAlphaClear(ref image, color, threshold);
    
    /// <inheritdoc cref="Raylib.ImageAlphaMask(ref Image, Image)"/>
    public static void AlphaMask(ref Image image, Image alphaMask) => Raylib.ImageAlphaMask(ref image, alphaMask);
    
    /// <inheritdoc cref="Raylib.ImageAlphaPremultiply(ref Image)"/>
    public static void AlphaPremultiply(ref Image image) => Raylib.ImageAlphaPremultiply(ref image);
    
    /// <inheritdoc cref="Raylib.ImageBlurGaussian(ref Image, int)"/>
    public static void BlurGaussian(ref Image image, int blurSize) => Raylib.ImageBlurGaussian(ref image, blurSize);
    
    /// <inheritdoc cref="Raylib.ImageResize(ref Image, int, int)"/>
    public static void Resize(ref Image image, int newWidth, int newHeight) => Raylib.ImageResize(ref image, newWidth, newHeight);
    
    /// <inheritdoc cref="Raylib.ImageResizeNN(ref Image, int, int)"/>
    public static void ResizeNN(ref Image image, int newWidth, int newHeight) => Raylib.ImageResizeNN(ref image, newWidth, newHeight);
    
    /// <inheritdoc cref="Raylib.ImageResizeCanvas(ref Image, int, int, int, int, Color)"/>
    public static void ResizeCanvas(ref Image image, int newWidth, int newHeight, int offsetX, int offsetY, Color color) => Raylib.ImageResizeCanvas(ref image, newWidth, newHeight, offsetX, offsetY, color);
    
    /// <inheritdoc cref="Raylib.ImageMipmaps(ref Image)"/>
    public static void Mipmaps(ref Image image) => Raylib.ImageMipmaps(ref image);
    
    /// <inheritdoc cref="Raylib.ImageDither(ref Image, int, int, int, int)"/>
    public static void Dither(ref Image image, int rBpp, int gBpp, int bBpp, int aBpp) => Raylib.ImageDither(ref image, rBpp, gBpp, bBpp, aBpp);
    
    /// <inheritdoc cref="Raylib.ImageFlipVertical(ref Image)"/>
    public static void FlipVertical(ref Image image) => Raylib.ImageFlipVertical(ref image);
    
    /// <inheritdoc cref="Raylib.ImageFlipHorizontal(ref Image)"/>
    public static void FlipHorizontal(ref Image image) => Raylib.ImageFlipHorizontal(ref image);
    
    /// <inheritdoc cref="Raylib.ImageRotateCW(ref Image)"/>
    public static void RotateCW(ref Image image) => Raylib.ImageRotateCW(ref image);
    
    /// <inheritdoc cref="Raylib.ImageRotateCCW(ref Image)"/>
    public static void RotateCCW(ref Image image) => Raylib.ImageRotateCCW(ref image);
    
    /// <inheritdoc cref="Raylib.ImageColorTint(ref Image, Color)"/>
    public static void ColorTint(ref Image image, Color color) => Raylib.ImageColorTint(ref image, color);
    
    /// <inheritdoc cref="Raylib.ImageColorInvert(ref Image)"/>
    public static void ColorInvert(ref Image image) => Raylib.ImageColorInvert(ref image);
    
    /// <inheritdoc cref="Raylib.ImageColorGrayscale(ref Image)"/>
    public static void ColorGrayscale(ref Image image) => Raylib.ImageColorGrayscale(ref image);
    
    /// <inheritdoc cref="Raylib.ImageColorContrast(ref Image, float)"/>
    public static void ColorContrast(ref Image image, float contrast) => Raylib.ImageColorContrast(ref image, contrast);
    
    /// <inheritdoc cref="Raylib.ImageColorBrightness(ref Image, int)"/>
    public static void ColorBrightness(ref Image image, int brightness) => Raylib.ImageColorBrightness(ref image, brightness);
    
    /// <inheritdoc cref="Raylib.ImageColorReplace(ref Image, Color, Color)"/>
    public static void ColorReplace(ref Image image, Color color, Color replace) => Raylib.ImageColorReplace(ref image, color, replace);
    
    /// <inheritdoc cref="Raylib.GetImageAlphaBorder(Image, float)"/>
    public static Rectangle GetAlphaBorder(Image image, float threshold) => Raylib.GetImageAlphaBorder(image, threshold);
    
    /// <inheritdoc cref="Raylib.GetImageColor(Image, int, int)"/>
    public static Color GetColor(Image image, int x, int y) => Raylib.GetImageColor(image, x, y);
    
    
    /// <inheritdoc cref="Raylib.ImageClearBackground(ref Image, Color)"/>
    public static void ClearBackground(ref Image dst, Color color) => Raylib.ImageClearBackground(ref dst, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawPixel(ref Image, int, int, Color)"/>
    public static void DrawPixel(ref Image dst, int posX, int posY, Color color) => Raylib.ImageDrawPixel(ref dst, posX, posY, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawPixelV(ref Image, Vector2, Color)"/>
    public static void DrawPixel(ref Image dst, Vector2 pos, Color color) => Raylib.ImageDrawPixelV(ref dst, pos, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawLine(ref Image, int, int, int, int, Color)"/>
    public static void DrawLine(ref Image dst, int startPosX, int startPosY, int endPosX, int endPosY, Color color) => Raylib.ImageDrawLine(ref dst, startPosX, startPosY, endPosX, endPosY, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawLineV(ref Image, Vector2, Vector2, Color)"/>
    public static void DrawLine(ref Image dst, Vector2 start, Vector2 end, Color color) => Raylib.ImageDrawLineV(ref dst, start, end, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawCircle(ref Image, int, int, int, Color)"/>
    public static void DrawCircle(ref Image dst, int centerX, int centerY, int radius, Color color) => Raylib.ImageDrawCircle(ref dst, centerX, centerY, radius, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawCircleV(ref Image, Vector2, int, Color)"/>
    public static void DrawCircle(ref Image dst, Vector2 center, int radius, Color color) => Raylib.ImageDrawCircleV(ref dst, center, radius, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawCircleLines"/>
    public static unsafe void DrawCircleLines(Image* dst, int centerX, int centerY, int radius, Color color) => Raylib.ImageDrawCircleLines(dst, centerX, centerY, radius, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawCircleLinesV"/>
    public static unsafe void DrawCircleLines(Image* dst, Vector2 center, int radius, Color color) => Raylib.ImageDrawCircleLinesV(dst, center, radius, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawRectangle(ref Image, int, int, int, int, Color)"/>
    public static void DrawRectangle(ref Image dst, int posX, int posY, int width, int height, Color color) => Raylib.ImageDrawRectangle(ref dst, posX, posY, width, height, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawRectangleV(ref Image, Vector2, Vector2, Color)"/>
    public static void DrawRectangle(ref Image dst, Vector2 pos, Vector2 size, Color color) => Raylib.ImageDrawRectangleV(ref dst, pos, size, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawRectangleRec(ref Image, Rectangle, Color)"/>
    public static void DrawRectangleRec(ref Image dst, Rectangle rec, Color color) => Raylib.ImageDrawRectangleRec(ref dst, rec, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawRectangleLines(ref Image, Rectangle, int, Color)"/>
    public static void DrawRectangleLines(ref Image dst, Rectangle rec, int thick, Color color) => Raylib.ImageDrawRectangleLines(ref dst, rec, thick, color);
    
    /// <inheritdoc cref="Raylib.ImageDraw(ref Image, Image, Rectangle, Rectangle, Color)"/>
    public static void Draw(ref Image dst, Image src, Rectangle srcRec, Rectangle dstRec, Color color) => Raylib.ImageDraw(ref dst, src, srcRec, dstRec, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawText(ref Image, string, int, int, int, Color)"/>
    public static void DrawText(ref Image dst, string text, int x, int y, int fontSize, Color color) => Raylib.ImageDrawText(ref dst, text, x, y, fontSize, color);
    
    /// <inheritdoc cref="Raylib.ImageDrawTextEx(ref Image, Font, string, Vector2, int, float, Color)"/>
    public static void DrawText(ref Image dst, Font font, string text, Vector2 pos, int fontSize, float spacing, Color color) => Raylib.ImageDrawTextEx(ref dst, font, text, pos, fontSize, spacing, color);
}
#endif