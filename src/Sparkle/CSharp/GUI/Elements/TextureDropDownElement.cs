using System.Numerics;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;
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
    /// The offset applied to the dropdown arrow indicator.
    /// </summary>
    public Vector2 ArrowOffset;
    
    /// <summary>
    /// The insets applied to the scroll mask for controlling the visible region of the dropdown menu's scrollable content.
    /// </summary>
    public (float Top, float Bottom) ScrollMaskInsets;
    
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
    /// <param name="sliderOffset">An optional offset applied to the position of the slider within the dropdown menu.</param>
    /// <param name="arrowOffset">The offset applied to the dropdown arrow indicator.</param>
    /// <param name="scrollMaskInsets">Defines top and bottom padding for the scroll text clipping mask.</param>
    /// <param name="size">The size of the dropdown element.</param>
    /// <param name="scale">Optional scale applied to the dropdown element.</param>
    /// <param name="origin">Optional origin point used for rotation and alignment.</param>
    /// <param name="rotation">The rotation of the dropdown element in radians.</param>
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
        Vector2? sliderOffset = null,
        Vector2? arrowOffset = null,
        (float Top, float Bottom)? scrollMaskInsets = null,
        Vector2? size = null,
        Vector2? scale = null,
        Vector2? origin = null,
        float rotation = 0.0F,
        Func<GuiElement, bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, clickFunc) {
        this.DropDownData = dropDownData;
        this.Options = options;
        this.MaxVisibleOptions = Math.Max(2, maxVisibleOptions);
        this.SelectedOption = this.Options.FirstOrDefault();
        this.FieldTextAlignment = fieldTextAlignment;
        this.MenuTextAlignment = menuTextAlignment;
        this.FieldTextOffset = fieldTextOffset ?? Vector2.Zero;
        this.MenuTextOffset = menuTextOffset ?? Vector2.Zero;
        this.SliderOffset = sliderOffset ?? Vector2.Zero;
        this.ArrowOffset = arrowOffset ?? new Vector2(10.0F, 0.0F);
        this.ScrollMaskInsets = scrollMaskInsets ?? (0.0F, 0.0F);
        this.Size = size ?? new Vector2(dropDownData.FieldSourceRect.Width, dropDownData.FieldSourceRect.Height);
    }
    
    /// <summary>
    /// Updates the state of the texture-based dropdown element, including handling user interactions such as clicking
    /// and selecting options, and managing the visibility of the dropdown menu.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update, in seconds, used for timing-related logic.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);
        
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
            this._scrollPercent = 0;
            this._isDraggingSlider = false;
        }
        
        // Handle scrolling and dragging.
        if (this.IsMenuOpen && this.Options.Count > this.MaxVisibleOptions) {
            Vector2 mousePos = Input.GetMousePosition();
            Vector2 scale = this.Scale * this.Gui.ScaleFactor;
            Vector2 fieldSize = this.Size * scale;
            
            // Define scrollbar track rectangle.
            float scrollBarWidth = this.DropDownData.ScrollBarWidth * scale.X;
            float scrollBarHeight = fieldSize.Y * this.MaxVisibleOptions;
            
            // The scrollbar is on the right side of the menu.
            RectangleF scrollTrackRect = new RectangleF(this.Position.X + fieldSize.X - scrollBarWidth, this.Position.Y + fieldSize.Y, scrollBarWidth, scrollBarHeight);
            
            // Define the full menu rectangle (to check for hover).
            RectangleF menuRect = new RectangleF(this.Position.X, this.Position.Y + fieldSize.Y, fieldSize.X, scrollBarHeight);
            Vector2 transformOrigin = this.Origin * scale;
            
            // Handle mouse wheel.
            if (!this._isDraggingSlider && menuRect.Contains(mousePos, transformOrigin, this.Rotation)) {
                if (Input.IsMouseScrolling(out Vector2 wheelDelta)) {
                    float scrollSensitivity = 0.1F; // TODO: MAKE AN OPTION FOR IT. (and make it smooth)
                    
                    this._scrollPercent = Math.Clamp(this._scrollPercent - wheelDelta.Y * scrollSensitivity, 0.0F, 1.0F);
                }
            }
            
            // Handle Dragging / Clicking on Scrollbar.
            if (Input.IsMouseButtonDown(MouseButton.Left)) {
                
                // Start dragging if the mouse is over the track.
                if (scrollTrackRect.Contains(mousePos, transformOrigin, this.Rotation)) {
                    this._isDraggingSlider = true;
                }
                
                if (this._isDraggingSlider) {
                    
                    // Calculate relative Y position within the track using an inverse transform.
                    Matrix3x2 transform = Matrix3x2.CreateTranslation(-this.Position) * Matrix3x2.CreateRotation(-float.DegreesToRadians(this.Rotation)) * Matrix3x2.CreateTranslation(transformOrigin);
                    Vector2 localMouse = Vector2.Transform(mousePos, transform);
                    
                    float sliderHeight = this.DropDownData.SliderSourceRect.Height * scale.Y;
                    float trackTop = fieldSize.Y;
                    
                    // Calculate percentage based on mouse Y relative to the track's vertical range.
                    float usableTrackHeight = scrollBarHeight - sliderHeight;
                    float relativeY = localMouse.Y - trackTop - (sliderHeight / 2.0F);
                    
                    this._scrollPercent = Math.Clamp(relativeY / usableTrackHeight, 0.0F, 1.0F);
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
                            itemSize.X -= this.DropDownData.ScrollBarWidth * this.Scale.X * this.Gui.ScaleFactor;
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
            this.IsMenuOpen = false;
            this.MenuToggled?.Invoke(this.IsMenuOpen);
        }
    }
    
    /// <summary>
    /// Renders the texture-based dropdown element, including its field, menu, arrow, and text components, based on the current state and interaction details.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations, including sprite batching.</param>
    /// <param name="framebuffer">The target framebuffer where the dropdown element will be drawn.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        // Draw field texture.
        Color buttonColor = this.IsHovered ? this.DropDownData.FieldHoverColor : this.DropDownData.FieldColor;
        
        if (!this.Interactable) {
            buttonColor = this.DropDownData.DisabledFieldColor;
        }
        
        switch (this.DropDownData.FieldResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(context.SpriteBatch, this.DropDownData.FieldTexture, this.DropDownData.FieldSampler, this.DropDownData.FieldSourceRect, buttonColor, this.DropDownData.FieldFlip);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(context.SpriteBatch, this.DropDownData.FieldTexture, this.DropDownData.FieldSampler, this.DropDownData.FieldSourceRect, this.DropDownData.FieldBorderInsets, this.DropDownData.FieldResizeMode == ResizeMode.TileCenter, buttonColor, this.DropDownData.FieldFlip);
                break;
        }
        
        // Draw arrow texture.
        this.DrawArrow(context.SpriteBatch);
        
        // Draw field text.
        if (this.SelectedOption != null) {
            this.DrawText(context.SpriteBatch, this.SelectedOption, this.FieldTextAlignment, this.FieldTextOffset);
        }
        
        if (this.IsMenuOpen) {
            
            // Draw menu texture.
            Color menuColor = this.IsHovered ? this.DropDownData.MenuHoverColor : this.DropDownData.MenuColor;
            
            if (!this.Interactable) {
                menuColor = this.DropDownData.DisabledMenuColor;
            }
            
            this.DrawMenu(context.SpriteBatch, menuColor);
            
            // Draw the scroll bar.
            this.DrawScrollBar(context.SpriteBatch);
            
            // Draw the slider.
            this.DrawSlider(context.SpriteBatch);
        }
        
        context.SpriteBatch.End();
        
        if (this.IsMenuOpen) {
            
            // Draw highlight.
            this.DrawHighlight(context.CommandList, framebuffer, context.SpriteBatch, context.PrimitiveBatch);
            
            // Draw options text.
            this.DrawOptionsText(context.CommandList, framebuffer, context.SpriteBatch, context.PrimitiveBatch);
        }
    }
    
    /// <summary>
    /// Renders the text of the dropdown menu options onto the screen, managing visible options and applying a clipping mask for scrolling.
    /// </summary>
    /// <param name="commandList">The command list used for issuing rendering commands.</param>
    /// <param name="framebuffer">The framebuffer used as the rendering target.</param>
    /// <param name="spriteBatch">The sprite batch renderer used for drawing the option text.</param>
    /// <param name="primitiveBatch">The primitive batch renderer used for drawing the stencil mask for scrolling.</param>
    private void DrawOptionsText(CommandList commandList, Framebuffer framebuffer, SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch) {
        int visibleOptions = this.Options.Count;
        float currentScrollIndex = 0;
        
        if (this.Options.Count > this.MaxVisibleOptions) {
            currentScrollIndex = this._scrollPercent * (this.Options.Count - this.MaxVisibleOptions);
            visibleOptions = Math.Min(this.Options.Count, this.MaxVisibleOptions + 1);
        }
        
        // Determine the starting index and the fractional offset for scrolling.
        int startIndex = (int) Math.Floor(currentScrollIndex);
        float scrollOffset = currentScrollIndex - startIndex;
        
        Vector2 fieldSize = this.Size * this.Scale * this.Gui.ScaleFactor;
        Vector2 scale = this.Scale * this.Gui.ScaleFactor;
        
        float maskWidth = fieldSize.X;
        
        // Reduce mask width if the scroll bar is present.
        if (this.Options.Count > this.MaxVisibleOptions) {
            maskWidth -= this.DropDownData.ScrollBarWidth * scale.X;
        }
        
        float maskHeight = fieldSize.Y * this.MaxVisibleOptions - (this.ScrollMaskInsets.Top + this.ScrollMaskInsets.Bottom) * scale.Y;
        
        // Define the clipping mask area (the visible part of the dropdown list).
        RectangleF maskRect = new RectangleF(this.Position.X, this.Position.Y, maskWidth, maskHeight);
        Vector2 maskOrigin = this.Origin * scale - new Vector2(0.0F, fieldSize.Y) - new Vector2(0.0F, this.ScrollMaskInsets.Top * scale.Y);
        
        DepthStencilStateDescription stencilWrite = new DepthStencilStateDescription {
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
        
        // Write to the stencil buffer to mark the scroll mask area.
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        primitiveBatch.PushDepthStencilState(stencilWrite);
        primitiveBatch.DrawFilledRectangle(maskRect, maskOrigin, this.Rotation, 0.5F, new Color(255, 255, 255, 0));
        primitiveBatch.PopDepthStencilState();
        primitiveBatch.End();
        
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
        
        // Draw each option label, clipped by the stencil mask.
        spriteBatch.Begin(commandList, framebuffer.OutputDescription);
        spriteBatch.PushDepthStencilState(stencilTest);
        
        for (int i = 0; i < visibleOptions; i++) {
            int optionIndex = startIndex + i;
            
            if (optionIndex >= this.Options.Count) {
                break;
            }
            
            Vector2 itemOffset = this.MenuTextOffset + new Vector2(0.0F, this.Size.Y * (i - scrollOffset + 1.0F));
            
            if (this.Options.Count > this.MaxVisibleOptions) {
                switch (this.MenuTextAlignment) {
                    case TextAlignment.Center:
                        itemOffset.X -= this.DropDownData.ScrollBarWidth / 2.0F;
                        break;
                    case TextAlignment.Right:
                        itemOffset.X -= this.DropDownData.ScrollBarWidth;
                        break;
                }
            }
            
            this.DrawText(spriteBatch, this.Options[optionIndex], this.MenuTextAlignment, itemOffset);
        }
        
        spriteBatch.PopDepthStencilState();
        spriteBatch.End();
    }
    
    /// <summary>
    /// Renders a visual highlight for the dropdown menu, including the visible options and hover effects.
    /// </summary>
    /// <param name="commandList">The command list used for drawing commands.</param>
    /// <param name="framebuffer">The framebuffer where the highlight will be rendered.</param>
    /// <param name="spriteBatch">The sprite batch for rendering textured elements.</param>
    /// <param name="primitiveBatch">The primitive batch for rendering geometric shapes.</param>
    private void DrawHighlight(CommandList commandList, Framebuffer framebuffer, SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch) {
        Vector2 fieldSize = this.Size * this.Scale * this.Gui.ScaleFactor;
        Vector2 scale = this.Scale * this.Gui.ScaleFactor;
        
        int visibleOptions = this.Options.Count;
        float currentScrollIndex = 0;
        
        // Calculate visible options and adjust field size if the scrollbar is active.
        if (this.Options.Count > this.MaxVisibleOptions) {
            fieldSize.X -= this.DropDownData.ScrollBarWidth * scale.X;
            currentScrollIndex = this._scrollPercent * (this.Options.Count - this.MaxVisibleOptions);
            visibleOptions = Math.Min(this.Options.Count, this.MaxVisibleOptions + 1);
        }
        
        // Determine the starting index and offset for scrolling.
        int startIndex = (int) Math.Floor(currentScrollIndex);
        float scrollOffset = currentScrollIndex - startIndex;
        
        float maskHeight = fieldSize.Y * this.MaxVisibleOptions;
        
        // Define the mask rectangle for the visible portion of the dropdown list.
        RectangleF maskRect = new RectangleF(this.Position.X, this.Position.Y, fieldSize.X, maskHeight);
        Vector2 maskOrigin = this.Origin * scale - new Vector2(0.0F, fieldSize.Y);
        
        // Check if the mouse is even within the visible menu area first.
        if (!maskRect.Contains(Input.GetMousePosition(), maskOrigin, this.Rotation)) {
            return;
        }
        
        DepthStencilStateDescription stencilMask = new DepthStencilStateDescription {
            StencilTestEnabled = true,
            StencilWriteMask = 1,
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
        
        // Draw the mask rectangle into the stencil buffer.
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        primitiveBatch.PushDepthStencilState(stencilMask);
        primitiveBatch.DrawFilledRectangle(maskRect, maskOrigin, this.Rotation, 0.5F, new Color(255, 255, 255, 0));
        primitiveBatch.PopDepthStencilState();
        primitiveBatch.End();
        
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
                
                // Draw the item's rectangle into the stencil buffer (setting bit 2).
                primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
                primitiveBatch.PushDepthStencilState(stencilItem);
                primitiveBatch.DrawFilledRectangle(itemRect, itemOrigin, this.Rotation, 0.5F, new Color(255, 255, 255, 0));
                primitiveBatch.PopDepthStencilState();
                primitiveBatch.End();
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
        spriteBatch.Begin(commandList, framebuffer.OutputDescription);
        spriteBatch.PushDepthStencilState(stencilTest);
        this.DrawMenu(spriteBatch, this.DropDownData.HighlightColor);
        spriteBatch.PopDepthStencilState();
        spriteBatch.End();
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
        Vector2 fieldScaledSize = this.Size * this.Scale * this.Gui.ScaleFactor;
        
        float width = GlobalGraphicsAssets.Window.GetWidth();
        float height = GlobalGraphicsAssets.Window.GetHeight();
        
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
        
        pos += (this.Offset * this.Gui.ScaleFactor) + this.Origin;
        
        return pos;
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
    /// Draws a texture at the specified position with optional scaling, rotation, and flipping.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used for drawing the texture.</param>
    /// <param name="texture">The texture to be rendered.</param>
    /// <param name="sampler">Optional sampler state specifying the sampling behavior for texture rendering.</param>
    /// <param name="sourceRect">The source rectangle defining the region of the texture to draw.</param>
    /// <param name="color">The color tint to apply to the texture during rendering.</param>
    /// <param name="flip">Specifies the sprite flipping options for the texture.</param>
    private void DrawNormal(SpriteBatch spriteBatch, Texture2D texture, Sampler? sampler, Rectangle sourceRect, Color color, SpriteFlip flip) {
        if (sampler != null) spriteBatch.PushSampler(sampler);
        spriteBatch.DrawTexture(texture, this.Position, 0.5F, sourceRect, this.Scale * this.Gui.ScaleFactor, this.Origin, this.Rotation, color, flip);
        if (sampler != null) spriteBatch.PopSampler();
    }
    
    /// <summary>
    /// Renders a nine-slice element with specified texture, border insets, and optional tiling for the center region.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used for drawing the nine-slice element.</param>
    /// <param name="texture">The texture to be rendered in the nine-slice layout.</param>
    /// <param name="sampler">Optional sampler state specifying the sampling behavior for texture rendering.</param>
    /// <param name="sourceRect">The source rectangle defining the region of the texture to draw.</param>
    /// <param name="borderInsets">The insets defining the borders of the nine-slice layout.</param>
    /// <param name="tileCenter">Indicates whether the center region of the nine-slice is tiled.</param>
    /// <param name="color">The color tint to apply to the texture during rendering.</param>
    /// <param name="flip">Specifies the sprite flipping options for the nine-slice element.</param>
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
    /// Draws the dropdown menu, including its background, resizing behavior, and the labels of its options, using the specified sprite batch and menu color.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to render the dropdown menu and its components.</param>
    /// <param name="color">The color applied to the menu background during rendering.</param>
    private void DrawMenu(SpriteBatch spriteBatch, Color color) {
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
            this.Size.X -= this.DropDownData.ScrollBarWidth;
        }
        
        // Calculate the origin so it draws below the field.
        this.Origin = originalOrigin - new Vector2(0.0F, originalSize.Y);
        
        switch (this.DropDownData.MenuResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(spriteBatch, this.DropDownData.MenuTexture, this.DropDownData.MenuSampler, this.DropDownData.MenuSourceRect, color, this.DropDownData.MenuFlip);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(spriteBatch, this.DropDownData.MenuTexture, this.DropDownData.MenuSampler, this.DropDownData.MenuSourceRect, this.DropDownData.MenuBorderInsets, this.DropDownData.MenuResizeMode == ResizeMode.TileCenter, color, this.DropDownData.MenuFlip);
                break;
        }
        
        // Restore original values.
        this.Size = originalSize;
        this.Origin = originalOrigin;
    }
    
    /// <summary>
    /// Draws the scrollbar for the dropdown menu based on the current number of options and visual properties.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to render the scrollbar.</param>
    private void DrawScrollBar(SpriteBatch spriteBatch) {
        if (this.Options.Count <= this.MaxVisibleOptions) {
            return;
        }
        
        Vector2 originalSize = this.Size;
        Vector2 originalOrigin = this.Origin;
        
        // Calculate the scrollbar size and offset origin.
        this.Size = new Vector2(this.DropDownData.ScrollBarWidth, originalSize.Y * this.MaxVisibleOptions);
        this.Origin = originalOrigin - new Vector2(originalSize.X - this.DropDownData.ScrollBarWidth, originalSize.Y);
        
        Color color = this.IsHovered ? this.DropDownData.SliderBarHoverColor : this.DropDownData.SliderBarColor;
        
        if (!this.Interactable) {
            color = this.DropDownData.DisabledSliderBarColor;
        }
        
        switch (this.DropDownData.FieldResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(spriteBatch, this.DropDownData.SliderBarTexture, this.DropDownData.SliderBarSampler, this.DropDownData.SliderBarSourceRect, color, this.DropDownData.SliderBarFlip);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(spriteBatch, this.DropDownData.SliderBarTexture, this.DropDownData.SliderBarSampler, this.DropDownData.SliderBarSourceRect, this.DropDownData.SliderBarBorderInsets, this.DropDownData.SliderBarResizeMode == ResizeMode.TileCenter, color, this.DropDownData.SliderBarFlip);
                break;
        }
        
        // Restore original values.
        this.Size = originalSize;
        this.Origin = originalOrigin;
    }
    
    /// <summary>
    /// Draws the slider within the dropdown menu based on the current scroll position and visual configuration.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to render the slider component.</param>
    private void DrawSlider(SpriteBatch spriteBatch) {
        if (this.Options.Count <= this.MaxVisibleOptions) {
            return;
        }
        
        float scrollBarHeight = this.Size.Y * (this.MaxVisibleOptions);
        float sliderHeight = this.DropDownData.SliderSourceRect.Height;
        float scrollRange = scrollBarHeight - sliderHeight;
        
        float xOffset = this.Size.X - this.DropDownData.ScrollBarWidth + (this.DropDownData.ScrollBarWidth - this.DropDownData.SliderSourceRect.Width) / 2.0F;
        float yOffset = this.Size.Y + scrollRange * this._scrollPercent;
        Vector2 origin = this.Origin - new Vector2(xOffset, yOffset) - this.SliderOffset;
        
        Color color = this.IsHovered ? this.DropDownData.SliderHoverColor : this.DropDownData.SliderColor;
        
        if (!this.Interactable) {
            color = this.DropDownData.DisabledSliderColor;
        }
        
        if (this.DropDownData.SliderSampler != null) spriteBatch.PushSampler(this.DropDownData.SliderSampler);
        spriteBatch.DrawTexture(this.DropDownData.SliderTexture, this.Position, 0.5F, this.DropDownData.SliderSourceRect, this.Scale * this.Gui.ScaleFactor, origin, this.Rotation, color, this.DropDownData.SliderFlip);
        if (this.DropDownData.SliderSampler != null) spriteBatch.PopSampler();
    }
    
    /// <summary>
    /// Draws the arrow texture on the screen, with its size, position, offset, and visual properties determined by the dropdown's data.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used to render the arrow texture.</param>
    private void DrawArrow(SpriteBatch spriteBatch) {
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
        
        if (this.DropDownData.ArrowSampler != null) spriteBatch.PushSampler(this.DropDownData.ArrowSampler);
        spriteBatch.DrawTexture(this.DropDownData.ArrowTexture, arrowWorldCenter, 0.5F, this.DropDownData.ArrowSourceRect, this.Scale * this.Gui.ScaleFactor, arrowCenter, this.Rotation + float.RadiansToDegrees(this._arrowRotation), color, this.DropDownData.ArrowFlip);
        if (this.DropDownData.ArrowSampler != null) spriteBatch.PopSampler();
    }
    
    /// <summary>
    /// Draws the specified text on the screen with alignment, offset, and style settings.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used for rendering the text.</param>
    /// <param name="labelData">The data containing font, text, size, style, and other text attributes.</param>
    /// <param name="textAlignment">The alignment of the text (Left, Center, or Right).</param>
    /// <param name="textOffset">The offset added to the text's position for rendering.</param>
    private void DrawText(SpriteBatch spriteBatch, LabelData labelData, TextAlignment textAlignment, Vector2 textOffset) {
        if (labelData.Text == string.Empty) {
            return;
        }
        
        Vector2 textPos = this.Position;
        Vector2 textSize = labelData.Font.MeasureText(labelData.Text, labelData.Size, Vector2.One, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
        
        Vector2 textOrigin = textAlignment switch {
            TextAlignment.Left => new Vector2(this.Size.X, labelData.Size) / 2.0F - (this.Size / 2.0F - (this.Origin - textOffset)),
            TextAlignment.Center => new Vector2(textSize.X, labelData.Size) / 2.0F - (this.Size / 2.0F - (this.Origin - textOffset)),
            TextAlignment.Right => new Vector2(-this.Size.X / 2.0F + (textSize.X - 2.0F), labelData.Size / 2.0F) - (this.Size / 2.0F - (this.Origin - textOffset)),
            _ => Vector2.Zero
        };
        
        Color color = this.IsHovered ? labelData.HoverColor : labelData.Color;
        
        if (!this.Interactable) {
            color = labelData.DisabledColor;
        }
        
        if (labelData.Sampler != null) spriteBatch.PushSampler(labelData.Sampler);
        spriteBatch.DrawText(labelData.Font, labelData.Text, textPos, labelData.Size, labelData.CharacterSpacing, labelData.LineSpacing, this.Scale * this.Gui.ScaleFactor, 0.5F, textOrigin, this.Rotation, color, labelData.Style, labelData.Effect, labelData.EffectAmount);
        if (labelData.Sampler != null) spriteBatch.PopSampler();
    }
}