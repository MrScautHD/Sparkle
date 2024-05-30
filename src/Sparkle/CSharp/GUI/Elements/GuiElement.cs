using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Transformations;
using Raylib_CSharp.Windowing;

namespace Sparkle.CSharp.GUI.Elements;

public abstract class GuiElement : Disposable {

    public readonly string Name;
    public bool Enabled;

    protected Vector2 Position;
    protected Vector2 ScaledSize;
    
    public Anchor? AnchorPoint;
    public Vector2 Offset;
    public Vector2 Size;
    public float Rotation;

    protected bool IsHovered;
    protected bool IsClicked;
    
    private Func<bool>? _clickFunc;
    
    public bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Initializes a new GUI element with the specified parameters.
    /// </summary>
    /// <param name="name">The name of the GUI element.</param>
    /// <param name="anchor">The anchor point for positioning the element.</param>
    /// <param name="offset">An optional offset for fine-tuning the position.</param>
    /// <param name="size">The size of the GUI element.</param>
    /// <param name="clickClickFunc">An optional function to handle click events.</param>
    protected GuiElement(string name, Anchor anchor, Vector2 offset, Vector2 size, Func<bool>? clickClickFunc) {
        this.Name = name;
        this.Enabled = true;
        this.AnchorPoint = anchor;
        this.Offset = offset;
        this.Size = size;
        this._clickFunc = clickClickFunc!;
    }

    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal virtual void Init() {
        this.HasInitialized = true;
    }

    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    protected internal virtual void Update() {
        this.CalculateSize();
        this.CalculatePosition();
        
        Rectangle rec = new Rectangle(this.Position.X, this.Position.Y, this.ScaledSize.X, this.ScaledSize.Y);
        if (this.CheckCollisionPointRec(Input.GetMousePosition(), rec, this.Rotation)) {
            this.IsHovered = true;

            if (Input.IsMouseButtonPressed(MouseButton.Left) && this.Enabled) {
                this.IsClicked = this._clickFunc == null || this._clickFunc.Invoke();
            }
            else {
                this.IsClicked = false;
            }
        }
        else {
            this.IsHovered = false;
            this.IsClicked = false;
        }
    }
    
    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    protected internal virtual void AfterUpdate() { }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() { }

    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal abstract void Draw();
    
    /// <summary>
    /// Calculates and updates the size of the GUI element based on the current display scale.
    /// </summary>
    protected virtual void CalculateSize() {
        float scale = Window.GetRenderHeight() / (float) Game.Instance.Settings.Height;
        this.ScaledSize = this.Size * scale * GuiManager.Scale;
    }

    /// <summary>
    /// Calculates the position of the GUI element based on its anchor point and offset.
    /// </summary>
    protected virtual void CalculatePosition() {
        Vector2 pos = Vector2.Zero;
        
        switch (this.AnchorPoint) {
            case Anchor.TopLeft:
                break;
            
            case Anchor.TopCenter:
                pos.X = Window.GetRenderWidth() / 2.0F - this.ScaledSize.X / 2.0F;
                break;
            
            case Anchor.TopRight:
                pos.X = Window.GetRenderWidth() - this.ScaledSize.X;
                break;
            
            case Anchor.CenterLeft:
                pos.Y = Window.GetRenderHeight() / 2.0F - this.ScaledSize.Y / 2.0F;
                break;
            
            case Anchor.Center:
                pos.X = Window.GetRenderWidth() / 2.0F - this.ScaledSize.X / 2.0F;
                pos.Y = Window.GetRenderHeight() / 2.0F - this.ScaledSize.Y / 2.0F;
                break;
            
            case Anchor.CenterRight:
                pos.X = Window.GetRenderWidth() - this.ScaledSize.X;
                pos.Y = Window.GetRenderHeight() / 2.0F - this.ScaledSize.Y / 2.0F;
                break;
            
            case Anchor.BottomLeft:
                pos.Y = Window.GetRenderHeight() - this.ScaledSize.Y;
                break;
            
            case Anchor.BottomCenter:
                pos.X = Window.GetRenderWidth() / 2.0F - this.ScaledSize.X / 2.0F;
                pos.Y = Window.GetRenderHeight() - this.ScaledSize.Y;
                break;
            
            case Anchor.BottomRight:
                pos.X = Window.GetRenderWidth() - this.ScaledSize.X;
                pos.Y = Window.GetRenderHeight() - this.ScaledSize.Y;
                break;
        }

        pos.X += this.Offset.X;
        pos.Y += this.Offset.Y;
        
        this.Position = pos;
    }
    
    /// <summary>
    /// Checks if a given point collides with a specified rectangle in the GUI element.
    /// </summary>
    /// <param name="point">The point to check for collision.</param>
    /// <param name="rec">The rectangle to check for collision.</param>
    /// <param name="rotation">The rotation of the rectangle in degrees.</param>
    /// <returns>True if the point collides with the rectangle, false otherwise.</returns>
    private bool CheckCollisionPointRec(Vector2 point, Rectangle rec, float rotation) {
        float rotationInRadians = rotation * RayMath.Deg2Rad;
        
        float centerX = rec.X + rec.Width / 2f;
        float centerY = rec.Y + rec.Height / 2f;

        float deltaX = point.X - centerX;
        float deltaY = point.Y - centerY;

        float rotatedX = centerX + (deltaX * MathF.Cos(rotationInRadians) - deltaY * MathF.Sin(rotationInRadians));
        float rotatedY = centerY + (deltaX * MathF.Sin(rotationInRadians) + deltaY * MathF.Cos(rotationInRadians));

        return rotatedX >= rec.X && rotatedY >= rec.Y && rotatedX <= (rec.X + rec.Width) && rotatedY <= (rec.Y + rec.Height);
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Enabled = false;
        }
    }
}