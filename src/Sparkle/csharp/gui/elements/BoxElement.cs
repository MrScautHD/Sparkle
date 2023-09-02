using System.Drawing;
using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Color = Raylib_cs.Color;

namespace Sparkle.csharp.gui.elements; 

public class BoxElement {}/* : GuiElement {
    
    public Texture2D Texture;
    
    private readonly Font _font;
    private string _defaultText;
    private string _text;
    private string _togText;
    private int _fontSize;
    private int _spacing;
    
    public Size TextSize { get; private set; }
    public Color FontColor;
    
    public bool Toggled { get; private set; }
    
    public BoxElement(string name, Texture2D texture, Font font, string text, string togText, int fontSize, Vector2 position, Color color, Color fontColor, Func<bool>? clickClickFunc) : this(name, texture, font, text, togText, fontSize, 4, position, Vector2.Zero, color, fontColor, clickClickFunc) {
        this.Size = new Vector2(texture.width, texture.height);
    }
    
    public BoxElement(string name, Texture2D texture, Font font, string text, string togText, int fontSize, Vector2 position, Vector2 size, Color color, Color fontColor, Func<bool>? clickClickFunc) : this(name, texture, font, text, togText, fontSize, 4, position, size, color, fontColor, clickClickFunc) {
    }
    
    public BoxElement(string name, Texture2D texture, Font font, string text, string togText, int fontSize, int spacing, Vector2 position, Color color, Color fontColor, Func<bool>? clickClickFunc) : this(name, texture, font, text, togText, fontSize, spacing, position, Vector2.Zero, color, fontColor, clickClickFunc) {
        this.Size = new Vector2(texture.width, texture.height);
    }

    public BoxElement(string name, Texture2D texture, Font font, string text, string togText, int fontSize, int spacing, Vector2 position, Vector2 size, Color color, Color fontColor, Func<bool>? clickClickFunc) : base(name, position, size, color, clickClickFunc) {
        this.Texture = texture;
        this._font = font;
        this._defaultText = text;
        this._text = text;
        this._togText = togText;
        this._fontSize = fontSize;
        this._spacing = spacing;
        this.FontColor = fontColor;
        this.Toggled = false;
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
        
        if (this.IsClicked) {
            this.Toggled = !this.Toggled;
            this.Text = this.Toggled ? this._togText : this._defaultText;
        }
        
        this.Color = this.DefaultColor;
        
        if (this.Enabled && !this.Toggled) {
            this.Color.r = Convert.ToByte(Convert.ToInt32(this.Color.r * 0.5F));
            this.Color.g = Convert.ToByte(Convert.ToInt32(this.Color.g * 0.5F));
            this.Color.b = Convert.ToByte(Convert.ToInt32(this.Color.b * 0.5F));
            this.Color.a = Convert.ToByte(Convert.ToInt32(this.Color.a * 0.5F));
        }
    }
    
    protected internal override void Draw() {
        TextureHelper.Draw(this.Texture, this.Position, this.Color);

        Vector2 textPos = new Vector2(this.Position.X + this.Size.X / 2F - this.TextSize.Width / 2F, this.Position.Y + this.Size.Y / 2F - this.TextSize.Height);
        FontHelper.DrawText(this._font, this._text, textPos, Vector2.Zero, 0, this._fontSize, this._spacing, this.FontColor);
    }
    
    private void ReloadTextSize() {
        Vector2 size = FontHelper.MeasureText(this._font, this._text, this._fontSize, this._spacing);
        this.TextSize = new Size((int) size.X, (int) size.Y);
    }
}*/