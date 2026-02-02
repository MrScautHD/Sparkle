using System.Numerics;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Interact.Mice.Cursors;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

public abstract class GuiElement {
    
    /// <summary>
    /// The GUI instance to which this element belongs.
    /// </summary>
    public Gui Gui { get; internal set; }
    
    /// <summary>
    /// The unique name of this element within its GUI.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Indicates whether this element is active and should be updated and drawn.
    /// </summary>
    public bool Enabled;
    
    /// <summary>
    /// Indicates whether the GUI element can respond to user interactions, such as clicks or hover events.
    /// </summary>
    public bool Interactable;
    
    /// <summary>
    /// The anchor point used for positioning this element relative to the screen.
    /// </summary>
    public Anchor AnchorPoint;
    
    /// <summary>
    /// The offset from the anchor point used to position this element.
    /// </summary>
    public Vector2 Offset;
    
    /// <summary>
    /// The base size of the element before scaling.
    /// </summary>
    public Vector2 Size;

    /// <summary>
    /// The scaling factor applied to the element, modifying its size along the X and Y axes.
    /// </summary>
    public Vector2 Scale;
    
    /// <summary>
    /// The origin point (pivot) used for rotation and scaling.
    /// </summary>
    public Vector2 Origin;
    
    /// <summary>
    /// The rotation applied to the element in radians.
    /// </summary>
    public float Rotation;
    
    /// <summary>
    /// Indicates whether the mouse is currently hovering over this element.
    /// </summary>
    public bool IsHovered { get; private set; }
    
    /// <summary>
    /// Indicates whether the element has been clicked during this update cycle.
    /// </summary>
    public bool IsClicked { get; private set; }
    
    /// <summary>
    /// Indicates whether this element is currently selected.
    /// </summary>
    public bool IsSelected { get; private set; }
    
    /// <summary>
    /// The on-screen position of the element after anchor and offset are applied.
    /// </summary>
    public Vector2 Position { get; private set; }
    
    /// <summary>
    /// The size of the element after scaling is applied.
    /// </summary>
    public Vector2 ScaledSize { get; private set; }
    
    /// <summary>
    /// A function invoked to handle click behavior for the GUI element.
    /// Should return a boolean indicating if the element was successfully clicked.
    /// </summary>
    private Func<GuiElement, bool>? _clickFunc;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GuiElement"/> class.
    /// </summary>
    /// <param name="anchor">The anchor point for positioning the element.</param>
    /// <param name="offset">The offset from the anchor position.</param>
    /// <param name="size">The unscaled size of the element.</param>
    /// <param name="scale">Optional scaling factor for the element. Defaults to (1, 1).</param>
    /// <param name="origin">Optional origin point for rotation/scaling. Defaults to (0, 0).</param>
    /// <param name="rotation">Optional rotation in radians. Defaults to 0.</param>
    /// <param name="clickFunc">Optional function that determines what happens on click. Should return true if handled.</param>
    public GuiElement(Anchor anchor, Vector2 offset, Vector2 size, Vector2? scale = null, Vector2? origin = null, float rotation = 0.0F, Func<GuiElement, bool>? clickFunc = null) {
        this.Enabled = true;
        this.Interactable = true;
        this.AnchorPoint = anchor;
        this.Offset = offset;
        this.Size = size;
        this.Scale = scale ?? Vector2.One;
        this.Origin = origin ?? Vector2.Zero;
        this.Rotation = rotation;
        this._clickFunc = clickFunc;
    }
    
    /// <summary>
    /// Updates the state of the GuiElement during each frame with the given time delta.
    /// </summary>
    /// <param name="delta">The time elapsed between the current and the previous frame, in seconds.</param>
    /// <param name="interactionHandled">A reference to a boolean tracking whether interaction has already been handled by another element.</param>
    protected internal virtual void Update(double delta, ref bool interactionHandled) {
        this.ScaledSize = this.CalculateSize();
        this.Position = this.CalculatePos();
        
        RectangleF rectangle = new RectangleF(this.Position.X, this.Position.Y, this.ScaledSize.X, this.ScaledSize.Y);
        
        if (!interactionHandled && rectangle.Contains(Input.GetMousePosition(), this.Origin * this.Scale * this.Gui.ScaleFactor, this.Rotation)) {
            this.IsHovered = true;
            interactionHandled = true;
            
            if (Input.IsMouseButtonPressed(MouseButton.Left) && this.Interactable) {
                if (this._clickFunc?.Invoke(this) ?? true) {
                    this.IsClicked = true;
                    this.IsSelected = true;
                }
            }
            else {
                this.IsClicked = false;
            }
        }
        else {
            this.IsHovered = false;
            this.IsClicked = false;
            
            if (Input.IsMouseButtonPressed(MouseButton.Left)) {
                this.IsSelected = false;
            }
        }
        
        if (Input.IsKeyPressed(KeyboardKey.Escape) || Input.IsKeyPressed(KeyboardKey.Enter) || !this.Interactable) {
            this.IsSelected = false;
        }
    }
    
    /// <summary>
    /// Invoked after the update process to perform additional operations specific to the element.
    /// </summary>
    /// <param name="delta">The time elapsed since the last update, typically used for time-dependent operations.</param>
    protected internal virtual void AfterUpdate(double delta) { }
    
    /// <summary>
    /// Executes logic that requires a constant fixed time step for this element.
    /// </summary>
    /// <param name="fixedStep">The fixed time step duration, typically used for physics or other time-dependent updates.</param>
    protected internal virtual void FixedUpdate(double fixedStep) { }

    /// <summary>
    /// Renders the graphical representation of the GUI element to the specified framebuffer.
    /// </summary>
    /// <param name="context">The graphics context used for rendering the element.</param>
    /// <param name="framebuffer">The framebuffer where the element will be drawn.</param>
    protected internal abstract void Draw(GraphicsContext context, Framebuffer framebuffer);
    
    /// <summary>
    /// Optional method for responding to window or viewport resize events.
    /// </summary>
    /// <param name="rectangle">The new size and bounds of the rendering area.</param>
    protected internal virtual void Resize(Rectangle rectangle) { }

    /// <summary>
    /// Calculates the position of the GUI element based on its anchor point, scaled size, offset, and origin.
    /// </summary>
    /// <returns>A <see cref="Vector2"/> representing the calculated position of the element on the screen.</returns>
    protected virtual Vector2 CalculatePos() {
        Vector2 pos = Vector2.Zero;
        int scaleFactor = this.Gui.ScaleFactor;
        
        float width = MathF.Floor((float) GlobalGraphicsAssets.Window.GetWidth() / scaleFactor) * scaleFactor;
        float height = MathF.Floor((float) GlobalGraphicsAssets.Window.GetHeight() / scaleFactor) * scaleFactor;
        
        switch (this.AnchorPoint) {
            case Anchor.TopLeft:
                break;
            
            case Anchor.TopCenter:
                pos.X = width / 2.0F - this.ScaledSize.X / 2.0F;
                break;
            
            case Anchor.TopRight:
                pos.X = width - this.ScaledSize.X;
                break;
            
            case Anchor.CenterLeft:
                pos.Y = height / 2.0F - this.ScaledSize.Y / 2.0F;
                break;
            
            case Anchor.Center:
                pos.X = width / 2.0F - this.ScaledSize.X / 2.0F;
                pos.Y = height / 2.0F - this.ScaledSize.Y / 2.0F;
                break;
            
            case Anchor.CenterRight:
                pos.X = width - this.ScaledSize.X;
                pos.Y = height / 2.0F - this.ScaledSize.Y / 2.0F;
                break;
            
            case Anchor.BottomLeft:
                pos.Y = height - this.ScaledSize.Y;
                break;
            
            case Anchor.BottomCenter:
                pos.X = width / 2.0F - this.ScaledSize.X / 2.0F;
                pos.Y = height - this.ScaledSize.Y;
                break;
            
            case Anchor.BottomRight:
                pos.X = width - this.ScaledSize.X;
                pos.Y = height - this.ScaledSize.Y;
                break;
        }
        
        Vector2 finalPos = pos + (this.Offset * scaleFactor) + this.Origin;
        
        return new Vector2(
            MathF.Floor(finalPos.X / scaleFactor) * scaleFactor,
            MathF.Floor(finalPos.Y / scaleFactor) * scaleFactor
        );
    }
    
    /// <summary>
    /// Calculates the scaled size of the GUI element based on the base size and the current scale factor.
    /// </summary>
    /// <returns>The scaled size of the GUI element as a <see cref="Vector2"/>.</returns>
    protected virtual Vector2 CalculateSize() {
        return this.Size * this.Scale * this.Gui.ScaleFactor;
    }
}