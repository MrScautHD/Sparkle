using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.window;
using Rectangle = Raylib_cs.Rectangle;

namespace Sparkle.csharp.gui.element; 

public abstract class GuiElement : Disposable {

    public readonly string Name;
    public bool Enabled;

    public Vector2 Position;
    public Vector2 Size;

    protected bool IsHovered;
    protected bool IsClicked;
    
    protected float WidthScale { get; private set; }
    protected float HeightScale { get; private set; }
    
    protected Vector2 CalcPos { get; private set; }
    protected Vector2 CalcSize { get; private set; }

    private Func<bool>? _clickFunc;
    
    public bool HasInitialized { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiElement"/> with specified parameters, setting its name, enabled state, position, size, and optional click function.
    /// </summary>
    /// <param name="name">The name of the GuiElement.</param>
    /// <param name="position">The position of the GuiElement on the screen.</param>
    /// <param name="size">The size of the GuiElement.</param>
    /// <param name="clickClickFunc">Optional click function to be executed when the GuiElement is clicked. Defaults to null.</param>
    public GuiElement(string name, Vector2 position, Vector2 size, Func<bool>? clickClickFunc) {
        this.Name = name;
        this.Enabled = true;
        this.Position = position;
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
        this.UpdateScale();
        
        this.CalcSize = new Vector2(this.Size.X * GuiManager.Scale, this.Size.Y * GuiManager.Scale);
        
        float differenceX = Math.Abs(this.Size.X - this.CalcSize.X);
        float differenceY = Math.Abs(this.Size.Y - this.CalcSize.Y);
        
        this.CalcPos = new Vector2((this.Position.X + differenceX + this.CalcSize.X) * this.WidthScale - this.CalcSize.X, (this.Position.Y + differenceY + this.CalcSize.Y) * this.HeightScale - this.CalcSize.Y);
        
        Rectangle rec = new Rectangle(this.CalcPos.X, this.CalcPos.Y, this.CalcSize.X, this.CalcSize.Y);
        if (ShapeHelper.CheckCollisionPointRec(Input.GetMousePosition(), rec)) {
            this.IsHovered = true;

            if (Input.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT) && this.Enabled) {
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
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="Update"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() { }

    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal abstract void Draw();
    
    /// <summary>
    /// Updates the scaling factors for width and height based on the render dimensions.
    /// </summary>
    private void UpdateScale() {
        this.WidthScale = Window.GetRenderWidth() / (float) Game.Instance.Settings.WindowWidth;
        this.HeightScale = Window.GetRenderHeight() / (float) Game.Instance.Settings.WindowHeight;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Enabled = false;
        }
    }
}