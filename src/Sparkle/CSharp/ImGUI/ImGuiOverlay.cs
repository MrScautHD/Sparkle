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
        set => field = Math.Clamp(value, 0.25F, 4.0F);
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
    /// The current window size.
    /// </summary>
    private Vector2 _windowSize;
    
    /// <summary>
    /// The current scaled minimum size.
    /// </summary>
    private Vector2 _minSize;
    
    /// <summary>
    /// The current scaled maximum size.
    /// </summary>
    private Vector2 _maxSize;
    
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
        if (Math.Abs(scaleFactor - this._appliedScale) < 0.01F) {
            return this._appliedScale;
        }
        
        // Cache the unscaled style, then reset to it before rescaling.
        this._defaultStyle ??= *style.Handle;
        *style.Handle = this._defaultStyle.Value;
        
        // Apply the new scale.
        ImGui.ScaleAllSizes(style, scaleFactor);
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
        
        Vector2 windowSize = new Vector2(controller.Window.GetWidth(), controller.Window.GetHeight());
        Vector2 scaledMinSize = minSize * this._appliedScale;
        Vector2 scaledMaxSize = Vector2.Max(scaledMinSize, maxSize * this._appliedScale);
        
        // Store initial placement as a fraction of the window.
        if (!this._relativeRectInitialized) {
            this._relativeRect = new RectangleF(
                position.X / this.Size.Width,
                position.Y / this.Size.Height,
                size.X / this.Size.Width,
                size.Y / this.Size.Height
            );
            
            this._relativeRectInitialized = true;
        }
        
        this._windowSize = windowSize;
        this._minSize = scaledMinSize;
        this._maxSize = scaledMaxSize;
        
        ImGui.SetNextWindowSizeConstraints(scaledMinSize, scaledMaxSize);
        
        // Reapply placement if the window resized.
        bool windowResized = this._lastWindowSize != Vector2.Zero && Vector2.DistanceSquared(this._lastWindowSize, windowSize) > 0.01F;
        bool shouldApplyPlacement = !this._placementInitialized || windowResized || condition == ImGuiCond.None || condition.HasFlag(ImGuiCond.Always) || condition.HasFlag(ImGuiCond.Appearing);
        
        if (shouldApplyPlacement) {
            
            // Convert the relative rect back to absolute size/position.
            Vector2 targetSize = Vector2.Clamp(new Vector2(this._relativeRect.Width * windowSize.X, this._relativeRect.Height * windowSize.Y), scaledMinSize, scaledMaxSize);
            Vector2 targetPosition = new Vector2(Math.Clamp(this._relativeRect.X * windowSize.X, 0.0F, MathF.Max(0.0F, windowSize.X - targetSize.X)), Math.Clamp(this._relativeRect.Y * windowSize.Y, 0.0F, MathF.Max(0.0F, windowSize.Y - targetSize.Y)));
            
            // Always force it through on resize, even with a one-time condition.
            ImGuiCond effectiveCondition = windowResized ? ImGuiCond.Always : condition;
            
            ImGui.SetNextWindowPos(targetPosition, effectiveCondition);
            ImGui.SetNextWindowSize(targetSize, effectiveCondition);
            this._placementInitialized = true;
        }
        
        this._lastWindowSize = windowSize;
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
        
        // Clamp in case the user dragged/resized outside the window.
        Vector2 currentPosition = ImGui.GetWindowPos();
        Vector2 size = Vector2.Clamp(ImGui.GetWindowSize(), this._minSize, this._maxSize);
        Vector2 position = new Vector2(
            Math.Clamp(currentPosition.X, 0.0F, MathF.Max(0.0F, this._windowSize.X - size.X)),
            Math.Clamp(currentPosition.Y, 0.0F, MathF.Max(0.0F, this._windowSize.Y - size.Y))
        );
        
        // Avoid division by zero.
        Vector2 windowSize = new Vector2(MathF.Max(1.0F, this._windowSize.X), MathF.Max(1.0F, this._windowSize.Y));
        
        // Store as a fraction of the window for next time.
        this._relativeRect = new RectangleF(
            position.X / windowSize.X,
            position.Y / windowSize.Y,
            size.X / windowSize.X,
            size.Y / windowSize.Y
        );
    }
}