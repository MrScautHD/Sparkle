using System.Numerics;
using System.Runtime.InteropServices;
using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Images;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Bliss.ImGUI.CSharp;
using Hexa.NET.ImGui;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI;
using Veldrith;

namespace Sparkle.CSharp.ImGUI;

public abstract class ImGuiOverlay : Disposable {
    
    /// <summary>
    /// The name of the overlay.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// The ImGui controller of this overlay, which owns the overlay's own ImGui context.
    /// </summary>
    public ImGuiController Controller { get; private set; }
    
    /// <summary>
    /// Determines whether the active camera may be rotated by input while this overlay is active.
    /// </summary>
    public bool AllowCameraRotation;
    
    /// <summary>
    /// Determines whether the active camera may be moved by input while this overlay is active.
    /// </summary>
    public bool AllowCameraMovement;
    
    /// <summary>
    /// The virtual design size this overlay targets.
    /// Used as the reference resolution for automatic scaling.
    /// </summary>
    public (int Width, int Height) Size { get; private set; }
    
    /// <summary>
    /// User-defined render scale multiplier.
    /// </summary>
    public float Scale {
        get;
        set => field = Math.Clamp(value, 0.25F, 4.0F);
    }
    
    /// <summary>
    /// Whether this overlay is active.
    /// </summary>
    public bool Enabled {
        get;
        set {
            field = value;
            this.StateChanged?.Invoke(this, value);
        }
    }

    /// <summary>
    /// Whether this overlay wanted to capture the mouse during its last frame.
    /// </summary>
    internal bool CapturesMouse;

    /// <summary>
    /// Whether a mouse button pressed on this overlay is still held (dragging a window, a slider, ...).
    /// </summary>
    internal bool OwnsMouse;
    
    /// <summary>
    /// Set by the manager. While true, this overlay ignores the mouse.
    /// </summary>
    internal bool MouseBlocked;
    
    /// <summary>
    /// Whether the RenderTexture scale handler is enabled.
    /// </summary>
    public bool EnableScaleHandler { get; private set; }
    
    /// <summary>
    /// If true, scale is rounded down to whole numbers where possible.
    /// </summary>
    public bool UseIntegerScaling;
    
    /// <summary>
    /// Final scale used by ImGui.
    /// </summary>
    public float CurrentScaleFactor { get; private set; }
    
    /// <summary>
    /// Destination rectangle of this overlay in real window pixels.
    /// This always covers the whole framebuffer.
    /// </summary>
    public RectangleF DestinationRectangle { get; private set; }
    
    /// <summary>
    /// Virtual ImGui display size after applying scale.
    /// </summary>
    public Vector2 VirtualDisplaySize { get; private set; }
    
    /// <summary>
    /// Event raised when the overlay's enabled state changes.
    /// </summary>
    public event Action<ImGuiOverlay, bool>? StateChanged;
    
    /// <summary>
    /// Render target containing the full-window ImGui overlay.
    /// </summary>
    private RenderTexture2D? _renderTarget;
    
    /// <summary>
    /// Texture copy of the render target, used for sprite drawing.
    /// </summary>
    private Texture2D? _resultTexture;
    
    /// <summary>
    /// Placement states for ImGui windows, keyed by their stable window id/name.
    /// </summary>
    private Dictionary<string, ImGuiWindowPlacementState> _windowPlacementStates;
    
    /// <summary>
    /// Creates a new ImGui overlay.
    /// </summary>
    /// <param name="name">The overlay name.</param>
    /// <param name="size">The virtual reference size. Defaults to 1280x720.</param>
    /// <param name="scale">User scale multiplier.</param>
    /// <param name="enabled">Whether the overlay starts enabled.</param>
    /// <param name="enableScaleHandler">Whether texture-based scaling is enabled.</param>
    /// <param name="useIntegerScaling">Whether to prefer integer scales for sharper output.</param>
    protected ImGuiOverlay(string name, (int, int)? size = null, float scale = 1.0F, bool enabled = false, bool enableScaleHandler = true, bool useIntegerScaling = false) {
        this.Name = name;
        this.Controller = new ImGuiController(GlobalGraphicsAssets.GraphicsDevice, GlobalGraphicsAssets.Window);
        this.AllowCameraRotation = true;
        this.AllowCameraMovement = true;
        this.Size = size ?? (1280, 720);
        this.Scale = scale;
        this.Enabled = enabled;
        this.EnableScaleHandler = enableScaleHandler;
        this.UseIntegerScaling = useIntegerScaling;
        this.CurrentScaleFactor = 1.0F;
        this.DestinationRectangle = new RectangleF(0.0F, 0.0F, this.Size.Width, this.Size.Height);
        this.VirtualDisplaySize = new Vector2(this.Size.Width, this.Size.Height);
        this._windowPlacementStates = new Dictionary<string, ImGuiWindowPlacementState>();
    }
    
    /// <summary>
    /// Called every frame to update this overlay.
    /// </summary>
    /// <param name="delta">Delta time in seconds.</param>
    protected internal virtual void Update(double delta) { }
    
    /// <summary>
    /// Called after update.
    /// </summary>
    /// <param name="delta">Delta time in seconds.</param>
    protected internal virtual void AfterUpdate(double delta) { }
    
    /// <summary>
    /// Called on fixed update.
    /// </summary>
    /// <param name="timeStep">Fixed timestep in seconds.</param>
    protected internal virtual void FixedUpdate(double timeStep) { }
    
    /// <summary>
    /// Draws this overlay in scaled virtual ImGui coordinates.
    /// </summary>
    /// <param name="controller">The ImGui controller.</param>
    protected abstract void Draw(ImGuiController controller);
    
    /// <summary>
    /// Called when the window is resized.
    /// </summary>
    /// <param name="rectangle">The new window rectangle.</param>
    protected internal virtual void Resize(Rectangle rectangle) { }
    
    /// <summary>
    /// Resolves the name of a window. Returns the given name if one is provided, otherwise the name of the window currently being built.
    /// </summary>
    /// <param name="name">An explicit window name, or <c>null</c> to use the currently active window.</param>
    /// <returns>The explicit name, or the name of the currently active window.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no name is given and no window is currently active, meaning the call happens outside of <c>ImGui.Begin</c> and <c>ImGui.End</c>.</exception>
    protected unsafe string GetCurrentWindowName(string? name = null) {
        string finalName;
        
        if (name != null) {
            finalName = name;
        }
        else {
            string? currentName = Marshal.PtrToStringUTF8((nint) ImGuiP.GetCurrentWindowRead().Name);
            
            if (string.IsNullOrEmpty(currentName)) {
                throw new InvalidOperationException($"{nameof(this.GetCurrentWindowName)} must be called between {nameof(ImGui.Begin)} and {nameof(ImGui.End)}, but no window is currently active.");
            }
            
            finalName = currentName;
        }
        
        return finalName;
    }
    
    /// <summary>
    /// Sets the position and size of the next window, placing it relative to an anchor point of the virtual display.
    /// The window's pivot is aligned with the anchor, so for example <see cref="Anchor.TopRight"/> keeps the window's top-right corner at the display's top-right corner.
    /// Must be called before <c>ImGui.Begin</c>.
    /// </summary>
    /// <param name="name">The name of the window the placement belongs to, which must match the name passed to <c>ImGui.Begin</c>.</param>
    /// <param name="size">The size of the window, applied according to <paramref name="sizeCondition"/>.</param>
    /// <param name="anchor">The anchor point of the virtual display the window is placed relative to.</param>
    /// <param name="offset">The initial offset from the anchor, used until the user moves the window or the placement is reset.</param>
    /// <param name="allowUserMove">Whether the user can drag the window away from its anchored position. If <c>false</c>, the window is forced back to the anchor plus offset every frame.</param>
    /// <param name="sizeCondition">The condition under which <paramref name="size"/> is applied.</param>
    protected void SetNextWindowAnchoredPlacement(string name, Vector2 size, Anchor anchor, Vector2 offset, bool allowUserMove = true, ImGuiCond sizeCondition = ImGuiCond.Once) {
        ImGuiWindowPlacementState state = this.GetWindowPlacementState(name);
        
        // Apply size before position so pivot-based placement is based on the expected window size.
        ImGui.SetNextWindowSize(size, sizeCondition);
        
        Vector2 anchorPosition;
        Vector2 pivot;
        
        switch (anchor) {
            case Anchor.TopLeft:
                anchorPosition = Vector2.Zero;
                pivot = Vector2.Zero;
                break;
            
            case Anchor.TopCenter:
                anchorPosition = new Vector2(this.VirtualDisplaySize.X * 0.5F, 0.0F);
                pivot = new Vector2(0.5F, 0.0F);
                break;
            
            case Anchor.TopRight:
                anchorPosition = new Vector2(this.VirtualDisplaySize.X, 0.0F);
                pivot = new Vector2(1.0F, 0.0F);
                break;
            
            case Anchor.CenterLeft:
                anchorPosition = new Vector2(0.0F, this.VirtualDisplaySize.Y * 0.5F);
                pivot = new Vector2(0.0F, 0.5F);
                break;
            
            case Anchor.Center:
                anchorPosition = this.VirtualDisplaySize * 0.5F;
                pivot = new Vector2(0.5F, 0.5F);
                break;
            
            case Anchor.CenterRight:
                anchorPosition = new Vector2(this.VirtualDisplaySize.X, this.VirtualDisplaySize.Y * 0.5F);
                pivot = new Vector2(1.0F, 0.5F);
                break;
            
            case Anchor.BottomLeft:
                anchorPosition = new Vector2(0.0F, this.VirtualDisplaySize.Y);
                pivot = new Vector2(0.0F, 1.0F);
                break;
            
            case Anchor.BottomCenter:
                anchorPosition = new Vector2(this.VirtualDisplaySize.X * 0.5F, this.VirtualDisplaySize.Y);
                pivot = new Vector2(0.5F, 1.0F);
                break;
            
            case Anchor.BottomRight:
                anchorPosition = this.VirtualDisplaySize;
                pivot = new Vector2(1.0F, 1.0F);
                break;
            
            default:
                anchorPosition = Vector2.Zero;
                pivot = Vector2.Zero;
                break;
        }
        
        state.CurrentAnchorPosition = anchorPosition;
        state.CurrentPivot = pivot;
        
        bool displaySizeChanged = state.HasLastVirtualDisplaySize && Vector2.DistanceSquared(state.LastVirtualDisplaySize, this.VirtualDisplaySize) > 0.001F;
        
        if (!state.HasUserOffset || !allowUserMove || state.ResetRequested) {
            state.UserOffset = offset;
            state.HasUserOffset = true;
            state.ResetRequested = false;
            state.WasMoved = false;
        }
        
        bool shouldApplyPlacement = !state.HasAppliedPlacement || !allowUserMove || displaySizeChanged;
        
        if (shouldApplyPlacement) {
            ImGui.SetNextWindowPos(anchorPosition + state.UserOffset, ImGuiCond.Always, pivot);
            state.HasAppliedPlacement = true;
        }
        
        state.LastVirtualDisplaySize = this.VirtualDisplaySize;
        state.HasLastVirtualDisplaySize = true;
    }
    
    /// <summary>
    /// Checks whether the user has moved a window away from its anchored position since the placement was set or last reset.
    /// </summary>
    /// <param name="name">The name of the window to check, or <c>null</c> to use the currently active window.</param>
    /// <returns><c>true</c> if the window was moved by the user, otherwise <c>false</c>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no name is given and no window is currently active.</exception>
    protected bool IsWindowPlacementMoved(string? name = null) {
        return this.GetWindowPlacementState(this.GetCurrentWindowName(name)).WasMoved;
    }
    
    /// <summary>
    /// Gets the placement state of a window, creating and storing a new one if none exists yet.
    /// </summary>
    /// <param name="name">The name of the window, or <c>null</c> to use the currently active window.</param>
    /// <returns>The placement state of the window.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no name is given and no window is currently active.</exception>
    private ImGuiWindowPlacementState GetWindowPlacementState(string? name = null) {
        string finalName = this.GetCurrentWindowName(name);
        
        if (!this._windowPlacementStates.TryGetValue(finalName, out ImGuiWindowPlacementState? state)) {
            state = new ImGuiWindowPlacementState();
            this._windowPlacementStates[finalName] = state;
        }
        
        return state;
    }
    
    /// <summary>
    /// Updates the placement state of a window. While the user drags the window, its position is stored as an offset from the current anchor, so it stays in place relative to the anchor when the display is resized.
    /// Must be called between <c>ImGui.Begin</c> and <c>ImGui.End</c>.
    /// </summary>
    /// <param name="name">The name of the window, or <c>null</c> to use the currently active window.</param>
    /// <exception cref="InvalidOperationException">Thrown if no name is given and no window is currently active.</exception>
    protected void UpdateWindowPlacementState(string? name = null) {
        ImGuiWindowPlacementState state = this.GetWindowPlacementState(this.GetCurrentWindowName(name));
        
        Vector2 windowPosition = ImGui.GetWindowPos();
        Vector2 windowSize = ImGui.GetWindowSize();
        
        if (!state.HasLastWindowState) {
            state.LastWindowPosition = windowPosition;
            state.LastWindowSize = windowSize;
            state.HasLastWindowState = true;
            return;
        }
        
        bool positionChanged = Vector2.DistanceSquared(state.LastWindowPosition, windowPosition) > 0.001F;
        
        if (positionChanged) {
            Vector2 pivotPosition = windowPosition + windowSize * state.CurrentPivot;
            
            state.UserOffset = pivotPosition - state.CurrentAnchorPosition;
            state.HasUserOffset = true;
            state.WasMoved = true;
        }
        
        state.LastWindowPosition = windowPosition;
        state.LastWindowSize = windowSize;
        state.HasLastWindowState = true;
    }
    
    /// <summary>
    /// Resets a window to its original anchored placement. The initial offset passed to <see cref="SetNextWindowAnchoredPlacement"/> is restored the next time it is called, and the window no longer counts as moved.
    /// </summary>
    /// <param name="name">The name of the window, or <c>null</c> to use the currently active window.</param>
    /// <exception cref="InvalidOperationException">Thrown if no name is given and no window is currently active.</exception>
    protected void ResetWindowPlacement(string? name = null) {
        ImGuiWindowPlacementState state = this.GetWindowPlacementState(this.GetCurrentWindowName(name));
        state.WasMoved = false;
        state.ResetRequested = true;
        state.HasUserOffset = false;
        state.HasAppliedPlacement = false;
        state.HasLastWindowState = false;
    }
    
    /// <summary>
    /// Updates the delta time of the private ImGui controller of this overlay.
    /// </summary>
    /// <param name="delta">The time in seconds since the last update.</param>
    internal void UpdateImGuiController(double delta) {
        this.Controller.Update(delta);
    }
    
    /// <summary>
    /// Renders this ImGui overlay into its offscreen render texture and returns the resolved texture.
    /// </summary>
    /// <param name="commandList">The command list used to record framebuffer, clear, ImGui, and texture copy commands.</param>
    /// <returns>The texture containing the rendered overlay for the current frame, or <c>null</c> if the render target could not be created.</returns>
    internal Texture2D? RenderToTexture(CommandList commandList) {
        this.HandleScale(this.Controller.Window.GetWidth(), this.Controller.Window.GetHeight());
        this.EnsureRenderTarget();
        
        if (this._renderTarget == null || this._resultTexture == null) {
            return null;
        }
        
        // Render into the target framebuffer.
        commandList.SetFramebuffer(this._renderTarget.Framebuffer);
        commandList.ClearColorTarget(0, new Color(0, 0, 0, 0).ToRgbaFloat());
        commandList.ClearDepthStencil(1.0F);
        
        // If gui is blocked it goes off-screen.
        Vector2 mousePosition = this.MouseBlocked ? new Vector2(-float.MaxValue, -float.MaxValue) : this.RealToVirtualMousePosition(Input.GetMousePosition());
        
        // Begin controller.
        this.Controller.Begin(commandList, this._renderTarget.Framebuffer, mousePosition);
        
        // Refreshes IO from the real window, so patch display values right after it.
        this.ApplyVirtualDisplayAfterBegin(this.Controller);
        
        // Draw overlay.
        this.Draw(this.Controller);
        
        // The context of this overlay is still current here.
        this.CapturesMouse = this.Controller.Io.WantCaptureMouse;
        this.OwnsMouse = this.CapturesMouse && (ImGui.IsMouseDown(ImGuiMouseButton.Left) || ImGui.IsMouseDown(ImGuiMouseButton.Right) || ImGui.IsMouseDown(ImGuiMouseButton.Middle));
        
        // End controller.
        this.Controller.End();
        
        // Copy render target to result texture.
        commandList.CopyTexture(this._renderTarget.ColorTexture, this._resultTexture.DeviceTexture);
        
        return this._resultTexture;
    }
    
    /// <summary>
    /// Updates the overlay scale state from the current window size.
    /// </summary>
    /// <param name="windowWidth">The current window width in logical pixels.</param>
    /// <param name="windowHeight">The current window height in logical pixels.</param>
    private void HandleScale(int windowWidth, int windowHeight) {
        windowWidth = Math.Max(1, windowWidth);
        windowHeight = Math.Max(1, windowHeight);
        
        if (!this.EnableScaleHandler) {
            this.CurrentScaleFactor = 1.0F;
            this.DestinationRectangle = new RectangleF(0.0F, 0.0F, windowWidth, windowHeight);
            this.VirtualDisplaySize = new Vector2(windowWidth, windowHeight);
            return;
        }
        
        float fitScale = Math.Min((float) windowWidth / this.Size.Width, (float) windowHeight / this.Size.Height);
        fitScale = MathF.Max(0.01F, fitScale);
        
        float scaleFactor = Math.Clamp(fitScale * this.Scale, 0.25F, 4.0F);
        
        if (this.UseIntegerScaling) {
            float integerScale = MathF.Floor(scaleFactor);
            
            if (integerScale >= 1.0F) {
                scaleFactor = integerScale;
            }
        }
        
        this.CurrentScaleFactor = scaleFactor;
        this.DestinationRectangle = new RectangleF(0.0F, 0.0F, windowWidth, windowHeight);
        this.VirtualDisplaySize = new Vector2(windowWidth / scaleFactor, windowHeight / scaleFactor);
    }
    
    /// <summary>
    /// Ensures the backing RenderTexture and result texture match the real window size.
    /// </summary>
    private void EnsureRenderTarget() {
        uint width = (uint) Math.Max(1, (int) MathF.Ceiling(this.DestinationRectangle.Width));
        uint height = (uint) Math.Max(1, (int) MathF.Ceiling(this.DestinationRectangle.Height));
        
        if (this._renderTarget == null || this._resultTexture == null) {
            this._renderTarget = new RenderTexture2D(GlobalGraphicsAssets.GraphicsDevice, width, height);
            this._resultTexture = new Texture2D(GlobalGraphicsAssets.GraphicsDevice, new Image((int) width, (int) height, new Color(0, 0, 0, 0)), false);
            return;
        }
        
        if (this._renderTarget.Width != width || this._renderTarget.Height != height) {
            this._renderTarget.Resize(width, height);
            this._resultTexture.Dispose();
            this._resultTexture = new Texture2D(GlobalGraphicsAssets.GraphicsDevice, new Image((int) width, (int) height, new Color(0, 0, 0, 0)), false);
        }
    }
    
    /// <summary>
    /// Applies virtual display values after Begin, because Begin refreshes IO from the real window.
    /// </summary>
    /// <param name="controller">The ImGui controller.</param>
    private void ApplyVirtualDisplayAfterBegin(ImGuiController controller) {
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        
        // Set new values.
        controller.Io.DisplaySize = this.VirtualDisplaySize;
        controller.Io.DisplayFramebufferScale = new Vector2(this.CurrentScaleFactor, this.CurrentScaleFactor);
        
        viewport.Pos = Vector2.Zero;
        viewport.Size = this.VirtualDisplaySize;
        viewport.WorkPos = Vector2.Zero;
        viewport.WorkSize = this.VirtualDisplaySize;
    }
    
    /// <summary>
    /// Converts the real window mouse position into virtual ImGui coordinates.
    /// </summary>
    /// <param name="realMouse">Mouse position in real window pixels.</param>
    /// <returns>Mouse position in virtual ImGui coordinates.</returns>
    private Vector2 RealToVirtualMousePosition(Vector2 realMouse) {
        if (this.CurrentScaleFactor <= 0.0F) {
            return new Vector2(-float.MaxValue, -float.MaxValue);
        }
        
        float x = realMouse.X / this.CurrentScaleFactor;
        float y = realMouse.Y / this.CurrentScaleFactor;
        
        if (x < 0.0F || y < 0.0F || x > this.VirtualDisplaySize.X || y > this.VirtualDisplaySize.Y) {
            return new Vector2(-float.MaxValue, -float.MaxValue);
        }
        
        return new Vector2(x, y);
    }
    
    /// <summary>
    /// The placement state of an ImGui window, encapsulating various properties that describe its current positioning and user interaction state.
    /// </summary>
    private class ImGuiWindowPlacementState {
        
        /// <summary>
        /// Whether the user has manually moved this window.
        /// </summary>
        public bool WasMoved;
        
        /// <summary>
        /// Whether the window position should be reset to the requested placement on the next draw.
        /// </summary>
        public bool ResetRequested;
        
        /// <summary>
        /// Whether placement has been applied at least once.
        /// </summary>
        public bool HasAppliedPlacement;
        
        /// <summary>
        /// Whether a user/default offset has been stored.
        /// </summary>
        public bool HasUserOffset;
        
        /// <summary>
        /// Offset from the selected anchor in virtual ImGui coordinates.
        /// </summary>
        public Vector2 UserOffset;
        
        /// <summary>
        /// Current anchor position in virtual ImGui coordinates.
        /// </summary>
        public Vector2 CurrentAnchorPosition;
        
        /// <summary>
        /// Current ImGui pivot for the selected anchor.
        /// </summary>
        public Vector2 CurrentPivot;
        
        /// <summary>
        /// Whether a previous window position and size have been stored.
        /// </summary>
        public bool HasLastWindowState;
        
        /// <summary>
        /// Previous window position.
        /// </summary>
        public Vector2 LastWindowPosition;
        
        /// <summary>
        /// Previous window size.
        /// </summary>
        public Vector2 LastWindowSize;
        
        /// <summary>
        /// Whether a previous virtual display size has been stored.
        /// </summary>
        public bool HasLastVirtualDisplaySize;
        
        /// <summary>
        /// Previous virtual display size.
        /// </summary>
        public Vector2 LastVirtualDisplaySize;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Controller.Dispose();
            this._renderTarget?.Dispose();
            this._resultTexture?.Dispose();
            this._windowPlacementStates.Clear();
        }
    }
}