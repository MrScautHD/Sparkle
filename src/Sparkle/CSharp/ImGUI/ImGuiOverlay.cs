using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Transformations;
using Bliss.ImGUI.CSharp;
using Hexa.NET.ImGui;

namespace Sparkle.CSharp.ImGUI;

public abstract class ImGuiOverlay : Disposable {
    
    /// <summary>
    /// The default reference resolution used as the baseline for ImGui scaling.
    /// </summary>
    protected static readonly Vector2 DefaultScaleReferenceResolution = new Vector2(1280.0F, 720.0F);
    
    /// <summary>
    /// The unique name used to register, look up and remove this overlay.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Whether this overlay is updated and drawn. Toggle this to show or hide the overlay.
    /// </summary>
    public bool Enabled;
    
    /// <summary>
    /// The most recently applied ImGui scale for this overlay.
    /// </summary>
    protected float GuiScale { get; private set; } = 1.0F;
    
    /// <summary>
    /// The cached scale states for ImGui styles, keyed by their native style handle.
    /// </summary>
    private static readonly Dictionary<nint, ImGuiScaleState> ScaleStates = new Dictionary<nint, ImGuiScaleState>();
    
    /// <summary>
    /// Stores tracked ImGui window placements for this overlay.
    /// </summary>
    private readonly Dictionary<string, ImGuiWindowPlacementState> _windowPlacementStates = new Dictionary<string, ImGuiWindowPlacementState>();
    
    /// <summary>
    /// Initializes a new <see cref="ImGuiOverlay"/>.
    /// </summary>
    /// <param name="name">The unique name of the overlay.</param>
    /// <param name="enabled">Whether the overlay starts enabled. Defaults to <see langword="false"/>.</param>
    protected ImGuiOverlay(string name, bool enabled = false) {
        this.Name = name;
        this.Enabled = enabled;
    }
    
    /// <summary>
    /// Updates the overlay each frame.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected internal virtual void Update(double delta) { }
    
    /// <summary>
    /// Executes logic after the update step.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected internal virtual void AfterUpdate(double delta) { }
    
    /// <summary>
    /// Executes fixed-step updates for the overlay.
    /// </summary>
    /// <param name="timeStep">The fixed time step interval for the update.</param>
    protected internal virtual void FixedUpdate(double timeStep) { }
    
    /// <summary>
    /// Emits the ImGui draw commands for this overlay. Called between <c>ImGui.NewFrame</c> and <c>ImGui.Render</c>.
    /// </summary>
    /// <param name="controller">The controller driving this overlay, for access to IO, style and texture bindings.</param>
    protected internal abstract void Draw(ImGuiController controller);
    
    /// <summary>
    /// Executes when the window is resized.
    /// </summary>
    /// <param name="rectangle">The rectangle specifying the window's updated size.</param>
    protected internal virtual void Resize(Rectangle rectangle) { }
    
    /// <summary>
    /// Sets the next ImGui window position and size from a resize-safe placement state.
    /// </summary>
    /// <param name="controller">The ImGui controller providing access to the host window.</param>
    /// <param name="windowName">The ImGui window name used as the placement key.</param>
    /// <param name="defaultRect">The default X, Y, Width and Height in reference-resolution pixels.</param>
    /// <param name="minSize">The unscaled minimum allowed window size.</param>
    /// <param name="maxSize">The unscaled maximum allowed window size.</param>
    /// <param name="condition">The ImGui condition used for the initial placement. Host-window resizes always force an update.</param>
    /// <param name="refResolution">The reference resolution used to convert <paramref name="defaultRect"/> into a relative rect.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="refResolution"/> contains non-positive values.</exception>
    protected void SetNextWindowPlacement(ImGuiController controller, string windowName, Vector4 defaultRect, Vector2 minSize, Vector2 maxSize, ImGuiCond condition = ImGuiCond.FirstUseEver, Vector2? refResolution = null) {
        Vector2 resolution = refResolution ?? DefaultScaleReferenceResolution;
        
        if (resolution.X <= 0 || resolution.Y <= 0) {
            throw new ArgumentOutOfRangeException(nameof(refResolution));
        }
        
        Vector2 hostWindowSize = new Vector2(controller.Window.GetWidth(), controller.Window.GetHeight());
        (Vector2 scaledMinSize, Vector2 scaledMaxSize) = this.ScaleWindowSizeConstraints(minSize, maxSize);
        
        this._windowPlacementStates.TryGetValue(windowName, out ImGuiWindowPlacementState placementState);
        
        if (!placementState.RelativeRectInitialized) {
            placementState.RelativeRect = new Vector4(
                defaultRect.X / resolution.X,
                defaultRect.Y / resolution.Y,
                defaultRect.Z / resolution.X,
                defaultRect.W / resolution.Y
            );
            
            placementState.RelativeRectInitialized = true;
        }
        
        placementState.HostWindowSize = hostWindowSize;
        placementState.MinSize = scaledMinSize;
        placementState.MaxSize = scaledMaxSize;
        
        ImGui.SetNextWindowSizeConstraints(scaledMinSize, scaledMaxSize);
        
        bool hostWindowResized = placementState.LastHostWindowSize != Vector2.Zero && Vector2.DistanceSquared(placementState.LastHostWindowSize, hostWindowSize) > 0.01F;
        bool shouldApplyPlacement = !placementState.WindowPlacementInitialized || hostWindowResized || condition == ImGuiCond.None || condition.HasFlag(ImGuiCond.Always) || condition.HasFlag(ImGuiCond.Appearing);
        
        if (shouldApplyPlacement) {
            (Vector2 windowPos, Vector2 windowSize) = this.ClampWindowRect(
                new Vector2(placementState.RelativeRect.X * hostWindowSize.X, placementState.RelativeRect.Y * hostWindowSize.Y),
                new Vector2(placementState.RelativeRect.Z * hostWindowSize.X, placementState.RelativeRect.W * hostWindowSize.Y),
                scaledMinSize,
                scaledMaxSize,
                hostWindowSize
            );
            
            ImGuiCond effectiveCondition = hostWindowResized ? ImGuiCond.Always : condition;
            
            ImGui.SetNextWindowPos(windowPos, effectiveCondition);
            ImGui.SetNextWindowSize(windowSize, effectiveCondition);
            placementState.WindowPlacementInitialized = true;
        }
        
        placementState.LastHostWindowSize = hostWindowSize;
        this._windowPlacementStates[windowName] = placementState;
    }
    
    /// <summary>
    /// Scales unscaled ImGui window size constraints by the current GUI scale.
    /// </summary>
    /// <param name="minSize">The unscaled minimum allowed window size.</param>
    /// <param name="maxSize">The unscaled maximum allowed window size.</param>
    /// <returns>The scaled minimum and maximum window sizes.</returns>
    protected (Vector2 MinSize, Vector2 MaxSize) ScaleWindowSizeConstraints(Vector2 minSize, Vector2 maxSize) {
        Vector2 scaledMinSize = minSize * this.GuiScale;
        Vector2 scaledMaxSize = maxSize * this.GuiScale;
        
        return (scaledMinSize, Vector2.Max(scaledMinSize, scaledMaxSize));
    }
    
    /// <summary>
    /// Stores the current ImGui window position and size as a relative placement for future host-window resizes.
    /// </summary>
    /// <param name="windowName">The ImGui window name used as the placement key.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="SetNextWindowPlacement"/> was not called for this window first.</exception>
    protected void UpdateWindowPlacement(string windowName) {
        if (!this._windowPlacementStates.TryGetValue(windowName, out ImGuiWindowPlacementState placementState)) {
            throw new InvalidOperationException($"The ImGui window placement for [{windowName}] was not initialized.");
        }
        
        (Vector2 windowPos, Vector2 windowSize) = this.ClampWindowRect(ImGui.GetWindowPos(), ImGui.GetWindowSize(), placementState.MinSize, placementState.MaxSize, placementState.HostWindowSize);
        
        Vector2 hostWindowSize = new Vector2(MathF.Max(1.0F, placementState.HostWindowSize.X), MathF.Max(1.0F, placementState.HostWindowSize.Y));
        
        placementState.RelativeRect = new Vector4(
            windowPos.X / hostWindowSize.X,
            windowPos.Y / hostWindowSize.Y,
            windowSize.X / hostWindowSize.X,
            windowSize.Y / hostWindowSize.Y
        );
        
        this._windowPlacementStates[windowName] = placementState;
    }
    
    /// <summary>
    /// Calculates and applies the ImGui style scale based on the current window size relative to the default reference resolution.
    /// </summary>
    /// <param name="controller">The ImGui controller providing access to the window and style settings.</param>
    /// <param name="scaleFactor">An optional factor to fine-tune the scaling, defaulting to 0.75.</param>
    /// <returns>The resulting scale factor that was applied to the ImGui style.</returns>
    protected internal float UpdateScale(ImGuiController controller, float scaleFactor = 0.75F) {
        return this.UpdateScale(controller, DefaultScaleReferenceResolution, scaleFactor);
    }
    
    /// <summary>
    /// Calculates and applies the ImGui style scale based on the current window size relative to a reference resolution.
    /// </summary>
    /// <param name="controller">The ImGui controller providing access to the window and style settings.</param>
    /// <param name="refResolution">The reference resolution used as the baseline for determining the scale.</param>
    /// <param name="scaleFactor">An optional factor to fine-tune the scaling, defaulting to 0.75.</param>
    /// <returns>The resulting scale factor that was applied to the ImGui style.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="refResolution"/> contains non-positive values.</exception>
    protected virtual unsafe float UpdateScale(ImGuiController controller, Vector2 refResolution, float scaleFactor = 0.75F) {
        if (refResolution.X <= 0 || refResolution.Y <= 0) {
            throw new ArgumentOutOfRangeException(nameof(refResolution));
        }
        
        float scale = Math.Clamp(Math.Min(controller.Window.GetWidth() / refResolution.X, controller.Window.GetHeight() / refResolution.Y) * scaleFactor, 0.25F, 4.0F);
        
        ImGuiStylePtr style = controller.Style;
        nint styleKey = (nint) style.Handle;
        ScaleStates.TryGetValue(styleKey, out ImGuiScaleState scaleState);
        
        if (Math.Abs(scale - scaleState.AppliedScale) < 0.01F) {
            this.GuiScale = scaleState.AppliedScale;
            return scaleState.AppliedScale;
        }
        
        scaleState.DefaultStyle ??= *style.Handle;
        *style.Handle = scaleState.DefaultStyle.Value;
        
        ImGui.ScaleAllSizes(style, scale);
        style.FontScaleDpi = scale;
        
        scaleState.AppliedScale = scale;
        ScaleStates[styleKey] = scaleState;
        this.GuiScale = scale;
        return scale;
    }
    
    /// <summary>
    /// Clamps a window rectangle so its size and position stay inside the host window.
    /// </summary>
    /// <param name="position">The requested window position.</param>
    /// <param name="size">The requested window size.</param>
    /// <param name="minSize">The minimum allowed window size.</param>
    /// <param name="maxSize">The maximum allowed window size.</param>
    /// <param name="hostSize">The host window size.</param>
    /// <returns>The clamped window position and size.</returns>
    protected (Vector2 Position, Vector2 Size) ClampWindowRect(Vector2 position, Vector2 size, Vector2 minSize, Vector2 maxSize, Vector2 hostSize) {
        size = Vector2.Clamp(size, minSize, maxSize);
        
        position = new Vector2(
            Math.Clamp(position.X, 0.0F, MathF.Max(0.0F, hostSize.X - size.X)),
            Math.Clamp(position.Y, 0.0F, MathF.Max(0.0F, hostSize.Y - size.Y))
        );
        
        return (position, size);
    }
    
    private struct ImGuiScaleState {
        
        /// <summary>
        /// A pristine copy of the unscaled ImGui style, captured once before scaling is applied.
        /// </summary>
        public ImGuiStyle? DefaultStyle;
        
        /// <summary>
        /// The scale factor currently applied to the ImGui style.
        /// </summary>
        public float AppliedScale;
    }
    
    private struct ImGuiWindowPlacementState {
        
        /// <summary>
        /// Stores the ImGui window rect relative to the host window as X, Y, Width and Height.
        /// </summary>
        public Vector4 RelativeRect;
        
        /// <summary>
        /// The last known size of the host application window.
        /// </summary>
        public Vector2 LastHostWindowSize;
        
        /// <summary>
        /// The current size of the host application window.
        /// </summary>
        public Vector2 HostWindowSize;
        
        /// <summary>
        /// The minimum allowed window size.
        /// </summary>
        public Vector2 MinSize;
        
        /// <summary>
        /// The maximum allowed window size.
        /// </summary>
        public Vector2 MaxSize;
        
        /// <summary>
        /// Indicates whether the relative window rect has been initialized.
        /// </summary>
        public bool RelativeRectInitialized;
        
        /// <summary>
        /// Indicates whether the ImGui window placement has been initialized.
        /// </summary>
        public bool WindowPlacementInitialized;
    }
}
