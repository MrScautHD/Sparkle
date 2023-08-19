using System.Numerics;
using Raylib_cs;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace Sparkle.csharp.graphics.util; 

public static class ShapeHelper {

    /// <summary> See <see cref="Raylib.SetShapesTexture"/> </summary>
    public static void SetShapesTexture(Texture2D texture, Rectangle source) => Raylib.SetShapesTexture(texture, source);

    
    /// <summary> See <see cref="Raylib.DrawPixel"/> </summary>
    public static void DrawPixel(int posX, int posY, Color color) => Raylib.DrawPixel(posX, posY, color);
    
    /// <summary> See <see cref="Raylib.DrawPixelV"/> </summary>
    public static void DrawPixel(Vector2 pos, Color color) => Raylib.DrawPixelV(pos, color);
    
    
    /// <summary> See <see cref="Raylib.DrawLine"/> </summary>
    public static void DrawLine(int startPosX, int startPosY, int endPosX, int endPosY, Color color) => Raylib.DrawLine(startPosX, startPosY, endPosX, endPosY, color);
    
    /// <summary> See <see cref="Raylib.DrawLineV"/> </summary>
    public static void DrawLine(Vector2 startPos, Vector2 endPos, Color color) => Raylib.DrawLineV(startPos, endPos, color);
    
    /// <summary> See <see cref="Raylib.DrawLineEx"/> </summary>
    public static void DrawLine(Vector2 startPos, Vector2 endPos, float thick, Color color) => Raylib.DrawLineEx(startPos, endPos, thick, color);
    
    /// <summary> See <see cref="Raylib.DrawLineBezier"/> </summary>
    public static void DrawLineBezier(Vector2 startPos, Vector2 endPos, float thick, Color color) => Raylib.DrawLineBezier(startPos, endPos, thick, color);
    
    /// <summary> See <see cref="Raylib.DrawLineBezierQuad"/> </summary>
    public static void DrawLineBezierQuad(Vector2 startPos, Vector2 endPos, Vector2 controlPos, float thick, Color color) => Raylib.DrawLineBezierQuad(startPos, endPos, controlPos, thick, color);
   
    /// <summary> See <see cref="Raylib.DrawLineBezierCubic"/> </summary>
    public static void DrawLineBezierCubic(Vector2 startPos, Vector2 endPos, Vector2 startControlPos, Vector2 endControlPos, float thick, Color color) => Raylib.DrawLineBezierCubic(startPos, endPos, startControlPos, endControlPos, thick, color);
    
    /// <summary> See <see cref="Raylib.DrawLineStrip(Vector2[], int, Color)"/> </summary>
    public static void DrawLineStrip(Vector2[] points, int pointCount, Color color) => Raylib.DrawLineStrip(points, pointCount, color);

    
    /// <summary> See <see cref="Raylib.DrawCircle"/> </summary>
    public static void DrawCircle(int centerX, int centerY, float radius, Color color) => Raylib.DrawCircle(centerX, centerY, radius, color);
    
    /// <summary> See <see cref="Raylib.DrawCircleV"/> </summary>
    public static void DrawCircle(Vector2 center, float radius, Color color) => Raylib.DrawCircleV(center, radius, color);
    
    /// <summary> See <see cref="Raylib.DrawCircleLines"/> </summary>
    public static void DrawCircleLines(int centerX, int centerY, float radius, Color color) => Raylib.DrawCircleLines(centerX, centerY, radius, color);
    
    /// <summary> See <see cref="Raylib.DrawCircleSector"/> </summary>
    public static void DrawCircleSector(Vector2 center, float radius, float startAngle, float endAngle, int segments, Color color) => Raylib.DrawCircleSector(center, radius, startAngle, endAngle, segments, color);
    
    /// <summary> See <see cref="Raylib.DrawCircleSectorLines"/> </summary>
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngle, float endAngle, int segments, Color color) => Raylib.DrawCircleSectorLines(center, radius, startAngle, endAngle, segments, color);
    
    /// <summary> See <see cref="Raylib.DrawCircleGradient"/> </summary>
    public static void DrawCircleGradient(int centerX, int centerY, float radius, Color color1, Color color2) => Raylib.DrawCircleGradient(centerX, centerY, radius, color1, color2);
    
    
    /// <summary> See <see cref="Raylib.DrawEllipse"/> </summary>
    public static void DrawEllipse(int centerX, int centerY, float radiusH, float radiusV, Color color) => Raylib.DrawEllipse(centerX, centerY, radiusH, radiusV, color);
    
    /// <summary> See <see cref="Raylib.DrawEllipseLines"/> </summary>
    public static void DrawEllipseLines(int centerX, int centerY, float radiusH, float radiusV, Color color) => Raylib.DrawEllipseLines(centerX, centerY, radiusH, radiusV, color);
    
    
    /// <summary> See <see cref="Raylib.DrawRing"/> </summary>
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngle, float endAngle, int segments, Color color) => Raylib.DrawRing(center, innerRadius, outerRadius, startAngle, endAngle, segments, color);
    
    /// <summary> See <see cref="Raylib.DrawRingLines"/> </summary>
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngle, float endAngle, int segments, Color color) => Raylib.DrawRingLines(center, innerRadius, outerRadius, startAngle, endAngle, segments, color);

    
    /// <summary> See <see cref="Raylib.DrawRectangle"/> </summary>
    public static void DrawRectangle(int posX, int posY, int width, int height, Color color) => Raylib.DrawRectangle(posX, posY, width, height, color);
    
    /// <summary> See <see cref="Raylib.DrawRectangleV"/> </summary>
    public static void DrawRectangle(Vector2 pos, Vector2 size, Color color) => Raylib.DrawRectangleV(pos, size, color);
    
    /// <summary> See <see cref="Raylib.DrawRectangleRec"/> </summary>
    public static void DrawRectangle(Rectangle rec, Color color) => Raylib.DrawRectangleRec(rec, color);
    
    /// <summary> See <see cref="Raylib.DrawRectanglePro"/> </summary>
    public static void DrawRectangle(Rectangle rec, Vector2 origin, float rotation, Color color) => Raylib.DrawRectanglePro(rec, origin, rotation, color);
    
    /// <summary> See <see cref="Raylib.DrawRectangleGradientEx"/> </summary>
    public static void DrawRectangleGradient(Rectangle rec, Color col1, Color col2, Color col3, Color col4) => Raylib.DrawRectangleGradientEx(rec, col1, col2, col3, col4);
    
    /// <summary> See <see cref="Raylib.DrawRectangleGradientV"/> </summary>
    public static void DrawRectangleGradientV(int posX, int posY, int width, int height, Color color1, Color color2) => Raylib.DrawRectangleGradientV(posX, posY, width, height, color1, color2);
    
    /// <summary> See <see cref="Raylib.DrawRectangleGradientH"/> </summary>
    public static void DrawRectangleGradientH(int posX, int posY, int width, int height, Color color1, Color color2) => Raylib.DrawRectangleGradientH(posX, posY, width, height, color1, color2);
    
    /// <summary> See <see cref="Raylib.DrawRectangleLines"/> </summary>
    public static void DrawRectangleLines(int posX, int posY, int width, int height, Color color) => Raylib.DrawRectangleLines(posX, posY, width, height, color);
    
    /// <summary> See <see cref="Raylib.DrawRectangleLinesEx"/> </summary>
    public static void DrawRectangleLines(Rectangle rec, float lineThick, Color color) => Raylib.DrawRectangleLinesEx(rec, lineThick, color);
    
    /// <summary> See <see cref="Raylib.DrawRectangleRounded"/> </summary>
    public static void DrawRectangleRounded(Rectangle rec, float roundness, int segments, Color color) => Raylib.DrawRectangleRounded(rec, roundness, segments, color);
    
    /// <summary> See <see cref="Raylib.DrawRectangleRoundedLines"/> </summary>
    public static void DrawRectangleRoundedLines(Rectangle rec, float roundness, int segments, float lineThick, Color color) => Raylib.DrawRectangleRoundedLines(rec, roundness, segments, lineThick, color);

    
    /// <summary> See <see cref="Raylib.DrawTriangle"/> </summary>
    public static void DrawTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Color color) => Raylib.DrawTriangle(v1, v2, v3, color);
    
    /// <summary> See <see cref="Raylib.DrawTriangleLines"/> </summary>
    public static void DrawTriangleLines(Vector2 v1, Vector2 v2, Vector2 v3, Color color) => Raylib.DrawTriangleLines(v1, v2, v3, color);
    
    /// <summary> See <see cref="Raylib.DrawTriangleFan(Vector2[], int, Color)"/> </summary>
    public static void DrawTriangleFan(Vector2[] points, int pointCount, Color color) => Raylib.DrawTriangleFan(points, pointCount, color);
    
    /// <summary> See <see cref="Raylib.DrawTriangleStrip(Vector2[], int, Color)"/> </summary>
    public static void DrawTriangleStrip(Vector2[] points, int pointCount, Color color) => Raylib.DrawTriangleStrip(points, pointCount, color);

    
    /// <summary> See <see cref="Raylib.DrawPoly"/> </summary>
    public static void DrawPoly(Vector2 center, int sides, float radius, float rotation, Color color) => Raylib.DrawPoly(center, sides, radius, rotation, color);
    
    /// <summary> See <see cref="Raylib.DrawPolyLines"/> </summary>
    public static void DrawPolyLines(Vector2 center, int sides, float radius, float rotation, Color color) => Raylib.DrawPolyLines(center, sides, radius, rotation, color);
    
    /// <summary> See <see cref="Raylib.DrawPolyLinesEx"/> </summary>
    public static void DrawPolyLines(Vector2 center, int sides, float radius, float rotation, float lineThick, Color color) => Raylib.DrawPolyLinesEx(center, sides, radius, rotation, lineThick, color);
    
    
    /// <summary> See <see cref="Raylib.CheckCollisionRecs"/> </summary>
    public static bool CheckCollisionRecs(Rectangle rec1, Rectangle rec2) => Raylib.CheckCollisionRecs(rec1, rec2);
    
    /// <summary> See <see cref="Raylib.CheckCollisionCircles"/> </summary>
    public static bool CheckCollisionCircles(Vector2 center1, float radius1, Vector2 center2, float radius2) => Raylib.CheckCollisionCircles(center1, radius1, center2, radius2);
    
    /// <summary> See <see cref="Raylib.CheckCollisionCircleRec"/> </summary>
    public static bool CheckCollisionCircleRec(Vector2 center, float radius, Rectangle rec) => Raylib.CheckCollisionCircleRec(center, radius, rec);
    
    /// <summary> See <see cref="Raylib.CheckCollisionPointRec"/> </summary>
    public static bool CheckCollisionPointRec(Vector2 point, Rectangle rec) => Raylib.CheckCollisionPointRec(point, rec);
    
    /// <summary> See <see cref="Raylib.CheckCollisionPointCircle"/> </summary>
    public static bool CheckCollisionPointCircle(Vector2 point, Vector2 center, float radius) => Raylib.CheckCollisionPointCircle(point, center, radius);
    
    /// <summary> See <see cref="Raylib.CheckCollisionPointTriangle"/> </summary>
    public static bool CheckCollisionPointTriangle(Vector2 point, Vector2 p1, Vector2 p2, Vector2 p3) => Raylib.CheckCollisionPointTriangle(point, p1, p2, p3);
    
    /// <summary> See <see cref="Raylib.CheckCollisionPointPoly"/> </summary>
    public static unsafe bool CheckCollisionPointPoly(Vector2 point, Vector2* points, int pointCount) => Raylib.CheckCollisionPointPoly(point, points, pointCount);
    
    /// <summary> See <see cref="Raylib.CheckCollisionLines(Vector2, Vector2, Vector2, Vector2, ref Vector2)"/> </summary>
    public static bool CheckCollisionLines(Vector2 startPos1, Vector2 endPos1, Vector2 startPos2, Vector2 endPos2, ref Vector2 collisionPoint) => Raylib.CheckCollisionLines(startPos1, endPos1, startPos2, endPos2, ref collisionPoint);
    
    /// <summary> See <see cref="Raylib.CheckCollisionPointLine"/> </summary>
    public static bool CheckCollisionPointLine(Vector2 point, Vector2 p1, Vector2 p2, int threshold) => Raylib.CheckCollisionPointLine(point, p1, p2, threshold);
    
    /// <summary> See <see cref="Raylib.GetCollisionRec"/> </summary>
    public static Rectangle GetCollisionRec(Rectangle rec1, Rectangle rec2) => Raylib.GetCollisionRec(rec1, rec2);
}