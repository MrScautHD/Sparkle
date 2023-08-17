using Raylib_cs;
using Sparkle.csharp.graphics.util;
using System.Numerics;

namespace Sparkle.csharp.gui.elements.data;
public class TextData : IData<TextData>
{
    public Font Font;
    public Vector2 Size { get; private set; }

    private string _text;
    private int _fontSize;
    private int _spacing;
    public TextData()
    {
        this.Font = FontHelper.GetDefault();
        this.Text = "";
        this.Spacing = 0;
        this.ReloadTextSize();
    }
    public TextData Clone()
    {
        return new TextData()
        {
            Font = this.Font,
            Text = this.Text,
            Spacing = this.Spacing,
        };
    }
    public string Text
    {
        get => this._text;
        set
        {
            this._text = value;
            this.ReloadTextSize();
        }
    }

    public int FontSize
    {
        get => this._fontSize;
        set
        {
            this._fontSize = value;
            this.ReloadTextSize();
        }
    }

    public int Spacing
    {
        get => this._spacing;
        set
        {
            this._spacing = value;
            this.ReloadTextSize();
        }
    }

    private void ReloadTextSize()
    {
        this.Size = FontHelper.MeasureText(this.Font, this._text, this._fontSize, this._spacing);
    }
}
