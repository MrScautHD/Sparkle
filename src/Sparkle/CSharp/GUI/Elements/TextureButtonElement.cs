using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
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
        switch (this.ButtonData.ResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(context.SpriteBatch);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(context.SpriteBatch, this.ButtonData.ResizeMode == ResizeMode.TileCenter);
                break;
        }
        
        // Draw text.
        this.DrawText(context.SpriteBatch);
        
        context.SpriteBatch.End();
    }
    
    /// <summary>
    /// Draws the texture of the button in a normal, unmodified state.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to render the button texture.</param>
    private void DrawNormal(SpriteBatch spriteBatch) {
        Color buttonColor = this.IsHovered ? this.ButtonData.HoverColor : this.ButtonData.Color;
        
        if (this.ButtonData.Sampler != null) spriteBatch.PushSampler(this.ButtonData.Sampler);
        spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, this.ButtonData.SourceRect, this.Scale * this.Gui.ScaleFactor, this.Origin, this.Rotation, buttonColor, this.ButtonData.Flip);
        if (this.ButtonData.Sampler != null) spriteBatch.PopSampler();
    }
    
    /// <summary>
    /// Renders a nine-slice sprite with optional tiling for the center region.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to draw, The nine-slice sprite.</param>
    /// <param name="tileCenter">Indicates whether the center region should be tiled instead of stretched.</param>
    private void DrawNineSlice(SpriteBatch spriteBatch, bool tileCenter) {
        Color color = this.IsHovered ? this.ButtonData.HoverColor : this.ButtonData.Color;
        
        Rectangle sourceRect = this.ButtonData.SourceRect;
        BorderInsets borderInsets = this.ButtonData.BorderInsets;
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
        
        // Calculate scaled edge dimensions.
        float leftW = borderInsets.Left * baseScale.X;
        float rightW = borderInsets.Right * baseScale.X;
        float topH = borderInsets.Top * baseScale.Y;
        float bottomH = borderInsets.Bottom * baseScale.Y;
        float innerW = finalSize.X - leftW - rightW;
        float innerH = finalSize.Y - topH - bottomH;
        
        // Define source rectangles for all 9 segments.
        int right  = sourceRect.X + sourceRect.Width;
        int bottom = sourceRect.Y + sourceRect.Height;
        
        Rectangle sourceTopLeft = new Rectangle(sourceRect.X, sourceRect.Y, borderInsets.Left, borderInsets.Top);
        Rectangle sourceTopRight = new Rectangle(right - borderInsets.Right, sourceRect.Y, borderInsets.Right, borderInsets.Top);
        Rectangle sourceBottomLeft = new Rectangle(sourceRect.X, bottom - borderInsets.Bottom, borderInsets.Left, borderInsets.Bottom);
        Rectangle sourceBottomRight = new Rectangle(right - borderInsets.Right, bottom - borderInsets.Bottom, borderInsets.Right, borderInsets.Bottom);
        
        Rectangle srcTopFull = new Rectangle(sourceRect.X + borderInsets.Left, sourceRect.Y, sourceRect.Width - borderInsets.Left - borderInsets.Right, borderInsets.Top);
        Rectangle srcBottomFull = new Rectangle(sourceRect.X + borderInsets.Left, bottom - borderInsets.Bottom, sourceRect.Width - borderInsets.Left - borderInsets.Right, borderInsets.Bottom);
        Rectangle srcLeft = new Rectangle(sourceRect.X, sourceRect.Y + borderInsets.Top, borderInsets.Left, sourceRect.Height - borderInsets.Top - borderInsets.Bottom);
        Rectangle srcRight = new Rectangle(right - borderInsets.Right, sourceRect.Y + borderInsets.Top, borderInsets.Right, sourceRect.Height - borderInsets.Top - borderInsets.Bottom);
        Rectangle srcCenter = new Rectangle(sourceRect.X + borderInsets.Left, sourceRect.Y + borderInsets.Top, sourceRect.Width - borderInsets.Left - borderInsets.Right, sourceRect.Height - borderInsets.Top - borderInsets.Bottom);
        
        // Corners: Always drawn at the original size (scaled by baseScale).
        spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, sourceTopLeft, baseScale, pivot / baseScale, this.Rotation, color);
        spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, sourceTopRight, baseScale, (pivot - new Vector2(finalSize.X - rightW, 0.0F)) / baseScale, this.Rotation, color);
        spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, sourceBottomLeft, baseScale, (pivot - new Vector2(0.0F, finalSize.Y - bottomH)) / baseScale, this.Rotation, color);
        spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, sourceBottomRight, baseScale, (pivot - new Vector2(finalSize.X - rightW, finalSize.Y - bottomH)) / baseScale, this.Rotation, color);
        
        // Edges: Either stretched or tiled based on configuration.
        if (innerH > 0.0F) {
            if (tileCenter) {
                float tileH = srcLeft.Height * baseScale.Y;
                for (float y = 0.0F; y < innerH; y += tileH) {
                    float drawH = MathF.Min(tileH, innerH - y);
                    Rectangle cL = new Rectangle(srcLeft.X, srcLeft.Y, srcLeft.Width, (int) MathF.Ceiling(drawH / baseScale.Y));
                    Rectangle cR = new Rectangle(srcRight.X, srcRight.Y, srcRight.Width, (int) MathF.Ceiling(drawH / baseScale.Y));
                    spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, cL, baseScale, (pivot - new Vector2(0.0F, topH + y)) / baseScale, this.Rotation, color);
                    spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, cR, baseScale, (pivot - new Vector2(finalSize.X - rightW, topH + y)) / baseScale, this.Rotation, color);
                }
            } else {
                Vector2 sV = new Vector2(baseScale.X, innerH / srcLeft.Height);
                spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, srcLeft, sV, (pivot - new Vector2(0.0F, topH)) / sV, this.Rotation, color);
                spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, srcRight, sV, (pivot - new Vector2(finalSize.X - rightW, topH)) / sV, this.Rotation, color);
            }
        }
        
        if (innerW > 0.0F) {
            if (tileCenter) {
                float tileW = srcTopFull.Width * baseScale.X;
                for (float x = 0.0F; x < innerW; x += tileW) {
                    float drawW = MathF.Min(tileW, innerW - x);
                    Rectangle cT = new Rectangle(srcTopFull.X, srcTopFull.Y, (int) MathF.Max(1.0F, MathF.Round(drawW / baseScale.X)), srcTopFull.Height);
                    Rectangle cB = new Rectangle(srcBottomFull.X, srcBottomFull.Y, (int) MathF.Max(1.0F, MathF.Round(drawW / baseScale.X)), srcBottomFull.Height);
                    spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, cT, baseScale, (pivot - new Vector2(leftW + x, 0.0F)) / baseScale, this.Rotation, color);
                    spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, cB, baseScale, (pivot - new Vector2(leftW + x, finalSize.Y - bottomH)) / baseScale, this.Rotation, color);
                }
            } else {
                int clipW = Math.Min(srcTopFull.Width, (int) MathF.Ceiling(innerW / baseScale.X));
                Rectangle cT = new Rectangle(srcTopFull.X, srcTopFull.Y, clipW, srcTopFull.Height);
                Rectangle cB = new Rectangle(srcBottomFull.X, srcBottomFull.Y, clipW, srcBottomFull.Height);
                Vector2 sH = (innerW > srcTopFull.Width * baseScale.X) ? new Vector2(innerW / srcTopFull.Width, baseScale.Y) : baseScale;
                spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, cT, sH, (pivot - new Vector2(leftW, 0.0F)) / sH, this.Rotation, color);
                spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, cB, sH, (pivot - new Vector2(leftW, finalSize.Y - bottomH)) / sH, this.Rotation, color);
            }
        }
        
        // Center: Fills the remaining interior space.
        if (innerW <= 0.0F || innerH <= 0.0F) {
            return;
        }
        
        if (tileCenter) {
            float tileW = srcCenter.Width * baseScale.X;
            float tileH = srcCenter.Height * baseScale.Y;
            for (float y = 0.0F; y < innerH; y += tileH) {
                float drawH = MathF.Min(tileH, innerH - y);
                for (float x = 0.0F; x < innerW; x += tileW) {
                    float drawW = MathF.Min(tileW, innerW - x);
                    Rectangle cC = new Rectangle(srcCenter.X, srcCenter.Y, (int) MathF.Ceiling(drawW / baseScale.X), (int) MathF.Ceiling(drawH / baseScale.Y));
                    spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, cC, baseScale, (pivot - new Vector2(leftW + x, topH + y)) / baseScale, this.Rotation, color);
                }
            }
        } else {
            int clipW = Math.Min(srcCenter.Width, (int) MathF.Ceiling(innerW / baseScale.X));
            Rectangle cC = new Rectangle(srcCenter.X, srcCenter.Y, clipW, srcCenter.Height);
            Vector2 sC = (innerW > srcCenter.Width * baseScale.X) ? new Vector2(innerW / srcCenter.Width, innerH / srcCenter.Height) : new Vector2(baseScale.X, innerH / srcCenter.Height);
            spriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, cC, sC, (pivot - new Vector2(leftW, topH)) / sC, this.Rotation, color);
        }
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
        
        Color textColor = this.IsHovered ? this.LabelData.HoverColor : this.LabelData.Color;
        
        if (this.LabelData.Sampler != null) spriteBatch.PushSampler(this.LabelData.Sampler);
        spriteBatch.DrawText(this.LabelData.Font, this.LabelData.Text, textPos, this.LabelData.Size, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.Scale * this.Gui.ScaleFactor, 0.5F, textOrigin, this.Rotation, textColor, this.LabelData.Style, this.LabelData.Effect, this.LabelData.EffectAmount);
        if (this.LabelData.Sampler != null) spriteBatch.PopSampler();
    }
}