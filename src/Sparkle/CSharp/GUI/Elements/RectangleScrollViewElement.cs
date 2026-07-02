using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Primitives;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Images;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Batching;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrith;

namespace Sparkle.CSharp.GUI.Elements;

public class RectangleScrollViewElement : GuiElement {
    
    /// <summary>
    /// The visual configuration used for the rectangle-based scroll view.
    /// </summary>
    public RectangleScrollViewData Data { get; private set; }
    
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
    /// Represents a rectangle-based scroll view element that can render and clip a list of child GUI elements.
    /// </summary>
    /// <param name="data">The visual data object containing colors, outline styles, and slider settings.</param>
    /// <param name="content">Optional initial content mapped by unique names.</param>
    /// <param name="anchor">Specifies the anchor position that determines the alignment of the scroll view within its parent element.</param>
    /// <param name="offset">Defines the offset position of the scroll view relative to its anchor.</param>
    /// <param name="size">The size of the scroll view in unscaled GUI units.</param>
    /// <param name="scrollSensitivity">Indicates the sensitivity of the scroll mechanics, influencing the movement per scroll input.</param>
    /// <param name="scrollLerpSpeed">Defines the speed of the scrolling animation, affecting how smoothly the content scrolls.</param>
    /// <param name="scale">Optional scaling factor applied to the scroll view.</param>
    /// <param name="origin">Optional origin point for transformations like rotation and scaling.</param>
    /// <param name="rotation">The rotation angle in degrees applied to the scroll view.</param>
    /// <param name="renderOrder">The order in which the element is rendered, relative to others.</param>
    /// <param name="clickFunc">Optional callback invoked when the element is clicked.</param>
    public RectangleScrollViewElement(
        RectangleScrollViewData data,
        IEnumerable<KeyValuePair<string, GuiElement>>? content,
        Anchor anchor,
        Vector2 offset,
        Vector2 size,
        float scrollSensitivity = 0.1F,
        float scrollLerpSpeed = 10.0F,
        Vector2? scale = null,
        Vector2? origin = null,
        float rotation = 0.0F,
        int renderOrder = 0,
        Func<GuiElement, bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, renderOrder, clickFunc) {
        this.Data = data;
        this.Size = size;
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
        
        if (this.Interactable && hasScrollableContent) {
            if (!this._isDraggingSlider && localViewRect.Contains(localMouse)) {
                if (Input.IsMouseScrolling(out Vector2 wheelDelta)) {
                    float scrollableHeight = this.GetScrollableHeight();
                    
                    if (scrollableHeight > 0.0F) {
                        float currentOffset = this._targetScrollPercent * scrollableHeight;
                        float wheelStep = MathF.Max(1.0F, this.GetVisibleContentSize(false).Y * this.ScrollSensitivity);
                        currentOffset = Math.Clamp(currentOffset - wheelDelta.Y * wheelStep, 0.0F, scrollableHeight);
                        this._targetScrollPercent = currentOffset / scrollableHeight;
                    }
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
                    float sliderHeight = this.Data.SliderSize.Y * scale.Y;
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
            this._scrollPercent = 0.0F;
            this._targetScrollPercent = 0.0F;
            this._isDraggingSlider = false;
        }
        
        this.RebuildContentTransform();
        
        // Update content elements.
        Vector2 contentInsetTopLeft = this.GetContentInsetTopLeft() * scale;
        Vector2 visibleContentSize = this.GetVisibleContentSize() * scale;
        RectangleF localContentRect = new RectangleF(contentInsetTopLeft.X, contentInsetTopLeft.Y, visibleContentSize.X, visibleContentSize.Y);
        bool contentInteractionHandled = interactionHandledBeforeBaseUpdate || !this.Interactable || !localContentRect.Contains(localMouse);
        
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
        
        Vector2 menuSize = this.Size;
        
        // Draw the menu smaller, leaving space on the right side for the slider bar.
        if (this.HasScrollableContent()) {
            menuSize = new Vector2(MathF.Max(0.0F, this.Size.X - this.Data.SliderBarWidth), this.Size.Y);
        }
        
        Color menuColor = this.IsHovered ? this.Data.MenuHoverColor : this.Data.MenuColor;
        
        if (!this.Interactable) {
            menuColor = this.Data.DisabledMenuColor;
        }
        
        Color menuOutlineColor = this.IsHovered ? this.Data.MenuOutlineHoverColor : this.Data.MenuOutlineColor;
        
        if (!this.Interactable) {
            menuOutlineColor = this.Data.DisabledMenuOutlineColor;
        }
        
        PrimitiveGuiRenderState primitiveState = new PrimitiveGuiRenderState(this.Data.Effect, this.Data.BlendState);
        
        // Draw menu.
        this.DrawMenu(renderQueue.UsePrimitive(primitiveState), menuSize, this.Origin * this.Scale * this.Gui.ScaleFactor, menuColor, menuOutlineColor);
        
        // Draw slider bar.
        this.DrawSliderBar(renderQueue.UsePrimitive());
        
        // Draw slider.
        this.DrawSlider(renderQueue.UsePrimitive());
        
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
    /// <param name="element">The content element to remove.</param>
    /// <exception cref="Exception">Thrown when the specified content element cannot be removed from the scroll view.</exception>
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
    /// <exception cref="Exception">Thrown when the specified content element cannot be removed from the scroll view.</exception>
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
    /// Draws the menu rectangle and its outline in a single pass.
    /// </summary>
    /// <param name="primitiveBatch">The primitive batch used to render the menu.</param>
    /// <param name="size">The unscaled menu size.</param>
    /// <param name="origin">The transformed origin used for drawing.</param>
    /// <param name="color">The fill color of the menu.</param>
    /// <param name="outlineColor">The outline color of the menu.</param>
    private void DrawMenu(PrimitiveBatch primitiveBatch, Vector2 size, Vector2 origin, Color color, Color outlineColor) {
        Vector2 scaledSize = size * this.Scale * this.Gui.ScaleFactor;
        primitiveBatch.DrawFilledRectangle(new RectangleF(this.Position.X, this.Position.Y, scaledSize.X, scaledSize.Y), origin, this.Rotation, 0.5F, color);
        
        if (this.Data.MenuOutlineThickness <= 0.0F) {
            return;
        }
        
        float scaledThickness = this.Data.MenuOutlineThickness * this.Gui.ScaleFactor;
        Matrix3x2 rotationMatrix = Matrix3x2.CreateRotation(float.DegreesToRadians(this.Rotation), this.Position);
        Vector2 p1 = Vector2.Transform(this.Position - origin, rotationMatrix);
        Vector2 p2 = Vector2.Transform(new Vector2(this.Position.X + scaledSize.X, this.Position.Y) - origin, rotationMatrix);
        Vector2 p3 = Vector2.Transform(new Vector2(this.Position.X, this.Position.Y + scaledSize.Y) - origin, rotationMatrix);
        Vector2 p4 = Vector2.Transform(new Vector2(this.Position.X + scaledSize.X, this.Position.Y + scaledSize.Y) - origin, rotationMatrix);
        
        Vector2 horizontalNormal = Vector2.Normalize(new Vector2(-(p2 - p1).Y, (p2 - p1).X)) * (scaledThickness / 2.0F);
        Vector2 verticalNormal = Vector2.Normalize(new Vector2(-(p3 - p1).Y, (p3 - p1).X)) * (scaledThickness / 2.0F);
        
        primitiveBatch.DrawLine(p1 + horizontalNormal, p2 + horizontalNormal, scaledThickness, 0.5F, outlineColor);
        primitiveBatch.DrawLine(p3 - horizontalNormal, p4 - horizontalNormal, scaledThickness, 0.5F, outlineColor);
        primitiveBatch.DrawLine(p1 - verticalNormal, p3 - verticalNormal, scaledThickness, 0.5F, outlineColor);
        primitiveBatch.DrawLine(p2 + verticalNormal, p4 + verticalNormal, scaledThickness, 0.5F, outlineColor);
    }
    
    /// <summary>
    /// Draws the slider bar track on the right side of the scroll view.
    /// </summary>
    /// <param name="primitiveBatch">The primitive batch used to render the slider bar.</param>
    private void DrawSliderBar(PrimitiveBatch primitiveBatch) {
        if (!this.HasScrollableContent()) {
            return;
        }
        
        Color barColor = this.IsHovered ? this.Data.SliderBarHoverColor : this.Data.SliderBarColor;
        
        if (!this.Interactable) {
            barColor = this.Data.DisabledSliderBarColor;
        }
        
        Vector2 barSize = new Vector2(this.Data.SliderBarWidth, this.Size.Y);
        Vector2 barOrigin = (this.Origin - new Vector2(this.Size.X - this.Data.SliderBarWidth, 0.0F)) * this.Scale * this.Gui.ScaleFactor;
        
        Vector2 scaledBarSize = barSize * this.Scale * this.Gui.ScaleFactor;
        primitiveBatch.DrawFilledRectangle(new RectangleF(this.Position.X, this.Position.Y, scaledBarSize.X, scaledBarSize.Y), barOrigin, this.Rotation, 0.5F, barColor);
        
        Color barOutlineColor = this.IsHovered ? this.Data.SliderBarOutlineHoverColor : this.Data.SliderBarOutlineColor;
        
        if (!this.Interactable) {
            barOutlineColor = this.Data.DisabledSliderBarOutlineColor;
        }
        
        if (this.Data.SliderBarOutlineThickness <= 0.0F) {
            return;
        }
        
        float scaledBarOutlineThickness = this.Data.SliderBarOutlineThickness * this.Gui.ScaleFactor;
        Matrix3x2 barRotationMatrix = Matrix3x2.CreateRotation(float.DegreesToRadians(this.Rotation), this.Position);
        
        Vector2 bp1 = Vector2.Transform(this.Position - barOrigin, barRotationMatrix);
        Vector2 bp2 = Vector2.Transform(new Vector2(this.Position.X + scaledBarSize.X, this.Position.Y) - barOrigin, barRotationMatrix);
        Vector2 bp3 = Vector2.Transform(new Vector2(this.Position.X, this.Position.Y + scaledBarSize.Y) - barOrigin, barRotationMatrix);
        Vector2 bp4 = Vector2.Transform(new Vector2(this.Position.X + scaledBarSize.X, this.Position.Y + scaledBarSize.Y) - barOrigin, barRotationMatrix);
        
        Vector2 barHorizontalNormal = Vector2.Normalize(new Vector2(-(bp2 - bp1).Y, (bp2 - bp1).X)) * (scaledBarOutlineThickness / 2.0F);
        Vector2 barVerticalNormal = Vector2.Normalize(new Vector2(-(bp3 - bp1).Y, (bp3 - bp1).X)) * (scaledBarOutlineThickness / 2.0F);
        
        primitiveBatch.DrawLine(bp1 + barHorizontalNormal, bp2 + barHorizontalNormal, scaledBarOutlineThickness, 0.5F, barOutlineColor);
        primitiveBatch.DrawLine(bp3 - barHorizontalNormal, bp4 - barHorizontalNormal, scaledBarOutlineThickness, 0.5F, barOutlineColor);
        primitiveBatch.DrawLine(bp2 + barVerticalNormal, bp4 + barVerticalNormal, scaledBarOutlineThickness, 0.5F, barOutlineColor);
    }
    
    /// <summary>
    /// Draws the slider handle inside the slider bar.
    /// </summary>
    /// <param name="primitiveBatch">The primitive batch used to render the slider.</param>
    private void DrawSlider(PrimitiveBatch primitiveBatch) {
        if (!this.HasScrollableContent()) {
            return;
        }
        
        float trackHeight = this.Size.Y - this.Data.MenuOutlineThickness / 2.0F;
        float sliderHeight = MathF.Min(this.Data.SliderSize.Y, trackHeight);
        float sliderRange = MathF.Max(0.0F, trackHeight - sliderHeight);
        
        float trackLeft = this.Size.X - this.Data.SliderBarWidth;
        float visibleTrackWidth = MathF.Max(0.0F, this.Data.SliderBarWidth - this.Data.MenuOutlineThickness);
        float xOffset = trackLeft + (visibleTrackWidth - this.Data.SliderSize.X) / 2.0F;
        float yOffset = sliderRange * this._scrollPercent;
        
        Vector2 sliderSize = new Vector2(this.Data.SliderSize.X, sliderHeight);
        Vector2 sliderOrigin = (this.Origin - new Vector2(xOffset, yOffset)) * this.Scale * this.Gui.ScaleFactor;
        
        Color sliderColor = this.IsHovered ? this.Data.SliderHoverColor : this.Data.SliderColor;
        
        if (!this.Interactable) {
            sliderColor = this.Data.DisabledSliderColor;
        }
        
        Vector2 scaledSliderSize = sliderSize * this.Scale * this.Gui.ScaleFactor;
        primitiveBatch.DrawFilledRectangle(new RectangleF(this.Position.X, this.Position.Y, scaledSliderSize.X, scaledSliderSize.Y), sliderOrigin, this.Rotation, 0.5F, sliderColor);
        
        Color sliderOutlineColor = this.IsHovered ? this.Data.SliderOutlineHoverColor : this.Data.SliderOutlineColor;
        
        if (!this.Interactable) {
            sliderOutlineColor = this.Data.DisabledSliderOutlineColor;
        }
        
        if (this.Data.SliderOutlineThickness <= 0.0F) {
            return;
        }
        
        float scaledSliderOutlineThickness = this.Data.SliderOutlineThickness * this.Gui.ScaleFactor;
        Matrix3x2 sliderRotationMatrix = Matrix3x2.CreateRotation(float.DegreesToRadians(this.Rotation), this.Position);
        
        Vector2 sp1 = Vector2.Transform(this.Position - sliderOrigin, sliderRotationMatrix);
        Vector2 sp2 = Vector2.Transform(new Vector2(this.Position.X + scaledSliderSize.X, this.Position.Y) - sliderOrigin, sliderRotationMatrix);
        Vector2 sp3 = Vector2.Transform(new Vector2(this.Position.X, this.Position.Y + scaledSliderSize.Y) - sliderOrigin, sliderRotationMatrix);
        Vector2 sp4 = Vector2.Transform(new Vector2(this.Position.X + scaledSliderSize.X, this.Position.Y + scaledSliderSize.Y) - sliderOrigin, sliderRotationMatrix);
        
        Vector2 sliderHorizontalNormal = Vector2.Normalize(new Vector2(-(sp2 - sp1).Y, (sp2 - sp1).X)) * (scaledSliderOutlineThickness / 2.0F);
        Vector2 sliderVerticalNormal = Vector2.Normalize(new Vector2(-(sp3 - sp1).Y, (sp3 - sp1).X)) * (scaledSliderOutlineThickness / 2.0F);
        
        primitiveBatch.DrawLine(sp1 + sliderHorizontalNormal, sp2 + sliderHorizontalNormal, scaledSliderOutlineThickness, 0.5F, sliderOutlineColor);
        primitiveBatch.DrawLine(sp3 - sliderHorizontalNormal, sp4 - sliderHorizontalNormal, scaledSliderOutlineThickness, 0.5F, sliderOutlineColor);
        primitiveBatch.DrawLine(sp1 - sliderVerticalNormal, sp3 - sliderVerticalNormal, scaledSliderOutlineThickness, 0.5F, sliderOutlineColor);
        primitiveBatch.DrawLine(sp2 + sliderVerticalNormal, sp4 + sliderVerticalNormal, scaledSliderOutlineThickness, 0.5F, sliderOutlineColor);
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
    /// Updates a single content element, temporarily reanchoring and offsetting it to account for the current scroll position, then restores its original transform.
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
    /// Gets the visible content size after subtracting inset derived from the menu outline thickness.
    /// </summary>
    /// <param name="reserveScrollbar">Whether to reserve horizontal space for the scrollbar.</param>
    /// <returns>The visible content size in unscaled units.</returns>
    private Vector2 GetVisibleContentSize(bool reserveScrollbar = true) {
        Vector2 contentAreaSize = this.GetContentAreaSize(reserveScrollbar);
        float contentInset = this.GetContentInsetThickness();
        
        float width = MathF.Max(0.0F, contentAreaSize.X - contentInset * 2.0F);
        float height = MathF.Max(0.0F, contentAreaSize.Y - contentInset * 2.0F);
        
        return new Vector2(width, height);
    }
    
    /// <summary>
    /// Gets the top-left content inset in unscaled units.
    /// </summary>
    /// <returns>The top-left inset vector.</returns>
    private Vector2 GetContentInsetTopLeft() {
        float contentInset = this.GetContentInsetThickness();
        return new Vector2(contentInset, contentInset);
    }
    
    /// <summary>
    /// Gets the effective content inset thickness derived from the menu outline thickness.
    /// </summary>
    /// <returns>The content inset thickness in unscaled units.</returns>
    private float GetContentInsetThickness() {
        return MathF.Max(0.0F, this.Data.MenuOutlineThickness);
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
    /// Calculates the local top-left position of an anchored GUI element relative to the scroll view content panel.
    /// </summary>
    /// <param name="element">The GUI element whose size and scale are used for anchor resolution.</param>
    /// <param name="anchor">The anchor point to resolve against the panel size.</param>
    /// <param name="offset">The local offset applied after anchor resolution.</param>
    /// <param name="panelSize">The available content panel size in unscaled units.</param>
    /// <returns>The local top-left position for the content element.</returns>
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
    /// Calculates the top-left corner position of a content element in world coordinates, taking insets, scroll offset, and parent transforms into account.
    /// </summary>
    /// <param name="localTopLeft">The local top-left position inside the content panel.</param>
    /// <returns>The top-left world position of the content element.</returns>
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
