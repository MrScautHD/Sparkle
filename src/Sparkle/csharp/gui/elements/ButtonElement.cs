using System.Drawing;
using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Color = Raylib_cs.Color;

namespace Sparkle.csharp.gui.elements; 

public class ButtonElement : GuiElement {
    
    private Font _font;
    private string _text;
    private int _fontSize;
    private int _spacing;
    
    public Color FontColor;
    public Size TextSize { get; private set; }
    
    public Texture2D Texture;
    
    private Color _hoverColor;

    public ButtonElement(string name, Texture2D texture, Font font, string text, int fontSize, int spacing, Vector2 position, Size size, Color color, Color fontColor, Func<bool> clickClickFunc) : base(name, position, size, color, clickClickFunc) {
        this.Texture = texture;
        this._font = font;
        this._text = text;
        this._fontSize = fontSize;
        this._spacing = spacing;
        this.FontColor = fontColor;
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

    protected internal override void Update() {
        base.Update();

        this._hoverColor = this.Color;
        
        if (this.IsHovered) {
            // TODO CHECK THE COLOR IDK IT NOT WORKS
            this._hoverColor.r *= Convert.ToByte(0.5F);
            this._hoverColor.g *= Convert.ToByte(0.5F);
            this._hoverColor.b *= Convert.ToByte(0.5F);
            this._hoverColor.a *= Convert.ToByte(0.5F);
        }
    }
    
    protected internal override void Draw() {
        TextureHelper.Draw(this.Texture, this.Position, this._hoverColor);

        Vector2 textPos = new Vector2(this.Position.X + this.Size.Width / 2F - this.TextSize.Width / 2F, this.Position.Y + this.Size.Height / 2F - this.TextSize.Height);
        FontHelper.DrawText(this._font, this._text, textPos, Vector2.Zero, 0, this._fontSize, this._spacing, this.FontColor);
    }

    private void ReloadTextSize() {
        Vector2 size = FontHelper.MeasureText(this._font, this._text, this._fontSize, this._spacing);
        this.TextSize = new Size((int) size.X, (int) size.Y);
    }
}