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
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrith;

namespace Sparkle.CSharp.GUI.Elements;

public class RectangleScrollViewElement : GuiElement {
    
    public RectangleScrollViewData Data { get; private set; }
    
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
        float rotation = 0,
        Func<GuiElement, bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, clickFunc) {
        this.Data = data;
        this.Size = size;
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
        
        // Create render target.
        this.EnsureContentRenderTarget();
    }
    
    protected internal override void Update(double delta, ref bool interactionHandled) {
        
        // Handle removing content.
        foreach (string name in this._contentToRemove) {
            if (this._content.Remove(name, out GuiElement? element)) {
                this._contentOffsets.Remove(element);
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
        
        // Update content elements.
        Vector2 contentInsetTopLeft = this.GetContentInsetTopLeft() * scale;
        Vector2 visibleContentSize = this.GetVisibleContentSize() * scale;
        RectangleF localContentRect = new RectangleF(contentInsetTopLeft.X, contentInsetTopLeft.Y, visibleContentSize.X, visibleContentSize.Y);
        bool contentInteractionHandled = interactionHandledBeforeBaseUpdate || !this.Interactable || !localContentRect.Contains(localMouse);
        
        foreach (GuiElement element in this._content.Values) {
            this.UpdateContentElement(element, delta, ref contentInteractionHandled);
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
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
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
        
        // Draw menu.
        this.DrawMenu(context.PrimitiveBatch, menuSize, this.Origin * this.Scale * this.Gui.ScaleFactor, menuColor, menuOutlineColor);
        
        // Draw slider bar.
        this.DrawSliderBar(context.PrimitiveBatch);
        
        // Draw slider.
        this.DrawSlider(context.PrimitiveBatch);
        
        context.PrimitiveBatch.End();
        
        // Draw content elements.
        this.DrawContent(context, framebuffer);
    }
    
    protected internal override void Resize(Rectangle rectangle) {
        base.Resize(rectangle);
        
        foreach (GuiElement element in this._content.Values) {
            element.Resize(rectangle);
        }
        
        this.EnsureContentRenderTarget(true);
    }
    
    public OrderedDictionary<string, GuiElement>.ValueCollection GetContent() {
        return this._content.Values;
    }
    
    public bool HasContent(string name) {
        return this._content.ContainsKey(name);
    }
    
    public GuiElement? GetContent(string name) {
        return this.TryGetContent(name, out GuiElement? element) ? element : null;
    }
    
    public bool TryGetContent(string name, [NotNullWhen(true)] out GuiElement? element) {
        return this._content.TryGetValue(name, out element);
    }
    
    public void AddContent(string name, GuiElement element) {
        if (!this.TryAddContent(name, element)) {
            throw new Exception($"The content element with the name [{name}] is already present in the scroll view or belongs to another GUI.");
        }
    }
    
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
    
    public void RemoveContent(GuiElement element) {
        if (!this.TryRemoveContent(element)) {
            throw new Exception($"Failed to remove content element [{element.Name}] from scroll view [{this.Name}].");
        }
    }
    
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
    
    public void RemoveContent(string name) {
        if (!this.TryRemoveContent(name)) {
            throw new Exception($"Failed to remove content element [{name}] from scroll view [{this.Name}].");
        }
    }
    
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
        primitiveBatch.DrawLine(bp1 - barVerticalNormal, bp3 - barVerticalNormal, scaledBarOutlineThickness, 0.5F, barOutlineColor);
        primitiveBatch.DrawLine(bp2 + barVerticalNormal, bp4 + barVerticalNormal, scaledBarOutlineThickness, 0.5F, barOutlineColor);
    }
    
    private void DrawSlider(PrimitiveBatch primitiveBatch) {
        if (!this.HasScrollableContent()) {
            return;
        }
        
        float sliderHeight = MathF.Min(this.Data.SliderSize.Y, this.Size.Y);
        float sliderRange = MathF.Max(0.0F, this.Size.Y - sliderHeight);
        
        float xOffset = this.Size.X - this.Data.SliderBarWidth + (this.Data.SliderBarWidth - this.Data.SliderSize.X) / 2.0F;
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
    
    private void DrawContent(GraphicsContext context, Framebuffer framebuffer) {
        if (this._content.Count <= 0) {
            return;
        }
        
        this.EnsureContentRenderTarget();
        
        if (this._contentRenderTarget == null || this._contentResult == null) {
            return;
        }
        
        context.CommandList.SetFramebuffer(this._contentRenderTarget.Framebuffer);
        context.CommandList.ClearColorTarget(0, new Color(0, 0, 0, 0).ToRgbaFloat());
        context.CommandList.ClearDepthStencil(1.0F);
        
        foreach (GuiElement element in this._content.Values) {
            this.DrawContentElement(context, this._contentRenderTarget.Framebuffer, element);
        }
        
        context.CommandList.CopyTexture(this._contentRenderTarget.ColorTexture, this._contentResult.DeviceTexture);
        context.CommandList.SetFramebuffer(framebuffer);
        
        this.DrawContentMask(context.CommandList, framebuffer, context.PrimitiveBatch);
        this.DrawContentResult(context.CommandList, framebuffer, context.SpriteBatch);
    }
    
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
    
    private void DrawContentElement(GraphicsContext context, Framebuffer framebuffer, GuiElement element) {
        Anchor originalAnchor = element.AnchorPoint;
        Vector2 originalOffset = element.Offset;
        Vector2 originalScale = element.Scale;
        float originalRotation = element.Rotation;
        bool originalInteractable = element.Interactable;
        Vector2 localOffset = this.GetContentOffset(element);
        Vector2 panelSize = this.GetVisibleContentSize();
        Vector2 anchoredLocalTopLeft = this.GetAnchoredContentLocalTopLeft(element, originalAnchor, localOffset, panelSize);
        Vector2 desiredTopLeftWorld = this.GetContentElementTopLeftWorld(anchoredLocalTopLeft);
        float guiScaleFactor = this.Gui.ScaleFactor;
        
        element.AnchorPoint = Anchor.TopLeft;
        element.Offset = (desiredTopLeftWorld - element.Origin) / guiScaleFactor;
        element.Scale = originalScale * this.Scale;
        element.Rotation = originalRotation + this.Rotation;
        element.Interactable = originalInteractable && this.Interactable;
        element.UpdatePosAndSize();
        element.Draw(context, framebuffer);
        
        element.AnchorPoint = originalAnchor;
        element.Offset = originalOffset;
        element.Scale = originalScale;
        element.Rotation = originalRotation;
        element.Interactable = originalInteractable;
        element.UpdatePosAndSize();
    }
    
    private void UpdateContentElement(GuiElement element, double delta, ref bool interactionHandled) {
        Anchor originalAnchor = element.AnchorPoint;
        Vector2 originalOffset = element.Offset;
        Vector2 originalScale = element.Scale;
        float originalRotation = element.Rotation;
        bool originalInteractable = element.Interactable;
        Vector2 localOffset = this.GetContentOffset(element);
        Vector2 panelSize = this.GetVisibleContentSize();
        Vector2 anchoredLocalTopLeft = this.GetAnchoredContentLocalTopLeft(element, originalAnchor, localOffset, panelSize);
        Vector2 desiredTopLeftWorld = this.GetContentElementTopLeftWorld(anchoredLocalTopLeft);
        float guiScaleFactor = this.Gui.ScaleFactor;
        
        element.AnchorPoint = Anchor.TopLeft;
        element.Offset = (desiredTopLeftWorld - element.Origin) / guiScaleFactor;
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
    
    private Vector2 GetContentAreaSize(bool reserveScrollbar = true) {
        float width = this.Size.X;
        
        if (reserveScrollbar && this.HasScrollableContent()) {
            width = MathF.Max(0.0F, width - this.Data.SliderBarWidth);
        }
        
        return new Vector2(width, this.Size.Y);
    }
    
    private Vector2 GetVisibleContentSize(bool reserveScrollbar = true) {
        Vector2 contentAreaSize = this.GetContentAreaSize(reserveScrollbar);
        float contentInset = this.GetContentInsetThickness();
        
        float width = MathF.Max(0.0F, contentAreaSize.X - contentInset * 2.0F);
        float height = MathF.Max(0.0F, contentAreaSize.Y - contentInset * 2.0F);
        
        return new Vector2(width, height);
    }
    
    private Vector2 GetContentInsetTopLeft() {
        float contentInset = this.GetContentInsetThickness();
        return new Vector2(contentInset, contentInset);
    }
    
    private float GetContentInsetThickness() {
        return MathF.Max(0.0F, this.Data.MenuOutlineThickness * 0.5F);
    }
    
    private (float MinY, float MaxY) GetContentBoundsY() {
        if (this._content.Count <= 0) {
            return (0.0F, 0.0F);
        }
        
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        Vector2 panelSize = this.GetVisibleContentSize(false);
        
        foreach (GuiElement element in this._content.Values) {
            Vector2 offset = this.GetContentOffset(element);
            Vector2 anchoredLocalTopLeft = this.GetAnchoredContentLocalTopLeft(element, element.AnchorPoint, offset, panelSize);
            minY = MathF.Min(minY, anchoredLocalTopLeft.Y);
            maxY = MathF.Max(maxY, anchoredLocalTopLeft.Y + element.Size.Y * element.Scale.Y);
        }
        
        return (minY, maxY);
    }
    
    private float GetContentHeight() {
        return this.GetContentBoundsY().MaxY;
    }
    
    private bool HasScrollableContent() {
        return this.GetScrollableHeight() > 0.0F;
    }
    
    private float GetScrollOffset() {
        return this.GetScrollableHeight() * this._scrollPercent;
    }
    
    private float GetScrollableHeight() {
        return MathF.Max(0.0F, this.GetContentHeight() + this.GetTrailingContentSpacing() - this.GetVisibleContentSize(false).Y);
    }
    
    private float GetTrailingContentSpacing() {
        return MathF.Max(0.0F, this.GetContentBoundsY().MinY);
    }
    
    private Vector2 GetContentOffset(GuiElement element) {
        if (this._contentOffsets.TryGetValue(element, out Vector2 offset)) {
            return offset;
        }
        
        this._contentOffsets[element] = element.Offset;
        return element.Offset;
    }
    
    private Vector2 GetViewTopLeftWorld() {
        return this.Position - this.Origin * this.Scale * this.Gui.ScaleFactor;
    }
    
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
    
    private Vector2 GetContentElementTopLeftWorld(Vector2 localTopLeft) {
        Vector2 scale = this.Scale * this.Gui.ScaleFactor;
        Vector2 contentInsetTopLeft = this.GetContentInsetTopLeft() * scale;
        Vector2 localPoint = contentInsetTopLeft + localTopLeft * scale - new Vector2(0.0F, this.GetScrollOffset() * scale.Y);
        
        Vector2 parentOrigin = this.Origin * scale;
        Vector2 viewTopLeftWorld = this.GetViewTopLeftWorld();
        Vector2 pointRelativeToOrigin = localPoint - parentOrigin;
        Vector2 rotatedPoint = Vector2.Transform(pointRelativeToOrigin, Matrix3x2.CreateRotation(float.DegreesToRadians(this.Rotation)));
        
        return viewTopLeftWorld + parentOrigin + rotatedPoint;
    }
    
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
            this._contentToAdd.Clear();
            this._contentToRemove.Clear();
            
            this._contentRenderTarget?.Dispose();
            this._contentResult?.Dispose();
        }
    }
}
