using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class ImageHelper {
    
    public static Image Load(string path) => Raylib.LoadImage(path);
    public static Image Load(string path, int width, int height, PixelFormat format, int headerSize) => Raylib.LoadImageRaw(path, width, height, format, headerSize);
    public static Image Load(string path, out int frames) => Raylib.LoadImageAnim(path, out frames);
    public static Image Load(string fileType, byte[] fileData) => Raylib.LoadImageFromMemory(fileType, fileData);
    public static Image Load(Texture2D texture) => Raylib.LoadImageFromTexture(texture);
    public static Image LoadFromScreen() => Raylib.LoadImageFromScreen();
    public static void Unload(Image image) => Raylib.UnloadImage(image);
    
    public static bool IsReady(Image image) => Raylib.IsImageReady(image);
    public static bool Export(Image image, string path) => Raylib.ExportImage(image, path);
    public static bool ExportAsCode(Image image, string path) => Raylib.ExportImageAsCode(image, path);
    
    public static unsafe Color* LoadColors(Image image) => Raylib.LoadImageColors(image);
    public static unsafe Color* LoadPalette(Image image, int maxPaletteSize, int* colorCount) => Raylib.LoadImagePalette(image, maxPaletteSize, colorCount);
    public static unsafe void UnloadColors(Color* color) => Raylib.UnloadImageColors(color);
    public static unsafe void UnloadPalette(Color* color) => Raylib.UnloadImagePalette(color);

    public static Image GenColor(int width, int height, Color color) => Raylib.GenImageColor(width, height, color);
    public static Image GenGradientH(int width, int height, Color left, Color right) => Raylib.GenImageGradientH(width, height, left, right);
    public static Image GenGradientV(int width, int height, Color top, Color bottom) => Raylib.GenImageGradientV(width, height, top, bottom);
    public static Image GenGradientRadial(int width, int height, float density, Color inner, Color outer) => Raylib.GenImageGradientRadial(width, height, density, inner, outer);
    public static Image GenChecked(int width, int height, int checksX, int checksY, Color col1, Color col2) => Raylib.GenImageChecked(width, height, checksX, checksY, col1, col2);
    public static Image GenWhiteNoise(int width, int height, float factor) => Raylib.GenImageWhiteNoise(width, height, factor);
    public static Image GenPerlinNoise(int width, int height, int offsetX, int offsetY, float scale) => Raylib.GenImagePerlinNoise(width, height, offsetX, offsetY, scale);
    public static Image GenCellular(int width, int height, int tileSize) => Raylib.GenImageCellular(width, height, tileSize);
    public static Image GenText(int width, int height, int tileSize) => Raylib.GenImageText(width, height, tileSize);
    
    public static Image Copy(Image image) => Raylib.ImageCopy(image);
    public static Image FromImage(Image image, Rectangle rec) => Raylib.ImageFromImage(image, rec);
    public static Image Text(string text, int fontSize, Color color) => Raylib.ImageText(text, fontSize, color);
    public static Image Text(Font font, string text, float fontSize, float spacing, Color color) => Raylib.ImageTextEx(font, text, fontSize, spacing, color);
    public static void Format(ref Image image, PixelFormat newFormat) => Raylib.ImageFormat(ref image, newFormat);
    public static void ToPOT(ref Image image, Color fill) => Raylib.ImageToPOT(ref image, fill);
    public static void Crop(ref Image image, Rectangle crop) => Raylib.ImageCrop(ref image, crop);
    public static void AlphaCrop(ref Image image, float threshold) => Raylib.ImageAlphaCrop(ref image, threshold);
    public static void AlphaClear(ref Image image, Color color, float threshold) => Raylib.ImageAlphaClear(ref image, color, threshold);
    public static void AlphaMask(ref Image image, Image alphaMask) => Raylib.ImageAlphaMask(ref image, alphaMask);
    public static void AlphaPremultiply(ref Image image) => Raylib.ImageAlphaPremultiply(ref image);
    public static void BlurGaussian(ref Image image, int blurSize) => Raylib.ImageBlurGaussian(ref image, blurSize);
    public static void Resize(ref Image image, int newWidth, int newHeight) => Raylib.ImageResize(ref image, newWidth, newHeight);
    public static void ResizeNN(ref Image image, int newWidth, int newHeight) => Raylib.ImageResizeNN(ref image, newWidth, newHeight);
    public static void ResizeCanvas(ref Image image, int newWidth, int newHeight, int offsetX, int offsetY, Color color) => Raylib.ImageResizeCanvas(ref image, newWidth, newHeight, offsetX, offsetY, color);
    public static void Mipmaps(ref Image image) => Raylib.ImageMipmaps(ref image);
    public static void Dither(ref Image image, int rBpp, int gBpp, int bBpp, int aBpp) => Raylib.ImageDither(ref image, rBpp, gBpp, bBpp, aBpp);
    public static void FlipVertical(ref Image image) => Raylib.ImageFlipVertical(ref image);
    public static void FlipHorizontal(ref Image image) => Raylib.ImageFlipHorizontal(ref image);
    public static void RotateCW(ref Image image) => Raylib.ImageRotateCW(ref image);
    public static void RotateCCW(ref Image image) => Raylib.ImageRotateCCW(ref image);
    public static void ColorTint(ref Image image, Color color) => Raylib.ImageColorTint(ref image, color);
    public static void ColorInvert(ref Image image) => Raylib.ImageColorInvert(ref image);
    public static void ColorGrayscale(ref Image image) => Raylib.ImageColorGrayscale(ref image);
    public static void ColorContrast(ref Image image, float contrast) => Raylib.ImageColorContrast(ref image, contrast);
    public static void ColorBrightness(ref Image image, int brightness) => Raylib.ImageColorBrightness(ref image, brightness);
    public static void ColorReplace(ref Image image, Color color, Color replace) => Raylib.ImageColorReplace(ref image, color, replace);
    public static Rectangle GetAlphaBorder(Image image, float threshold) => Raylib.GetImageAlphaBorder(image, threshold);
    public static Color GetColor(Image image, int x, int y) => Raylib.GetImageColor(image, x, y);
    
    public static void ClearBackground(ref Image dst, Color color) => Raylib.ImageClearBackground(ref dst, color);
    public static void DrawPixel(ref Image dst, int posX, int posY, Color color) => Raylib.ImageDrawPixel(ref dst, posX, posY, color);
    public static void DrawPixel(ref Image dst, Vector2 pos, Color color) => Raylib.ImageDrawPixelV(ref dst, pos, color);
    public static void DrawLine(ref Image dst, int startPosX, int startPosY, int endPosX, int endPosY, Color color) => Raylib.ImageDrawLine(ref dst, startPosX, startPosY, endPosX, endPosY, color);
    public static void DrawLine(ref Image dst, Vector2 start, Vector2 end, Color color) => Raylib.ImageDrawLineV(ref dst, start, end, color);
    public static void DrawCircle(ref Image dst, int centerX, int centerY, int radius, Color color) => Raylib.ImageDrawCircle(ref dst, centerX, centerY, radius, color);
    public static void DrawCircle(ref Image dst, Vector2 center, int radius, Color color) => Raylib.ImageDrawCircleV(ref dst, center, radius, color);
    public static unsafe void DrawCircleLines(Image* dst, int centerX, int centerY, int radius, Color color) => Raylib.ImageDrawCircleLines(dst, centerX, centerY, radius, color);
    public static unsafe void DrawCircleLines(Image* dst, Vector2 center, int radius, Color color) => Raylib.ImageDrawCircleLinesV(dst, center, radius, color);
    public static void DrawRectangle(ref Image dst, int posX, int posY, int width, int height, Color color) => Raylib.ImageDrawRectangle(ref dst, posX, posY, width, height, color);
    public static void DrawRectangle(ref Image dst, Vector2 pos, Vector2 size, Color color) => Raylib.ImageDrawRectangleV(ref dst, pos, size, color);
    public static void DrawRectangleRec(ref Image dst, Rectangle rec, Color color) => Raylib.ImageDrawRectangleRec(ref dst, rec, color);
    public static void DrawRectangleLines(ref Image dst, Rectangle rec, int thick, Color color) => Raylib.ImageDrawRectangleLines(ref dst, rec, thick, color);
    public static void Draw(ref Image dst, Image src, Rectangle srcRec, Rectangle dstRec, Color color) => Raylib.ImageDraw(ref dst, src, srcRec, dstRec, color);
    public static void DrawText(ref Image dst, string text, int x, int y, int fontSize, Color color) => Raylib.ImageDrawText(ref dst, text, x, y, fontSize, color);
    public static void DrawText(ref Image dst, Font font, string text, Vector2 pos, int fontSize, float spacing, Color color) => Raylib.ImageDrawTextEx(ref dst, font, text, pos, fontSize, spacing, color);
}