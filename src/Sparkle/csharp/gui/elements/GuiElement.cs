using System.Drawing;
using System.Numerics;
using Raylib_cs;
using Color = Raylib_cs.Color;
using Rectangle = Raylib_cs.Rectangle;

namespace Sparkle.csharp.gui.elements; 

public abstract class GuiElement : IDisposable {

    public string Name;
    
    public bool Enabled;
    public bool Invisible;

    public Vector2 Position;
    public Vector2 Size;
    public Color DefaultColor;
    public Color Color;

    protected bool IsHovered;
    protected bool IsClicked;

    private Func<bool>? _clickFunc;

    public GuiElement(string name, Vector2 position, Vector2 size, Color color, Func<bool>? clickClickFunc) {
        this.Name = name;
        this.Enabled = true;
        this.Position = position;
        this.Size = size;
        this.DefaultColor = color;
        this.Color = color;
        this._clickFunc = clickClickFunc!;
    }
    
    protected internal virtual void Init() {
        
    }

    protected internal virtual void Update() {
        this.Color = this.DefaultColor;
        
        if (!this.Enabled) {
            this.Color.r = Convert.ToByte(Convert.ToInt32(this.Color.r * 0.5F));
            this.Color.g = Convert.ToByte(Convert.ToInt32(this.Color.g * 0.5F));
            this.Color.b = Convert.ToByte(Convert.ToInt32(this.Color.b * 0.5F));
            this.Color.a = Convert.ToByte(Convert.ToInt32(this.Color.a * 0.5F));
        }

        Rectangle rec = new Rectangle(this.Position.X, this.Position.Y, this.Size.X, this.Size.Y);
        
        if (Raylib.CheckCollisionPointRec(Input.GetMousePosition(), rec)) {
            this.IsHovered = true;

            if (Input.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT) && !this.Invisible && this.Enabled) {
                if (this._clickFunc != null) {
                    this.IsClicked = this._clickFunc.Invoke();
                }
                else {
                    this.IsClicked = true;
                }
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