using System.Numerics;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Mathematics;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Batching;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrith;
using Color = Bliss.CSharp.Colors.Color;

namespace Sparkle.CSharp.GUI.Elements;

public class TextureDropDownElement : GuiElement {
    
    /// <summary>
    /// The visual and texture configuration used to render the dropdown field and menu.
    /// </summary>
    public TextureDropDownData DropDownData { get; private set; }
    
    /// <summary>
    /// The list of selectable options displayed in the dropdown menu.
    /// </summary>
    public List<LabelData> Options { get; private set; }
    
    /// <summary>
    /// The maximum number of dropdown menu options visible at once.
    /// </summary>
    public int MaxVisibleOptions { get; private set; }
    
    /// <summary>
    /// The currently selected option from the dropdown menu.
    /// </summary>
    public LabelData? SelectedOption { get; private set; }
    
    /// <summary>
    /// The text alignment used for the selected value displayed in the dropdown field.
    /// </summary>
    public TextAlignment FieldTextAlignment;
    
    /// <summary>
    /// The text alignment used for options displayed in the dropdown menu.
    /// </summary>
    public TextAlignment MenuTextAlignment;
    
    /// <summary>
    /// The offset applied to the text displayed in the dropdown field.
    /// </summary>
    public Vector2 FieldTextOffset;
    
    /// <summary>
    /// The offset applied to the text displayed in the dropdown menu items.
    /// </summary>
    public Vector2 MenuTextOffset;

    /// <summary>
    /// The offset applied to the slider element used within the dropdown component.
    /// </summary>
    public Vector2 SliderOffset;
    
    /// <summary>
    /// The scale applied to the text displayed in the dropdown field, determining its size and proportions.
    /// </summary>
    public Vector2 FieldTextScale;
    
    /// <summary>
    /// The scale applied to the text displayed in the dropdown menu items.
    /// </summary>
    public Vector2 MenuTextScale;
    
    /// <summary>
    /// The offset applied to the dropdown arrow indicator.
    /// </summary>
    public Vector2 ArrowOffset;
    
    /// <summary>
    /// The insets applied to the dropdown menu content mask, used to keep option text and hover highlights away from the menu outline.
    /// </summary>
    public (float Left, float Right, float Top, float Bottom) MenuContentInsets;
    
    /// <summary>
    /// The sensitivity of the dropdown scroll when navigating through options.
    /// </summary>
    public float ScrollSensitivity;
    
    /// <summary>
    /// The interpolation speed at which the menu scrolls to a target position.
    /// </summary>
    public float ScrollLerpSpeed;
    
    /// <summary>
    /// The dropdown menu is currently open.
    /// </summary>
    public bool IsMenuOpen { get; private set; }
    
    /// <summary>
    /// Triggered when the selected option in the dropdown menu changes.
    /// </summary>
    public event Action<LabelData>? OptionChanged;

    /// <summary>
    /// Event triggered when the dropdown menu is toggled open or closed.
    /// </summary>
    public event Action<bool>? MenuToggled;
    
    /// <summary>
    /// The current rotational state of the dropdown arrow, used for visual animation when opening or closing the menu.
    /// </summary>
    private float _arrowRotation;
    
    /// <summary>
    /// The current scroll offset for the dropdown menu.
    /// </summary>
    private float _scrollPercent;
    
    /// <summary>
    /// The target scroll offset for smooth scrolling.
    /// </summary>
    private float _targetScrollPercent;
    
    /// <summary>
    /// Indicates if the scroller is currently being dragged.
    /// </summary>
    private bool _isDraggingSlider;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureDropDownElement"/> class.
    /// </summary>
    /// <param name="dropDownData">The visual and texture data used to render the dropdown field and menu.</param>
    /// <param name="options">The list of label data entries representing selectable options.</param>
    /// <param name="maxVisibleOptions">The maximum number of options visible in the dropdown menu at a time.</param>
    /// <param name="anchor">The anchor point used to position the dropdown element.</param>
    /// <param name="offset">The offset from the anchor position.</param>
    /// <param name="fieldTextAlignment">The text alignment used for the selected value displayed in the field.</param>
    /// <param name="menuTextAlignment">The text alignment used for options displayed in the dropdown menu.</param>
    /// <param name="fieldTextOffset">The offset applied to the field text.</param>
    /// <param name="menuTextOffset">The offset applied to the menu option text.</param>
    /// <param name="fieldTextScale">Scaling factor applied to the text within the dropdown field.</param>
    /// <param name="menuTextScale">Scaling factor applied to the text within the dropdown menu options.</param>
    /// <param name="sliderOffset">An optional offset applied to the position of the slider within the dropdown menu.</param>
    /// <param name="arrowOffset">The offset applied to the dropdown arrow indicator.</param>
    /// <param name="menuContentInsets">Defines the padding used to clip menu option text and hover highlights away from the menu outline.</param>
    /// <param name="scrollSensitivity">Indicates how sensitive the dropdown menu scrolling is to user input.</param>
    /// <param name="scrollLerpSpeed">Specifies the speed at which the dropdown menu's scroll position interpolates to the target position.</param>
    /// <param name="size">The size of the dropdown element.</param>
    /// <param name="scale">Optional scale applied to the dropdown element.</param>
    /// <param name="origin">Optional origin point used for rotation and alignment.</param>
    /// <param name="rotation">The rotation of the dropdown element in radians.</param>
    /// <param name="renderOrder">The order in which the element is rendered, relative to others.</param>
    /// <param name="clickFunc">Optional function invoked when the dropdown field is clicked.</param>
    public TextureDropDownElement(
        TextureDropDownData dropDownData,
        List<LabelData> options,
        int maxVisibleOptions,
        Anchor anchor,
        Vector2 offset,
        TextAlignment fieldTextAlignment = TextAlignment.Left,
        TextAlignment menuTextAlignment = TextAlignment.Left,
        Vector2? fieldTextOffset = null,
        Vector2? menuTextOffset = null,
        Vector2? fieldTextScale = null,
        Vector2? menuTextScale = null,
        Vector2? sliderOffset = null,
        Vector2? arrowOffset = null,
        (float Left, float Right, float Top, float Bottom)? menuContentInsets = null,
        float scrollSensitivity = 0.1F,
        float scrollLerpSpeed = 10.0F,
        Vector2? size = null,
        Vector2? scale = null,
        Vector2? origin = null,
        float rotation = 0.0F,
        int renderOrder = 0,
        Func<GuiElement, bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, renderOrder, clickFunc) {
        this.DropDownData = dropDownData;
        this.Options = options;
        this.MaxVisibleOptions = Math.Max(2, maxVisibleOptions);
        this.SelectedOption = this.Options.FirstOrDefault();
        this.FieldTextAlignment = fieldTextAlignment;
        this.MenuTextAlignment = menuTextAlignment;
        this.FieldTextOffset = fieldTextOffset ?? Vector2.Zero;
        this.MenuTextOffset = menuTextOffset ?? Vector2.Zero;
        this.FieldTextScale = fieldTextScale ?? Vector2.One;
        this.MenuTextScale = menuTextScale ?? Vector2.One;
        this.SliderOffset = sliderOffset ?? Vector2.Zero;
        this.ArrowOffset = arrowOffset ?? new Vector2(10.0F, 0.0F);
        this.MenuContentInsets = menuContentInsets ?? (0.0F, 0.0F, 0.0F, 0.0F);
        this.ScrollSensitivity = scrollSensitivity;
        this.ScrollLerpSpeed = scrollLerpSpeed;
        this.Size = size ?? new Vector2(dropDownData.FieldSourceRect.Width, dropDownData.FieldSourceRect.Height);
    }
    
    /// <summary>
    /// Updates the state of the texture-based dropdown element, including handling user interactions such as clicking
    /// and selecting options, and managing the visibility of the dropdown menu.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update, in seconds, used for timing-related logic.</param>
    /// <param name="interactionHandled">A reference to a boolean tracking whether interaction has already been handled by another element.</param>
    protected internal override void Update(double delta, ref bool interactionHandled) {
        base.Update(delta, ref interactionHandled);
        
        // Arrow animation.
        float targetRotation = this.IsMenuOpen ? MathF.PI : 0.0F;
        float animationSpeed = 15.0F;
        
        if (MathF.Abs(this._arrowRotation - targetRotation) > 0.001F) {
            this._arrowRotation = float.Lerp(this._arrowRotation, targetRotation, (float) (animationSpeed * delta));
        }
        else {
            this._arrowRotation = targetRotation;
        }
        
        // Reset scrolling if the menu closed.
        if (!this.IsMenuOpen) {
            this._scrollPercent = 0.0F;
            this._targetScrollPercent = 0.0F;
            this._isDraggingSlider = false;
        }
        
        // Handle scrolling and dragging.
        if (this.IsMenuOpen && this.Options.Count > this.MaxVisibleOptions) {
            Vector2 mousePos = Input.GetMousePosition();
            Vector2 scale = this.Scale * this.Gui.ScaleFactor;
            Vector2 fieldSize = this.Size * scale;
            
            // Transform the mouse pos into local space.
            Matrix3x2 transform = Matrix3x2.CreateTranslation(-this.Position) *
                                  Matrix3x2.CreateRotation(-float.DegreesToRadians(this.Rotation)) *
                                  Matrix3x2.CreateTranslation(this.Origin * scale);
            
            Vector2 localMouse = Vector2.Transform(mousePos, transform);
            
            // Define scrollbar track rectangle.
            float scrollBarWidth = this.DropDownData.SliderBarWidth * scale.X;
            float scrollBarHeight = fieldSize.Y * this.MaxVisibleOptions;
            
            // Simple local hit-testing.
            RectangleF localMenuRect = new RectangleF(0.0F, fieldSize.Y, fieldSize.X, scrollBarHeight);
            RectangleF localTrackRect = new RectangleF(fieldSize.X - scrollBarWidth, fieldSize.Y, scrollBarWidth, scrollBarHeight);
            
            // Handle mouse wheel.
            if (!this._isDraggingSlider && localMenuRect.Contains(localMouse)) {
                if (Input.IsMouseScrolling(out Vector2 wheelDelta)) {
                    float scrollableHeight = fieldSize.Y * MathF.Max(0, this.Options.Count - this.MaxVisibleOptions);
                    
                    if (scrollableHeight > 0.0F) {
                        float currentOffset = this._targetScrollPercent * scrollableHeight;
                        float wheelStep = MathF.Max(1.0F, (fieldSize.Y * this.MaxVisibleOptions) * this.ScrollSensitivity);
                        currentOffset = Math.Clamp(currentOffset - wheelDelta.Y * wheelStep, 0.0F, scrollableHeight);
                        this._targetScrollPercent = currentOffset / scrollableHeight;
                    }
                }
            }
            
            // Smoothly interpolate scroll percent.
            if (!this._isDraggingSlider) {
                this._scrollPercent = float.Lerp(this._scrollPercent, this._targetScrollPercent, (float) (this.ScrollLerpSpeed * delta));
            }
            
            this._scrollPercent = Math.Clamp(this._scrollPercent, 0.0F, 1.0F);
            this._targetScrollPercent = Math.Clamp(this._targetScrollPercent, 0.0F, 1.0F);
            
            // Handle dragging / clicking on scrollbar.
            if (Input.IsMouseButtonDown(MouseButton.Left)) {
                
                // Use localMouse for the initial click check
                if (localTrackRect.Contains(localMouse)) {
                    this._isDraggingSlider = true;
                }
                
                if (this._isDraggingSlider) {
                    float sliderHeight = this.DropDownData.SliderSourceRect.Height * scale.Y;
                    float trackTop = fieldSize.Y;
                    
                    // Calculate percentage using the localMouse.Y we already computed.
                    float usableTrackHeight = scrollBarHeight - sliderHeight;
                    float relativeY = localMouse.Y - trackTop - sliderHeight / 2.0F;
                    
                    this._scrollPercent = Math.Clamp(relativeY / usableTrackHeight, 0.0F, 1.0F);
                    this._targetScrollPercent = this._scrollPercent;
                }
            }
            else {
                this._isDraggingSlider = false;
            }
        }
        
        // Open/Close menu and clicking options.
        if (this.IsClicked) {
            Vector2 mousePos = Input.GetMousePosition();
            Vector2 fieldSize = this.Size * this.Scale * this.Gui.ScaleFactor;
            RectangleF fieldRect = new RectangleF(this.Position.X, this.Position.Y, fieldSize.X, fieldSize.Y);
            Vector2 fieldOrigin = this.Origin * this.Scale * this.Gui.ScaleFactor;
            
            if (fieldRect.Contains(mousePos, fieldOrigin, this.Rotation)) {
                this.IsMenuOpen = !this.IsMenuOpen;
                this.MenuToggled?.Invoke(this.IsMenuOpen);
            }
            else if (this.IsMenuOpen) {
                
                // Define the visible mask area for the menu.
                float maskHeight = fieldSize.Y * this.MaxVisibleOptions;
                RectangleF maskRect = new RectangleF(this.Position.X, this.Position.Y, fieldSize.X, maskHeight);
                Vector2 maskOrigin = fieldOrigin - new Vector2(0.0F, fieldSize.Y);
                
                // Check if the click is inside the visible part of the menu.
                if (maskRect.Contains(mousePos, maskOrigin, this.Rotation)) {
                    int visibleOptions = this.Options.Count;
                    float currentScrollIndex = 0.0F;
                    
                    if (this.Options.Count > this.MaxVisibleOptions) {
                        currentScrollIndex = this._scrollPercent * (this.Options.Count - this.MaxVisibleOptions);
                        visibleOptions = Math.Min(this.Options.Count, this.MaxVisibleOptions + 1);
                    }
                    
                    int startIndex = (int) Math.Floor(currentScrollIndex);
                    float scrollOffset = currentScrollIndex - startIndex;
                    
                    for (int i = 0; i < visibleOptions; i++) {
                        int optionIndex = startIndex + i;
                        
                        if (optionIndex >= this.Options.Count) {
                            break;
                        }
                        
                        Vector2 itemSize = new Vector2(fieldSize.X, fieldSize.Y);
                        
                        if (this.Options.Count > this.MaxVisibleOptions) {
                            itemSize.X -= this.DropDownData.SliderBarWidth * this.Scale.X * this.Gui.ScaleFactor;
                        }
                        
                        RectangleF itemRect = new RectangleF(this.Position.X, this.Position.Y, itemSize.X, itemSize.Y);
                        Vector2 itemOrigin = fieldOrigin - new Vector2(0.0F, itemSize.Y * (i - scrollOffset + 1.0F));
                        
                        if (itemRect.Contains(mousePos, itemOrigin, this.Rotation)) {
                            this.SelectedOption = this.Options[optionIndex];
                            this.IsMenuOpen = false;
                            this.OptionChanged?.Invoke(this.SelectedOption);
                            this.MenuToggled?.Invoke(this.IsMenuOpen);
                            break;
                        }
                    }
                }
                else {
                    
                    // Clicked outside the field and outside the visible menu, then close the menu.
                    this.IsMenuOpen = false;
                    this.MenuToggled?.Invoke(this.IsMenuOpen);
                }
            }
        }
        
        if (!this.IsSelected || this.Options.Count <= 0) {
            if (this.IsMenuOpen) {
                this.IsMenuOpen = false;
                this.MenuToggled?.Invoke(this.IsMenuOpen);
            }
        }
    }
    
    /// <summary>
    /// Submits the draw commands required to render the GUI element using the appropriate visual state and rendering mode.
    /// </summary>
    /// <param name="renderQueue">The render queue that collects and batches draw commands for later execution.</param>
    protected internal override void Draw(GuiRenderQueue renderQueue) {
        base.Draw(renderQueue);
        
        // Draw field texture.
        Color buttonColor = this.IsHovered ? this.DropDownData.FieldHoverColor : this.DropDownData.FieldColor;
        
        if (!this.Interactable) {
            buttonColor = this.DropDownData.DisabledFieldColor;
        }
        
        switch (this.DropDownData.FieldResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(renderQueue, this.DropDownData.FieldTexture, this.DropDownData.FieldSampler, this.DropDownData.FieldSourceRect, buttonColor, this.DropDownData.FieldFlip, this.DropDownData.FieldPixelSnap, this.DropDownData.Effect, this.DropDownData.BlendState);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(renderQueue, this.DropDownData.FieldTexture, this.DropDownData.FieldSampler, this.DropDownData.FieldSourceRect, this.DropDownData.FieldBorderInsets, this.DropDownData.FieldResizeMode == ResizeMode.TileCenter, buttonColor, this.DropDownData.FieldFlip, this.DropDownData.FieldPixelSnap, this.DropDownData.Effect, this.DropDownData.BlendState);
                break;
        }
        
        // Draw arrow texture.
        this.DrawArrow(renderQueue);
        
        // Draw field text.
        if (this.SelectedOption != null) {
            this.DrawText(renderQueue, this.SelectedOption, this.FieldTextAlignment, this.FieldTextOffset, this.FieldTextScale);
        }
        
        if (this.IsMenuOpen) {
            
            // Draw menu texture.
            Color menuColor = this.IsHovered ? this.DropDownData.MenuHoverColor : this.DropDownData.MenuColor;
            
            if (!this.Interactable) {
                menuColor = this.DropDownData.DisabledMenuColor;
            }
            
            this.DrawMenu(renderQueue, menuColor);
            
            // Draw the scroll bar.
            this.DrawSliderBar(renderQueue);
            
            // Draw the slider.
            this.DrawSlider(renderQueue);
        }
        
        if (this.IsMenuOpen) {
            
            // Draw highlight.
            this.DrawHighlight(renderQueue);
            
            // Draw options text.
            this.DrawOptionsText(renderQueue);
        }
    }
    
    /// <summary>
    /// Calculates the position of the texture drop-down element based on its anchor point, offset, scale, and other transformation factors.
    /// If the drop-down menu is closed, the position is determined using the base implementation.
    /// </summary>
    /// <returns>
    /// A <see cref="Vector2"/> representing the calculated position of the element on the screen.
    /// </returns>
    protected override Vector2 CalculatePos() {
        if (!this.IsMenuOpen) {
            return base.CalculatePos();
        }
        
        // We calculate the position based on the field size only, ignoring the extra height from the open menu.
        Vector2 pos = Vector2.Zero;
        int scaleFactor = this.Gui.ScaleFactor;
        Vector2 fieldScaledSize = this.Size * this.Scale * scaleFactor;
        
        float width = MathF.Floor((float) GlobalGraphicsAssets.Window.GetWidth() / scaleFactor) * scaleFactor;
        float height = MathF.Floor((float) GlobalGraphicsAssets.Window.GetHeight() / scaleFactor) * scaleFactor;
        
        switch (this.AnchorPoint) {
            case Anchor.TopLeft:
                break;
            
            case Anchor.TopCenter:
                pos.X = width / 2.0F - fieldScaledSize.X / 2.0F;
                break;
            
            case Anchor.TopRight:
                pos.X = width - fieldScaledSize.X;
                break;
            
            case Anchor.CenterLeft:
                pos.Y = height / 2.0F - fieldScaledSize.Y / 2.0F;
                break;
            
            case Anchor.Center:
                pos.X = width / 2.0F - fieldScaledSize.X / 2.0F;
                pos.Y = height / 2.0F - fieldScaledSize.Y / 2.0F;
                break;
            
            case Anchor.CenterRight:
                pos.X = width - fieldScaledSize.X;
                pos.Y = height / 2.0F - fieldScaledSize.Y / 2.0F;
                break;
            
            case Anchor.BottomLeft:
                pos.Y = height - fieldScaledSize.Y;
                break;
            
            case Anchor.BottomCenter:
                pos.X = width / 2.0F - fieldScaledSize.X / 2.0F;
                pos.Y = height - fieldScaledSize.Y;
                break;
            
            case Anchor.BottomRight:
                pos.X = width - fieldScaledSize.X;
                pos.Y = height - fieldScaledSize.Y;
                break;
        }
        
        Vector2 finalPos = pos + (this.Offset * scaleFactor) + this.Origin;
        return finalPos / scaleFactor * scaleFactor;
    }
    
    /// <summary>
    /// Calculates the size of the texture drop-down element, taking into account whether the drop-down menu is open or closed.
    /// </summary>
    /// <returns>
    /// A <see cref="Vector2"/> representing the size of the element. If the menu is open, the size is adjusted to include the additional height for the drop-down options.
    /// </returns>
    protected override Vector2 CalculateSize() {
        Vector2 baseSize = base.CalculateSize();
        
        if (this.IsMenuOpen) {
            int visibleOptions = Math.Min(this.Options.Count, this.MaxVisibleOptions);
            return new Vector2(baseSize.X, baseSize.Y * (visibleOptions + 1));
        }
        
        return baseSize;
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
        SpriteGuiRenderState renderState = new SpriteGuiRenderState(sampler, effect, blendState, depthStencilState);
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
        SpriteGuiRenderState renderState = new SpriteGuiRenderState(sampler, effect, blendState, depthStencilState);
        
        // Draw Corners.
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
    /// Draws the dropdown menu, including its background, resizing behavior, and the labels of its options, using the specified sprite batch and menu color.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    /// <param name="color">The color applied to the menu background during rendering.</param>
    /// <param name="depthStencilState">Optional depth-stencil state used for depth testing and stencil operations during rendering. If <c>null</c>, the default state is used.</param>
    private void DrawMenu(GuiRenderQueue renderQueue, Color color, DepthStencilStateDescription? depthStencilState = null) {
        Vector2 originalSize = this.Size;
        Vector2 originalOrigin = this.Origin;
        
        int visibleOptions = this.Options.Count;
        
        if (this.Options.Count > this.MaxVisibleOptions) {
            visibleOptions = this.MaxVisibleOptions;
        }
        
        // Calculate menu size.
        this.Size = new Vector2(this.Size.X, this.Size.Y * visibleOptions);
        
        // Changing the size of the menu when the scrollbar appears.
        if (this.Options.Count > this.MaxVisibleOptions) {
            this.Size.X -= this.DropDownData.SliderBarWidth;
        }
        
        // Calculate the origin so it draws below the field.
        this.Origin = originalOrigin - new Vector2(0.0F, originalSize.Y);
        
        switch (this.DropDownData.MenuResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(renderQueue, this.DropDownData.MenuTexture, this.DropDownData.MenuSampler, this.DropDownData.MenuSourceRect, color, this.DropDownData.MenuFlip, this.DropDownData.MenuPixelSnap, this.DropDownData.Effect, this.DropDownData.BlendState, depthStencilState);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(renderQueue, this.DropDownData.MenuTexture, this.DropDownData.MenuSampler, this.DropDownData.MenuSourceRect, this.DropDownData.MenuBorderInsets, this.DropDownData.MenuResizeMode == ResizeMode.TileCenter, color, this.DropDownData.MenuFlip, this.DropDownData.MenuPixelSnap, this.DropDownData.Effect, this.DropDownData.BlendState, depthStencilState);
                break;
        }
        
        // Restore original values.
        this.Size = originalSize;
        this.Origin = originalOrigin;
    }
    
    /// <summary>
    /// Renders the text of the dropdown menu options onto the screen, managing visible options and applying a clipping mask for scrolling.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    private void DrawOptionsText(GuiRenderQueue renderQueue) {
        int visibleOptions = this.Options.Count;
        float currentScrollIndex = 0.0F;
        
        if (this.Options.Count > this.MaxVisibleOptions) {
            currentScrollIndex = Math.Clamp(this._scrollPercent * (this.Options.Count - this.MaxVisibleOptions), 0.0F, this.Options.Count - 1.0F);
            visibleOptions = this.MaxVisibleOptions + 1;
        }
        
        // Determine the starting index and the fractional offset for scrolling.
        int startIndex = (int) Math.Floor(currentScrollIndex);
        float scrollOffset = currentScrollIndex - startIndex;
        
        Vector2 fieldSize = this.Size * this.Scale * this.Gui.ScaleFactor;
        Vector2 scale = this.Scale * this.Gui.ScaleFactor;
        
        float maskWidth = fieldSize.X;
        
        // Reduce mask width if the scroll bar is present.
        if (this.Options.Count > this.MaxVisibleOptions) {
            maskWidth -= this.DropDownData.SliderBarWidth * scale.X;
        }
        
        Vector2 contentInsetTopLeft = new Vector2(this.MenuContentInsets.Left * scale.X, this.MenuContentInsets.Top * scale.Y);
        
        float contentMaskWidth = MathF.Max(0.0F, maskWidth - (this.MenuContentInsets.Left + this.MenuContentInsets.Right) * scale.X);
        float contentMaskHeight = MathF.Max(0.0F, fieldSize.Y * this.MaxVisibleOptions - (this.MenuContentInsets.Top + this.MenuContentInsets.Bottom) * scale.Y);
        
        // Define the clipping mask area for the menu content.
        RectangleF maskRect = new RectangleF(this.Position.X, this.Position.Y, contentMaskWidth, contentMaskHeight);
        Vector2 maskOrigin = this.Origin * scale - new Vector2(0.0F, fieldSize.Y) - contentInsetTopLeft;
        
        DepthStencilStateDescription stencilWrite = new DepthStencilStateDescription() {
            StencilTestEnabled = true,
            StencilWriteMask = 0xFF,
            StencilReference = 2,
            StencilFront = new StencilBehaviorDescription() {
                Comparison = ComparisonKind.Always,
                Pass = StencilOperation.Replace
            },
            StencilBack = new StencilBehaviorDescription() {
                Comparison = ComparisonKind.Always,
                Pass = StencilOperation.Replace
            }
        };
        
        // Write to the stencil buffer to mark the menu content mask area.
        PrimitiveGuiRenderState primitiveRenderState = new PrimitiveGuiRenderState(depthStencilState: stencilWrite);
        renderQueue.UsePrimitive(primitiveRenderState).DrawFilledRectangle(maskRect, maskOrigin, this.Rotation, 0.5F, new Color(255, 255, 255, 0));
        
        DepthStencilStateDescription stencilTest = new DepthStencilStateDescription() {
            StencilTestEnabled = true,
            StencilReadMask = 0xFF,
            StencilReference = 2,
            StencilFront = new StencilBehaviorDescription() {
                Comparison = ComparisonKind.Equal,
                Pass = StencilOperation.Keep
            },
            StencilBack = new StencilBehaviorDescription() {
                Comparison = ComparisonKind.Equal,
                Pass = StencilOperation.Keep
            }
        };
        
        // Draw each option label, clipped by the menu content mask.
        for (int i = 0; i < visibleOptions; i++) {
            int optionIndex = startIndex + i;
            
            if (optionIndex < 0 || optionIndex >= this.Options.Count) {
                break;
            }
            
            Vector2 itemOffset = this.MenuTextOffset + new Vector2(0.0F, this.Size.Y * (i - scrollOffset + 1.0F));
            
            if (this.Options.Count > this.MaxVisibleOptions) {
                switch (this.MenuTextAlignment) {
                    case TextAlignment.Center:
                        itemOffset.X -= this.DropDownData.SliderBarWidth / 2.0F;
                        break;
                    case TextAlignment.Right:
                        itemOffset.X -= this.DropDownData.SliderBarWidth;
                        break;
                }
            }
            
            this.DrawText(renderQueue, this.Options[optionIndex], this.MenuTextAlignment, itemOffset, this.MenuTextScale, stencilTest);
        }
    }
    
    /// <summary>
    /// Renders a visual highlight for the dropdown menu, including the visible options and hover effects.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    private void DrawHighlight(GuiRenderQueue renderQueue) {
        Vector2 fieldSize = this.Size * this.Scale * this.Gui.ScaleFactor;
        Vector2 scale = this.Scale * this.Gui.ScaleFactor;
        
        int visibleOptions = this.Options.Count;
        float currentScrollIndex = 0.0F;
        
        // Calculate visible options and adjust field size if the scrollbar is active.
        if (this.Options.Count > this.MaxVisibleOptions) {
            fieldSize.X -= this.DropDownData.SliderBarWidth * scale.X;
            currentScrollIndex = this._scrollPercent * (this.Options.Count - this.MaxVisibleOptions);
            visibleOptions = Math.Min(this.Options.Count, this.MaxVisibleOptions + 1);
        }
        
        // Determine the starting index and offset for scrolling.
        int startIndex = (int) Math.Floor(currentScrollIndex);
        float scrollOffset = currentScrollIndex - startIndex;
        
        float menuMaskHeight = fieldSize.Y * this.MaxVisibleOptions;
        
        // Keep the full menu area for mouse hit-testing.
        RectangleF menuHitRect = new RectangleF(this.Position.X, this.Position.Y, fieldSize.X, menuMaskHeight);
        Vector2 menuHitOrigin = this.Origin * scale - new Vector2(0.0F, fieldSize.Y);
        
        // Check if the mouse is inside the visible menu area first.
        if (!menuHitRect.Contains(Input.GetMousePosition(), menuHitOrigin, this.Rotation)) {
            return;
        }
        
        // Create a smaller highlight mask so the highlight does not cover the menu outline.
        Vector2 highlightInsetTopLeft = new Vector2(this.MenuContentInsets.Left * scale.X, this.MenuContentInsets.Top * scale.Y);
        
        float highlightMaskWidth = MathF.Max(0.0F, fieldSize.X - (this.MenuContentInsets.Left + this.MenuContentInsets.Right) * scale.X);
        float highlightMaskHeight = MathF.Max(0.0F, menuMaskHeight - (this.MenuContentInsets.Top + this.MenuContentInsets.Bottom) * scale.Y);
        
        RectangleF highlightMaskRect = new RectangleF(this.Position.X, this.Position.Y, highlightMaskWidth, highlightMaskHeight);
        Vector2 highlightMaskOrigin = menuHitOrigin - highlightInsetTopLeft;
        
        DepthStencilStateDescription stencilMask = new DepthStencilStateDescription() {
            StencilTestEnabled = true,
            StencilWriteMask = 1,
            StencilReference = 1,
            StencilFront = new StencilBehaviorDescription() {
                Comparison = ComparisonKind.Always,
                Pass = StencilOperation.Replace
            },
            StencilBack = new StencilBehaviorDescription() {
                Comparison = ComparisonKind.Always,
                Pass = StencilOperation.Replace
            }
        };
        
        // Draw the smaller highlight mask rectangle into the stencil buffer.
        PrimitiveGuiRenderState primitiveReadMask0RenderState = new PrimitiveGuiRenderState(depthStencilState: stencilMask);
        renderQueue.UsePrimitive(primitiveReadMask0RenderState).DrawFilledRectangle(highlightMaskRect, highlightMaskOrigin, this.Rotation, 0.5F, new Color(255, 255, 255, 0));
        
        bool itemHovered = false;
        
        // Iterate through visible options to find which one is hovered by the mouse.
        for (int i = 0; i < visibleOptions; i++) {
            int optionIndex = startIndex + i;
            
            if (optionIndex >= this.Options.Count) {
                break;
            }
            
            RectangleF itemRect = new RectangleF(this.Position.X, this.Position.Y, fieldSize.X, fieldSize.Y);
            Vector2 itemOrigin = this.Origin * scale - new Vector2(0.0F, fieldSize.Y * (i - scrollOffset + 1.0F));
            
            // If the mouse is over this specific item.
            if (itemRect.Contains(Input.GetMousePosition(), itemOrigin, this.Rotation)) {
                itemHovered = true;
                
                DepthStencilStateDescription stencilItem = new DepthStencilStateDescription() {
                    StencilTestEnabled = true,
                    StencilReadMask = 1,
                    StencilWriteMask = 2,
                    StencilReference = 3,
                    StencilFront = new StencilBehaviorDescription() {
                        Comparison = ComparisonKind.Equal,
                        Pass = StencilOperation.Replace
                    },
                    StencilBack = new StencilBehaviorDescription() {
                        Comparison = ComparisonKind.Equal,
                        Pass = StencilOperation.Replace
                    }
                };
                
                // Draw the hovered item rectangle into the stencil buffer.
                // Because stencilReadMask is 1, only the part inside highlightMaskRect can be written.
                PrimitiveGuiRenderState primitiveReadMask1RenderState = new PrimitiveGuiRenderState(depthStencilState: stencilItem);
                renderQueue.UsePrimitive(primitiveReadMask1RenderState).DrawFilledRectangle(itemRect, itemOrigin, this.Rotation, 0.5F, new Color(255, 255, 255, 0));
                break;
            }
        }
        
        if (!itemHovered) {
            return;
        }
        
        DepthStencilStateDescription stencilTest = new DepthStencilStateDescription() {
            StencilTestEnabled = true,
            StencilReadMask = 2,
            StencilReference = 2,
            StencilFront = new StencilBehaviorDescription {
                Comparison = ComparisonKind.Equal,
                Pass = StencilOperation.Keep
            },
            StencilBack = new StencilBehaviorDescription {
                Comparison = ComparisonKind.Equal,
                Pass = StencilOperation.Keep
            }
        };
        
        // Draw the actual highlight.
        this.DrawMenu(renderQueue, this.DropDownData.HighlightColor, stencilTest);
    }
    
    /// <summary>
    /// Draws the scrollbar for the dropdown menu based on the current number of options and visual properties.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    private void DrawSliderBar(GuiRenderQueue renderQueue) {
        if (this.Options.Count <= this.MaxVisibleOptions) {
            return;
        }
        
        Vector2 originalSize = this.Size;
        Vector2 originalOrigin = this.Origin;
        
        // Calculate the scrollbar size and offset origin.
        this.Size = new Vector2(this.DropDownData.SliderBarWidth, originalSize.Y * this.MaxVisibleOptions);
        this.Origin = originalOrigin - new Vector2(originalSize.X - this.DropDownData.SliderBarWidth, originalSize.Y);
        
        Color color = this.IsHovered ? this.DropDownData.SliderBarHoverColor : this.DropDownData.SliderBarColor;
        
        if (!this.Interactable) {
            color = this.DropDownData.DisabledSliderBarColor;
        }
        
        switch (this.DropDownData.FieldResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(renderQueue, this.DropDownData.SliderBarTexture, this.DropDownData.SliderBarSampler, this.DropDownData.SliderBarSourceRect, color, this.DropDownData.SliderBarFlip, this.DropDownData.SliderBarPixelSnap, this.DropDownData.Effect, this.DropDownData.BlendState);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(renderQueue, this.DropDownData.SliderBarTexture, this.DropDownData.SliderBarSampler, this.DropDownData.SliderBarSourceRect, this.DropDownData.SliderBarBorderInsets, this.DropDownData.SliderBarResizeMode == ResizeMode.TileCenter, color, this.DropDownData.SliderBarFlip, this.DropDownData.SliderBarPixelSnap, this.DropDownData.Effect, this.DropDownData.BlendState);
                break;
        }
        
        // Restore original values.
        this.Size = originalSize;
        this.Origin = originalOrigin;
    }
    
    /// <summary>
    /// Draws the slider within the dropdown menu based on the current scroll position and visual configuration.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    private void DrawSlider(GuiRenderQueue renderQueue) {
        if (this.Options.Count <= this.MaxVisibleOptions) {
            return;
        }
        
        float scrollBarHeight = this.Size.Y * (this.MaxVisibleOptions);
        float sliderHeight = this.DropDownData.SliderSourceRect.Height;
        float scrollRange = scrollBarHeight - sliderHeight;
        
        float xOffset = this.Size.X - this.DropDownData.SliderBarWidth + (this.DropDownData.SliderBarWidth - this.DropDownData.SliderSourceRect.Width) / 2.0F;
        float yOffset = this.Size.Y + scrollRange * this._scrollPercent;
        Vector2 origin = this.Origin - new Vector2(xOffset, yOffset) - this.SliderOffset;
        
        Color color = this.IsHovered ? this.DropDownData.SliderHoverColor : this.DropDownData.SliderColor;
        
        if (!this.Interactable) {
            color = this.DropDownData.DisabledSliderColor;
        }
        
        SpriteGuiRenderState renderState = new SpriteGuiRenderState(this.DropDownData.SliderSampler, this.DropDownData.Effect, this.DropDownData.BlendState);
        renderQueue.UseSprite(renderState).DrawTexture(this.DropDownData.SliderTexture, this.Position, 0.5F, this.DropDownData.SliderSourceRect, this.Scale * this.Gui.ScaleFactor, origin, this.DropDownData.SliderPixelSnap, this.Rotation, color, this.DropDownData.SliderFlip);
    }
    
    /// <summary>
    /// Draws the arrow texture on the screen, with its size, position, offset, and visual properties determined by the dropdown's data.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    private void DrawArrow(GuiRenderQueue renderQueue) {
        if (this.DropDownData.ArrowTexture == null) {
            return;
        }
        
        float offsetX = this.Size.X - this.DropDownData.ArrowSourceRect.Width - this.ArrowOffset.X;
        float offsetY = (this.Size.Y - this.DropDownData.ArrowSourceRect.Height) / 2.0F + this.ArrowOffset.Y;
        
        Vector2 arrowSize = new Vector2(this.DropDownData.ArrowSourceRect.Width, this.DropDownData.ArrowSourceRect.Height);
        Vector2 arrowCenter = arrowSize / 2.0F;
        
        Vector2 localToArrowCenter = (new Vector2(offsetX, offsetY) + arrowCenter - this.Origin) * this.Scale * this.Gui.ScaleFactor;
        Vector2 rotatedLocalToArrowCenter = Vector2.Transform(localToArrowCenter, Matrix3x2.CreateRotation(float.DegreesToRadians(this.Rotation)));
        Vector2 arrowWorldCenter = this.Position + rotatedLocalToArrowCenter;
        
        Color color = this.IsHovered ? this.DropDownData.ArrowHoverColor : this.DropDownData.ArrowColor;
        
        if (!this.Interactable) {
            color = this.DropDownData.DisabledArrowColor;
        }
        
        SpriteGuiRenderState renderState = new SpriteGuiRenderState(this.DropDownData.ArrowSampler, this.DropDownData.Effect, this.DropDownData.BlendState);
        renderQueue.UseSprite(renderState).DrawTexture(this.DropDownData.ArrowTexture, arrowWorldCenter, 0.5F, this.DropDownData.ArrowSourceRect, this.Scale * this.Gui.ScaleFactor, arrowCenter, this.DropDownData.ArrowPixelSnap, this.Rotation + float.RadiansToDegrees(this._arrowRotation), color, this.DropDownData.ArrowFlip);
    }
    
    /// <summary>
    /// Draws the specified text on the screen with alignment, offset, and style settings.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    /// <param name="labelData">The data containing font, text, size, style, and other text attributes.</param>
    /// <param name="textAlignment">The alignment of the text (Left, Center, or Right).</param>
    /// <param name="textOffset">The offset added to the text's position for rendering.</param>
    /// <param name="textScale">The scale applied to the rendered text.</param>
    /// <param name="depthStencilState">Optional depth-stencil state used for depth testing and stencil operations during rendering. If <c>null</c>, the default state is used.</param>
    private void DrawText(GuiRenderQueue renderQueue, LabelData labelData, TextAlignment textAlignment, Vector2 textOffset, Vector2 textScale, DepthStencilStateDescription? depthStencilState = null) {
        if (labelData.Text == string.Empty) {
            return;
        }
        
        Vector2 textPos = this.Position;
        Vector2 textSize = labelData.Font.MeasureText(labelData.Text, labelData.Size, Vector2.One, labelData.CharacterSpacing, labelData.LineSpacing, labelData.FontSystemEffect, labelData.EffectAmount);
        
        Vector2 textOrigin = textAlignment switch {
            TextAlignment.Left => new Vector2(this.Size.X / textScale.X, labelData.Size) / 2.0F - (this.Size / 2.0F - (this.Origin - textOffset)) / textScale,
            TextAlignment.Center => new Vector2(textSize.X, labelData.Size) / 2.0F - (this.Size / 2.0F - (this.Origin - textOffset)) / textScale,
            TextAlignment.Right => new Vector2((-this.Size.X / 2.0F) / textScale.X + (textSize.X - 2.0F), labelData.Size / 2.0F) - (this.Size / 2.0F - (this.Origin - textOffset)) / textScale,
            _ => Vector2.Zero
        };
        
        Color color = this.IsHovered ? labelData.HoverColor : labelData.Color;
        
        if (!this.Interactable) {
            color = labelData.DisabledColor;
        }
        
        SpriteGuiRenderState renderState = new SpriteGuiRenderState(labelData.Sampler, labelData.Effect, labelData.BlendState, depthStencilState);
        renderQueue.UseSprite(renderState).DrawText(labelData.Font, labelData.Text, textPos, labelData.Size, labelData.CharacterSpacing, labelData.LineSpacing, this.Scale * textScale * this.Gui.ScaleFactor, 0.5F, textOrigin, labelData.PixelSnap, this.Rotation, color, labelData.Style, labelData.FontSystemEffect, labelData.EffectAmount);
    }
    
    protected override void Dispose(bool disposing) { }
}