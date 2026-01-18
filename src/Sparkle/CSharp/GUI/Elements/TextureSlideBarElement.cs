using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

public class TextureSlideBarElement : GuiElement {
    
    /// <summary>
    /// The data used to render the slide bar.
    /// </summary>
    public TextureSlideBarData Data { get; private set; }
    
    /// <summary>
    /// The minimum allowable value for the slider bar, defining the lower limit of the slider's range.
    /// </summary>
    public float MinValue;
    
    /// <summary>
    /// The maximum allowable value for the slider bar, defining the upper limit of the slider's range.
    /// </summary>
    public float MaxValue;
    
    /// <summary>
    /// The current value of the slider bar, clamped between the minimum and maximum values.
    /// </summary>
    public float Value {
        get;
        set {
            float clampedValue = Math.Clamp(value, this.MinValue, this.MaxValue);
            field = this.WholeNumbers ? MathF.Round(clampedValue) : clampedValue;
        }
    }
    
    /// <summary>
    /// The slider value should be rounded to the nearest whole number.
    /// </summary>
    public bool WholeNumbers;
    
    /// <summary>
    /// The user is currently dragging the slider bar.
    /// </summary>
    private bool _isDragging;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureSlideBarElement"/> class.
    /// </summary>
    /// <param name="data">Defines the texture and styling data for the slider bar.</param>
    /// <param name="anchor">Specifies the alignment of the slider bar to its parent container.</param>
    /// <param name="offset">Defines the positional offset of the slider bar in relation to its anchor.</param>
    /// <param name="minValue">The minimum value the slider bar represents.</param>
    /// <param name="maxValue">The maximum value the slider bar represents.</param>
    /// <param name="value">The initial value of the slider bar, which can be changed by user interaction.</param>
    /// <param name="wholeNumbers">Indicates whether the slider should operate in whole number (integer) intervals or support floating-point values.</param>
    /// <param name="size">Specifies the size of the slider bar. Defaults to the size defined by the texture data if not provided.</param>
    /// <param name="scale">Specifies the scale factor for the slider bar's dimensions.</param>
    /// <param name="origin">Defines the origin point for transformations applied to the slider bar.</param>
    /// <param name="rotation">The rotation angle (in degrees) to apply to the slider bar.</param>
    /// <param name="clickFunc">A function that is invoked when the slider bar is clicked, returning a boolean to indicate success or state change.</param>
    public TextureSlideBarElement(TextureSlideBarData data, Anchor anchor, Vector2 offset, float minValue, float maxValue, float value = 0.0F, bool wholeNumbers = false, Vector2? size = null, Vector2? scale = null, Vector2? origin = null, float rotation = 0.0F, Func<bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, clickFunc) {
        this.Data = data;
        this.Size = size ?? new Vector2(data.BarSourceRect.Width, data.BarSourceRect.Height);
        this.MinValue = minValue;
        this.MaxValue = maxValue;
        this.Value = value;
        this.WholeNumbers = wholeNumbers;
    }
    
    /// <summary>
    /// Updates the state of the texture slider bar element, including checking for interaction,
    /// handling dragging behavior, and updating the current value based on user input.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update, in seconds.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);
        
        if (this.Interactable) {
            if (this.IsClicked) {
                this._isDragging = true;
            }
            
            if (this._isDragging) {
                if (Input.IsMouseButtonDown(MouseButton.Left)) {
                    
                    // Transform mouse position to local space (considering rotation and scale).
                    Matrix4x4 rotation = Matrix4x4.CreateRotationZ(float.DegreesToRadians(-this.Rotation));
                    Vector2 localClickPos = Vector2.Transform(Input.GetMousePosition() - this.Position, rotation) + this.Origin * this.Scale * this.Gui.ScaleFactor;
                    
                    // Calculate the usable width (Bar Width - Slider Width) to keep the slider inside the bar.
                    float sliderWidth = this.Data.SliderSourceRect.Width;
                    float usableWidth = this.Size.X - sliderWidth;
                    
                    // Calculate percentage based on local X position, offset by half the slider width.
                    float percent = Math.Clamp((localClickPos.X / (this.Scale.X * this.Gui.ScaleFactor) - (sliderWidth / 2.0F)) / usableWidth, 0.0F, 1.0F);
                    
                    // Update the value based on the percentage.
                    this.Value = this.MinValue + (this.MaxValue - this.MinValue) * percent;
                }
                else {
                    this._isDragging = false;
                }
            }
        }
        else {
            this._isDragging = false;
        }
    }
    
    /// <summary>
    /// Draws the texture button element and its label onto the specified framebuffer.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer where the element will be drawn.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);

        // Draw bar.
        Color barColor = this.IsHovered ? this.Data.BarHoverColor : this.Data.BarColor;
        
        if (!this.Interactable) {
            barColor = this.Data.DisabledColor;
        }
        
        switch (this.Data.BarResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(context.SpriteBatch, this.Data.BarTexture, this.Data.BarSampler, this.Data.BarSourceRect, barColor, this.Data.BarFlip);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(context.SpriteBatch, this.Data.BarTexture, this.Data.BarSampler, this.Data.BarSourceRect, this.Data.BarBorderInsets, this.Data.BarResizeMode == ResizeMode.TileCenter, barColor, this.Data.BarFlip);
                break;
        }
        
        // Draw slider.
        this.DrawSlider(context.SpriteBatch);
        
        context.SpriteBatch.End();
    }

    /// <summary>
    /// Renders the slider bar texture onto the screen using the specified parameters.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used for batch rendering of textures.</param>
    /// <param name="texture">The texture of the slider bar to be drawn.</param>
    /// <param name="sampler">The sampler state to apply when rendering the texture. Can be null.</param>
    /// <param name="sourceRect">The source rectangle defining the area of the texture to render.</param>
    /// <param name="color">The color to tint the rendered texture.</param>
    /// <param name="flip">The flip mode to apply when rendering the texture.</param>
    private void DrawNormal(SpriteBatch spriteBatch, Texture2D texture, Sampler? sampler, Rectangle sourceRect, Color color, SpriteFlip flip) {
        if (sampler != null) spriteBatch.PushSampler(sampler);
        spriteBatch.DrawTexture(texture, this.Position, 0.5F, sourceRect, this.Scale * this.Gui.ScaleFactor, this.Origin, this.Rotation, color, flip);
        if (sampler != null) spriteBatch.PopSampler();
    }

    /// <summary>
    /// Renders a nine-slice texture onto the specified target using the provided parameters.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used for managing texture drawing operations.</param>
    /// <param name="texture">The texture to be drawn as a nine-slice element.</param>
    /// <param name="sampler">The optional sampler used for texture filtering and addressing.</param>
    /// <param name="sourceRect">The source rectangle defining the region of the texture to be used.</param>
    /// <param name="borderInsets">The border insets specifying the nine-slice division points.</param>
    /// <param name="tileCenter">Determines if the center area of the nine-slice should be tiled or stretched.</param>
    /// <param name="color">The color to apply as a tint to the texture.</param>
    /// <param name="flip">The directional flip to apply to the texture.</param>
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
    /// Draws the slider for the texture slider bar element at its calculated position within the parent bar.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used for rendering the slider texture.</param>
    private void DrawSlider(SpriteBatch spriteBatch) {
        Color sliderColor = this.IsHovered ? this.Data.SliderHoverColor : this.Data.SliderColor;
        
        if (!this.Interactable) {
            sliderColor = this.Data.DisabledColor;
        }
        
        // Clamp just for safety, the Value should also be clamped already.
        float value = Math.Clamp(this.Value, this.MinValue, this.MaxValue);
        float percent = (value - this.MinValue) / (this.MaxValue - this.MinValue);
        
        // Calculate the slider position within the usable width.
        float sliderWidth = this.Data.SliderSourceRect.Width;
        float sliderHeight = this.Data.SliderSourceRect.Height;
        float usableWidth = this.Size.X - sliderWidth;
        float xPos = (sliderWidth / 2.0F) + (percent * usableWidth);
        
        // Calculate the slider origin to align it correctly with the bar.
        Vector2 sliderOrigin = new Vector2(this.Data.SliderTexture.Width / 2.0F - xPos + (this.Size.X / 2.0F), sliderHeight / 2.0F) - (this.Size / 2.0F - this.Origin);
        
        // Draw.
        if (this.Data.SliderSampler != null) spriteBatch.PushSampler(this.Data.SliderSampler);
        spriteBatch.DrawTexture(this.Data.SliderTexture, this.Position, 0.5F, this.Data.SliderSourceRect, this.Scale * this.Gui.ScaleFactor, sliderOrigin, this.Rotation, sliderColor, this.Data.SliderFlip);
        if (this.Data.SliderSampler != null) spriteBatch.PopSampler();
    }
}