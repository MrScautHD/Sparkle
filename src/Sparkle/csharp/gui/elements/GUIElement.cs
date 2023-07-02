using System.Drawing;
using System.Numerics;
using Raylib_cs;
using Rectangle = Raylib_cs.Rectangle;

namespace Sparkle.csharp.gui.elements; 

public abstract class GUIElement : IDisposable {

    public string Name;

    public Vector2 Position;
    public Size Size;

    protected bool IsHovered;
    protected bool IsClicked;

    private Func<bool> _clickFunc;

    public GUIElement(string name, Vector2 position, Size size, Func<bool> clickClickFunc) {
        this.Name = name;
        this.Position = position;
        this.Size = size;
        this._clickFunc = clickClickFunc;
    }
    
    protected internal virtual void Init() {
        
    }

    protected internal virtual void Update() {
        Rectangle rec = new Rectangle(this.Position.X, this.Position.Y, this.Size.Width, this.Size.Height);
        
        if (Raylib.CheckCollisionPointRec(Input.GetMousePosition(), rec)) {
            if (Input.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) {
                this.IsClicked = this._clickFunc.Invoke();
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