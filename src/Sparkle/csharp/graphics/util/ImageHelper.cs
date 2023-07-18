using System.Numerics;
using Raylib_cs;

namespace Sparkle.csharp.graphics.util; 

public static class ImageHelper {
    
    public static Image LoadImage(string path) => Raylib.LoadImage(path);
    public static Image LoadImage(string path, int width, int height, PixelFormat format, int headerSize) => Raylib.LoadImageRaw(path, width, height, format, headerSize);
    public static Image LoadImage(string path, out int frames) => Raylib.LoadImageAnim(path, out frames);
    public static Image LoadImage(string fileType, byte[] fileData) => Raylib.LoadImageFromMemory(fileType, fileData);
    public static Image LoadImage(Texture2D texture) => Raylib.LoadImageFromTexture(texture);
    public static Image LoadImageFromScreen() => Raylib.LoadImageFromScreen();
    public static void UnloadImage(Image image) => Raylib.UnloadImage(image);
    
    public static bool IsImageReady(Image image) => Raylib.IsImageReady(image);
    public static bool ExportImage(Image image, string path) => Raylib.ExportImage(image, path);
    public static bool ExportImageAsCode(Image image, string path) => Raylib.ExportImageAsCode(image, path);
    
    public static unsafe Color* LoadImageColors(Image image) => Raylib.LoadImageColors(image);
    public static unsafe Color* LoadImagePalette(Image image, int maxPaletteSize, int* colorCount) => Raylib.LoadImagePalette(image, maxPaletteSize, colorCount);
    public static unsafe void UnloadImageColors(Color* color) => Raylib.UnloadImageColors(color);
    public static unsafe void UnloadImagePalette(Color* color) => Raylib.UnloadImagePalette(color);

    public static Image GenImageColor(int width, int height, Color color) => Raylib.GenImageColor(width, height, color);
    public static Image GenImageGradientH(int width, int height, Color left, Color right) => Raylib.GenImageGradientH(width, height, left, right);
    public static Image GenImageGradientV(int width, int height, Color top, Color bottom) => Raylib.GenImageGradientV(width, height, top, bottom);
    public static Image GenImageGradientRadial(int width, int height, float density, Color inner, Color outer) => Raylib.GenImageGradientRadial(width, height, density, inner, outer);
    public static Image GenImageChecked(int width, int height, int checksX, int checksY, Color col1, Color col2) => Raylib.GenImageChecked(width, height, checksX, checksY, col1, col2);
    public static Image GenImageWhiteNoise(int width, int height, float factor) => Raylib.GenImageWhiteNoise(width, height, factor);
    public static Image GenImagePerlinNoise(int width, int height, int offsetX, int offsetY, float scale) => Raylib.GenImagePerlinNoise(width, height, offsetX, offsetY, scale);
    public static Image GenImageCellular(int width, int height, int tileSize) => Raylib.GenImageCellular(width, height, tileSize);
    public static Image GenImageText(int width, int height, int tileSize) => Raylib.GenImageText(width, height, tileSize);
    
    public static Image ImageCopy(Image image) => Raylib.ImageCopy(image);
    public static Image ImageFromImage(Image image, Rectangle rec) => Raylib.ImageFromImage(image, rec);
    public static Image ImageText(string text, int fontSize, Color color) => Raylib.ImageText(text, fontSize, color);
    public static Image ImageText(Font font, string text, float fontSize, float spacing, Color color) => Raylib.ImageTextEx(font, text, fontSize, spacing, color);
    public static void ImageFormat(ref Image image, PixelFormat newFormat) => Raylib.ImageFormat(ref image, newFormat);
    public static void ImageToPOT(ref Image image, Color fill) => Raylib.ImageToPOT(ref image, fill);
    public static void ImageCrop(ref Image image, Rectangle crop) => Raylib.ImageCrop(ref image, crop);
    public static void ImageAlphaCrop(ref Image image, float threshold) => Raylib.ImageAlphaCrop(ref image, threshold);
    public static void ImageAlphaClear(ref Image image, Color color, float threshold) => Raylib.ImageAlphaClear(ref image, color, threshold);
    public static void ImageAlphaMask(ref Image image, Image alphaMask) => Raylib.ImageAlphaMask(ref image, alphaMask);
    public static void ImageAlphaPremultiply(ref Image image) => Raylib.ImageAlphaPremultiply(ref image);
    public static void ImageBlurGaussian(ref Image image, int blurSize) => Raylib.ImageBlurGaussian(ref image, blurSize);
    public static void ImageResize(ref Image image, int newWidth, int newHeight) => Raylib.ImageResize(ref image, newWidth, newHeight);
    public static void ImageResizeNN(ref Image image, int newWidth, int newHeight) => Raylib.ImageResizeNN(ref image, newWidth, newHeight);
    public static void ImageResizeCanvas(ref Image image, int newWidth, int newHeight, int offsetX, int offsetY, Color color) => Raylib.ImageResizeCanvas(ref image, newWidth, newHeight, offsetX, offsetY, color);
    public static void ImageMipmaps(ref Image image) => Raylib.ImageMipmaps(ref image);
    public static void ImageDither(ref Image image, int rBpp, int gBpp, int bBpp, int aBpp) => Raylib.ImageDither(ref image, rBpp, gBpp, bBpp, aBpp);
    public static void ImageFlipVertical(ref Image image) => Raylib.ImageFlipVertical(ref image);
    public static void ImageFlipHorizontal(ref Image image) => Raylib.ImageFlipHorizontal(ref image);
    public static void ImageRotateCW(ref Image image) => Raylib.ImageRotateCW(ref image);
    public static void ImageRotateCCW(ref Image image) => Raylib.ImageRotateCCW(ref image);
    public static void ImageColorTint(ref Image image, Color color) => Raylib.ImageColorTint(ref image, color);
    public static void ImageColorInvert(ref Image image) => Raylib.ImageColorInvert(ref image);
    public static void ImageColorGrayscale(ref Image image) => Raylib.ImageColorGrayscale(ref image);
    public static void ImageColorContrast(ref Image image, float contrast) => Raylib.ImageColorContrast(ref image, contrast);
    public static void ImageColorBrightness(ref Image image, int brightness) => Raylib.ImageColorBrightness(ref image, brightness);
    public static void ImageColorReplace(ref Image image, Color color, Color replace) => Raylib.ImageColorReplace(ref image, color, replace);
    public static Rectangle GetImageAlphaBorder(Image image, float threshold) => Raylib.GetImageAlphaBorder(image, threshold);
    public static Color GetImageColor(Image image, int x, int y) => Raylib.GetImageColor(image, x, y);
    
    public static void ImageClearBackground(ref Image dst, Color color) => Raylib.ImageClearBackground(ref dst, color);
    public static void ImageDrawPixel(ref Image dst, int posX, int posY, Color color) => Raylib.ImageDrawPixel(ref dst, posX, posY, color);
    public static void ImageDrawPixel(ref Image dst, Vector2 position, Color color) => Raylib.ImageDrawPixelV(ref dst, position, color);
    public static void ImageDrawLine(ref Image dst, int startPosX, int startPosY, int endPosX, int endPosY, Color color) => Raylib.ImageDrawLine(ref dst, startPosX, startPosY, endPosX, endPosY, color);
    public static void ImageDrawLine(ref Image dst, Vector2 start, Vector2 end, Color color) => Raylib.ImageDrawLineV(ref dst, start, end, color);
    public static void ImageDrawCircle(ref Image dst, int centerX, int centerY, int radius, Color color) => Raylib.ImageDrawCircle(ref dst, centerX, centerY, radius, color);
    public static void ImageDrawCircle(ref Image dst, Vector2 center, int radius, Color color) => Raylib.ImageDrawCircleV(ref dst, center, radius, color);
    public static unsafe void ImageDrawCircleLines(Image* dst, int centerX, int centerY, int radius, Color color) => Raylib.ImageDrawCircleLines(dst, centerX, centerY, radius, color);
    public static unsafe void ImageDrawCircleLines(Image* dst, Vector2 center, int radius, Color color) => Raylib.ImageDrawCircleLinesV(dst, center, radius, color);
    public static void ImageDrawRectangle(ref Image dst, int posX, int posY, int width, int height, Color color) => Raylib.ImageDrawRectangle(ref dst, posX, posY, width, height, color);
    public static void ImageDrawRectangle(ref Image dst, Vector2 position, Vector2 size, Color color) => Raylib.ImageDrawRectangleV(ref dst, position, size, color);
    public static void ImageDrawRectangleRec(ref Image dst, Rectangle rec, Color color) => Raylib.ImageDrawRectangleRec(ref dst, rec, color);
    public static void ImageDrawRectangleLines(ref Image dst, Rectangle rec, int thick, Color color) => Raylib.ImageDrawRectangleLines(ref dst, rec, thick, color);
    public static void ImageDraw(ref Image dst, Image src, Rectangle srcRec, Rectangle dstRec, Color color) => Raylib.ImageDraw(ref dst, src, srcRec, dstRec, color);
    public static void ImageDrawText(ref Image dst, string text, int x, int y, int fontSize, Color color) => Raylib.ImageDrawText(ref dst, text, x, y, fontSize, color);
    public static void ImageDrawText(ref Image dst, Font font, string text, Vector2 position, int fontSize, float spacing, Color color) => Raylib.ImageDrawTextEx(ref dst, font, text, position, fontSize, spacing, color);
}