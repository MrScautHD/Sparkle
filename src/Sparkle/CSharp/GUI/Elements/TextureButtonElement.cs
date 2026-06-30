using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Mathematics;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Batching;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrith;

namespace Sparkle.CSharp.GUI.Elements;

public class TextureButtonElement : GuiElement {
    
    /// <summary>
    /// The associated data for a texture-based button in the GUI.
    /// </summary>
    public TextureButtonData ButtonData { get; private set; }
    
    /// <summary>
    /// The data and properties necessary for rendering and handling text on a GUI element.
    /// </summary>
    public LabelData LabelData { get; private set; }
    
    /// <summary>
    /// The alignment of text within a GUI element.
    /// </summary>
    public TextAlignment TextAlignment;
    
    /// <summary>
    /// The offset of the text relative to its position.
    /// </summary>
    public Vector2 TextOffset;
    
    /// <summary>
    /// The scaling factor applied to text rendering within the GUI element.
    /// </summary>
    public Vector2 TextScale;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureButtonElement"/> class.
    /// </summary>
    /// <param name="buttonData">The texture and visual configuration for the button.</param>
    /// <param name="labelData">The label configuration to be drawn over the button.</param>
    /// <param name="anchor">The anchor point determining the element's relative position.</param>
    /// <param name="offset">The offset from the anchor point.</param>
    /// <param name="textAlignment">The alignment of text within a GUI element.</param>
    /// <param name="scale">The scale applied to the button.</param>
    /// <param name="textOffset">The offset of the text relative to its position.</param>
    /// <param name="textScale">The scale of the text. Defaults to (1, 1).</param>
    /// <param name="size">Optional override for the size. If not provided, defaults to the texture size.</param>
    /// <param name="origin">Optional origin point for transformations like rotation and scaling.</param>
    /// <param name="rotation">Optional rotation angle in radians.</param>
    /// <param name="renderOrder">The order in which the element is rendered, relative to others.</param>
    /// <param name="clickFunc">Optional function to execute when the button is clicked. Should return true if handled.</param>
    public TextureButtonElement(
        TextureButtonData buttonData,
        LabelData labelData,
        Anchor anchor,
        Vector2 offset,
        TextAlignment textAlignment = TextAlignment.Center,
        Vector2? textOffset = null,
        Vector2? textScale = null,
        Vector2? size = null,
        Vector2? scale = null,
        Vector2? origin = null,
        float rotation = 0.0F,
        int renderOrder = 0,
        Func<GuiElement, bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, renderOrder, clickFunc) {
        this.ButtonData = buttonData;
        this.LabelData = labelData;
        this.TextAlignment = textAlignment;
        this.TextOffset = textOffset ?? Vector2.Zero;
        this.TextScale = textScale ?? Vector2.One;
        this.Size = size ?? new Vector2(buttonData.SourceRect.Width, buttonData.SourceRect.Height);
    }
    
    /// <summary>
    /// Submits the draw commands required to render the GUI element using the appropriate visual state and rendering mode.
    /// </summary>
    /// <param name="renderQueue">The render queue that collects and batches draw commands for later execution.</param>
    protected internal override void Draw(GuiRenderQueue renderQueue) {
        base.Draw(renderQueue);
        
        // Draw button texture.
        Color buttonColor = this.IsHovered ? this.ButtonData.HoverColor : this.ButtonData.Color;
        
        if (!this.Interactable) {
            buttonColor = this.ButtonData.DisabledColor;
        }
        
        switch (this.ButtonData.ResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(renderQueue, this.ButtonData.Texture, this.ButtonData.Sampler, this.ButtonData.SourceRect, buttonColor, this.ButtonData.Flip, this.ButtonData.PixelSnap, this.ButtonData.Effect, this.ButtonData.BlendState);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(renderQueue, this.ButtonData.Texture, this.ButtonData.Sampler, this.ButtonData.SourceRect, this.ButtonData.BorderInsets, this.ButtonData.ResizeMode == ResizeMode.TileCenter, buttonColor, this.ButtonData.Flip, this.ButtonData.PixelSnap, this.ButtonData.Effect, this.ButtonData.BlendState);
                break;
        }
        
        // Draw text.
        this.DrawText(renderQueue);
    }
    
    /// <summary>
    /// Draws a sprite to the screen using the specified texture, source rectangle, and other parameters.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="sampler">The optional sampler state used for texture sampling. Default is null.</param>
    /// <param name="sourceRect">The rectangle defining the portion of the texture to draw.</param>
    /// <param name="color">The color mask applied to the rendered sprite.</param>
    /// <param name="flip">The sprite flipping mode (horizontal, vertical, or none).</param>
    /// <param name="pixelSnap">A boolean specifying whether to align the texture to pixel boundaries.</param>
    /// <param name="effect">The optional effect used when rendering. If <c>null</c>, the batch's current effect is used.</param>
    /// <param name="blendState">The optional blend state used when rendering. If <c>null</c>, the batch's current blend state is used.</param>
    private void DrawNormal(GuiRenderQueue renderQueue, Texture2D texture, Sampler? sampler, Rectangle sourceRect, Color color, SpriteFlip flip, bool pixelSnap, Effect? effect = null, BlendStateDescription? blendState = null) {
        SpriteGuiRenderState renderState = new SpriteGuiRenderState(sampler, effect, blendState);
        renderQueue.UseSprite(renderState).DrawTexture(texture, this.Position, 0.5F, sourceRect, this.Scale * this.Gui.ScaleFactor, this.Origin, pixelSnap, this.Rotation, color, flip);
    }
    
    /// <summary>
    /// Draws a nine-slice sprite to the screen using the specified texture, source rectangle, and other parameters.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    /// <param name="texture">The texture containing the nine-slice source image.</param>
    /// <param name="sampler">The optional sampler state used for texture sampling. Default is null.</param>
    /// <param name="sourceRect">The rectangle defining the portion of the texture to use for the nine-slice rendering.</param>
    /// <param name="borderInsets">The insets defining the border areas of the nine-slice sprite.</param>
    /// <param name="tileCenter">A boolean indicating whether the central area of the nine-slice sprite should be tiled.</param>
    /// <param name="color">The color mask applied to the rendered sprite.</param>
    /// <param name="flip">The sprite flipping mode (horizontal, vertical, or none).</param>
    /// <param name="pixelSnap">A boolean specifying whether to align the texture to pixel boundaries.</param>
    /// <param name="effect">The optional effect used when rendering. If <c>null</c>, the batch's current effect is used.</param>
    /// <param name="blendState">The optional blend state used when rendering. If <c>null</c>, the batch's current blend state is used.</param>
    private void DrawNineSlice(GuiRenderQueue renderQueue, Texture2D texture, Sampler? sampler, Rectangle sourceRect, BorderInsets borderInsets, bool tileCenter, Color color, SpriteFlip flip, bool pixelSnap, Effect? effect = null, BlendStateDescription? blendState = null) {
        Vector2 position = pixelSnap ? Vector2.Floor(this.Position) : this.Position;
        Vector2 origin = pixelSnap ? Vector2.Floor(this.Origin) : this.Origin;
        Vector2 size = pixelSnap ? Vector2.Floor(this.Size) : this.Size;
        Vector2 scale = pixelSnap ? Vector2.Max(Vector2.One, Vector2.Floor(this.Scale)) * this.Gui.ScaleFactor : this.Scale * this.Gui.ScaleFactor;
        
        // Calculate sizes and clamp to a minimum to prevent overlap.
        float minW = borderInsets.Left + borderInsets.Right;
        float minH = borderInsets.Top + borderInsets.Bottom;
        
        Vector2 visualSize = new Vector2(MathF.Max(size.X, minW), MathF.Max(size.Y, minH));
        Vector2 finalSize = visualSize * scale;
        
        // Centering logic for buttons smaller than their borders.
        float diffX = (size.X < minW) ? (minW - size.X) * scale.X : 0.0F;
        float diffY = (size.Y < minH) ? (minH - size.Y) * scale.Y : 0.0F;
        Vector2 pivot = (origin * scale) + new Vector2(diffX, diffY) * 0.5F;
        
        // Calculate edge dimensions.
        float leftW = borderInsets.Left * scale.X;
        float rightW = borderInsets.Right * scale.X;
        float topH = borderInsets.Top * scale.Y;
        float bottomH = borderInsets.Bottom * scale.Y;
        float innerW = finalSize.X - leftW - rightW;
        float innerH = finalSize.Y - topH - bottomH;
        
        // Define source rectangles for all 9 segments.
        int right = sourceRect.X + sourceRect.Width;
        int bottom = sourceRect.Y + sourceRect.Height;
        
        Rectangle sourceTopLeft = new Rectangle(sourceRect.X, sourceRect.Y, borderInsets.Left, borderInsets.Top);
        Rectangle sourceTopRight = new Rectangle(right - borderInsets.Right, sourceRect.Y, borderInsets.Right, borderInsets.Top);
        Rectangle sourceBottomLeft = new Rectangle(sourceRect.X, bottom - borderInsets.Bottom, borderInsets.Left, borderInsets.Bottom);
        Rectangle sourceBottomRight = new Rectangle(right - borderInsets.Right, bottom - borderInsets.Bottom, borderInsets.Right, borderInsets.Bottom);
        
        Rectangle sourceTop = new Rectangle(sourceRect.X + borderInsets.Left, sourceRect.Y, sourceRect.Width - borderInsets.Left - borderInsets.Right, borderInsets.Top);
        Rectangle sourceBottom = new Rectangle(sourceRect.X + borderInsets.Left, bottom - borderInsets.Bottom, sourceRect.Width - borderInsets.Left - borderInsets.Right, borderInsets.Bottom);
        Rectangle sourceLeft = new Rectangle(sourceRect.X, sourceRect.Y + borderInsets.Top, borderInsets.Left, sourceRect.Height - borderInsets.Top - borderInsets.Bottom);
        Rectangle sourceRight = new Rectangle(right - borderInsets.Right, sourceRect.Y + borderInsets.Top, borderInsets.Right, sourceRect.Height - borderInsets.Top - borderInsets.Bottom);
        Rectangle sourceCenter = new Rectangle(sourceRect.X + borderInsets.Left, sourceRect.Y + borderInsets.Top, sourceRect.Width - borderInsets.Left - borderInsets.Right, sourceRect.Height - borderInsets.Top - borderInsets.Bottom);
        
        // Adjust for Horizontal Flip.
        if (flip.HasFlag(SpriteFlip.Horizontal)) {
            (sourceTopLeft, sourceTopRight) = (sourceTopRight, sourceTopLeft);
            (sourceBottomLeft, sourceBottomRight) = (sourceBottomRight, sourceBottomLeft);
            (sourceLeft, sourceRight) = (sourceRight, sourceLeft);
            (leftW, rightW) = (rightW, leftW);
        }
        
        // Adjust for Vertical Flip.
        if (flip.HasFlag(SpriteFlip.Vertical)) {
            (sourceTopLeft, sourceBottomLeft) = (sourceBottomLeft, sourceTopLeft);
            (sourceTopRight, sourceBottomRight) = (sourceBottomRight, sourceTopRight);
            (sourceTop, sourceBottom) = (sourceBottom, sourceTop);
            (topH, bottomH) = (bottomH, topH);
        }
        
        // Create render state.
        SpriteGuiRenderState renderState = new SpriteGuiRenderState(sampler, effect, blendState);
        
        // Draw Corners.
        renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, sourceTopLeft, scale, pivot / scale, false, this.Rotation, color, flip);
        renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, sourceTopLeft, scale, pivot / scale, false, this.Rotation, color, flip);
        renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, sourceTopRight, scale, (pivot - new Vector2(finalSize.X - rightW, 0.0F)) / scale, false, this.Rotation, color, flip);
        renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, sourceBottomLeft, scale, (pivot - new Vector2(0.0F, finalSize.Y - bottomH)) / scale, false, this.Rotation, color, flip);
        renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, sourceBottomRight, scale, (pivot - new Vector2(finalSize.X - rightW, finalSize.Y - bottomH)) / scale, false, this.Rotation, color, flip);
        
        // Draw Edges.
        if (innerH > 0.0F) {
            if (tileCenter) {
                float tileH = sourceLeft.Height * scale.Y;
                for (float y = 0.0F; y < innerH; y += tileH) {
                    float drawH = MathF.Min(tileH, innerH - y);
                    Rectangle cL = new Rectangle(sourceLeft.X, sourceLeft.Y, sourceLeft.Width, (int) MathF.Ceiling(drawH / scale.Y));
                    Rectangle cR = new Rectangle(sourceRight.X, sourceRight.Y, sourceRight.Width, (int) MathF.Ceiling(drawH / scale.Y));
                    renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, cL, scale, (pivot - new Vector2(0.0F, topH + y)) / scale, false, this.Rotation, color, flip);
                    renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, cR, scale, (pivot - new Vector2(finalSize.X - rightW, topH + y)) / scale, false, this.Rotation, color, flip);
                }
            }
            else {
                Vector2 sV = new Vector2(scale.X, innerH / sourceLeft.Height);
                renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, sourceLeft, sV, (pivot - new Vector2(0.0F, topH)) / sV, false, this.Rotation, color, flip);
                renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, sourceRight, sV, (pivot - new Vector2(finalSize.X - rightW, topH)) / sV, false, this.Rotation, color, flip);
            }
        }
        
        if (innerW > 0.0F) {
            if (tileCenter) {
                float tileW = sourceTop.Width * scale.X;
                for (float x = 0.0F; x < innerW; x += tileW) {
                    float drawW = MathF.Min(tileW, innerW - x);
                    Rectangle cT = new Rectangle(sourceTop.X, sourceTop.Y, (int) MathF.Max(1.0F, MathF.Round(drawW / scale.X)), sourceTop.Height);
                    Rectangle cB = new Rectangle(sourceBottom.X, sourceBottom.Y, (int) MathF.Max(1.0F, MathF.Round(drawW / scale.X)), sourceBottom.Height);
                    renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, cT, scale, (pivot - new Vector2(leftW + x, 0.0F)) / scale, false, this.Rotation, color, flip);
                    renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, cB, scale, (pivot - new Vector2(leftW + x, finalSize.Y - bottomH)) / scale, false, this.Rotation, color, flip);
                }
            }
            else {
                int clipW = Math.Min(sourceTop.Width, (int) MathF.Ceiling(innerW / scale.X));
                Rectangle cT = new Rectangle(sourceTop.X, sourceTop.Y, clipW, sourceTop.Height);
                Rectangle cB = new Rectangle(sourceBottom.X, sourceBottom.Y, clipW, sourceBottom.Height);
                Vector2 sH = (innerW > sourceTop.Width * scale.X) ? new Vector2(innerW / sourceTop.Width, scale.Y) : scale;
                renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, cT, sH, (pivot - new Vector2(leftW, 0.0F)) / sH, false, this.Rotation, color, flip);
                renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, cB, sH, (pivot - new Vector2(leftW, finalSize.Y - bottomH)) / sH, false, this.Rotation, color, flip);
            }
        }
        
        // Draw Center.
        if (innerW > 0.0F && innerH > 0.0F) {
            if (tileCenter) {
                float tileW = sourceCenter.Width * scale.X;
                float tileH = sourceCenter.Height * scale.Y;
                
                for (float y = 0.0F; y < innerH; y += tileH) {
                    float drawH = MathF.Min(tileH, innerH - y);
                    for (float x = 0.0F; x < innerW; x += tileW) {
                        float drawW = MathF.Min(tileW, innerW - x);
                        Rectangle cC = new Rectangle(sourceCenter.X, sourceCenter.Y, (int) MathF.Ceiling(drawW / scale.X), (int) MathF.Ceiling(drawH / scale.Y));
                        renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, cC, scale, (pivot - new Vector2(leftW + x, topH + y)) / scale, false, this.Rotation, color, flip);
                    }
                }
            }
            else {
                int clipW = Math.Min(sourceCenter.Width, (int) MathF.Ceiling(innerW / scale.X));
                Rectangle cC = new Rectangle(sourceCenter.X, sourceCenter.Y, clipW, sourceCenter.Height);
                Vector2 sC = (innerW > sourceCenter.Width * scale.X) ? new Vector2(innerW / sourceCenter.Width, innerH / sourceCenter.Height) : new Vector2(scale.X, innerH / sourceCenter.Height);
                renderQueue.UseSprite(renderState).DrawTexture(texture, position, 0.5F, cC, sC, (pivot - new Vector2(leftW, topH)) / sC, false, this.Rotation, color, flip);
            }
        }
    }
    
    /// <summary>
    /// Renders the button's text using the provided sprite batch.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    private void DrawText(GuiRenderQueue renderQueue) {
        if (this.LabelData.Text == string.Empty) {
            return;
        }
        
        Vector2 textPos = this.Position + (this.TextOffset * this.Scale * this.Gui.ScaleFactor);
        Vector2 textSize = this.LabelData.Font.MeasureText(this.LabelData.Text, this.LabelData.Size, Vector2.One, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.FontSystemEffect, this.LabelData.EffectAmount);
        Vector2 textOrigin = this.TextAlignment switch {
            TextAlignment.Left => new Vector2(this.Size.X / this.TextScale.X, this.LabelData.Size) / 2.0F - ((this.Size / 2.0F - this.Origin) / this.TextScale),
            TextAlignment.Right => new Vector2((-this.Size.X / 2.0F) / this.TextScale.X + textSize.X - 2.0F, this.LabelData.Size / 2.0F) - ((this.Size / 2.0F - this.Origin) / this.TextScale),
            TextAlignment.Center => new Vector2(textSize.X, this.LabelData.Size) / 2.0F - ((this.Size / 2.0F - this.Origin) / this.TextScale),
            _ => Vector2.Zero
        };
        
        Color color = this.IsHovered ? this.LabelData.HoverColor : this.LabelData.Color;
        
        if (!this.Interactable) {
            color = this.LabelData.DisabledColor;
        }
        
        SpriteGuiRenderState renderState = new SpriteGuiRenderState(this.LabelData.Sampler, this.LabelData.Effect, this.LabelData.BlendState);
        renderQueue.UseSprite(renderState).DrawText(this.LabelData.Font, this.LabelData.Text, textPos, this.LabelData.Size, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.Scale * this.TextScale * this.Gui.ScaleFactor, 0.5F, textOrigin, this.LabelData.PixelSnap, this.Rotation, color, this.LabelData.Style, this.LabelData.FontSystemEffect, this.LabelData.EffectAmount);
    }
        
    protected override void Dispose(bool disposing) { }
}