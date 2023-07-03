using System.Drawing;
using System.Numerics;
using Raylib_cs;
using Color = Raylib_cs.Color;

namespace Sparkle.csharp.gui.elements; 

public class LabelElement : GuiElement {
    
    private Font _font;
    private string _text;
    private int _fontSize;
    private int _spacing;
    
    public LabelElement(string name, Font font, string text, int fontSize, Vector2 position, Func<bool> clickClickFunc) : this(name, font, text, fontSize, 2, position, Color.WHITE, clickClickFunc) {

    }
    
    public LabelElement(string name, Font font, string text, int fontSize, int spacing, Vector2 position, Color color, Func<bool> clickClickFunc) : base(name, position, Size.Empty, color, clickClickFunc) {
        this._font = font;
        this._text = text;
        this._fontSize = fontSize;
        this._spacing = spacing;
        //this.Size = Raylib.MeasureTextEx(this._font, this.text, this._fontSize, this._spacing);
    }

    protected internal override void Draw() {
        Raylib.DrawTextPro(this._font, this._text, this.Position, Vector2.Zero, 0, this._fontSize, this._spacing, this.Color);
    }
}