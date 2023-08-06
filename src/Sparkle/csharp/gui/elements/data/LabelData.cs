using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;

namespace Sparkle.csharp.gui.elements.data; 

public struct LabelData {
    
    public Font Font;
    public float Rotation;
    
    private string _text;
    private int _fontSize;
    private int _spacing;
    
    public Vector2 TextSize { get; private set; }

    public LabelData() {
        this.Font = FontHelper.GetDefault();
        this.Rotation = 0;
        this._text = string.Empty;
        this._fontSize = 18;
        this._spacing = 4;
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
    
    private void ReloadTextSize() {
        this.TextSize = FontHelper.MeasureText(this.Font, this._text, this._fontSize, this._spacing);
    }
}