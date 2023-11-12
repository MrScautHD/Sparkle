using System.Numerics;
using Raylib_cs;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace Sparkle.csharp.graphics.helper; 

public static class ShapeHelper {

    /// <inheritdoc cref="Raylib.SetShapesTexture"/>
    public static void SetShapesTexture(Texture2D texture, Rectangle source) => Raylib.SetShapesTexture(texture, source);

    
    /// <inheritdoc cref="Raylib.DrawPixel"/>
    public static void DrawPixel(int posX, int posY, Color color) => Raylib.DrawPixel(posX, posY, color);
    
    /// <inheritdoc cref="Raylib.DrawPixelV"/>
    public static void DrawPixel(Vector2 pos, Color color) => Raylib.DrawPixelV(pos, color);
    
    
    /// <inheritdoc cref="Raylib.DrawLine"/>
    public static void DrawLine(int startPosX, int startPosY, int endPosX, int endPosY, Color color) => Raylib.DrawLine(startPosX, startPosY, endPosX, endPosY, color);
    
    /// <inheritdoc cref="Raylib.DrawLineV"/>
    public static void DrawLine(Vector2 startPos, Vector2 endPos, Color color) => Raylib.DrawLineV(startPos, endPos, color);
    
    /// <inheritdoc cref="Raylib.DrawLineEx"/>
    public static void DrawLine(Vector2 startPos, Vector2 endPos, float thick, Color color) => Raylib.DrawLineEx(startPos, endPos, thick, color);
    
    /// <inheritdoc cref="Raylib.DrawLineBezier"/>
    public static void DrawLineBezier(Vector2 startPos, Vector2 endPos, float thick, Color color) => Raylib.DrawLineBezier(startPos, endPos, thick, color);
    
    /// <inheritdoc cref="Raylib.DrawLineBezierQuad"/>
    public static void DrawLineBezierQuad(Vector2 startPos, Vector2 endPos, Vector2 controlPos, float thick, Color color) => Raylib.DrawLineBezierQuad(startPos, endPos, controlPos, thick, color);
   
    /// <inheritdoc cref="Raylib.DrawLineBezierCubic"/>
    public static void DrawLineBezierCubic(Vector2 startPos, Vector2 endPos, Vector2 startControlPos, Vector2 endControlPos, float thick, Color color) => Raylib.DrawLineBezierCubic(startPos, endPos, startControlPos, endControlPos, thick, color);
    
    /// <inheritdoc cref="Raylib.DrawLineStrip(Vector2[], int, Color)"/>
    public static void DrawLineStrip(Vector2[] points, int pointCount, Color color) => Raylib.DrawLineStrip(points, pointCount, color);

    
    /// <inheritdoc cref="Raylib.DrawCircle"/>
    public static void DrawCircle(int centerX, int centerY, float radius, Color color) => Raylib.DrawCircle(centerX, centerY, radius, color);
    
    /// <inheritdoc cref="Raylib.DrawCircleV"/>
    public static void DrawCircle(Vector2 center, float radius, Color color) => Raylib.DrawCircleV(center, radius, color);
    
    /// <inheritdoc cref="Raylib.DrawCircleLines"/>
    public static void DrawCircleLines(int centerX, int centerY, float radius, Color color) => Raylib.DrawCircleLines(centerX, centerY, radius, color);
    
    /// <inheritdoc cref="Raylib.DrawCircleSector"/>
    public static void DrawCircleSector(Vector2 center, float radius, float startAngle, float endAngle, int segments, Color color) => Raylib.DrawCircleSector(center, radius, startAngle, endAngle, segments, color);
    
    /// <inheritdoc cref="Raylib.DrawCircleSectorLines"/>
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngle, float endAngle, int segments, Color color) => Raylib.DrawCircleSectorLines(center, radius, startAngle, endAngle, segments, color);
    
    /// <inheritdoc cref="Raylib.DrawCircleGradient"/>
    public static void DrawCircleGradient(int centerX, int centerY, float radius, Color color1, Color color2) => Raylib.DrawCircleGradient(centerX, centerY, radius, color1, color2);
    
    
    /// <inheritdoc cref="Raylib.DrawEllipse"/>
    public static void DrawEllipse(int centerX, int centerY, float radiusH, float radiusV, Color color) => Raylib.DrawEllipse(centerX, centerY, radiusH, radiusV, color);
    
    /// <inheritdoc cref="Raylib.DrawEllipseLines"/>
    public static void DrawEllipseLines(int centerX, int centerY, float radiusH, float radiusV, Color color) => Raylib.DrawEllipseLines(centerX, centerY, radiusH, radiusV, color);
    
    
    /// <inheritdoc cref="Raylib.DrawRing"/>
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngle, float endAngle, int segments, Color color) => Raylib.DrawRing(center, innerRadius, outerRadius, startAngle, endAngle, segments, color);
    
    /// <inheritdoc cref="Raylib.DrawRingLines"/>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngle, float endAngle, int segments, Color color) => Raylib.DrawRingLines(center, innerRadius, outerRadius, startAngle, endAngle, segments, color);

    
    /// <inheritdoc cref="Raylib.DrawRectangle"/>
    public static void DrawRectangle(int posX, int posY, int width, int height, Color color) => Raylib.DrawRectangle(posX, posY, width, height, color);
    
    /// <inheritdoc cref="Raylib.DrawRectangleV"/>
    public static void DrawRectangle(Vector2 pos, Vector2 size, Color color) => Raylib.DrawRectangleV(pos, size, color);
    
    /// <inheritdoc cref="Raylib.DrawRectangleRec"/>
    public static void DrawRectangle(Rectangle rec, Color color) => Raylib.DrawRectangleRec(rec, color);
    
    /// <inheritdoc cref="Raylib.DrawRectanglePro"/>
    public static void DrawRectangle(Rectangle rec, Vector2 origin, float rotation, Color color) => Raylib.DrawRectanglePro(rec, origin, rotation, color);
    
    /// <inheritdoc cref="Raylib.DrawRectangleGradientEx"/>
    public static void DrawRectangleGradient(Rectangle rec, Color col1, Color col2, Color col3, Color col4) => Raylib.DrawRectangleGradientEx(rec, col1, col2, col3, col4);
    
    /// <inheritdoc cref="Raylib.DrawRectangleGradientV"/>
    public static void DrawRectangleGradientV(int posX, int posY, int width, int height, Color color1, Color color2) => Raylib.DrawRectangleGradientV(posX, posY, width, height, color1, color2);
    
    /// <inheritdoc cref="Raylib.DrawRectangleGradientH"/>
    public static void DrawRectangleGradientH(int posX, int posY, int width, int height, Color color1, Color color2) => Raylib.DrawRectangleGradientH(posX, posY, width, height, color1, color2);
    
    /// <inheritdoc cref="Raylib.DrawRectangleLines"/>
    public static void DrawRectangleLines(int posX, int posY, int width, int height, Color color) => Raylib.DrawRectangleLines(posX, posY, width, height, color);
    
    /// <inheritdoc cref="Raylib.DrawRectangleLinesEx"/>
    public static void DrawRectangleLines(Rectangle rec, float lineThick, Color color) => Raylib.DrawRectangleLinesEx(rec, lineThick, color);
    
    /// <inheritdoc cref="Raylib.DrawRectangleRounded"/>
    public static void DrawRectangleRounded(Rectangle rec, float roundness, int segments, Color color) => Raylib.DrawRectangleRounded(rec, roundness, segments, color);
    
    /// <inheritdoc cref="Raylib.DrawRectangleRoundedLines"/>
    public static void DrawRectangleRoundedLines(Rectangle rec, float roundness, int segments, float lineThick, Color color) => Raylib.DrawRectangleRoundedLines(rec, roundness, segments, lineThick, color);

    
    /// <inheritdoc cref="Raylib.DrawTriangle"/>
    public static void DrawTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Color color) => Raylib.DrawTriangle(v1, v2, v3, color);
    
    /// <inheritdoc cref="Raylib.DrawTriangleLines"/>
    public static void DrawTriangleLines(Vector2 v1, Vector2 v2, Vector2 v3, Color color) => Raylib.DrawTriangleLines(v1, v2, v3, color);
    
    /// <inheritdoc cref="Raylib.DrawTriangleFan(Vector2[], int, Color)"/>
    public static void DrawTriangleFan(Vector2[] points, int pointCount, Color color) => Raylib.DrawTriangleFan(points, pointCount, color);
    
    /// <inheritdoc cref="Raylib.DrawTriangleStrip(Vector2[], int, Color)"/>
    public static void DrawTriangleStrip(Vector2[] points, int pointCount, Color color) => Raylib.DrawTriangleStrip(points, pointCount, color);

    
    /// <inheritdoc cref="Raylib.DrawPoly"/>
    public static void DrawPoly(Vector2 center, int sides, float radius, float rotation, Color color) => Raylib.DrawPoly(center, sides, radius, rotation, color);
    
    /// <inheritdoc cref="Raylib.DrawPolyLines"/>
    public static void DrawPolyLines(Vector2 center, int sides, float radius, float rotation, Color color) => Raylib.DrawPolyLines(center, sides, radius, rotation, color);
    
    /// <inheritdoc cref="Raylib.DrawPolyLinesEx"/>
    public static void DrawPolyLines(Vector2 center, int sides, float radius, float rotation, float lineThick, Color color) => Raylib.DrawPolyLinesEx(center, sides, radius, rotation, lineThick, color);
    
    
    /// <inheritdoc cref="Raylib.CheckCollisionRecs"/>
    public static bool CheckCollisionRecs(Rectangle rec1, Rectangle rec2) => Raylib.CheckCollisionRecs(rec1, rec2);
    
    /// <inheritdoc cref="Raylib.CheckCollisionCircles"/>
    public static bool CheckCollisionCircles(Vector2 center1, float radius1, Vector2 center2, float radius2) => Raylib.CheckCollisionCircles(center1, radius1, center2, radius2);
    
    /// <inheritdoc cref="Raylib.CheckCollisionCircleRec"/>
    public static bool CheckCollisionCircleRec(Vector2 center, float radius, Rectangle rec) => Raylib.CheckCollisionCircleRec(center, radius, rec);
    
    /// <inheritdoc cref="Raylib.CheckCollisionPointRec"/>
    public static bool CheckCollisionPointRec(Vector2 point, Rectangle rec) => Raylib.CheckCollisionPointRec(point, rec);
    
    /// <inheritdoc cref="Raylib.CheckCollisionPointCircle"/>
    public static bool CheckCollisionPointCircle(Vector2 point, Vector2 center, float radius) => Raylib.CheckCollisionPointCircle(point, center, radius);
    
    /// <inheritdoc cref="Raylib.CheckCollisionPointTriangle"/>
    public static bool CheckCollisionPointTriangle(Vector2 point, Vector2 p1, Vector2 p2, Vector2 p3) => Raylib.CheckCollisionPointTriangle(point, p1, p2, p3);
    
    /// <inheritdoc cref="Raylib.CheckCollisionPointPoly"/>
    public static unsafe bool CheckCollisionPointPoly(Vector2 point, Vector2* points, int pointCount) => Raylib.CheckCollisionPointPoly(point, points, pointCount);
    
    /// <inheritdoc cref="Raylib.CheckCollisionLines(Vector2, Vector2, Vector2, Vector2, ref Vector2)"/>
    public static bool CheckCollisionLines(Vector2 startPos1, Vector2 endPos1, Vector2 startPos2, Vector2 endPos2, ref Vector2 collisionPoint) => Raylib.CheckCollisionLines(startPos1, endPos1, startPos2, endPos2, ref collisionPoint);
    
    /// <inheritdoc cref="Raylib.CheckCollisionPointLine"/>
    public static bool CheckCollisionPointLine(Vector2 point, Vector2 p1, Vector2 p2, int threshold) => Raylib.CheckCollisionPointLine(point, p1, p2, threshold);
    
    /// <inheritdoc cref="Raylib.GetCollisionRec"/>
    public static Rectangle GetCollisionRec(Rectangle rec1, Rectangle rec2) => Raylib.GetCollisionRec(rec1, rec2);
}