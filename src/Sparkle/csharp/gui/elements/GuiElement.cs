using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Rectangle = Raylib_cs.Rectangle;

namespace Sparkle.csharp.gui.elements; 

public abstract class GuiElement : IDisposable {

    public readonly string Name;
    public bool Enabled;

    public Vector2 Position;
    public Vector2 Size;

    protected bool IsHovered;
    protected bool IsClicked;

    private Func<bool>? _clickFunc;

    public GuiElement(string name, Vector2 position, Vector2 size, Func<bool>? clickClickFunc) {
        this.Name = name;
        this.Enabled = true;
        this.Position = position;
        this.Size = size;
        this._clickFunc = clickClickFunc!;
    }
    
    protected internal virtual void Init() {
        
    }

    protected internal virtual void Update() {
        Rectangle rec = new Rectangle(this.Position.X, this.Position.Y, this.Size.X, this.Size.Y);
        
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
    
    protected internal virtual void FixedUpdate() {
        
    }

    protected internal abstract void Draw();
    
    public virtual void Dispose() {
        
    }
}