using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Images;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Mathematics;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Batching;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrith;

namespace Sparkle.CSharp.GUI.Elements;

public class TextureScrollViewElement : GuiElement {
    
    /// <summary>
    /// The visual and rendering configuration used for the scroll view, including textures, colors, and slider settings.
    /// </summary>
    public TextureScrollViewData Data { get; private set; }
    
    /// <summary>
    /// Insets applied to the content area, controlling padding inside the scroll view.
    /// </summary>
    public (float Left, float Right, float Top, float Bottom) MenuContentInsets;
    
    /// <summary>
    /// Controls how sensitive mouse wheel scrolling is.
    /// </summary>
    public float ScrollSensitivity;
    
    /// <summary>
    /// Controls how quickly the scroll position interpolates toward the target value.
    /// </summary>
    public float ScrollLerpSpeed;
    
    /// <summary>
    /// The total number of GPU draw calls issued by the content render queue during the last draw pass.
    /// </summary>
    public int ContentDrawCallCount => this._renderQueue.DrawCallCount;
    
    /// <summary>
    /// The render queue used to collect and batch content draw commands.
    /// </summary>
    private GuiRenderQueue _renderQueue;
    
    /// <summary>
    /// A list of content elements that need to be rendered during the draw phase of the content lifecycle.
    /// </summary>
    private List<GuiElement> _contentToDraw;
    
    /// <summary>
    /// Stable content insertion order used as the secondary draw sort key.
    /// </summary>
    private Dictionary<string, int> _contentDrawOrder;
    
    /// <summary>
    /// All active child GUI elements contained inside this scroll view.
    /// </summary>
    private OrderedDictionary<string, GuiElement> _content;
    
    /// <summary>
    /// Queue of elements that will be added on the next update tick.
    /// </summary>
    private List<GuiElement> _contentToAdd;
    
    /// <summary>
    /// Queue of content names scheduled for removal.
    /// </summary>
    private List<string> _contentToRemove;
    
    /// <summary>
    /// Stores original offsets of content elements for stable scrolling calculations.
    /// </summary>
    private Dictionary<GuiElement, Vector2> _contentOffsets;
    
    /// <summary>
    /// Current local top-left position for each content element.
    /// </summary>
    private Dictionary<GuiElement, Vector2> _contentLocalTopLefts;
    
    /// <summary>
    /// Current local bounds for each content element.
    /// </summary>
    private Dictionary<GuiElement, RectangleF> _contentLocalBounds;
    
    /// <summary>
    /// Cached scrollable height.
    /// </summary>
    private float _scrollableHeight;
    
    /// <summary>
    /// Current visible content window in local content coordinates.
    /// </summary>
    private RectangleF _visibleContentWindow;
    
    /// <summary>
    /// Cached scale used to transform local content positions to world positions.
    /// </summary>
    private Vector2 _contentTransformScale;
    
    /// <summary>
    /// Cached scaled content inset.
    /// </summary>
    private Vector2 _contentTransformInsetTopLeft;
    
    /// <summary>
    /// Cached scaled origin of this scroll view.
    /// </summary>
    private Vector2 _contentTransformParentOrigin;
    
    /// <summary>
    /// Cached world top-left position of this scroll view.
    /// </summary>
    private Vector2 _contentTransformViewTopLeftWorld;
    
    /// <summary>
    /// Cached rotation matrix of this scroll view.
    /// </summary>
    private Matrix3x2 _contentTransformRotation;
    
    /// <summary>
    /// Cached scaled scroll offset.
    /// </summary>
    private float _contentTransformScrollOffsetY;
    
    /// <summary>
    /// Cached GUI scale factor.
    /// </summary>
    private float _contentTransformGuiScaleFactor;
    
    /// <summary>
    /// Initial content provided during construction, applied during Init.
    /// </summary>
    private List<KeyValuePair<string, GuiElement>> _initialContent;
    
    /// <summary>
    /// Offscreen render target used to draw scrollable content.
    /// </summary>
    private RenderTexture2D? _contentRenderTarget;
    
    /// <summary>
    /// Final resolved texture containing rendered scroll content.
    /// </summary>
    private Texture2D? _contentResult;
    
    /// <summary>
    /// Current scroll percentage (0 = top, 1 = bottom).
    /// </summary>
    private float _scrollPercent;
    
    /// <summary>
    /// Target scroll percentage used for smooth interpolation.
    /// </summary>
    private float _targetScrollPercent;
    
    /// <summary>
    /// True when the user is dragging the scrollbar slider.
    /// </summary>
    private bool _isDraggingSlider;
    
    /// <summary>
    /// Represents a scrollable GUI element that displays a texture-based menu or content with the ability to scroll.
    /// </summary>
    /// <param name="data">The data object containing texture and layout information for the scroll view.</param>
    /// <param name="content">An optional collection of key-value pairs where each key is a unique identifier and each value is a GUI element to be displayed in the scroll view.</param>
    /// <param name="anchor">Specifies the anchor position that determines the alignment of the scroll view within its parent element.</param>
    /// <param name="offset">Defines the offset position of the scroll view relative to its anchor.</param>
    /// <param name="menuContentInsets">Optional insets defining padding or margins around the scrollable content in the form of left, right, top, and bottom offsets.</param>
    /// <param name="scrollSensitivity">Indicates the sensitivity of the scroll mechanics, influencing the movement per scroll input.</param>
    /// <param name="scrollLerpSpeed">Defines the speed of the scrolling animation, affecting how smoothly the content scrolls.</param>
    /// <param name="size">Optional size of the scroll view. Default value is derived from the menu source rectangle dimensions in the <paramref name="data"/> parameter.</param>
    /// <param name="scale">Optional scaling factor applied to the scroll view, modifying its rendered size proportionally.</param>
    /// <param name="origin">Optional origin point for transformations like rotation and scaling, specified in normalized coordinates.</param>
    /// <param name="rotation">The rotation angle in radians applied to the scroll view.</param>
    /// <param name="renderOrder">The order in which the element is rendered, relative to others.</param>
    /// <param name="clickFunc">Optional callback function invoked when the scroll view detects a click interaction. This function receives the clicked <see cref="GuiElement"/> as a parameter and returns a boolean indicating success.</param>
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
        float rotation = 0.0F,
        int renderOrder = 0,
        Func<GuiElement, bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, renderOrder, clickFunc) {
        this.Data = data;
        this.Size = size ?? new Vector2(data.MenuSourceRect.Width, data.MenuSourceRect.Height);
        this.MenuContentInsets = menuContentInsets ?? (0.0F, 0.0F, 0.0F, 0.0F);
        this.ScrollSensitivity = scrollSensitivity;
        this.ScrollLerpSpeed = scrollLerpSpeed;
        
        this._renderQueue = new GuiRenderQueue();
        this._contentToDraw = new List<GuiElement>();
        this._contentDrawOrder = new Dictionary<string, int>();
        this._content = new OrderedDictionary<string, GuiElement>();
        this._contentToAdd = new List<GuiElement>();
        this._contentToRemove = new List<string>();
        this._contentOffsets = new Dictionary<GuiElement, Vector2>();
        this._contentLocalTopLefts = new Dictionary<GuiElement, Vector2>();
        this._contentLocalBounds = new Dictionary<GuiElement, RectangleF>();
        this._scrollableHeight = 0.0F;
        this._visibleContentWindow = new RectangleF(0.0F, 0.0F, 0.0F, 0.0F);
        this._contentTransformScale = Vector2.One;
        this._contentTransformInsetTopLeft = Vector2.Zero;
        this._contentTransformParentOrigin = Vector2.Zero;
        this._contentTransformViewTopLeftWorld = Vector2.Zero;
        this._contentTransformRotation = Matrix3x2.Identity;
        this._contentTransformScrollOffsetY = 0.0F;
        this._contentTransformGuiScaleFactor = 1.0F;
        this._initialContent = new List<KeyValuePair<string, GuiElement>>();
        
        if (content != null) {
            foreach ((string name, GuiElement element) in content) {
                this._initialContent.Add(new KeyValuePair<string, GuiElement>(name, element));
            }
        }
    }
    
    /// <summary>
    /// Initializes the scroll view and applies any initial content.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        
        // Add content elements to the Scroll view element.
        foreach ((string name, GuiElement element) in this._initialContent) {
            this.AddContent(name, element);
        }
        
        this._initialContent.Clear();
        
        // Create render target.
        this.EnsureContentRenderTarget();
    }
    
    /// <summary>
    /// Updates scroll input, slider interaction, and all child content elements.
    /// </summary>
    /// <param name="delta">The elapsed time in seconds since the previous update.</param>
    /// <param name="interactionHandled">A reference flag indicating whether an interaction has already been consumed this tick.</param>
    protected internal override void Update(double delta, ref bool interactionHandled) {
        
        // Handle removing content.
        foreach (string name in this._contentToRemove) {
            if (this._content.Remove(name, out GuiElement? element)) {
                this._contentOffsets.Remove(element);
                this._contentLocalTopLefts.Remove(element);
                this._contentLocalBounds.Remove(element);
                element.Dispose();
            }
        }
        
        this._contentToRemove.Clear();
        
        // Handle adding content.
        foreach (GuiElement element in this._contentToAdd) {
            this._content.Add(element.Name, element);
        }
        
        this._contentToAdd.Clear();
        
        bool interactionHandledBeforeBaseUpdate = interactionHandled;
        
        // Update base element.
        base.Update(delta, ref interactionHandled);
        this.RebuildContentLayout(false);
        
        Vector2 mousePos = Input.GetMousePosition();
        Vector2 scale = this.Scale * this.Gui.ScaleFactor;
        Vector2 viewSize = this.Size * scale;
        
        Matrix3x2 transform = Matrix3x2.CreateTranslation(-this.Position) *
                              Matrix3x2.CreateRotation(-float.DegreesToRadians(this.Rotation)) *
                              Matrix3x2.CreateTranslation(this.Origin * scale);
        
        Vector2 localMouse = Vector2.Transform(mousePos, transform);
        
        bool hasScrollableContent = this.HasScrollableContent();
        float sliderBarWidth = this.Data.SliderBarWidth * scale.X;
        float sliderBarHeight = viewSize.Y;
        
        RectangleF localViewRect = new RectangleF(0.0F, 0.0F, viewSize.X, viewSize.Y);
        RectangleF localSliderBarRect = new RectangleF(viewSize.X - sliderBarWidth, 0.0F, sliderBarWidth, sliderBarHeight);
        
        bool canStartScrollViewInteraction = !interactionHandledBeforeBaseUpdate;
        bool canUseScrollViewInteraction = canStartScrollViewInteraction || this._isDraggingSlider;
        
        if (this.Interactable && hasScrollableContent) {
            if (canStartScrollViewInteraction && !this._isDraggingSlider && localViewRect.Contains(localMouse)) {
                if (Input.IsMouseScrolling(out Vector2 wheelDelta)) {
                    float scrollableHeight = this.GetScrollableHeight();
                    
                    if (scrollableHeight > 0.0F) {
                        float currentOffset = this._targetScrollPercent * scrollableHeight;
                        float wheelStep = MathF.Max(1.0F, this.GetVisibleContentSize(false).Y * this.ScrollSensitivity);
                        currentOffset = Math.Clamp(currentOffset - wheelDelta.Y * wheelStep, 0.0F, scrollableHeight);
                        this._targetScrollPercent = currentOffset / scrollableHeight;
                        
                        interactionHandled = true;
                    }
                }
            }
            
            if (!this._isDraggingSlider) {
                this._scrollPercent = float.Lerp(this._scrollPercent, this._targetScrollPercent, (float) (this.ScrollLerpSpeed * delta));
            }
            
            this._scrollPercent = Math.Clamp(this._scrollPercent, 0.0F, 1.0F);
            this._targetScrollPercent = Math.Clamp(this._targetScrollPercent, 0.0F, 1.0F);
            
            if (Input.IsMouseButtonDown(MouseButton.Left)) {
                if (canStartScrollViewInteraction && Input.IsMouseButtonPressed(MouseButton.Left) && localSliderBarRect.Contains(localMouse)) {
                    this._isDraggingSlider = true;
                    interactionHandled = true;
                }
                
                if (canUseScrollViewInteraction && this._isDraggingSlider) {
                    float sliderHeight = this.Data.SliderSourceRect.Height * scale.Y;
                    float usableSliderBarHeight = sliderBarHeight - sliderHeight;
                    float relativeY = localMouse.Y - sliderHeight / 2.0F;
                    
                    this._scrollPercent = usableSliderBarHeight > 0.0F ? Math.Clamp(relativeY / usableSliderBarHeight, 0.0F, 1.0F) : 0.0F;
                    this._targetScrollPercent = this._scrollPercent;
                    interactionHandled = true;
                }
            }
            else {
                this._isDraggingSlider = false;
            }
        }
        else {
            this._scrollPercent = 0.0F;
            this._targetScrollPercent = 0.0F;
            this._isDraggingSlider = false;
        }
        
        this.RebuildContentTransform();
        
        // Update content elements.
        Vector2 contentInsetTopLeft = this.GetContentInsetTopLeft() * scale;
        Vector2 visibleContentSize = this.GetVisibleContentSize() * scale;
        RectangleF localContentRect = new RectangleF(contentInsetTopLeft.X, contentInsetTopLeft.Y, visibleContentSize.X, visibleContentSize.Y);
        
        bool contentInteractionHandled = interactionHandledBeforeBaseUpdate || this._isDraggingSlider || !this.Interactable || !localContentRect.Contains(localMouse);
        
        foreach (GuiElement element in this._content.Values) {
            this.UpdateContentElement(element, delta, ref contentInteractionHandled);
        }
        
        this.RebuildContentLayout();
    }
    
    /// <summary>
    /// Runs the post-update pass on the scroll view and propagates it to all content elements.
    /// </summary>
    /// <param name="delta">The elapsed time in seconds since the previous update.</param>
    protected internal override void AfterUpdate(double delta) {
        base.AfterUpdate(delta);
        
        foreach (GuiElement element in this._content.Values) {
            element.AfterUpdate(delta);
        }
    }
    
    /// <summary>
    /// Runs the fixed-timestep update on the scroll view and propagates it to all content elements.
    /// </summary>
    /// <param name="fixedStep">The fixed time step in seconds.</param>
    protected internal override void FixedUpdate(double fixedStep) {
        base.FixedUpdate(fixedStep);
        
        foreach (GuiElement element in this._content.Values) {
            element.FixedUpdate(fixedStep);
        }
    }
    
    /// <summary>
    /// Submits draw commands for this GUI element to the render queue for later batched rendering.
    /// </summary>
    /// <param name="renderQueue">The queue that collects draw commands for deferred execution.</param>
    protected internal override void Draw(GuiRenderQueue renderQueue) {
        base.Draw(renderQueue);
        
        Color menuColor = this.IsHovered ? this.Data.MenuHoverColor : this.Data.MenuColor;
        
        if (!this.Interactable) {
            menuColor = this.Data.DisabledMenuColor;
        }
        
        Vector2 originalSize = this.Size;
        
        // Draw the menu smaller, leaving space on the right side for the slider bar.
        if (this.HasScrollableContent()) {
            this.Size = new Vector2(MathF.Max(0.0F, originalSize.X - this.Data.SliderBarWidth), originalSize.Y);
        }
        
        switch (this.Data.MenuResizeMode) {
            case ResizeMode.None:
                this.DrawNormal(renderQueue, this.Data.MenuTexture, this.Data.MenuSampler, this.Data.MenuSourceRect, menuColor, this.Data.MenuFlip, this.Data.MenuPixelSnap, this.Data.Effect, this.Data.BlendState);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(renderQueue, this.Data.MenuTexture, this.Data.MenuSampler, this.Data.MenuSourceRect, this.Data.MenuBorderInsets, this.Data.MenuResizeMode == ResizeMode.TileCenter, menuColor, this.Data.MenuFlip, this.Data.MenuPixelSnap, this.Data.Effect, this.Data.BlendState);
                break;
        }
        
        this.Size = originalSize;
        
        // Draw slider bar and slider.
        this.DrawSliderBar(renderQueue);
        this.DrawSlider(renderQueue);
        
        // Draw content elements via direct GPU call.
        renderQueue.SubmitDirect(static (context, framebuffer, self) => {
            self.DrawContent(context, framebuffer);
        }, this);
    }
    
    /// <summary>
    /// Handles a layout/window resize by forwarding it to all content elements and recreating the content render target.
    /// </summary>
    /// <param name="rectangle">The new bounding rectangle to resize against.</param>
    protected internal override void Resize(Rectangle rectangle) {
        base.Resize(rectangle);
        
        foreach (GuiElement element in this._content.Values) {
            element.Resize(rectangle);
        }
        
        this.RebuildContentLayout();
        this.EnsureContentRenderTarget(true);
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
        
        this._contentOffsets[element] = element.Offset;
        
        // Init the element.
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
    /// Draws the slider bar track on the right side of the scroll view.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    private void DrawSliderBar(GuiRenderQueue renderQueue) {
        if (!this.HasScrollableContent()) {
            return;
        }
        
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
                this.DrawNormal(renderQueue, this.Data.SliderBarTexture, this.Data.SliderBarSampler, this.Data.SliderBarSourceRect, color, this.Data.SliderBarFlip, this.Data.SliderBarPixelSnap, this.Data.Effect, this.Data.BlendState);
                break;
            
            case ResizeMode.NineSlice:
            case ResizeMode.TileCenter:
                this.DrawNineSlice(renderQueue, this.Data.SliderBarTexture, this.Data.SliderBarSampler, this.Data.SliderBarSourceRect, this.Data.SliderBarBorderInsets, this.Data.SliderBarResizeMode == ResizeMode.TileCenter, color, this.Data.SliderBarFlip, this.Data.SliderBarPixelSnap, this.Data.Effect, this.Data.BlendState);
                break;
        }
        
        this.Size = originalSize;
        this.Origin = originalOrigin;
    }
    
    /// <summary>
    /// Draws the slider handle inside the slider bar.
    /// </summary>
    /// <param name="renderQueue">The render queue used to render the sprite.</param>
    private void DrawSlider(GuiRenderQueue renderQueue) {
        if (!this.HasScrollableContent()) {
            return;
        }
        
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
        
        SpriteGuiRenderState renderState = new SpriteGuiRenderState(this.Data.SliderSampler, this.Data.Effect, this.Data.BlendState);
        renderQueue.UseSprite(renderState).DrawTexture(this.Data.SliderTexture, this.Position, 0.5F, this.Data.SliderSourceRect, this.Scale * this.Gui.ScaleFactor, origin, this.Data.SliderPixelSnap, this.Rotation, color, this.Data.SliderFlip);
    }
    
    /// <summary>
    /// Renders all content elements into the offscreen target and composites the result into the visible content area, clipped by a stencil mask.
    /// </summary>
    /// <param name="context">The graphics context providing the command list and batches.</param>
    /// <param name="framebuffer">The destination framebuffer to composite the content into.</param>
    private void DrawContent(GraphicsContext context, Framebuffer framebuffer) {
        if (this._content.Count <= 0) {
            return;
        }
        
        this.EnsureContentRenderTarget();
        
        if (this._contentRenderTarget == null || this._contentResult == null) {
            return;
        }
        
        this.RebuildContentTransform();
        
        context.CommandList.SetFramebuffer(this._contentRenderTarget.Framebuffer);
        context.CommandList.ClearColorTarget(0, new Color(0, 0, 0, 0).ToRgbaFloat());
        context.CommandList.ClearDepthStencil(1.0F);
        
        // Draw content elements.
        this._renderQueue.Begin(context, framebuffer);
        
        // Draw content.
        foreach (GuiElement element in this._contentToDraw) {
            if (element.Enabled && this.IsContentElementVisibleInLayout(element, out bool needsWorldVisibilityCheck)) {
                this.DrawContentElement(element, this._renderQueue, needsWorldVisibilityCheck);
            }
        }
        
        this._renderQueue.End();
        
        context.CommandList.CopyTexture(this._contentRenderTarget.ColorTexture, this._contentResult.DeviceTexture);
        context.CommandList.SetFramebuffer(framebuffer);
        
        this.DrawContentMask(context.CommandList, framebuffer, context.PrimitiveBatch);
        this.DrawContentResult(context.CommandList, framebuffer, context.SpriteBatch);
    }
    
    /// <summary>
    /// Writes a stencil mask covering the visible content area so that the composited content is clipped to the view bounds.
    /// </summary>
    /// <param name="commandList">The command list used to record the mask draw.</param>
    /// <param name="framebuffer">The framebuffer the mask is written into.</param>
    /// <param name="primitiveBatch">The primitive batch used to draw the masking rectangle.</param>
    private void DrawContentMask(CommandList commandList, Framebuffer framebuffer, PrimitiveBatch primitiveBatch) {
        Vector2 scale = this.Scale * this.Gui.ScaleFactor;
        Vector2 contentInsetTopLeft = this.GetContentInsetTopLeft() * scale;
        Vector2 visibleContentSize = this.GetVisibleContentSize() * scale;
        
        RectangleF maskRect = new RectangleF(this.Position.X, this.Position.Y, visibleContentSize.X, visibleContentSize.Y);
        Vector2 maskOrigin = this.Origin * scale - contentInsetTopLeft;
        
        DepthStencilStateDescription stencilWrite = new DepthStencilStateDescription() {
            StencilTestEnabled = true,
            StencilWriteMask = 0xFF,
            StencilReference = 4,
            StencilFront = new StencilBehaviorDescription() {
                Comparison = ComparisonKind.Always,
                Pass = StencilOperation.Replace
            },
            StencilBack = new StencilBehaviorDescription() {
                Comparison = ComparisonKind.Always,
                Pass = StencilOperation.Replace
            }
        };
        
        primitiveBatch.Begin(commandList, framebuffer.OutputDescription);
        primitiveBatch.PushDepthStencilState(stencilWrite);
        primitiveBatch.DrawFilledRectangle(maskRect, maskOrigin, this.Rotation, 0.5F, new Color(255, 255, 255, 0));
        primitiveBatch.PopDepthStencilState();
        primitiveBatch.End();
    }
    
    /// <summary>
    /// Draws the rendered content texture into the framebuffer, clipped to the stencil mask written by <see cref="DrawContentMask"/>.
    /// </summary>
    /// <param name="commandList">The command list used to record the draw.</param>
    /// <param name="framebuffer">The destination framebuffer.</param>
    /// <param name="spriteBatch">The sprite batch used to draw the content result texture.</param>
    private void DrawContentResult(CommandList commandList, Framebuffer framebuffer, SpriteBatch spriteBatch) {
        if (this._contentResult == null) {
            return;
        }
        
        DepthStencilStateDescription stencilTest = new DepthStencilStateDescription() {
            StencilTestEnabled = true,
            StencilReadMask = 0xFF,
            StencilReference = 4,
            StencilFront = new StencilBehaviorDescription() {
                Comparison = ComparisonKind.Equal,
                Pass = StencilOperation.Keep
            },
            StencilBack = new StencilBehaviorDescription() {
                Comparison = ComparisonKind.Equal,
                Pass = StencilOperation.Keep
            }
        };
        
        Rectangle sourceRect = new Rectangle(0, 0, (int) this._contentResult.Width, (int) this._contentResult.Height);
        
        spriteBatch.Begin(commandList, framebuffer.OutputDescription);
        spriteBatch.PushDepthStencilState(stencilTest);
        spriteBatch.DrawTexture(this._contentResult, Vector2.Zero, 0.5F, sourceRect, Vector2.One, Vector2.Zero, false, 0.0F, Color.White);
        spriteBatch.PopDepthStencilState();
        spriteBatch.End();
    }
    
    /// <summary>
    /// Submits the batched draw commands for a single content element, temporarily reanchoring and offsetting it to account for the current scroll position and content insets, then restores its original transform.
    /// </summary>
    /// <param name="element">The content element whose draw commands are submitted.</param>
    /// <param name="renderQueue">The render queue the draw commands are submitted into.</param>
    /// <param name="needsWorldVisibilityCheck">Whether to run the transformed world-space visibility check before drawing.</param>
    private void DrawContentElement(GuiElement element, GuiRenderQueue renderQueue, bool needsWorldVisibilityCheck) {
        Anchor originalAnchor = element.AnchorPoint;
        Vector2 originalOffset = element.Offset;
        Vector2 originalScale = element.Scale;
        float originalRotation = element.Rotation;
        bool originalInteractable = element.Interactable;
        Vector2 desiredTopLeftWorld = this.GetContentElementTopLeftWorld(this.GetContentLocalTopLeft(element));
        
        element.AnchorPoint = Anchor.TopLeft;
        element.Offset = (desiredTopLeftWorld - element.Origin) / this._contentTransformGuiScaleFactor;
        element.Scale = originalScale * this.Scale;
        element.Rotation = originalRotation + this.Rotation;
        element.Interactable = originalInteractable && this.Interactable;
        element.UpdatePosAndSize();
        
        if (!needsWorldVisibilityCheck || this.IsContentElementVisible(element)) {
            element.Draw(renderQueue);
        }
        
        element.AnchorPoint = originalAnchor;
        element.Offset = originalOffset;
        element.Scale = originalScale;
        element.Rotation = originalRotation;
        element.Interactable = originalInteractable;
        element.UpdatePosAndSize();
    }
    
    /// <summary>
    /// Updates a single content element, temporarily reanchoring and offsetting it to account for the current scroll position and content insets, then restores its original transform.
    /// </summary>
    /// <param name="element">The content element to update.</param>
    /// <param name="delta">The elapsed time in seconds since the previous update.</param>
    /// <param name="interactionHandled">A reference flag indicating whether an interaction has already been consumed this tick.</param>
    private void UpdateContentElement(GuiElement element, double delta, ref bool interactionHandled) {
        Anchor originalAnchor = element.AnchorPoint;
        Vector2 originalOffset = element.Offset;
        Vector2 originalScale = element.Scale;
        float originalRotation = element.Rotation;
        bool originalInteractable = element.Interactable;
        Vector2 desiredTopLeftWorld = this.GetContentElementTopLeftWorld(this.GetContentLocalTopLeft(element));
        
        element.AnchorPoint = Anchor.TopLeft;
        element.Offset = (desiredTopLeftWorld - element.Origin) / this._contentTransformGuiScaleFactor;
        element.Scale = originalScale * this.Scale;
        element.Rotation = originalRotation + this.Rotation;
        element.Interactable = originalInteractable && this.Interactable;
        element.Update(delta, ref interactionHandled);
        
        element.AnchorPoint = originalAnchor;
        element.Offset = originalOffset;
        element.Scale = originalScale;
        element.Rotation = originalRotation;
        element.Interactable = originalInteractable;
        element.UpdatePosAndSize();
    }
    
    /// <summary>
    /// Gets the size of the content area, optionally reserving horizontal space for the scrollbar when content is scrollable.
    /// </summary>
    /// <param name="reserveScrollbar">Whether to subtract the slider bar width from the available width.</param>
    /// <returns>The content area size in unscaled units.</returns>
    private Vector2 GetContentAreaSize(bool reserveScrollbar = true) {
        float width = this.Size.X;
        
        if (reserveScrollbar && this.HasScrollableContent()) {
            width = MathF.Max(0.0F, width - this.Data.SliderBarWidth);
        }
        
        return new Vector2(width, this.Size.Y);
    }
    
    /// <summary>
    /// Gets the visible content size after subtracting the menu content insets from the content area.
    /// </summary>
    /// <param name="reserveScrollbar">Whether to reserve horizontal space for the scrollbar.</param>
    /// <returns>The visible content size in unscaled units.</returns>
    private Vector2 GetVisibleContentSize(bool reserveScrollbar = true) {
        Vector2 contentAreaSize = this.GetContentAreaSize(reserveScrollbar);
        
        float width = MathF.Max(0.0F, contentAreaSize.X - this.MenuContentInsets.Left - this.MenuContentInsets.Right);
        float height = MathF.Max(0.0F, contentAreaSize.Y - this.MenuContentInsets.Top - this.MenuContentInsets.Bottom);
        
        return new Vector2(width, height);
    }
    
    /// <summary>
    /// Gets the top-left content inset (left and top padding) as a vector.
    /// </summary>
    /// <returns>The top-left inset.</returns>
    private Vector2 GetContentInsetTopLeft() {
        return new Vector2(this.MenuContentInsets.Left, this.MenuContentInsets.Top);
    }
    
    /// <summary>
    /// Determines whether the content exceeds the visible area and can therefore be scrolled.
    /// </summary>
    /// <returns><c>true</c> if there is scrollable content; otherwise, <c>false</c>.</returns>
    private bool HasScrollableContent() {
        return this.GetScrollableHeight() > 0.0F;
    }
    
    /// <summary>
    /// Gets the current vertical scroll offset in pixels, derived from the scrollable height and the current scroll percentage.
    /// </summary>
    /// <returns>The scroll offset in pixels.</returns>
    private float GetScrollOffset() {
        return this.GetScrollableHeight() * this._scrollPercent;
    }
    
    /// <summary>
    /// Gets the total scrollable height: the content height (plus trailing spacing) minus the visible content height.
    /// </summary>
    /// <returns>The scrollable height, clamped to be non-negative.</returns>
    private float GetScrollableHeight() {
        return this._scrollableHeight;
    }
    
    /// <summary>
    /// Rebuilds content layout data used by scrolling, interaction, and draw ordering.
    /// </summary>
    private void RebuildContentLayout(bool rebuildDrawList = true) {
        this._contentLocalTopLefts.Clear();
        this._contentLocalBounds.Clear();
        
        if (rebuildDrawList) {
            this._contentDrawOrder.Clear();
            this._contentToDraw.Clear();
        }
        
        if (this._content.Count <= 0) {
            this._scrollableHeight = 0.0F;
            this._visibleContentWindow = new RectangleF(0.0F, 0.0F, 0.0F, 0.0F);
            return;
        }
        
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        Vector2 boundsPanelSize = this.GetVisibleContentSize(false);
        int order = 0;
        
        foreach ((string name, GuiElement element) in this._content) {
            if (rebuildDrawList) {
                this._contentDrawOrder[name] = order;
            }
            
            order++;
            
            Vector2 offset = this.GetContentOffset(element);
            Vector2 boundsTopLeft = this.GetAnchoredContentLocalTopLeft(element, element.AnchorPoint, offset, boundsPanelSize);
            
            minY = MathF.Min(minY, boundsTopLeft.Y);
            maxY = MathF.Max(maxY, boundsTopLeft.Y + element.Size.Y * element.Scale.Y);
        }
        
        float trailingContentSpacing = MathF.Max(0.0F, minY);
        this._scrollableHeight = MathF.Max(0.0F, maxY + trailingContentSpacing - this.GetVisibleContentSize(false).Y);
        
        Vector2 layoutPanelSize = this.GetVisibleContentSize();
        this._visibleContentWindow = new RectangleF(0.0F, this.GetScrollOffset(), layoutPanelSize.X, layoutPanelSize.Y);
        
        foreach (GuiElement element in this._content.Values) {
            Vector2 offset = this.GetContentOffset(element);
            Vector2 localTopLeft = this.GetAnchoredContentLocalTopLeft(element, element.AnchorPoint, offset, layoutPanelSize);
            Vector2 elementSize = element.Size * element.Scale;
            
            this._contentLocalTopLefts[element] = localTopLeft;
            this._contentLocalBounds[element] = new RectangleF(localTopLeft.X, localTopLeft.Y, elementSize.X, elementSize.Y);
            
            if (rebuildDrawList) {
                this._contentToDraw.Add(element);
            }
        }
        
        if (rebuildDrawList) {
            this._contentToDraw.Sort((a, b) => {
                int result = a.RenderOrder.CompareTo(b.RenderOrder);
                return result != 0 ? result : this._contentDrawOrder[a.Name].CompareTo(this._contentDrawOrder[b.Name]);
            });
        }
    }
    
    /// <summary>
    /// Gets the cached original offset of a content element, caching the element's current offset on first access.
    /// </summary>
    /// <param name="element">The content element.</param>
    /// <returns>The stored original offset.</returns>
    private Vector2 GetContentOffset(GuiElement element) {
        if (this._contentOffsets.TryGetValue(element, out Vector2 offset)) {
            return offset;
        }
        
        this._contentOffsets[element] = element.Offset;
        return element.Offset;
    }
    
    /// <summary>
    /// Gets the current layout position of a content element.
    /// </summary>
    /// <param name="element">The content element.</param>
    /// <returns>The element's local top-left position.</returns>
    private Vector2 GetContentLocalTopLeft(GuiElement element) {
        if (this._contentLocalTopLefts.TryGetValue(element, out Vector2 localTopLeft)) {
            return localTopLeft;
        }
        
        Vector2 offset = this.GetContentOffset(element);
        return this.GetAnchoredContentLocalTopLeft(element, element.AnchorPoint, offset, this.GetVisibleContentSize());
    }
    
    /// <summary>
    /// Checks whether an element's current local layout bounds overlap the visible content window.
    /// </summary>
    /// <param name="element">The content element.</param>
    /// <param name="needsWorldVisibilityCheck">Whether the caller should run a transformed world-space visibility check.</param>
    /// <returns><c>true</c> when the element may be visible.</returns>
    private bool IsContentElementVisibleInLayout(GuiElement element, out bool needsWorldVisibilityCheck) {
        if (element.Rotation != 0.0F) {
            needsWorldVisibilityCheck = true;
            return true;
        }
        
        if (!this._contentLocalBounds.TryGetValue(element, out RectangleF bounds)) {
            needsWorldVisibilityCheck = true;
            return true;
        }
        
        needsWorldVisibilityCheck = false;
        
        float minX = MathF.Min(bounds.X, bounds.X + bounds.Width);
        float maxX = MathF.Max(bounds.X, bounds.X + bounds.Width);
        float minY = MathF.Min(bounds.Y, bounds.Y + bounds.Height);
        float maxY = MathF.Max(bounds.Y, bounds.Y + bounds.Height);
        
        return minX < this._visibleContentWindow.X + this._visibleContentWindow.Width &&
               maxX > this._visibleContentWindow.X &&
               minY < this._visibleContentWindow.Y + this._visibleContentWindow.Height &&
               maxY > this._visibleContentWindow.Y;
    }
    
    /// <summary>
    /// Gets the top-left corner of the view in unscaled GUI coordinates.
    /// </summary>
    /// <returns>The top-left position of the view.</returns>
    private Vector2 GetViewTopLeftWorld() {
        return this.Position - this.Origin * this.Scale * this.Gui.ScaleFactor;
    }
    
    /// <summary>
    /// Rebuilds cached values used to transform local content positions into world positions.
    /// </summary>
    private void RebuildContentTransform() {
        this._contentTransformGuiScaleFactor = this.Gui.ScaleFactor;
        this._contentTransformScale = this.Scale * this._contentTransformGuiScaleFactor;
        this._contentTransformInsetTopLeft = this.GetContentInsetTopLeft() * this._contentTransformScale;
        this._contentTransformParentOrigin = this.Origin * this._contentTransformScale;
        this._contentTransformViewTopLeftWorld = this.GetViewTopLeftWorld();
        this._contentTransformRotation = Matrix3x2.CreateRotation(float.DegreesToRadians(this.Rotation));
        this._contentTransformScrollOffsetY = this.GetScrollOffset() * this._contentTransformScale.Y;
    }
    
    /// <summary>
    /// Calculates the local top-left position of an anchored GUI element relative to its parent container.
    /// </summary>
    /// <param name="element">The GUI element whose position is being calculated. Its size and scale are used in the calculation.</param>
    /// <param name="anchor">Specifies the anchor point that defines the alignment of the element within the parent container.</param>
    /// <param name="offset">A vector representing additional offset applied to the calculated position.</param>
    /// <param name="panelSize">The size of the parent container, used to determine the element's position based on the anchor settings.</param>
    /// <returns>The local top-left position of the GUI element, adjusted for the provided anchor, offset, and parent container size.</returns>
    private Vector2 GetAnchoredContentLocalTopLeft(GuiElement element, Anchor anchor, Vector2 offset, Vector2 panelSize) {
        Vector2 elementSize = element.Size * element.Scale;
        Vector2 anchoredPos = Vector2.Zero;
        
        switch (anchor) {
            case Anchor.TopLeft:
                break;
            
            case Anchor.TopCenter:
                anchoredPos.X = panelSize.X / 2.0F - elementSize.X / 2.0F;
                break;
            
            case Anchor.TopRight:
                anchoredPos.X = panelSize.X - elementSize.X;
                break;
            
            case Anchor.CenterLeft:
                anchoredPos.Y = panelSize.Y / 2.0F - elementSize.Y / 2.0F;
                break;
            
            case Anchor.Center:
                anchoredPos.X = panelSize.X / 2.0F - elementSize.X / 2.0F;
                anchoredPos.Y = panelSize.Y / 2.0F - elementSize.Y / 2.0F;
                break;
            
            case Anchor.CenterRight:
                anchoredPos.X = panelSize.X - elementSize.X;
                anchoredPos.Y = panelSize.Y / 2.0F - elementSize.Y / 2.0F;
                break;
            
            case Anchor.BottomLeft:
                anchoredPos.Y = panelSize.Y - elementSize.Y;
                break;
            
            case Anchor.BottomCenter:
                anchoredPos.X = panelSize.X / 2.0F - elementSize.X / 2.0F;
                anchoredPos.Y = panelSize.Y - elementSize.Y;
                break;
            
            case Anchor.BottomRight:
                anchoredPos.X = panelSize.X - elementSize.X;
                anchoredPos.Y = panelSize.Y - elementSize.Y;
                break;
        }
        
        return anchoredPos + offset;
    }
    
    /// <summary>
    /// Calculates the top-left corner position of a content element in world coordinates, taking into account the local position,
    /// scaling, rotation, and any scrolling or insets applied to the parent scroll view.
    /// </summary>
    /// <param name="localTopLeft">The local top-left position of the content element relative to the scroll view's internal coordinate system.</param>
    /// <returns>The calculated top-left position of the content element in world coordinates.</returns>
    private Vector2 GetContentElementTopLeftWorld(Vector2 localTopLeft) {
        Vector2 localPoint = this._contentTransformInsetTopLeft + localTopLeft * this._contentTransformScale - new Vector2(0.0F, this._contentTransformScrollOffsetY);
        Vector2 pointRelativeToOrigin = localPoint - this._contentTransformParentOrigin;
        Vector2 rotatedPoint = Vector2.Transform(pointRelativeToOrigin, this._contentTransformRotation);
        
        return this._contentTransformViewTopLeftWorld + this._contentTransformParentOrigin + rotatedPoint;
    }
    
    /// <summary>
    /// Ensures the offscreen render target and result texture exist and match the current window size, recreating or resizing them if needed.
    /// </summary>
    /// <param name="forceResize">When <c>true</c>, forces the targets to be resized even if the window size is unchanged.</param>
    private void EnsureContentRenderTarget(bool forceResize = false) {
        uint width = (uint) Math.Max(1, GlobalGraphicsAssets.Window.GetWidth());
        uint height = (uint) Math.Max(1, GlobalGraphicsAssets.Window.GetHeight());
        
        if (this._contentRenderTarget == null || this._contentResult == null) {
            this._contentRenderTarget = new RenderTexture2D(GlobalGraphicsAssets.GraphicsDevice, width, height);
            this._contentResult = new Texture2D(GlobalGraphicsAssets.GraphicsDevice, new Image((int) width, (int) height, new Color(0, 0, 0, 0)), false);
            return;
        }
        
        if (forceResize || this._contentRenderTarget.Width != width || this._contentRenderTarget.Height != height) {
            this._contentRenderTarget.Resize(width, height);
            this._contentResult.Dispose();
            this._contentResult = new Texture2D(GlobalGraphicsAssets.GraphicsDevice, new Image((int) width, (int) height, new Color(0, 0, 0, 0)), false);
        }
    }
    
    /// <summary>
    /// Determines whether a transformed content element overlaps the visible content area of this scroll view.
    /// </summary>
    /// <param name="element">The already transformed content element to test.</param>
    /// <returns><c>true</c> if the content element is at least partially visible inside the scroll view; otherwise, <c>false</c>.</returns>
    private bool IsContentElementVisible(GuiElement element) {
        if (element.ScaledSize.X <= 0.0F || element.ScaledSize.Y <= 0.0F) {
            return false;
        }
        
        Vector2 scale = this.Scale * this.Gui.ScaleFactor;
        Vector2 visibleContentSize = this.GetVisibleContentSize() * scale;
        Vector2 contentInsetTopLeft = this.GetContentInsetTopLeft() * scale;
        Vector2 viewTopLeft = this.GetViewTopLeftWorld();
        Vector2 contentTopLeft = viewTopLeft + contentInsetTopLeft;
        
        RectangleF contentBounds = new RectangleF(contentTopLeft.X, contentTopLeft.Y, visibleContentSize.X, visibleContentSize.Y);
        
        Vector2 scaledOrigin = element.Origin * element.Scale * this.Gui.ScaleFactor;
        
        Vector2 corner1 = new Vector2(element.Position.X, element.Position.Y) - scaledOrigin;
        Vector2 corner2 = new Vector2(element.Position.X + element.ScaledSize.X, element.Position.Y) - scaledOrigin;
        Vector2 corner3 = new Vector2(element.Position.X, element.Position.Y + element.ScaledSize.Y) - scaledOrigin;
        Vector2 corner4 = new Vector2(element.Position.X + element.ScaledSize.X, element.Position.Y + element.ScaledSize.Y) - scaledOrigin;
        
        Matrix3x2 transform = Matrix3x2.CreateTranslation(-element.Position) *
                              Matrix3x2.CreateRotation(float.DegreesToRadians(element.Rotation)) *
                              Matrix3x2.CreateTranslation(element.Position);
        
        corner1 = Vector2.Transform(corner1, transform);
        corner2 = Vector2.Transform(corner2, transform);
        corner3 = Vector2.Transform(corner3, transform);
        corner4 = Vector2.Transform(corner4, transform);
        
        if (contentBounds.Contains(corner1) ||
            contentBounds.Contains(corner2) ||
            contentBounds.Contains(corner3) ||
            contentBounds.Contains(corner4)) {
            return true;
        }
        
        float minX = MathF.Min(MathF.Min(corner1.X, corner2.X), MathF.Min(corner3.X, corner4.X));
        float minY = MathF.Min(MathF.Min(corner1.Y, corner2.Y), MathF.Min(corner3.Y, corner4.Y));
        float maxX = MathF.Max(MathF.Max(corner1.X, corner2.X), MathF.Max(corner3.X, corner4.X));
        float maxY = MathF.Max(MathF.Max(corner1.Y, corner2.Y), MathF.Max(corner3.Y, corner4.Y));
        
        RectangleF elementBounds = new RectangleF(minX, minY, maxX - minX, maxY - minY);
        
        return elementBounds.X < contentBounds.X + contentBounds.Width &&
               elementBounds.X + elementBounds.Width > contentBounds.X &&
               elementBounds.Y < contentBounds.Y + contentBounds.Height &&
               elementBounds.Y + elementBounds.Height > contentBounds.Y;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            HashSet<GuiElement> disposedElements = new HashSet<GuiElement>();
            
            foreach (GuiElement element in this._content.Values) {
                if (disposedElements.Add(element)) {
                    element.Dispose();
                }
            }
            
            foreach (GuiElement element in this._contentToAdd) {
                if (disposedElements.Add(element)) {
                    element.Dispose();
                }
            }
            
            this._content.Clear();
            this._contentToDraw.Clear();
            this._contentDrawOrder.Clear();
            this._contentToAdd.Clear();
            this._contentToRemove.Clear();
            this._contentOffsets.Clear();
            this._contentLocalTopLefts.Clear();
            this._contentLocalBounds.Clear();
            
            this._contentRenderTarget?.Dispose();
            this._contentResult?.Dispose();
        }
    }
}
