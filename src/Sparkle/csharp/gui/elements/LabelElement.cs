using System.Drawing;
using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Color = Raylib_cs.Color;

namespace Sparkle.csharp.gui.elements; 

public class LabelElement : GuiElement {
    
    private Font _font;
    private string _text;
    private int _fontSize;
    private int _spacing;
    
    public LabelElement(string name, Font font, string text, int fontSize, Vector2 position, Func<bool>? clickClickFunc) : this(name, font, text, fontSize, 4, position, Color.WHITE, clickClickFunc) {

    }
    
    public LabelElement(string name, Font font, string text, int fontSize, int spacing, Vector2 position, Color color, Func<bool>? clickClickFunc) : base(name, position, Size.Empty, color, clickClickFunc) {
        this._font = font;
        this._text = text;
        this._fontSize = fontSize;
        this._spacing = spacing;
        this.ReloadTextSize();
    }

    public string Text {
        get => this._text;
        set {
            this._text = value;
            this.ReloadTextSize();
        }
    }
    
    public int FontSize {
        get => this._fontSize;
        set {
            this._fontSize = value;
            this.ReloadTextSize();
        }
    }
    
    public int Spacing {
        get => this._spacing;
        set {
            this._spacing = value;
            this.ReloadTextSize();
        }
    }

    protected internal override void Draw() {
        FontHelper.DrawText(this._font, this._text, this.Position, Vector2.Zero, 0, this._fontSize, this._spacing, this.Color);
    }

    private void ReloadTextSize() {
        Vector2 size = FontHelper.MeasureText(this._font, this._text, this._fontSize, this._spacing);
        this.Size = new Size((int) size.X, (int) size.Y);
    }
}