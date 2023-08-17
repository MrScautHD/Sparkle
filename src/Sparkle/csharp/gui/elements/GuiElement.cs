using System.Numerics;
using Raylib_cs;
using Rectangle = Raylib_cs.Rectangle;
using Sparkle.csharp.gui.elements.data;

namespace Sparkle.csharp.gui.elements; 

public abstract class GuiElement : IDisposable {

    public GUIElementData elementData;
    public string Name;

    protected bool IsHovered;
    protected bool IsClicked;

    private Func<bool>? _clickFunc;

    public GuiElement(string name, GUIElementData data, Func<bool>? clickClickFunc) {
        this.Name = name;
        this.elementData = data;
        this._clickFunc = clickClickFunc!;
    }
    
    protected internal virtual void Init() {
        
    }

    protected internal virtual void Update() {
        Rectangle rec = new Rectangle(this.elementData.Position.X, this.elementData.Position.Y, this.elementData.Size.X, this.elementData.Size.Y);
        
        if (Raylib.CheckCollisionPointRec(Input.GetMousePosition(), rec)) {
            this.IsHovered = true;

            if (Input.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT) && this.elementData.Enabled) {
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