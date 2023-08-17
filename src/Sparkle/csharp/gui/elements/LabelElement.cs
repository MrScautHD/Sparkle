using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.elements.data;

namespace Sparkle.csharp.gui.elements; 

public class LabelElement : GuiElement {
    
    public Font Font;
    public float Rotation;
    public Color Color;
    public Color HoverColor;
    
    private string _text;
    private int _fontSize;
    private int _spacing;
    
    public LabelElement(string name, LabelData data, Vector2 position, Func<bool>? clickClickFunc = null) : base(name, position, data.Size, clickClickFunc) {
        this.Font = data.Font;
        this.Rotation = data.Rotation;
        this.Color = data.Color;
        this.HoverColor = data.HoverColor;
        
        this._text = data.Text;
        this._fontSize = data.FontSize;
        this._spacing = data.Spacing;
        this.ReloadTextSize();
    }

    protected internal override void Draw() {
        FontHelper.DrawText(this.Font, this.Text, this.CalcPos, Vector2.Zero, this.Rotation, this.FontSize, this.Spacing, this.IsHovered ? this.HoverColor : this.Color);
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
    
    private void ReloadTextSize() {
        this.Size = FontHelper.MeasureText(this.Font, this.Text, this.FontSize, this.Spacing);
    }
}