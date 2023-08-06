using System.Drawing;
using System.Numerics;
using Raylib_cs;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace Sparkle.csharp.gui.elements; 

public abstract class GuiElement : IDisposable {

    public string Name;

    public Vector2 Position;
    public Size Size;
    public Color Color;

    protected bool IsHovered;
    protected bool IsClicked;

    private Func<bool> _clickFunc;

    public GuiElement(string name, Vector2 position, Size size, Color color, Func<bool> clickClickFunc) {
        this.Name = name;
        this.Position = position;
        this.Size = size;
        this.Color = color;
        this._clickFunc = clickClickFunc;
    }
    
    protected internal virtual void Init() {
        
    }

    protected internal virtual void Update() {
        Rectangle rec = new Rectangle(this.Position.X, this.Position.Y, this.Size.Width, this.Size.Height);
        
        if (Raylib.CheckCollisionPointRec(Input.GetMousePosition(), rec)) {
            this.IsHovered = true;
            this.IsClicked = Input.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT) && this._clickFunc.Invoke();
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