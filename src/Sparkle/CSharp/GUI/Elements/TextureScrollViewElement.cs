using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Mathematics;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrith;

namespace Sparkle.CSharp.GUI.Elements;

public class TextureScrollViewElement : GuiElement {
    
    public TextureScrollViewData Data { get; private set; }
    
    public float ScrollSensitivity;
    
    public float ScrollLerpSpeed;
    
    private OrderedDictionary<string, GuiElement> _content;
    
    private List<GuiElement> _contentToAdd;
    
    private List<string> _contentToRemove;
    
    private Dictionary<GuiElement, Vector2> _contentOffsets;
    
    private List<KeyValuePair<string, GuiElement>> _initialContent;
    
    private RenderTexture2D? _contentRenderTarget;
    
    private Texture2D? _contentResult;
    
    private float _scrollPercent;
    
    private float _targetScrollPercent;
    
    private bool _isDraggingSlider;
    
    public TextureScrollViewElement(
        TextureScrollViewData data,
        IEnumerable<KeyValuePair<string, GuiElement>>? content,
        Anchor anchor,
        Vector2 offset,
        (float Left, float Right, float Top, float Bottom)? menuContentInsets = null,
        float scrollSensitivity = 0.1F,
        float scrollLerpSpeed = 10.0F,
        Vector2? size = null,
        Vector2? scale = null,
        Vector2? origin = null,
        float rotation = 0,
        Func<GuiElement, bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, clickFunc) {
        this.Data = data;
        this.Size = size ?? new Vector2(data.MenuSourceRect.Width, data.MenuSourceRect.Height);
        this.ScrollSensitivity = scrollSensitivity;
        this.ScrollLerpSpeed = scrollLerpSpeed;
        
        this._content = new OrderedDictionary<string, GuiElement>();
        this._contentToAdd = new List<GuiElement>();
        this._contentToRemove = new List<string>();
        this._contentOffsets = new Dictionary<GuiElement, Vector2>();
        this._initialContent = new List<KeyValuePair<string, GuiElement>>();
        
        if (content != null) {
            foreach ((string name, GuiElement element) in content) {
                this._initialContent.Add(new KeyValuePair<string, GuiElement>(name, element));
            }
        }
    }
    
    protected internal override void Init() {
        base.Init();
        
        // Add content elements to the Scroll view element.
        foreach ((string name, GuiElement element) in this._initialContent) {
            this.AddContent(name, element);
        }
        
        this._initialContent.Clear();
        
        // TODO: CREATE RENDERTARGET HERE USE GLOBALGRAPTICSASSETS FOR GRAPTICSDEVICE.
    }
    
    protected internal override void Update(double delta, ref bool interactionHandled) {
        
        // Handle removing content.
        foreach (string name in this._contentToRemove) {
            if (this._content.Remove(name, out GuiElement? element)) {
                element.Dispose();
            }
        }
        
        this._contentToRemove.Clear();
        
        // Handle adding content.
        foreach (GuiElement element in this._contentToAdd) {
            this._content.Add(element.Name, element);
        }
        
        this._contentToAdd.Clear();
        
        // Update base element.
        base.Update(delta, ref interactionHandled);
        
        Vector2 mousePos = Input.GetMousePosition();
        Vector2 scale = this.Scale * this.Gui.ScaleFactor;
        Vector2 viewSize = this.Size * scale;
        
        Matrix3x2 transform = Matrix3x2.CreateTranslation(-this.Position) *
                              Matrix3x2.CreateRotation(-float.DegreesToRadians(this.Rotation)) *
                              Matrix3x2.CreateTranslation(this.Origin * scale);
        
        Vector2 localMouse = Vector2.Transform(mousePos, transform);
        
        float sliderBarWidth = this.Data.SliderBarWidth * scale.X;
        float sliderBarHeight = viewSize.Y;
        
        RectangleF localViewRect = new RectangleF(0.0F, 0.0F, viewSize.X, viewSize.Y);
        RectangleF localSliderBarRect = new RectangleF(viewSize.X - sliderBarWidth, 0.0F, sliderBarWidth, sliderBarHeight);
        
        if (this.Interactable) {
            if (!this._isDraggingSlider && localViewRect.Contains(localMouse)) {
                if (Input.IsMouseScrolling(out Vector2 wheelDelta)) {
                    this._targetScrollPercent = Math.Clamp(this._targetScrollPercent - wheelDelta.Y * this.ScrollSensitivity, 0.0F, 1.0F);
                }
            }
            
            if (!this._isDraggingSlider) {
                this._scrollPercent = float.Lerp(this._scrollPercent, this._targetScrollPercent, (float) (this.ScrollLerpSpeed * delta));
            }
            
            this._scrollPercent = Math.Clamp(this._scrollPercent, 0.0F, 1.0F);
            this._targetScrollPercent = Math.Clamp(this._targetScrollPercent, 0.0F, 1.0F);
            
            if (Input.IsMouseButtonDown(MouseButton.Left)) {
                if (localSliderBarRect.Contains(localMouse)) {
                    this._isDraggingSlider = true;
                }
                
                if (this._isDraggingSlider) {
                    float sliderHeight = this.Data.SliderSourceRect.Height * scale.Y;
                    float usableSliderBarHeight = sliderBarHeight - sliderHeight;
                    float relativeY = localMouse.Y - sliderHeight / 2.0F;
                    
                    this._scrollPercent = usableSliderBarHeight > 0.0F ? Math.Clamp(relativeY / usableSliderBarHeight, 0.0F, 1.0F) : 0.0F;
                    this._targetScrollPercent = this._scrollPercent;
                }
            }
            else {
                this._isDraggingSlider = false;
            }
        }
        else {
            this._isDraggingSlider = false;
        }
        
        // Update content elements.
        foreach (GuiElement element in this._content.Values) {
            element.AfterUpdate(delta);
        }
    }
    
    protected internal override void AfterUpdate(double delta) {
        base.AfterUpdate(delta);
        
        foreach (GuiElement element in this._content.Values) {
            element.AfterUpdate(delta);
        }
    }
    
    protected internal override void FixedUpdate(double fixedStep) {
        base.FixedUpdate(fixedStep);
        
        foreach (GuiElement element in this._content.Values) {
            element.FixedUpdate(fixedStep);
        }
    }
    
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        Color menuColor = this.IsHovered ? this.Data.MenuHoverColor : this.Data.MenuColor;
        
        if (!this.Interactable) {
            menuColor = this.Data.DisabledMenuColor;
        }
        
        Vector2 originalSize = this.Size;
        
        // Draw the menu smaller, leaving space on the right side for the slider bar.
        this.Size = new Vector2(MathF.Max(0.0F, originalSize.X - this.Data.SliderBarWidth), originalSize.Y);
        
        switch (this.Data.MenuResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(context.SpriteBatch, this.Data.MenuTexture, this.Data.MenuSampler, this.Data.MenuSourceRect, menuColor, this.Data.MenuFlip, this.Data.MenuPixelSnap);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(context.SpriteBatch, this.Data.MenuTexture, this.Data.MenuSampler, this.Data.MenuSourceRect, this.Data.MenuBorderInsets, this.Data.MenuResizeMode == ResizeMode.TileCenter, menuColor, this.Data.MenuFlip, this.Data.MenuPixelSnap);
                break;
        }
        
        this.Size = originalSize;
        
        // Draw slider bar.
        this.DrawSliderBar(context.SpriteBatch);
        
        // Draw slider.
        this.DrawSlider(context.SpriteBatch);
        
        context.SpriteBatch.End();
        
        // TODO: ADD METHOD CALLED DrawContent that handles the content drawing with the rendertarget (every content element gets drawed on this rendertarget)
    }
    
    protected internal override void Resize(Rectangle rectangle) {
        base.Resize(rectangle);
        
        foreach (GuiElement element in this._content.Values) {
            element.Resize(rectangle);
        }
        
        // TODO: RESIZE RENDERTARGET HERE.
    }
    
    /// <summary>
    /// Gets all content elements contained by this scroll view.
    /// </summary>
    /// <returns>The content elements.</returns>
    public OrderedDictionary<string, GuiElement>.ValueCollection GetContent() {
        return this._content.Values;
    }
    
    /// <summary>
    /// Determines whether content with the specified name exists.
    /// </summary>
    /// <param name="name">The content name.</param>
    /// <returns><c>true</c> if the content exists; otherwise, <c>false</c>.</returns>
    public bool HasContent(string name) {
        return this._content.ContainsKey(name);
    }
    
    /// <summary>
    /// Gets content by name.
    /// </summary>
    /// <param name="name">The content name.</param>
    /// <returns>The content element if found; otherwise, <c>null</c>.</returns>
    public GuiElement? GetContent(string name) {
        return this.TryGetContent(name, out GuiElement? element) ? element : null;
    }
    
    /// <summary>
    /// Attempts to get content by name.
    /// </summary>
    /// <param name="name">The content name.</param>
    /// <param name="element">The found element, or null.</param>
    /// <returns><c>true</c> if found; otherwise, <c>false</c>.</returns>
    public bool TryGetContent(string name, [NotNullWhen(true)] out GuiElement? element) {
        return this._content.TryGetValue(name, out element);
    }
    
    /// <summary>
    /// Adds a GUI element as scroll view content.
    /// The element is owned by this scroll view and must not be added to the GUI directly.
    /// </summary>
    /// <param name="name">The unique content name.</param>
    /// <param name="element">The element to add.</param>
    public void AddContent(string name, GuiElement element) {
        if (!this.TryAddContent(name, element)) {
            throw new Exception($"The content element with the name [{name}] is already present in the scroll view or belongs to another GUI.");
        }
    }
    
    /// <summary>
    /// Attempts to add a GUI element as scroll view content.
    /// </summary>
    /// <param name="name">The unique content name.</param>
    /// <param name="element">The element to add.</param>
    /// <returns><c>true</c> if the element was added; otherwise, <c>false</c>.</returns>
    public bool TryAddContent(string name, GuiElement element) {
        if (name == string.Empty) {
            return false;
        }
        
        if (this._content.ContainsKey(name)) {
            return false;
        }
        
        if (this._content.Values.Contains(element)) {
            return false;
        }
        
        if (element.Gui != null!) {
            return false;
        }

        element.Gui = this.Gui;
        element.Name = name;
        element.Init();
        
        // Forces an immediate recalculation of position and size to avoid a first-tick flicker at (0, 0).
        element.UpdatePosAndSize();
        
        this._contentToAdd.Add(element);
        return true;
    }
    
    /// <summary>
    /// Removes a content element from the scroll view.
    /// </summary>
    /// <param name="element">The Gui Element of the content element to be removed.</param>
    /// <exception cref="Exception">Thrown when the specified content element cannot be removed from the scroll view. </exception>
    public void RemoveContent(GuiElement element) {
        if (!this.TryRemoveContent(element)) {
            throw new Exception($"Failed to remove content element [{element.Name}] from scroll view [{this.Name}].");
        }
    }
    
    /// <summary>
    /// Attempts to remove the content element.
    /// </summary>
    /// <param name="element">The content element.</param>
    /// <returns><c>true</c> if removed; otherwise, <c>false</c>.</returns>
    public bool TryRemoveContent(GuiElement element) {
        if (element.Name == string.Empty) {
            return false;
        }
        
        if (!this._content.ContainsKey(element.Name)) {
            return false;
        }
        
        if (this._contentToRemove.Contains(element.Name)) {
            return false;
        }
        
        this._contentToRemove.Add(element.Name);
        return true;
    }
    
    /// <summary>
    /// Removes a content element identified by its name from the scroll view.
    /// </summary>
    /// <param name="name">The name of the content element to be removed.</param>
    /// <exception cref="Exception">Thrown when the specified content element cannot be removed from the scroll view. </exception>
    public void RemoveContent(string name) {
        if (!this.TryRemoveContent(name)) {
            throw new Exception($"Failed to remove content element [{name}] from scroll view [{this.Name}].");
        }
    }
    
    /// <summary>
    /// Attempts to remove content by name.
    /// </summary>
    /// <param name="name">The content name.</param>
    /// <returns><c>true</c> if removed; otherwise, <c>false</c>.</returns>
    public bool TryRemoveContent(string name) {
        if (name == string.Empty) {
            return false;
        }
        
        if (!this._content.ContainsKey(name)) {
            return false;
        }
        
        if (this._contentToRemove.Contains(name)) {
            return false;
        }
        
        this._contentToRemove.Add(name);
        return true;
    }
    
        /// <summary>
    /// Draws a texture at the specified position with optional scaling, rotation, and flipping.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used for drawing the texture.</param>
    /// <param name="texture">The texture to be rendered.</param>
    /// <param name="sampler">Optional sampler state specifying the sampling behavior for texture rendering.</param>
    /// <param name="sourceRect">The source rectangle defining the region of the texture to draw.</param>
    /// <param name="color">The color tint to apply to the texture during rendering.</param>
    /// <param name="flip">Specifies the sprite flipping options for the texture.</param>
    /// <param name="pixelSnap">A boolean specifying whether to align the texture to pixel boundaries.</param>
    private void DrawNormal(SpriteBatch spriteBatch, Texture2D texture, Sampler? sampler, Rectangle sourceRect, Color color, SpriteFlip flip, bool pixelSnap) {
        if (sampler != null) spriteBatch.PushSampler(sampler);
        spriteBatch.DrawTexture(texture, this.Position, 0.5F, sourceRect, this.Scale * this.Gui.ScaleFactor, this.Origin, pixelSnap, this.Rotation, color, flip);
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
    /// <param name="pixelSnap">A boolean specifying whether to align the texture to pixel boundaries.</param>
    private void DrawNineSlice(SpriteBatch spriteBatch, Texture2D texture, Sampler? sampler, Rectangle sourceRect, BorderInsets borderInsets, bool tileCenter, Color color, SpriteFlip flip, bool pixelSnap) {
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
        
        // Push sampler.
        if (sampler != null) spriteBatch.PushSampler(sampler);
        
        // Draw Corners.
        spriteBatch.DrawTexture(texture, position, 0.5F, sourceTopLeft, scale, pivot / scale, false, this.Rotation, color, flip);
        spriteBatch.DrawTexture(texture, position, 0.5F, sourceTopRight, scale, (pivot - new Vector2(finalSize.X - rightW, 0.0F)) / scale, false, this.Rotation, color, flip);
        spriteBatch.DrawTexture(texture, position, 0.5F, sourceBottomLeft, scale, (pivot - new Vector2(0.0F, finalSize.Y - bottomH)) / scale, false, this.Rotation, color, flip);
        spriteBatch.DrawTexture(texture, position, 0.5F, sourceBottomRight, scale, (pivot - new Vector2(finalSize.X - rightW, finalSize.Y - bottomH)) / scale, false, this.Rotation, color, flip);
        
        // Draw Edges.
        if (innerH > 0.0F) {
            if (tileCenter) {
                float tileH = sourceLeft.Height * scale.Y;
                for (float y = 0.0F; y < innerH; y += tileH) {
                    float drawH = MathF.Min(tileH, innerH - y);
                    Rectangle cL = new Rectangle(sourceLeft.X, sourceLeft.Y, sourceLeft.Width, (int) MathF.Ceiling(drawH / scale.Y));
                    Rectangle cR = new Rectangle(sourceRight.X, sourceRight.Y, sourceRight.Width, (int) MathF.Ceiling(drawH / scale.Y));
                    spriteBatch.DrawTexture(texture, position, 0.5F, cL, scale, (pivot - new Vector2(0.0F, topH + y)) / scale, false, this.Rotation, color, flip);
                    spriteBatch.DrawTexture(texture, position, 0.5F, cR, scale, (pivot - new Vector2(finalSize.X - rightW, topH + y)) / scale, false, this.Rotation, color, flip);
                }
            }
            else {
                Vector2 sV = new Vector2(scale.X, innerH / sourceLeft.Height);
                spriteBatch.DrawTexture(texture, position, 0.5F, sourceLeft, sV, (pivot - new Vector2(0.0F, topH)) / sV, false, this.Rotation, color, flip);
                spriteBatch.DrawTexture(texture, position, 0.5F, sourceRight, sV, (pivot - new Vector2(finalSize.X - rightW, topH)) / sV, false, this.Rotation, color, flip);
            }
        }
        
        if (innerW > 0.0F) {
            if (tileCenter) {
                float tileW = sourceTop.Width * scale.X;
                for (float x = 0.0F; x < innerW; x += tileW) {
                    float drawW = MathF.Min(tileW, innerW - x);
                    Rectangle cT = new Rectangle(sourceTop.X, sourceTop.Y, (int) MathF.Max(1.0F, MathF.Round(drawW / scale.X)), sourceTop.Height);
                    Rectangle cB = new Rectangle(sourceBottom.X, sourceBottom.Y, (int) MathF.Max(1.0F, MathF.Round(drawW / scale.X)), sourceBottom.Height);
                    spriteBatch.DrawTexture(texture, position, 0.5F, cT, scale, (pivot - new Vector2(leftW + x, 0.0F)) / scale, false, this.Rotation, color, flip);
                    spriteBatch.DrawTexture(texture, position, 0.5F, cB, scale, (pivot - new Vector2(leftW + x, finalSize.Y - bottomH)) / scale, false, this.Rotation, color, flip);
                }
            }
            else {
                int clipW = Math.Min(sourceTop.Width, (int) MathF.Ceiling(innerW / scale.X));
                Rectangle cT = new Rectangle(sourceTop.X, sourceTop.Y, clipW, sourceTop.Height);
                Rectangle cB = new Rectangle(sourceBottom.X, sourceBottom.Y, clipW, sourceBottom.Height);
                Vector2 sH = (innerW > sourceTop.Width * scale.X) ? new Vector2(innerW / sourceTop.Width, scale.Y) : scale;
                spriteBatch.DrawTexture(texture, position, 0.5F, cT, sH, (pivot - new Vector2(leftW, 0.0F)) / sH, false, this.Rotation, color, flip);
                spriteBatch.DrawTexture(texture, position, 0.5F, cB, sH, (pivot - new Vector2(leftW, finalSize.Y - bottomH)) / sH, false, this.Rotation, color, flip);
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
                        spriteBatch.DrawTexture(texture, position, 0.5F, cC, scale, (pivot - new Vector2(leftW + x, topH + y)) / scale, false, this.Rotation, color, flip);
                    }
                }
            }
            else {
                int clipW = Math.Min(sourceCenter.Width, (int) MathF.Ceiling(innerW / scale.X));
                Rectangle cC = new Rectangle(sourceCenter.X, sourceCenter.Y, clipW, sourceCenter.Height);
                Vector2 sC = (innerW > sourceCenter.Width * scale.X) ? new Vector2(innerW / sourceCenter.Width, innerH / sourceCenter.Height) : new Vector2(scale.X, innerH / sourceCenter.Height);
                spriteBatch.DrawTexture(texture, position, 0.5F, cC, sC, (pivot - new Vector2(leftW, topH)) / sC, false, this.Rotation, color, flip);
            }
        }
        
        // Pop sampler.
        if (sampler != null) spriteBatch.PopSampler();
    }
    
    /// <summary>
    /// Draws the slider bar track on the right side of the scroll view.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to render the slider bar.</param>
    private void DrawSliderBar(SpriteBatch spriteBatch) {
        Vector2 originalSize = this.Size;
        Vector2 originalOrigin = this.Origin;
        
        this.Size = new Vector2(this.Data.SliderBarWidth, originalSize.Y);
        this.Origin = originalOrigin - new Vector2(originalSize.X - this.Data.SliderBarWidth, 0.0F);
        
        Color color = this.IsHovered ? this.Data.SliderBarHoverColor : this.Data.SliderBarColor;
        
        if (!this.Interactable) {
            color = this.Data.DisabledSliderBarColor;
        }
        
        switch (this.Data.SliderBarResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(spriteBatch, this.Data.SliderBarTexture, this.Data.SliderBarSampler, this.Data.SliderBarSourceRect, color, this.Data.SliderBarFlip, this.Data.SliderBarPixelSnap);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(spriteBatch, this.Data.SliderBarTexture, this.Data.SliderBarSampler, this.Data.SliderBarSourceRect, this.Data.SliderBarBorderInsets, this.Data.SliderBarResizeMode == ResizeMode.TileCenter, color, this.Data.SliderBarFlip, this.Data.SliderBarPixelSnap);
                break;
        }
        
        this.Size = originalSize;
        this.Origin = originalOrigin;
    }
    
    /// <summary>
    /// Draws the slider handle inside the slider bar.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to render the slider.</param>
    private void DrawSlider(SpriteBatch spriteBatch) {
        float sliderBarHeight = this.Size.Y;
        float sliderHeight = this.Data.SliderSourceRect.Height;
        float sliderRange = MathF.Max(0.0F, sliderBarHeight - sliderHeight);

        float xOffset = this.Size.X - this.Data.SliderBarWidth + (this.Data.SliderBarWidth - this.Data.SliderSourceRect.Width) / 2.0F;
        float yOffset = sliderRange * this._scrollPercent;
        Vector2 origin = this.Origin - new Vector2(xOffset, yOffset);

        Color color = this.IsHovered ? this.Data.SliderHoverColor : this.Data.SliderColor;

        if (!this.Interactable) {
            color = this.Data.DisabledSliderColor;
        }

        if (this.Data.SliderSampler != null) spriteBatch.PushSampler(this.Data.SliderSampler);
        spriteBatch.DrawTexture(this.Data.SliderTexture, this.Position, 0.5F, this.Data.SliderSourceRect, this.Scale * this.Gui.ScaleFactor, origin, this.Data.SliderPixelSnap, this.Rotation, color, this.Data.SliderFlip);
        if (this.Data.SliderSampler != null) spriteBatch.PopSampler();
    }
    
    private void DrawContent(GraphicsContext context, Framebuffer framebuffer) {
        
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            
        }
    }
}