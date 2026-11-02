using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Transformations;
using Bliss.ImGUI.CSharp;
using Hexa.NET.ImGui;

namespace Sparkle.CSharp.ImGUI;

public abstract class ImGuiOverlay : Disposable {
    
    /// <summary>
    /// The name of the Overlay.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// The base size of this overlay's window was designed for.
    /// </summary>
    public (int Width, int Height) Size { get; private set; }
    
    /// <summary>
    /// The user-defined scale multiplier applied on top of the automatically calculated scale factor, clamped between 0.25 and 4.0.
    /// </summary>
    public float Scale {
        get;
        set {
            field = Math.Clamp(value, 0.25F, 4.0F);
            this._appliedScale = 0.0F;
        }
    }
    
    /// <summary>
    /// Whether this overlay is currently active and should be drawn/updated.
    /// </summary>
    public bool Enabled;
    
    /// <summary>
    /// Whether the scale handler is enabled for this overlay.
    /// </summary>
    public bool EnableScaleHandler { get; private set; }
    
    /// <summary>
    /// The cached unscaled ImGui style, captured before any scaling was applied, used to reset before rescaling.
    /// </summary>
    private ImGuiStyle? _defaultStyle;
    
    /// <summary>
    /// The style handle the cached scale state belongs to, used to detect when the ImGui context/style has changed.
    /// </summary>
    private nint _scaledStyleHandle;
    
    /// <summary>
    /// The scale factor most recently applied to the style.
    /// </summary>
    private float _appliedScale;
    
    /// <summary>
    /// This overlay's window position and size, stored as fractions (0-1) of the window, so placement stays proportional across resizes.
    /// </summary>
    private RectangleF _relativeRect;
    
    /// <summary>
    /// The window size from the previous call, used to detect when the window has been resized.
    /// </summary>
    private Vector2 _lastWindowSize;
    
    /// <summary>
    /// The UI scale factor from the previous placement update, used to detect when the scale has changed.
    /// </summary>
    private float _lastPlacementScale;
    
    /// <summary>
    /// The current window size.
    /// </summary>
    private Vector2 _windowSize;
    
    /// <summary>
    /// Whether <see cref="_relativeRect"/> has been set at least once.
    /// </summary>
    private bool _relativeRectInitialized;
    
    /// <summary>
    /// Whether the placement has been applied at least once.
    /// </summary>
    private bool _placementInitialized;
    
    /// <summary>
    /// Creates a new <see cref="ImGuiOverlay"/>.
    /// </summary>
    /// <param name="name">The display name of this overlay.</param>
    /// <param name="size">The base (unscaled) window size this overlay was designed for. Defaults to 1280x720.</param>
    /// <param name="scale">The initial user-defined scale multiplier.</param>
    /// <param name="enabled">Whether this overlay should start out active.</param>
    /// <param name="enableScaleHandler">Whether the scale handler is enabled, allowing use of <see cref="SetNextWindowScaledPlacement"/>, and <see cref="UpdateWindowScaledPlacement"/>.</param>
    protected ImGuiOverlay(string name, (int, int)? size = null, float scale = 1.0F, bool enabled = false, bool enableScaleHandler = true) {
        this.Name = name;
        this.Size = size ?? (1280, 720);
        this.Scale = scale;
        this.Enabled = enabled;
        this.EnableScaleHandler = enableScaleHandler;
    }
    
    /// <summary>
    /// Called every frame to update this overlay's state.
    /// </summary>
    /// <param name="delta">The time elapsed, in seconds, since the last update.</param>
    protected internal virtual void Update(double delta) { }
    
    /// <summary>
    /// Called every frame, after <see cref="Update"/> has run for all overlays.
    /// </summary>
    /// <param name="delta">The time elapsed, in seconds, since the last update.</param>
    protected internal virtual void AfterUpdate(double delta) { }
    
    /// <summary>
    /// Called on a fixed time step, independent of the frame rate.
    /// </summary>
    /// <param name="timeStep">The fixed time step, in seconds.</param>
    protected internal virtual void FixedUpdate(double timeStep) { }
    
    /// <summary>
    /// Draws this overlay's ImGui content.
    /// </summary>
    /// <param name="controller">The ImGui controller used to render the overlay.</param>
    /// <param name="scaleFactor">The current UI scale factor, as calculated by <see cref="HandleScale"/>.</param>
    protected internal abstract void Draw(ImGuiController controller, float scaleFactor);
    
    /// <summary>
    /// Called when the window is resized.
    /// </summary>
    /// <param name="rectangle">The new bounds of the window.</param>
    protected internal virtual void Resize(Rectangle rectangle) { }
    
    /// <summary>
    /// Calculates the UI scale factor based on window size and applies it to the ImGui style.
    /// </summary>
    /// <param name="controller">The ImGui controller providing the window and style.</param>
    /// <returns>The applied scale factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="EnableScaleHandler"/> is <c>false</c>.</exception>
    protected internal virtual unsafe float HandleScale(ImGuiController controller) {
        if (!this.EnableScaleHandler) {
            throw new InvalidOperationException($"The scale handler for [{this.Name}] is disabled. Enable {nameof(EnableScaleHandler)} before using this method.");
        }
        
        float scaleFactor = Math.Clamp(Math.Min((float) controller.Window.GetWidth() / (float) this.Size.Width, (float) controller.Window.GetHeight() / (float) this.Size.Height) * this.Scale, 0.25F, 4.0F);
        
        ImGuiStylePtr style = controller.Style;
        nint styleKey = (nint) style.Handle;
        
        // Reset cache if the style handle changed.
        if (this._scaledStyleHandle != styleKey) {
            this._scaledStyleHandle = styleKey;
            this._defaultStyle = null;
            this._appliedScale = 0.0F;
        }
        
        // Skip if scale hasn't meaningfully changed.
        if (Math.Abs(scaleFactor - this._appliedScale) < float.Epsilon) {
            return this._appliedScale;
        }
        
        // Cache the unscaled style, then reset to it before rescaling.
        this._defaultStyle ??= *style.Handle;
        
        ImGuiStyle originalStyle = this._defaultStyle.Value;
        
        *style.Handle = originalStyle;
        
        ScaleStyleSizes(style, originalStyle, scaleFactor);
        
        style.FontScaleDpi = scaleFactor;
        
        this._appliedScale = scaleFactor;
        return scaleFactor;
    }
    
    /// <summary>
    /// Sets the position and size for the next ImGui window, keeping it proportionally placed as the window resizes.
    /// </summary>
    /// <param name="controller">The ImGui controller providing the window's current size.</param>
    /// <param name="position">The initial position, used only on first init.</param>
    /// <param name="size">The initial size, used only on first init.</param>
    /// <param name="minSize">The minimum size, before scaling.</param>
    /// <param name="maxSize">The maximum size, before scaling.</param>
    /// <param name="condition">The condition under which the placement should normally be applied.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="EnableScaleHandler"/> is <c>false</c>.</exception>
    protected virtual void SetNextWindowScaledPlacement(ImGuiController controller, Vector2 position, Vector2 size, Vector2 minSize, Vector2 maxSize, ImGuiCond condition = ImGuiCond.FirstUseEver) {
        if (!this.EnableScaleHandler) {
            throw new InvalidOperationException($"The scale handler for [{this.Name}] is disabled. Enable {nameof(EnableScaleHandler)} before using this method.");
        }
        
        Vector2 windowSize = new(controller.Window.GetWidth(), controller.Window.GetHeight());
        Vector2 referenceSize = new(Math.Max(1, this.Size.Width), Math.Max(1, this.Size.Height));
        
        float scaleFactor = this._appliedScale;
        
        Vector2 scaledMinSize = minSize * scaleFactor;
        Vector2 scaledMaxSize = Vector2.Max(scaledMinSize, maxSize * scaleFactor);
        
        // Store the initial placement relative to the reference resolution.
        if (!this._relativeRectInitialized) {
            this._relativeRect = new RectangleF(
                position.X / referenceSize.X,
                position.Y / referenceSize.Y,
                size.X / referenceSize.X,
                size.Y / referenceSize.Y
            );
            
            this._relativeRectInitialized = true;
        }
        
        this._windowSize = windowSize;
        
        ImGui.SetNextWindowSizeConstraints(scaledMinSize, scaledMaxSize);
        
        // Detect application window resizing.
        bool windowResized = this._lastWindowSize != Vector2.Zero && Vector2.DistanceSquared(this._lastWindowSize, windowSize) > 0.01F;
        
        // Detect UI scaling changes independently of application resizing.
        bool scaleChanged = Math.Abs(this._lastPlacementScale - scaleFactor) > 0.001F;
        
        if (!this._placementInitialized || windowResized || scaleChanged || condition == ImGuiCond.None || condition.HasFlag(ImGuiCond.Always) || condition.HasFlag(ImGuiCond.Appearing)) {
            
            // Convert reference-resolution placement into scaled coordinates.
            Vector2 targetSize = new(this._relativeRect.Width * referenceSize.X * scaleFactor, this._relativeRect.Height * referenceSize.Y * scaleFactor);
            targetSize = Vector2.Clamp(targetSize, scaledMinSize, scaledMaxSize);
            
            Vector2 targetPosition = new(this._relativeRect.X * referenceSize.X * scaleFactor, this._relativeRect.Y * referenceSize.Y * scaleFactor);
            
            // Keep the window inside the application window.
            targetPosition.X = Math.Clamp(targetPosition.X, 0.0F, MathF.Max(0.0F, windowSize.X - targetSize.X));
            targetPosition.Y = Math.Clamp(targetPosition.Y, 0.0F, MathF.Max(0.0F, windowSize.Y - targetSize.Y));
            
            // Force placement when either the application size or UI scale changes.
            ImGuiCond effectiveCondition = windowResized || scaleChanged ? ImGuiCond.Always : condition;
            
            ImGui.SetNextWindowPos(targetPosition, effectiveCondition);
            ImGui.SetNextWindowSize(targetSize, effectiveCondition);
            this._placementInitialized = true;
        }
        
        this._lastWindowSize = windowSize;
        this._lastPlacementScale = scaleFactor;
    }
    
    /// <summary>
    /// Reads back the window's current position and size after rendering, and stores it as the new relative placement.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="EnableScaleHandler"/> is <c>false</c>, or if called before <see cref="SetNextWindowScaledPlacement"/> has initialized the overlay's placement.</exception>
    protected virtual void UpdateWindowScaledPlacement() {
        if (!this.EnableScaleHandler) {
            throw new InvalidOperationException($"The scale handler for [{this.Name}] is disabled. Enable {nameof(EnableScaleHandler)} before using this method.");
        }
        
        if (!this._relativeRectInitialized) {
            throw new InvalidOperationException($"The ImGui overlay placement for [{this.Name}] has not been initialized. Initialize the overlay's placement with {nameof(SetNextWindowScaledPlacement)} before using this method.");
        }
        
        // Only save placement while the user is interacting with this window.
        if (!ImGui.IsMouseDragging(ImGuiMouseButton.Left) ||
            !ImGui.IsWindowFocused()) {
            return;
        }
        
        Vector2 position = ImGui.GetWindowPos();
        Vector2 size = ImGui.GetWindowSize();
        
        // Clamp position to the application window.
        position.X = Math.Clamp(position.X, 0.0F, MathF.Max(0.0F, this._windowSize.X - size.X));
        position.Y = Math.Clamp(position.Y, 0.0F, MathF.Max(0.0F, this._windowSize.Y - size.Y));
        
        // Convert from current scaled coordinates to reference coordinates.
        float scaleFactor = MathF.Max(this._appliedScale, 0.0001F);
        
        Vector2 referenceSize = new(Math.Max(1, this.Size.Width), Math.Max(1, this.Size.Height));
        Vector2 referencePosition = position / scaleFactor;
        Vector2 referenceWindowSize = size / scaleFactor;
        
        // Store the placement relative to the reference resolution.
        this._relativeRect = new RectangleF(
            referencePosition.X / referenceSize.X,
            referencePosition.Y / referenceSize.Y,
            referenceWindowSize.X / referenceSize.X,
            referenceWindowSize.Y / referenceSize.Y
        );
    }
    
    /// <summary>
    /// Applies the specified scale factor to the ImGui style's dimensions and spacing values using the original style as the baseline, preserving fractional values.
    /// </summary>
    /// <param name="style">The ImGui style to modify.</param>
    /// <param name="original">The original, unscaled ImGui style.</param>
    /// <param name="scale">The scale factor to apply.</param>
    private static void ScaleStyleSizes(ImGuiStylePtr style, ImGuiStyle original, float scale) {
        style.WindowPadding = original.WindowPadding * scale;
        style.WindowRounding = original.WindowRounding * scale;
        style.WindowBorderSize = original.WindowBorderSize * scale;
        style.WindowMinSize = original.WindowMinSize * scale;
        style.WindowBorderHoverPadding = original.WindowBorderHoverPadding * scale;
        
        style.ChildRounding = original.ChildRounding * scale;
        style.ChildBorderSize = original.ChildBorderSize * scale;
        style.PopupRounding = original.PopupRounding * scale;
        style.PopupBorderSize = original.PopupBorderSize * scale;
        
        style.FramePadding = original.FramePadding * scale;
        style.FrameBorderSize = original.FrameBorderSize * scale;
        style.FrameRounding = original.FrameRounding * scale;
        
        style.ItemSpacing = original.ItemSpacing * scale;
        style.ItemInnerSpacing = original.ItemInnerSpacing * scale;
        style.CellPadding = original.CellPadding * scale;
        style.TouchExtraPadding = original.TouchExtraPadding * scale;
        
        style.IndentSpacing = original.IndentSpacing * scale;
        style.ColumnsMinSpacing = original.ColumnsMinSpacing * scale;
        
        style.ScrollbarSize = original.ScrollbarSize * scale;
        style.ScrollbarRounding = original.ScrollbarRounding * scale;
        style.ScrollbarPadding = original.ScrollbarPadding * scale;
        
        style.GrabMinSize = original.GrabMinSize * scale;
        style.GrabRounding = original.GrabRounding * scale;
        style.LogSliderDeadzone = original.LogSliderDeadzone * scale;
        
        style.ImageRounding = original.ImageRounding * scale;
        style.ImageBorderSize = original.ImageBorderSize * scale;
        
        style.TabRounding = original.TabRounding * scale;
        style.TabBorderSize = original.TabBorderSize * scale;
        style.TabMinWidthBase = original.TabMinWidthBase * scale;
        style.TabMinWidthShrink = original.TabMinWidthShrink * scale;
        
        style.TabCloseButtonMinWidthSelected = original.TabCloseButtonMinWidthSelected > 0.0F && Math.Abs(original.TabCloseButtonMinWidthSelected - float.MaxValue) > float.Epsilon ? original.TabCloseButtonMinWidthSelected * scale : original.TabCloseButtonMinWidthSelected;
        style.TabCloseButtonMinWidthUnselected = original.TabCloseButtonMinWidthUnselected > 0.0F && Math.Abs(original.TabCloseButtonMinWidthUnselected - float.MaxValue) > float.Epsilon ? original.TabCloseButtonMinWidthUnselected * scale : original.TabCloseButtonMinWidthUnselected;
        
        style.TabBarBorderSize = original.TabBarBorderSize * scale;
        style.TabBarOverlineSize = original.TabBarOverlineSize * scale;
        
        style.TreeLinesSize = original.TreeLinesSize * scale;
        style.TreeLinesRounding = original.TreeLinesRounding * scale;
        
        style.MenuItemRounding = original.MenuItemRounding * scale;
        style.SelectableRounding = original.SelectableRounding * scale;
        
        style.DragDropTargetRounding = original.DragDropTargetRounding * scale;
        style.DragDropTargetBorderSize = original.DragDropTargetBorderSize * scale;
        style.DragDropTargetPadding = original.DragDropTargetPadding * scale;
        
        style.ColorMarkerSize = original.ColorMarkerSize * scale;
        style.InputTextCursorSize = original.InputTextCursorSize * scale;
        
        style.SeparatorSize = original.SeparatorSize * scale;
        style.SeparatorTextBorderSize = original.SeparatorTextBorderSize * scale;
        style.SeparatorTextPadding = original.SeparatorTextPadding * scale;
        
        style.DisplayWindowPadding = original.DisplayWindowPadding * scale;
        style.DisplaySafeAreaPadding = original.DisplaySafeAreaPadding * scale;
        
        style.MouseCursorScale = original.MouseCursorScale * scale;
        
        style.MainScale = original.MainScale * scale;
        style.DockingSeparatorSize = original.DockingSeparatorSize * scale;
    }
}