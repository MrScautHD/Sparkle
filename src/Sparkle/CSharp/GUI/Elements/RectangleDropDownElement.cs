using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

public class RectangleDropDownElement : GuiElement {
    
    /// <summary>
    /// The data used to configure and customize the behavior and appearance of the dropdown element.
    /// </summary>
    public RectangleDropDownData DropDownData { get; private set; }
    
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
    /// The offset applied to the dropdown arrow indicator.
    /// </summary>
    public Vector2 ArrowOffset;
    
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
    /// Initializes a new instance of the <see cref="RectangleDropDownElement"/> class.
    /// </summary>
    /// <param name="dropDownData">Visual styling data for the dropdown.</param>
    /// <param name="options">List of selectable options.</param>
    /// <param name="maxVisibleOptions">Maximum number of visible menu items.</param>
    /// <param name="anchor">Anchor point of the element.</param>
    /// <param name="offset">Offset from the anchor.</param>
    /// <param name="size">Size of the dropdown field.</param>
    /// <param name="scale">Optional scale override.</param>
    /// <param name="fieldTextAlignment">Alignment of the selected field text.</param>
    /// <param name="menuTextAlignment">Alignment of menu option text.</param>
    /// <param name="fieldTextOffset">Offset applied to field text.</param>
    /// <param name="menuTextOffset">Offset applied to menu text.</param>
    /// <param name="arrowOffset">Offset applied to the dropdown arrow.</param>
    /// <param name="scrollSensitivity">Mouse wheel scroll sensitivity.</param>
    /// <param name="scrollLerpSpeed">Smooth scroll interpolation speed.</param>
    /// <param name="origin">Origin point for rotation and scaling.</param>
    /// <param name="rotation">Initial rotation in degrees.</param>
    /// <param name="clickFunc">Optional custom click handler.</param>
    public RectangleDropDownElement(
        RectangleDropDownData dropDownData,
        List<LabelData> options,
        int maxVisibleOptions,
        Anchor anchor,
        Vector2 offset,
        Vector2 size,
        Vector2? scale = null,
        TextAlignment fieldTextAlignment = TextAlignment.Left,
        TextAlignment menuTextAlignment = TextAlignment.Left,
        Vector2? fieldTextOffset = null,
        Vector2? menuTextOffset = null,
        Vector2? arrowOffset = null,
        float scrollSensitivity = 0.1F,
        float scrollLerpSpeed = 10.0F,
        Vector2? origin = null,
        float rotation = 0,
        Func<GuiElement, bool>? clickFunc = null) : base(anchor, offset, size, scale, origin, rotation, clickFunc) {
        this.DropDownData = dropDownData;
        this.Options = options;
        this.MaxVisibleOptions = Math.Max(2, maxVisibleOptions);
        this.SelectedOption = this.Options.FirstOrDefault();
        this.FieldTextAlignment = fieldTextAlignment;
        this.MenuTextAlignment = menuTextAlignment;
        this.FieldTextOffset = fieldTextOffset ?? Vector2.Zero;
        this.MenuTextOffset = menuTextOffset ?? Vector2.Zero;
        this.ArrowOffset = arrowOffset ?? new Vector2(10.0F, 0.0F);
        this.ScrollSensitivity = scrollSensitivity;
        this.ScrollLerpSpeed = scrollLerpSpeed;
    }

    /// <summary>
    /// Updates the state of the <see cref="RectangleDropDownElement"/> and handles animations, scrolling,
    /// user interaction, and menu state transitions.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update, used for animations and smooth transitions.</param>
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
            RectangleF localMenuRect = new RectangleF(0, fieldSize.Y, fieldSize.X, scrollBarHeight);
            RectangleF localTrackRect = new RectangleF(fieldSize.X - scrollBarWidth, fieldSize.Y, scrollBarWidth, scrollBarHeight);
            
            // Handle mouse wheel.
            if (!this._isDraggingSlider && localMenuRect.Contains(localMouse)) {
                if (Input.IsMouseScrolling(out Vector2 wheelDelta)) {
                    this._targetScrollPercent = Math.Clamp(this._targetScrollPercent - wheelDelta.Y * this.ScrollSensitivity, 0.0F, 1.0F);
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
                    float sliderHeight = this.DropDownData.SliderSize.Y * scale.Y;
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
    /// Renders the <see cref="RectangleDropDownElement"/> and its associated visual components.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer to which the rendering output should be drawn.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        // Draw the filled field rectangle.
        Color fieldColor = this.IsHovered ? this.DropDownData.FieldHoverColor : this.DropDownData.FieldColor;
        
        if (!this.Interactable) {
            fieldColor = this.DropDownData.DisabledFieldColor;
        }

        Vector2 fieldSize = this.Size * this.Scale * this.Gui.ScaleFactor;
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(this.Position.X, this.Position.Y, fieldSize.X, fieldSize.Y), this.Origin * this.Scale * this.Gui.ScaleFactor, this.Rotation, 0.5F, fieldColor);
        
        // Draw the empty field rectangle.
        if (this.DropDownData.FieldOutlineThickness > 0.0F) {
            Color fieldOutlineColor = this.IsHovered ? this.DropDownData.FieldOutlineHoverColor : this.DropDownData.FieldOutlineColor;
            
            if (!this.Interactable) {
                fieldOutlineColor = this.DropDownData.DisabledFieldOutlineColor;
            }
            
            Vector2 fieldOutlineSize = this.Size * this.Scale * this.Gui.ScaleFactor;
            float outlineThickness = this.DropDownData.FieldOutlineThickness * this.Gui.ScaleFactor;
            context.PrimitiveBatch.DrawEmptyRectangle(new RectangleF(this.Position.X, this.Position.Y, fieldOutlineSize.X, fieldOutlineSize.Y), outlineThickness, this.Origin * this.Scale * this.Gui.ScaleFactor, this.Rotation, 0.5F, fieldOutlineColor);
        }
        
        // Draw arrow.
        this.DrawArrow(context.PrimitiveBatch);
        
        if (this.IsMenuOpen) {
            
            // Menu color.
            Color menuColor = this.IsHovered ? this.DropDownData.MenuHoverColor : this.DropDownData.MenuColor;
            
            if (!this.Interactable) {
                menuColor = this.DropDownData.DisabledMenuColor;
            }
            
            // Menu outline color.
            Color menuOutlineColor = this.IsHovered ? this.DropDownData.MenuOutlineHoverColor : this.DropDownData.MenuOutlineColor;
            
            if (!this.Interactable) {
                menuOutlineColor = this.DropDownData.DisabledMenuOutlineColor;
            }
            
            // Draw menu rectangle.
            this.DrawMenu(context.PrimitiveBatch, menuColor, menuOutlineColor);
            
            // Draw the scroll bar.
            this.DrawScrollBar(context.PrimitiveBatch);
            
            // Draw the slider.
            this.DrawSlider(context.PrimitiveBatch);
        }
        
        context.PrimitiveBatch.End();
        
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        // Draw field text.
        if (this.SelectedOption != null) {
            this.DrawText(context.SpriteBatch, this.SelectedOption, this.FieldTextAlignment, this.FieldTextOffset);
        }
        
        context.SpriteBatch.End();
        
        if (this.IsMenuOpen) {
            
            // Draw highlight.
            this.DrawHighlight(context.CommandList, framebuffer, context.PrimitiveBatch);
            
            // Draw options text.
            this.DrawOptionsText(context.CommandList, framebuffer, context.SpriteBatch, context.PrimitiveBatch);
        }
    }

    /// <summary>
    /// Calculates the position of the dropdown element based on its field size, anchor point, offset, and origin.
    /// </summary>
    /// <returns>A <see cref="Vector2"/> representing the calculated position of the element in the GUI.</returns>
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
        
        return new Vector2(
            MathF.Floor(finalPos.X / scaleFactor) * scaleFactor,
            MathF.Floor(finalPos.Y / scaleFactor) * scaleFactor
        );
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
    /// Draws the dropdown menu, including its background and outline.
    /// </summary>
    /// <param name="primitiveBatch">The primitive batch for rendering graphical primitives.</param>
    /// <param name="color">The fill color of the menu background.</param>
    /// <param name="outlineColor">The color of the menu's outline.</param>
    private void DrawMenu(PrimitiveBatch primitiveBatch, Color color, Color outlineColor) {
        int visibleOptions = Math.Min(this.Options.Count, this.MaxVisibleOptions);
        float scaleFactor = this.Gui.ScaleFactor;
        Vector2 scale = this.Scale * scaleFactor;

        // Calculate the menu dimensions locally
        Vector2 menuSize = new Vector2(this.Size.X, this.Size.Y * visibleOptions);
            
        if (this.Options.Count > this.MaxVisibleOptions) {
            menuSize.X -= this.DropDownData.SliderBarWidth;
        }

        // Calculate the origin offset locally so it draws below the field
        Vector2 menuOrigin = (this.Origin - new Vector2(0.0F, this.Size.Y)) * scale;
        Vector2 scaledMenuSize = menuSize * scale;
        
        // Draw the filled menu rectangle
        primitiveBatch.DrawFilledRectangle(new RectangleF(this.Position.X, this.Position.Y, scaledMenuSize.X, scaledMenuSize.Y), menuOrigin, this.Rotation, 0.5F, color);

        // Draw the empty menu rectangle
        if (this.DropDownData.MenuOutlineThickness > 0.0F) {
            float outlineThickness = this.DropDownData.MenuOutlineThickness * scaleFactor;
            Matrix3x2 rotationMatrix = Matrix3x2.CreateRotation(float.DegreesToRadians(this.Rotation), this.Position);
            
            // Calculate the four corners relative to Position, adjusted by Origin.
            Vector2 p1 = Vector2.Transform(this.Position - menuOrigin, rotationMatrix);
            Vector2 p2 = Vector2.Transform(new Vector2(this.Position.X + scaledMenuSize.X, this.Position.Y) - menuOrigin, rotationMatrix);
            Vector2 p3 = Vector2.Transform(new Vector2(this.Position.X, this.Position.Y + scaledMenuSize.Y) - menuOrigin, rotationMatrix);
            Vector2 p4 = Vector2.Transform(new Vector2(this.Position.X + scaledMenuSize.X, this.Position.Y + scaledMenuSize.Y) - menuOrigin, rotationMatrix);
            
            // Calculate thickness offsets.
            Vector2 horizontalNormal = Vector2.Normalize(new Vector2(-(p2 - p1).Y, (p2 - p1).X)) * (outlineThickness / 2f);
            Vector2 verticalNormal = Vector2.Normalize(new Vector2(-(p3 - p1).Y, (p3 - p1).X)) * (outlineThickness / 2f);
            
            // Left line.
            primitiveBatch.DrawLine(p1 - verticalNormal, p3 - verticalNormal, outlineThickness, 0.5F, outlineColor);
            
            // Bottom line.
            primitiveBatch.DrawLine(p3 - horizontalNormal, p4 - horizontalNormal, outlineThickness, 0.5F, outlineColor);
            
            // Right line.
            primitiveBatch.DrawLine(p2 + verticalNormal, p4 + verticalNormal, outlineThickness, 0.5F, outlineColor);
        }
    }
    
    /// <summary>
    /// Renders the dropdown menu's selectable options text, applying clipping and scrolling as needed.
    /// </summary>
    /// <param name="commandList">The command list used to issue rendering commands.</param>
    /// <param name="framebuffer">The framebuffer to which the options text is rendered.</param>
    /// <param name="spriteBatch">The sprite batch used for rendering text and graphical elements.</param>
    /// <param name="primitiveBatch">The primitive batch used to draw stencil masks for clipping.</param>
    private void DrawOptionsText(CommandList commandList, Framebuffer framebuffer, SpriteBatch spriteBatch, PrimitiveBatch primitiveBatch) {
        int visibleOptions = this.Options.Count;
        float currentScrollIndex = 0;
        
        if (this.Options.Count > this.MaxVisibleOptions) {
            currentScrollIndex = Math.Clamp(this._scrollPercent * (this.Options.Count - this.MaxVisibleOptions), 0, this.Options.Count - 1);
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
        
        float outlineThicknessScale = this.DropDownData.MenuOutlineThickness * scale.Y;
        float maskHeight = fieldSize.Y * this.MaxVisibleOptions - (this.DropDownData.MenuOutlineThickness) * scale.Y + outlineThicknessScale / 2.0F;
        
        // Define the clipping mask area (the visible part of the dropdown list).
        RectangleF maskRect = new RectangleF(this.Position.X, this.Position.Y, maskWidth, maskHeight);
        Vector2 maskOrigin = this.Origin * scale - new Vector2(0.0F, fieldSize.Y) - new Vector2(0.0F, this.DropDownData.MenuOutlineThickness / 2.0F * scale.Y) + new Vector2(0.0F, outlineThicknessScale / 2.0F);

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
    /// <param name="primitiveBatch">The primitive batch for rendering geometric shapes.</param>
    private void DrawHighlight(CommandList commandList, Framebuffer framebuffer, PrimitiveBatch primitiveBatch) {
        Vector2 fieldSize = this.Size * this.Scale * this.Gui.ScaleFactor;
        Vector2 scale = this.Scale * this.Gui.ScaleFactor;
        
        int visibleOptions = this.Options.Count;
        float currentScrollIndex = 0;
        
        // Calculate visible options and adjust field size if the scrollbar is active.
        if (this.Options.Count > this.MaxVisibleOptions) {
            fieldSize.X -= this.DropDownData.SliderBarWidth * scale.X;
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
        
        // Menu outline color.
        Color menuOutlineColor = this.IsHovered ? this.DropDownData.MenuOutlineHoverColor : this.DropDownData.MenuOutlineColor;
        
        if (!this.Interactable) {
            menuOutlineColor = this.DropDownData.DisabledMenuOutlineColor;
        }
        
        // Draw the actual highlight.
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        primitiveBatch.PushDepthStencilState(stencilTest);
        this.DrawMenu(primitiveBatch, this.DropDownData.HighlightColor, menuOutlineColor);
        primitiveBatch.PopDepthStencilState();
        primitiveBatch.End();
    }
    
    /// <summary>
    /// Renders the scrollbar for the dropdown menu if the total number of options exceeds the maximum visible options.
    /// </summary>
    /// <param name="primitiveBatch">The rendering batch used to draw primitive shapes.</param>
    private void DrawScrollBar(PrimitiveBatch primitiveBatch) {
        if (this.Options.Count <= this.MaxVisibleOptions) {
            return;
        }
        
        float scaleFactor = this.Gui.ScaleFactor;
        Vector2 scale = this.Scale * scaleFactor;
        
        // Calculate local dimensions for the scrollbar track.
        Vector2 trackSize = new Vector2(this.DropDownData.SliderBarWidth, this.Size.Y * this.MaxVisibleOptions) * scale;
        Vector2 trackOrigin = (this.Origin - new Vector2(this.Size.X - this.DropDownData.SliderBarWidth, this.Size.Y)) * scale;
        
        // Slide bar color.
        Color color = this.IsHovered ? this.DropDownData.SliderBarHoverColor : this.DropDownData.SliderBarColor;
        
        if (!this.Interactable) {
            color = this.DropDownData.DisabledSliderBarColor;
        }
        
        // Draw track background.
        primitiveBatch.DrawFilledRectangle(new RectangleF(this.Position.X, this.Position.Y, trackSize.X, trackSize.Y), trackOrigin, this.Rotation, 0.5F, color);
        
        // Draw track outline.
        if (this.DropDownData.MenuOutlineThickness > 0.0F) {
            float outlineThickness = this.DropDownData.MenuOutlineThickness * scaleFactor;
            Matrix3x2 rotationMatrix = Matrix3x2.CreateRotation(float.DegreesToRadians(this.Rotation), this.Position);
            
            // Calculate corners.
            Vector2 p1 = Vector2.Transform(this.Position - trackOrigin, rotationMatrix);
            Vector2 p2 = Vector2.Transform(new Vector2(this.Position.X + trackSize.X, this.Position.Y) - trackOrigin, rotationMatrix);
            Vector2 p3 = Vector2.Transform(new Vector2(this.Position.X, this.Position.Y + trackSize.Y) - trackOrigin, rotationMatrix);
            Vector2 p4 = Vector2.Transform(new Vector2(this.Position.X + trackSize.X, this.Position.Y + trackSize.Y) - trackOrigin, rotationMatrix);
            
            // Normals for thickness offset.
            Vector2 hNormal = Vector2.Normalize(new Vector2(-(p2 - p1).Y, (p2 - p1).X)) * (outlineThickness / 2.0F);
            Vector2 vNormal = Vector2.Normalize(new Vector2(-(p3 - p1).Y, (p3 - p1).X)) * (outlineThickness / 2.0F);
            
            // Slide bar outline color.
            Color outlineColor = this.IsHovered ? this.DropDownData.MenuOutlineHoverColor : this.DropDownData.MenuOutlineColor;
        
            if (!this.Interactable) {
                outlineColor = this.DropDownData.DisabledMenuOutlineColor;
            }
            
            // Draw outline segments.
            primitiveBatch.DrawLine(p3 - hNormal, p4 - hNormal, outlineThickness, 0.5F, outlineColor);
            primitiveBatch.DrawLine(p2 + vNormal, p4 + vNormal, outlineThickness, 0.5F, outlineColor);
        }
    }
    
    /// <summary>
    /// Draws the slider for the dropdown menu, visually representing the scroll position.
    /// </summary>
    /// <param name="primitiveBatch">The primitive batch used for rendering graphical elements.</param>
    private void DrawSlider(PrimitiveBatch primitiveBatch) {
        if (this.Options.Count <= this.MaxVisibleOptions) {
            return;
        }
        
        float scaleFactor = this.Gui.ScaleFactor;
        Vector2 scale = this.Scale * scaleFactor;
        
        // Calculate the total track height.
        float trackHeight = (this.Size.Y * this.MaxVisibleOptions) - this.DropDownData.MenuOutlineThickness / 2.0F;
        float sliderHeight = this.DropDownData.SliderSize.Y;
        float usableTrackHeight = trackHeight - sliderHeight;
        
        // Calculate the Y offset of the slider within the track based on scroll percentage.
        float sliderYOffset = this.Size.Y + (this._scrollPercent * usableTrackHeight);
        
        // The slider is horizontally centered in the SliderBarWidth area, plus the SliderOffset.X.
        float sliderXOffset = this.Size.X - this.DropDownData.SliderBarWidth + (this.DropDownData.SliderBarWidth - this.DropDownData.SliderSize.X) / 2.0F - this.DropDownData.MenuOutlineThickness / 4.0F;
        
        Vector2 sliderSize = this.DropDownData.SliderSize * scale;
        Vector2 sliderOrigin = (this.Origin - new Vector2(sliderXOffset, sliderYOffset)) * scale;
        
        // Slider color.
        Color sliderColor = this.IsHovered ? this.DropDownData.SliderHoverColor : this.DropDownData.SliderColor;
        
        if (!this.Interactable) {
            sliderColor = this.DropDownData.DisabledSliderColor;
        }
        
        // Draw slider rectangle.
        primitiveBatch.DrawFilledRectangle(new RectangleF(this.Position.X, this.Position.Y, sliderSize.X, sliderSize.Y), sliderOrigin, this.Rotation, 0.5F, sliderColor);
        
        // Draw slider outline.
        if (this.DropDownData.SliderOutlineThickness > 0.0F) {
            float thickness = this.DropDownData.SliderOutlineThickness * scaleFactor;
            Matrix3x2 rotationMatrix = Matrix3x2.CreateRotation(float.DegreesToRadians(this.Rotation), this.Position);
            
            // Calculate corners.
            Vector2 p1 = Vector2.Transform(this.Position - sliderOrigin, rotationMatrix);
            Vector2 p2 = Vector2.Transform(new Vector2(this.Position.X + sliderSize.X, this.Position.Y) - sliderOrigin, rotationMatrix);
            Vector2 p3 = Vector2.Transform(new Vector2(this.Position.X, this.Position.Y + sliderSize.Y) - sliderOrigin, rotationMatrix);
            Vector2 p4 = Vector2.Transform(new Vector2(this.Position.X + sliderSize.X, this.Position.Y + sliderSize.Y) - sliderOrigin, rotationMatrix);
            
            // Normals for thickness offset.
            Vector2 hNormal = Vector2.Normalize(new Vector2(-(p2 - p1).Y, (p2 - p1).X)) * (thickness / 2.0F);
            Vector2 vNormal = Vector2.Normalize(new Vector2(-(p3 - p1).Y, (p3 - p1).X)) * (thickness / 2.0F);
            
            Color outlineColor = this.IsHovered ? this.DropDownData.SliderOutlineHoverColor : this.DropDownData.SliderOutlineColor;
            
            if (!this.Interactable) {
                outlineColor = this.DropDownData.DisabledSliderOutlineColor;
            }
            
            // Draw outline segments.
            primitiveBatch.DrawLine(p1 + hNormal, p2 + hNormal, thickness, 0.5F, outlineColor);
            primitiveBatch.DrawLine(p3 - hNormal, p4 - hNormal, thickness, 0.5F, outlineColor);
            primitiveBatch.DrawLine(p1 - vNormal, p3 - vNormal, thickness, 0.5F, outlineColor);
            primitiveBatch.DrawLine(p2 + vNormal, p4 + vNormal, thickness, 0.5F, outlineColor); 
        }
    }
    
    /// <summary>
    /// Draws the arrow for the dropdown element using the specified primitive batch.
    /// </summary>
    /// <param name="primitiveBatch">The primitive batch used for rendering the arrow shape.</param>
    private void DrawArrow(PrimitiveBatch primitiveBatch) {
        if (!this.DropDownData.ArrowSize.HasValue) {
            return;
        }
        
        float scaleFactor = this.Gui.ScaleFactor;
        Vector2 scale = this.Scale * scaleFactor;
        
        float arrowWidth = this.DropDownData.ArrowSize.Value.X;
        float arrowHeight = this.DropDownData.ArrowSize.Value.Y;
        
        float offsetX = this.Size.X - arrowWidth - this.ArrowOffset.X;
        float offsetY = (this.Size.Y - arrowHeight) / 2.0F + this.ArrowOffset.Y;
        
        Vector2 arrowSize = new Vector2(arrowWidth, arrowHeight);
        Vector2 arrowCenter = arrowSize / 2.0F;
        
        // Calculate the world position of the arrow center.
        Vector2 localToArrowCenter = (new Vector2(offsetX, offsetY) + arrowCenter - this.Origin) * scale;
        Vector2 rotatedLocalToArrowCenter = Vector2.Transform(localToArrowCenter, Matrix3x2.CreateRotation(float.DegreesToRadians(this.Rotation)));
        Vector2 arrowWorldCenter = this.Position + rotatedLocalToArrowCenter;
        
        // Local triangle vertices relative to the arrow center.
        Vector2 v1 = new Vector2(-arrowCenter.X, -arrowCenter.Y) * scaleFactor;
        Vector2 v2 = new Vector2(arrowCenter.X, -arrowCenter.Y) * scaleFactor;
        Vector2 v3 = new Vector2(0.0F, arrowCenter.Y) * scaleFactor;
        
        // Apply rotation.
        float totalRotation = float.DegreesToRadians(this.Rotation) + this._arrowRotation;
        Matrix3x2 arrowRotationMatrix = Matrix3x2.CreateRotation(totalRotation);
        
        Vector2 p1 = arrowWorldCenter + Vector2.Transform(v1, arrowRotationMatrix);
        Vector2 p2 = arrowWorldCenter + Vector2.Transform(v2, arrowRotationMatrix);
        Vector2 p3 = arrowWorldCenter + Vector2.Transform(v3, arrowRotationMatrix);
        
        Color color = this.IsHovered ? this.DropDownData.ArrowHoverColor : this.DropDownData.ArrowColor;
        
        if (!this.Interactable) {
            color = this.DropDownData.DisabledArrowColor;
        }
        
        primitiveBatch.DrawFilledTriangle(p1, p2, p3, 0.5F, color);
    }
    
    /// <summary>
    /// Renders text on the GUI element using the specified parameters.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> used to render the text.</param>
    /// <param name="labelData">The <see cref="LabelData"/> containing font, text, and styling information.</param>
    /// <param name="textAlignment">The <see cref="TextAlignment"/> specifying the alignment of the text (Left, Center, or Right).</param>
    /// <param name="textOffset">The offset for the text position relative to the element.</param>
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