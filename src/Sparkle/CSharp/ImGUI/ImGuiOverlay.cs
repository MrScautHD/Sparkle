using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Images;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Bliss.ImGUI.CSharp;
using Hexa.NET.ImGui;
using Sparkle.CSharp.Graphics;
using Veldrith;

namespace Sparkle.CSharp.ImGUI;

public abstract class ImGuiOverlay : Disposable {
    
    /// <summary>
    /// The name of the overlay.
    /// </summary>
    public string Name { get; private set; }
    
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
    public bool Enabled;
    
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
    /// Render target containing the full-window ImGui overlay.
    /// </summary>
    private RenderTexture2D? _renderTarget;
    
    /// <summary>
    /// Texture copy of the render target, used for sprite drawing.
    /// </summary>
    private Texture2D? _resultTexture;
    
    /// <summary>
    /// Previous ImGui display size.
    /// </summary>
    private Vector2 _previousDisplaySize;
    
    /// <summary>
    /// Previous ImGui framebuffer scale.
    /// </summary>
    private Vector2 _previousFramebufferScale;
    
    /// <summary>
    /// Previous ImGui mouse position.
    /// </summary>
    private Vector2 _previousMousePos;
    
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
        this.Size = size ?? (1280, 720);
        this.Scale = scale;
        this.Enabled = enabled;
        this.EnableScaleHandler = enableScaleHandler;
        this.UseIntegerScaling = useIntegerScaling;
        this.CurrentScaleFactor = 1.0F;
        this.DestinationRectangle = new RectangleF(0.0F, 0.0F, this.Size.Width, this.Size.Height);
        this.VirtualDisplaySize = new Vector2(this.Size.Width, this.Size.Height);
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
    /// Renders this ImGui overlay into its offscreen render texture and returns the resolved texture.
    /// </summary>
    /// <param name="commandList">The command list used to record framebuffer, clear, ImGui, and texture copy commands.</param>
    /// <param name="controller">The ImGui controller responsible for beginning, ending, and rendering the ImGui frame.</param>
    /// <returns>The texture containing the rendered overlay for the current frame, or <c>null</c> if the render target could not be created.</returns>
    internal Texture2D? RenderToTexture(CommandList commandList, ImGuiController controller) {
        this.HandleScale(controller.Window.GetWidth(), controller.Window.GetHeight());
        this.EnsureRenderTarget();
        
        if (this._renderTarget == null || this._resultTexture == null) {
            return null;
        }
        
        // Queue mouse before NewFrame so dragging gets the correct scaled mouse position.
        this.ApplyVirtualMouseBeforeBegin(controller);
        
        // Render into the target framebuffer.
        commandList.SetFramebuffer(this._renderTarget.Framebuffer);
        commandList.ClearColorTarget(0, new Color(0, 0, 0, 0).ToRgbaFloat());
        commandList.ClearDepthStencil(1.0F);
        
        // Begin controller.
        controller.Begin(commandList, this._renderTarget.Framebuffer);
        
        // Refreshes IO from the real window, so patch display values right after it.
        this.ApplyVirtualDisplayAfterBegin(controller);
        
        // Draw overlay.
        this.Draw(controller);
        
        // End controller.
        controller.End();
        
        // Restore previous ImGui values.
        this.RestoreImGuiIo(controller);
        
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
    /// Applies the virtual mouse position before ImGuiController.Begin calls ImGui.NewFrame.
    /// </summary>
    /// <param name="controller">The ImGui controller.</param>
    private void ApplyVirtualMouseBeforeBegin(ImGuiController controller) {
        this._previousDisplaySize = controller.Io.DisplaySize;
        this._previousFramebufferScale = controller.Io.DisplayFramebufferScale;
        this._previousMousePos = controller.Io.MousePos;
        
        Vector2 virtualMouse = this.RealToVirtualMousePosition(Input.GetMousePosition());
        controller.Io.AddMousePosEvent(virtualMouse.X, virtualMouse.Y);
        controller.Io.MousePos = virtualMouse;
    }
    
    /// <summary>
    /// Applies virtual display values after Begin, because Begin refreshes IO from the real window.
    /// </summary>
    /// <param name="controller">The ImGui controller.</param>
    private void ApplyVirtualDisplayAfterBegin(ImGuiController controller) {
        controller.Io.DisplaySize = this.VirtualDisplaySize;
        controller.Io.DisplayFramebufferScale = new Vector2(this.CurrentScaleFactor, this.CurrentScaleFactor);
        
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        viewport.Pos = Vector2.Zero;
        viewport.Size = this.VirtualDisplaySize;
        viewport.WorkPos = Vector2.Zero;
        viewport.WorkSize = this.VirtualDisplaySize;
    }
    
    /// <summary>
    /// Restores ImGui IO values after rendering into the overlay target.
    /// </summary>
    /// <param name="controller">The ImGui controller.</param>
    private void RestoreImGuiIo(ImGuiController controller) {
        controller.Io.DisplaySize = this._previousDisplaySize;
        controller.Io.DisplayFramebufferScale = this._previousFramebufferScale;
        controller.Io.MousePos = this._previousMousePos;
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
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._renderTarget?.Dispose();
            this._resultTexture?.Dispose();
        }
    }
}