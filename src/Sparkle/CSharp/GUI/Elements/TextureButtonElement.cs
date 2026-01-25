using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

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
    /// Initializes a new instance of the <see cref="TextureButtonElement"/> class.
    /// </summary>
    /// <param name="buttonData">The texture and visual configuration for the button.</param>
    /// <param name="labelData">The label configuration to be drawn over the button.</param>
    /// <param name="anchor">The anchor point determining the element's relative position.</param>
    /// <param name="offset">The offset from the anchor point.</param>
    /// <param name="textAlignment">The alignment of text within a GUI element.</param>
    /// <param name="scale">The scale applied to the button.</param>
    /// <param name="textOffset">The offset of the text relative to its position.</param>
    /// <param name="size">Optional override for the size. If not provided, defaults to the texture size.</param>
    /// <param name="origin">Optional origin point for transformations like rotation and scaling.</param>
    /// <param name="rotation">Optional rotation angle in radians.</param>
    /// <param name="clickFunc">Optional function to execute when the button is clicked. Should return true if handled.</param>
    public TextureButtonElement(TextureButtonData buttonData, LabelData labelData, Anchor anchor, Vector2 offset, TextAlignment textAlignment = TextAlignment.Center, Vector2? textOffset = null, Vector2? size = null, Vector2? scale = null, Vector2? origin = null, float rotation = 0.0F, Func<bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, clickFunc) {
        this.ButtonData = buttonData;
        this.LabelData = labelData;
        this.TextAlignment = textAlignment;
        this.TextOffset = textOffset ?? Vector2.Zero;
        this.Size = size ?? new Vector2(buttonData.SourceRect.Width, buttonData.SourceRect.Height);
    }
    
    /// <summary>
    /// Draws the texture button element and its label onto the specified framebuffer.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer where the element will be drawn.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        // Draw button texture.
        Color buttonColor = this.IsHovered ? this.ButtonData.HoverColor : this.ButtonData.Color;
        
        if (!this.Interactable) {
            buttonColor = this.ButtonData.DisabledColor;
        }
        
        switch (this.ButtonData.ResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(context.SpriteBatch, this.ButtonData.Texture, this.ButtonData.Sampler, this.ButtonData.SourceRect, buttonColor, this.ButtonData.Flip);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(context.SpriteBatch, this.ButtonData.Texture, this.ButtonData.Sampler, this.ButtonData.SourceRect, this.ButtonData.BorderInsets, this.ButtonData.ResizeMode == ResizeMode.TileCenter, buttonColor, this.ButtonData.Flip);
                break;
        }
        
        // Draw text.
        this.DrawText(context.SpriteBatch);
        
        context.SpriteBatch.End();
    }

    /// <summary>
    /// Draws the texture of the button in its normal state without any size or layout modifications.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch instance used to render the texture.</param>
    /// <param name="texture">The texture to be drawn on the button.</param>
    /// <param name="sampler">The optional sampler to configure texture sampling behavior. Can be null.</param>
    /// <param name="sourceRect">The source rectangle within the texture to be drawn.</param>
    /// <param name="color">The color tint to be applied to the texture during rendering.</param>
    /// <param name="flip">The sprite flipping mode to apply during rendering.</param>
    private void DrawNormal(SpriteBatch spriteBatch, Texture2D texture, Sampler? sampler, Rectangle sourceRect, Color color, SpriteFlip flip) {
        if (sampler != null) spriteBatch.PushSampler(sampler);
        spriteBatch.DrawTexture(texture, this.Position, 0.5F, sourceRect, this.Scale * this.Gui.ScaleFactor, this.Origin, this.Rotation, color, flip);
        if (sampler != null) spriteBatch.PopSampler();
    }
    
    /// <summary>
    /// Draws a nine-slice sprite to the screen using the specified texture, source rectangle, and other parameters.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to render the sprite.</param>
    /// <param name="texture">The texture containing the nine-slice source image.</param>
    /// <param name="sampler">The optional sampler state used for texture sampling. Default is null.</param>
    /// <param name="sourceRect">The rectangle defining the portion of the texture to use for the nine-slice rendering.</param>
    /// <param name="borderInsets">The insets defining the border areas of the nine-slice sprite.</param>
    /// <param name="tileCenter">A boolean indicating whether the central area of the nine-slice sprite should be tiled.</param>
    /// <param name="color">The color mask applied to the rendered sprite.</param>
    /// <param name="flip">The sprite flipping mode (horizontal, vertical, or none).</param>
    private void DrawNineSlice(SpriteBatch spriteBatch, Texture2D texture, Sampler? sampler, Rectangle sourceRect, BorderInsets borderInsets, bool tileCenter, Color color, SpriteFlip flip) {
        Vector2 baseScale = this.Scale * this.Gui.ScaleFactor;
        
        // Calculate sizes and clamp to a minimum to prevent overlap.
        float minW = borderInsets.Left + borderInsets.Right;
        float minH = borderInsets.Top + borderInsets.Bottom;
        
        Vector2 visualSize = new Vector2(MathF.Max(this.Size.X, minW), MathF.Max(this.Size.Y, minH));
        Vector2 finalSize = visualSize * baseScale;
        
        // Centering logic for buttons smaller than their borders.
        float diffX = (this.Size.X < minW) ? (minW - this.Size.X) * baseScale.X : 0.0F;
        float diffY = (this.Size.Y < minH) ? (minH - this.Size.Y) * baseScale.Y : 0.0F;
        Vector2 pivot = (this.Origin * baseScale) + new Vector2(diffX, diffY) * 0.5F;
        
        // Calculate edge dimensions.
        float leftW = borderInsets.Left * baseScale.X;
        float rightW = borderInsets.Right * baseScale.X;
        float topH = borderInsets.Top * baseScale.Y;
        float bottomH = borderInsets.Bottom * baseScale.Y;
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
        
        // Push sampler.
        if (sampler != null) spriteBatch.PushSampler(sampler);
        
        // Draw Corners.
        spriteBatch.DrawTexture(texture, this.Position, 0.5F, sourceTopLeft, baseScale, pivot / baseScale, this.Rotation, color, flip);
        spriteBatch.DrawTexture(texture, this.Position, 0.5F, sourceTopRight, baseScale, (pivot - new Vector2(finalSize.X - rightW, 0.0F)) / baseScale, this.Rotation, color, flip);
        spriteBatch.DrawTexture(texture, this.Position, 0.5F, sourceBottomLeft, baseScale, (pivot - new Vector2(0.0F, finalSize.Y - bottomH)) / baseScale, this.Rotation, color, flip);
        spriteBatch.DrawTexture(texture, this.Position, 0.5F, sourceBottomRight, baseScale, (pivot - new Vector2(finalSize.X - rightW, finalSize.Y - bottomH)) / baseScale, this.Rotation, color, flip);
        
        // Draw Edges.
        if (innerH > 0.0F) {
            if (tileCenter) {
                float tileH = sourceLeft.Height * baseScale.Y;
                for (float y = 0.0F; y < innerH; y += tileH) {
                    float drawH = MathF.Min(tileH, innerH - y);
                    Rectangle cL = new Rectangle(sourceLeft.X, sourceLeft.Y, sourceLeft.Width, (int) MathF.Ceiling(drawH / baseScale.Y));
                    Rectangle cR = new Rectangle(sourceRight.X, sourceRight.Y, sourceRight.Width, (int) MathF.Ceiling(drawH / baseScale.Y));
                    spriteBatch.DrawTexture(texture, this.Position, 0.5F, cL, baseScale, (pivot - new Vector2(0.0F, topH + y)) / baseScale, this.Rotation, color, flip);
                    spriteBatch.DrawTexture(texture, this.Position, 0.5F, cR, baseScale, (pivot - new Vector2(finalSize.X - rightW, topH + y)) / baseScale, this.Rotation, color, flip);
                }
            }
            else {
                Vector2 sV = new Vector2(baseScale.X, innerH / sourceLeft.Height);
                spriteBatch.DrawTexture(texture, this.Position, 0.5F, sourceLeft, sV, (pivot - new Vector2(0.0F, topH)) / sV, this.Rotation, color, flip);
                spriteBatch.DrawTexture(texture, this.Position, 0.5F, sourceRight, sV, (pivot - new Vector2(finalSize.X - rightW, topH)) / sV, this.Rotation, color, flip);
            }
        }
        
        if (innerW > 0.0F) {
            if (tileCenter) {
                float tileW = sourceTop.Width * baseScale.X;
                for (float x = 0.0F; x < innerW; x += tileW) {
                    float drawW = MathF.Min(tileW, innerW - x);
                    Rectangle cT = new Rectangle(sourceTop.X, sourceTop.Y, (int) MathF.Max(1.0F, MathF.Round(drawW / baseScale.X)), sourceTop.Height);
                    Rectangle cB = new Rectangle(sourceBottom.X, sourceBottom.Y, (int) MathF.Max(1.0F, MathF.Round(drawW / baseScale.X)), sourceBottom.Height);
                    spriteBatch.DrawTexture(texture, this.Position, 0.5F, cT, baseScale, (pivot - new Vector2(leftW + x, 0.0F)) / baseScale, this.Rotation, color, flip);
                    spriteBatch.DrawTexture(texture, this.Position, 0.5F, cB, baseScale, (pivot - new Vector2(leftW + x, finalSize.Y - bottomH)) / baseScale, this.Rotation, color, flip);
                }
            }
            else {
                int clipW = Math.Min(sourceTop.Width, (int) MathF.Ceiling(innerW / baseScale.X));
                Rectangle cT = new Rectangle(sourceTop.X, sourceTop.Y, clipW, sourceTop.Height);
                Rectangle cB = new Rectangle(sourceBottom.X, sourceBottom.Y, clipW, sourceBottom.Height);
                Vector2 sH = (innerW > sourceTop.Width * baseScale.X) ? new Vector2(innerW / sourceTop.Width, baseScale.Y) : baseScale;
                spriteBatch.DrawTexture(texture, this.Position, 0.5F, cT, sH, (pivot - new Vector2(leftW, 0.0F)) / sH, this.Rotation, color, flip);
                spriteBatch.DrawTexture(texture, this.Position, 0.5F, cB, sH, (pivot - new Vector2(leftW, finalSize.Y - bottomH)) / sH, this.Rotation, color, flip);
            }
        }
        
        // Draw Center.
        if (innerW > 0.0F && innerH > 0.0F) {
            if (tileCenter) {
                float tileW = sourceCenter.Width * baseScale.X;
                float tileH = sourceCenter.Height * baseScale.Y;
                
                for (float y = 0.0F; y < innerH; y += tileH) {
                    float drawH = MathF.Min(tileH, innerH - y);
                    for (float x = 0.0F; x < innerW; x += tileW) {
                        float drawW = MathF.Min(tileW, innerW - x);
                        Rectangle cC = new Rectangle(sourceCenter.X, sourceCenter.Y, (int) MathF.Ceiling(drawW / baseScale.X), (int) MathF.Ceiling(drawH / baseScale.Y));
                        spriteBatch.DrawTexture(texture, this.Position, 0.5F, cC, baseScale, (pivot - new Vector2(leftW + x, topH + y)) / baseScale, this.Rotation, color, flip);
                    }
                }
            }
            else {
                int clipW = Math.Min(sourceCenter.Width, (int) MathF.Ceiling(innerW / baseScale.X));
                Rectangle cC = new Rectangle(sourceCenter.X, sourceCenter.Y, clipW, sourceCenter.Height);
                Vector2 sC = (innerW > sourceCenter.Width * baseScale.X) ? new Vector2(innerW / sourceCenter.Width, innerH / sourceCenter.Height) : new Vector2(baseScale.X, innerH / sourceCenter.Height);
                spriteBatch.DrawTexture(texture, this.Position, 0.5F, cC, sC, (pivot - new Vector2(leftW, topH)) / sC, this.Rotation, color, flip);
            }
        }
        
        // Pop sampler.
        if (sampler != null) spriteBatch.PopSampler();
    }
    
    /// <summary>
    /// Renders the button's text using the provided sprite batch.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to draw the text.</param>
    private void DrawText(SpriteBatch spriteBatch) {
        if (this.LabelData.Text == string.Empty) {
            return;
        }
        
        Vector2 textPos = this.Position + (this.TextOffset * this.Scale * this.Gui.ScaleFactor);
        Vector2 textSize = this.LabelData.Font.MeasureText(this.LabelData.Text, this.LabelData.Size, Vector2.One, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Effect, this.LabelData.EffectAmount);
        Vector2 textOrigin = this.TextAlignment switch {
            TextAlignment.Left => new Vector2(this.Size.X, this.LabelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin),
            TextAlignment.Center => new Vector2(textSize.X, this.LabelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin),
            TextAlignment.Right => new Vector2(-this.Size.X / 2.0F + (textSize.X - 2.0F), this.LabelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin),
            _ => Vector2.Zero
        };
        
        Color color = this.IsHovered ? this.LabelData.HoverColor : this.LabelData.Color;
        
        if (!this.Interactable) {
            color = this.LabelData.DisabledColor;
        }
        
        if (this.LabelData.Sampler != null) spriteBatch.PushSampler(this.LabelData.Sampler);
        spriteBatch.DrawText(this.LabelData.Font, this.LabelData.Text, textPos, this.LabelData.Size, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.Scale * this.Gui.ScaleFactor, 0.5F, textOrigin, this.Rotation, color, this.LabelData.Style, this.LabelData.Effect, this.LabelData.EffectAmount);
        if (this.LabelData.Sampler != null) spriteBatch.PopSampler();
    }
}