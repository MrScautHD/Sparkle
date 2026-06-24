using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Mathematics;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.GUI.Batching;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrith;

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
            this.ValueUpdated?.Invoke(field);
        }
    }
    
    /// <summary>
    /// The slider value should be rounded to the nearest whole number.
    /// </summary>
    public bool WholeNumbers;

    /// <summary>
    /// An event that triggers when the slider value is updated.
    /// The new value of the slider is passed as a parameter to the attached event handlers.
    /// </summary>
    public event Action<float>? ValueUpdated;
    
    /// <summary>
    /// The user is currently dragging the slider.
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
    public TextureSlideBarElement(TextureSlideBarData data, Anchor anchor, Vector2 offset, float minValue, float maxValue, float value = 0.0F, bool wholeNumbers = false, Vector2? size = null, Vector2? scale = null, Vector2? origin = null, float rotation = 0.0F, Func<GuiElement, bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, clickFunc) {
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
    /// <param name="interactionHandled">A reference to a boolean tracking whether interaction has already been handled by another element.</param>
    protected internal override void Update(double delta, ref bool interactionHandled) {
        base.Update(delta, ref interactionHandled);
        
        if (this.Interactable) {
            if (this.IsClicked) {
                this._isDragging = true;
            }
            
            if (this._isDragging) {
                if (Input.IsMouseButtonDown(MouseButton.Left)) {
                    
                    // Transform mouse position to local space (considering rotation and scale).
                    Matrix4x4 rotation = Matrix4x4.CreateRotationZ(float.DegreesToRadians(-this.Rotation));
                    Vector2 localClickPos = Vector2.Transform(Input.GetMousePosition() - this.Position, rotation) + this.Origin * this.Scale * this.Gui.ScaleFactor;
                    
                    float percent;
                    
                    // Check if the slider is there because then the graping need to be synced with his size.
                    if (this.Data.SliderTexture != null) {
                        float sliderWidth = this.Data.SliderSourceRect.Width;
                        float usableWidth = this.Size.X - sliderWidth;
                        
                        percent = Math.Clamp((localClickPos.X / (this.Scale.X * this.Gui.ScaleFactor) - sliderWidth / 2.0F) / usableWidth, 0.0F, 1.0F);
                    }
                    else {
                        percent = Math.Clamp(localClickPos.X / (this.Scale.X * this.Gui.ScaleFactor) / this.Size.X, 0.0F, 1.0F);
                    }
                    
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
    /// Submits the draw commands required to render the GUI element using the appropriate visual state and rendering mode.
    /// </summary>
    /// <param name="renderQueue">The render queue that collects and batches draw commands for later execution.</param>
    protected internal override void SubmitDrawCommands(GuiRenderQueue renderQueue) {
        base.SubmitDrawCommands(renderQueue);
        
        // Draw bar.
        Color barColor = this.IsHovered ? this.Data.BarHoverColor : this.Data.BarColor;
        
        if (!this.Interactable) {
            barColor = this.Data.DisabledBarColor;
        }
        
        this.DrawBar(renderQueue, this.Data.BarTexture, this.Data.BarSampler, this.Data.BarSourceRect, this.Data.BarResizeMode, this.Data.BarBorderInsets, barColor, this.Data.BarFlip, this.Data.BarPixelSnap, this.Data.Effect, this.Data.BlendState);
        
        // Draw the progress bar.
        float value = Math.Clamp(this.Value, this.MinValue, this.MaxValue);
        float percent = (value - this.MinValue) / (this.MaxValue - this.MinValue);
        
        if (this.Data.FilledBarTexture != null && percent > 0) {
            float fillWidth;
            
            // Check if the slider is there because then the progress bar needs to be synced with his size.
            if (this.Data.SliderTexture != null) {
                float sliderWidth = this.Data.SliderSourceRect.Width;
                float usableWidth = this.Size.X - sliderWidth;
                fillWidth = (sliderWidth / 2.0F) + (percent * usableWidth);
            }
            else {
                fillWidth = this.Size.X * percent;
            }
            
            DepthStencilStateDescription stencilWrite = new DepthStencilStateDescription() {
                StencilTestEnabled = true,
                StencilWriteMask = 0xFF,
                StencilReference = 1,
                StencilFront = new StencilBehaviorDescription {
                    Comparison = ComparisonKind.Always,
                    Pass = StencilOperation.Replace
                },
                StencilBack = new StencilBehaviorDescription {
                    Comparison = ComparisonKind.Always,
                    Pass = StencilOperation.Replace
                }
            };
            
            RectangleF progressArea = new RectangleF(this.Position.X, this.Position.Y, fillWidth * this.Scale.X * this.Gui.ScaleFactor, this.ScaledSize.Y);
            
            GuiRenderState primitiveRenderState = new GuiRenderState(depthStencilState: stencilWrite);
            renderQueue.UsePrimitive(primitiveRenderState).DrawFilledRectangle(progressArea, this.Origin * this.Scale * this.Gui.ScaleFactor, this.Rotation, 0.5F, new Color(255, 255, 255, 0));
            
            DepthStencilStateDescription stencilTest = new DepthStencilStateDescription() {
                StencilTestEnabled = true,
                StencilReadMask = 0xFF,
                StencilReference = 1,
                StencilFront = new StencilBehaviorDescription {
                    Comparison = ComparisonKind.Equal,
                    Pass = StencilOperation.Keep
                },
                StencilBack = new StencilBehaviorDescription {
                    Comparison = ComparisonKind.Equal,
                    Pass = StencilOperation.Keep
                }
            };
            
            Color filledBarColor = this.IsHovered ? this.Data.FilledBarHoverColor : this.Data.FilledBarColor;
            
            if (!this.Interactable) {
                filledBarColor = this.Data.DisabledFilledBarColor;
            }
            
            this.DrawBar(renderQueue, this.Data.FilledBarTexture, this.Data.FilledBarSampler, this.Data.FilledBarSourceRect, this.Data.FilledBarResizeMode, this.Data.FilledBarBorderInsets, filledBarColor, this.Data.FilledBarFlip, this.Data.FilledBarPixelSnap, this.Data.Effect, this.Data.BlendState, stencilTest);
        }
        
        // Draw slider.
        this.DrawSlider(renderQueue);
    }
    
    /// <summary>
    /// Renders a bar element on the screen using the specified texture, resizing properties, and visual appearance settings.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    /// <param name="texture">The texture to be used for rendering the bar.</param>
    /// <param name="sampler">The sampler that determines how the texture is sampled. Can be null for default sampling.</param>
    /// <param name="sourceRect">The portion of the texture to draw. This defines the texture's source bounds.</param>
    /// <param name="resizeMode">Specifies how the bar texture should be resized or scaled across its dimensions.</param>
    /// <param name="borderInsets">Defines the insets for the texture's edges, used for resizing or nine-slice operations.</param>
    /// <param name="color">The color tint to apply to the bar texture during rendering.</param>
    /// <param name="flip">Specifies how the texture should be flipped (horizontally, vertically, or both) during rendering.</param>
    /// <param name="pixelSnap">Indicates whether the texture rendering should align to pixel boundaries for improved visual clarity.</param>
    /// <param name="effect">The optional effect used when rendering. If <c>null</c>, the batch's current effect is used.</param>
    /// <param name="blendState">The optional blend state used when rendering. If <c>null</c>, the batch's current blend state is used.</param>
    /// <param name="depthStencilState">Optional depth-stencil state used for depth testing and stencil operations during rendering. If <c>null</c>, the default state is used.</param>
    private void DrawBar(GuiRenderQueue renderQueue, Texture2D texture, Sampler? sampler, Rectangle sourceRect, ResizeMode resizeMode, BorderInsets borderInsets, Color color, SpriteFlip flip, bool pixelSnap, Effect? effect = null, BlendStateDescription? blendState = null, DepthStencilStateDescription? depthStencilState = null) {
        switch (resizeMode) {
            case ResizeMode.None:
                this.DrawNormal(renderQueue, texture, sampler, sourceRect, color, flip, pixelSnap, effect, blendState, depthStencilState);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(renderQueue, texture, sampler, sourceRect, borderInsets, resizeMode == ResizeMode.TileCenter, color, flip, pixelSnap, effect, blendState, depthStencilState);
                break;
        }
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
    /// <param name="depthStencilState">Optional depth-stencil state used for depth testing and stencil operations during rendering. If <c>null</c>, the default state is used.</param>
    private void DrawNormal(GuiRenderQueue renderQueue, Texture2D texture, Sampler? sampler, Rectangle sourceRect, Color color, SpriteFlip flip, bool pixelSnap, Effect? effect = null, BlendStateDescription? blendState = null, DepthStencilStateDescription? depthStencilState = null) {
        GuiRenderState renderState = new GuiRenderState(sampler, effect, blendState, depthStencilState);
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
    /// <param name="depthStencilState">Optional depth-stencil state used for depth testing and stencil operations during rendering. If <c>null</c>, the default state is used.</param>
    private void DrawNineSlice(GuiRenderQueue renderQueue, Texture2D texture, Sampler? sampler, Rectangle sourceRect, BorderInsets borderInsets, bool tileCenter, Color color, SpriteFlip flip, bool pixelSnap, Effect? effect = null, BlendStateDescription? blendState = null, DepthStencilStateDescription? depthStencilState = null) {
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
        GuiRenderState renderState = new GuiRenderState(sampler, effect, blendState, depthStencilState);
        
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
    /// Draws the slider for the texture slider bar element at its calculated position within the parent bar.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    private void DrawSlider(GuiRenderQueue renderQueue) {
        if (this.Data.SliderTexture == null) {
            return;
        }

        Color color = this.IsHovered ? this.Data.SliderHoverColor : this.Data.SliderColor;
        
        if (!this.Interactable) {
            color = this.Data.DisabledSliderColor;
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
        Vector2 origin = new Vector2(this.Data.SliderTexture.Width / 2.0F - xPos + (this.Size.X / 2.0F), sliderHeight / 2.0F) - (this.Size / 2.0F - this.Origin);
        
        // Draw.
        GuiRenderState renderState = new GuiRenderState(this.Data.SliderSampler, this.Data.Effect, this.Data.BlendState);
        renderQueue.UseSprite(renderState).DrawTexture(this.Data.SliderTexture, this.Position, 0.5F, this.Data.SliderSourceRect, this.Scale * this.Gui.ScaleFactor, origin, this.Data.SliderPixelSnap, this.Rotation, color, this.Data.SliderFlip);
    }
    
    protected override void Dispose(bool disposing) { }
}