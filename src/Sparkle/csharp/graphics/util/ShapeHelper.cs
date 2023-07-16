using System.Numerics;
using Raylib_cs;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace Sparkle.csharp.graphics.util; 

public static class ShapeHelper {

    public static void SetShapesTexture(Texture2D texture, Rectangle source) => Raylib.SetShapesTexture(texture, source);

    public static void DrawPixel(Vector2 pos, Color color) => Raylib.DrawPixelV(pos, color);
    
    // LINE
    public static void DrawLine(Vector2 startPos, Vector2 endPos, Color color) => Raylib.DrawLineV(startPos, endPos, color);
    public static void DrawLine(Vector2 startPos, Vector2 endPos, float thick, Color color) => Raylib.DrawLineEx(startPos, endPos, thick, color);
    public static void DrawLineBezier(Vector2 startPos, Vector2 endPos, float thick, Color color) => Raylib.DrawLineBezier(startPos, endPos, thick, color);
    public static void DrawLineBezierQuad(Vector2 startPos, Vector2 endPos, Vector2 controlPos, float thick, Color color) => Raylib.DrawLineBezierQuad(startPos, endPos, controlPos, thick, color);
    public static void DrawLineBezierCubic(Vector2 startPos, Vector2 endPos, Vector2 startControlPos, Vector2 endControlPos, float thick, Color color) => Raylib.DrawLineBezierCubic(startPos, endPos, startControlPos, endControlPos, thick, color);
    
    // CIRCLE
    public static void DrawCircle(Vector2 center, float radius, Color color) => Raylib.DrawCircleV(center, radius, color);
    public static void DrawCircleLines(int centerX, int centerY, float radius, Color color) => Raylib.DrawCircleLines(centerX, centerY, radius, color);
    public static void DrawCircleSector(Vector2 center, float radius, float startAngle, float endAngle, int segments, Color color) => Raylib.DrawCircleSector(center, radius, startAngle, endAngle, segments, color);
    public static void DrawCircleSectorLines(Vector2 center, float radius, float startAngle, float endAngle, int segments, Color color) => Raylib.DrawCircleSectorLines(center, radius, startAngle, endAngle, segments, color);
    public static void DrawCircleGradient(int centerX, int centerY, float radius, Color color1, Color color2) => Raylib.DrawCircleGradient(centerX, centerY, radius, color1, color2);
    
    // ELLIPSE
    public static void DrawEllipse(int centerX, int centerY, float radiusH, float radiusV, Color color) => Raylib.DrawEllipse(centerX, centerY, radiusH, radiusV, color);
    public static void DrawEllipseLines(int centerX, int centerY, float radiusH, float radiusV, Color color) => Raylib.DrawEllipseLines(centerX, centerY, radiusH, radiusV, color);
    
    // RING
    public static void DrawRing(Vector2 center, float innerRadius, float outerRadius, float startAngle, float endAngle, int segments, Color color) => Raylib.DrawRing(center, innerRadius, outerRadius, startAngle, endAngle, segments, color);
    public static void DrawRingLines(Vector2 center, float innerRadius, float outerRadius, float startAngle, float endAngle, int segments, Color color) => Raylib.DrawRingLines(center, innerRadius, outerRadius, startAngle, endAngle, segments, color);

    // RECTANGLE
    public static void DrawRectangle(Rectangle rec, Color color) => Raylib.DrawRectangleRec(rec, color);
    public static void DrawRectangle(Rectangle rec, Vector2 origin, float rotation, Color color) => Raylib.DrawRectanglePro(rec, origin, rotation, color);
    public static void DrawRectangleLines(Rectangle rec, float lineThick, Color color) => Raylib.DrawRectangleLinesEx(rec, lineThick, color);
    public static void DrawRectangleRounded(Rectangle rec, float roundness, int segments, Color color) => Raylib.DrawRectangleRounded(rec, roundness, segments, color);
    public static void DrawRectangleRoundedLines(Rectangle rec, float roundness, int segments, float lineThick, Color color) => Raylib.DrawRectangleRoundedLines(rec, roundness, segments, lineThick, color);
    
    // TRIANGLE
    public static void DrawTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Color color) => Raylib.DrawTriangle(v1, v2, v3, color);
    public static void DrawTriangleLines(Vector2 v1, Vector2 v2, Vector2 v3, Color color) => Raylib.DrawTriangleLines(v1, v2, v3, color);
    
    // POLY
    public static void DrawPoly(Vector2 center, int sides, float radius, float rotation, Color color) => Raylib.DrawPoly(center, sides, radius, rotation, color);
    public static void DrawPolyLines(Vector2 center, int sides, float radius, float rotation, float lineThick, Color color) => Raylib.DrawPolyLinesEx(center, sides, radius, rotation, lineThick, color);
}