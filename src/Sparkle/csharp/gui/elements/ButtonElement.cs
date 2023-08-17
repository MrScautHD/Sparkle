using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.elements.data;

namespace Sparkle.csharp.gui.elements; 

public class ButtonElement : GuiElement {
    
    public Texture2D? Texture;
    public float Rotation;
    public Color Color;
    public Color HoverColor;
    
    public Font Font;
    public float TextRotation;
    public Vector2 TextSize;
    public Vector2 CalcTextSize;
    public Color TextColor;
    public Color TextHoverColor;
    
    private string _text;
    private int _fontSize;
    private int _spacing;
    
    public ButtonElement(string name, ButtonData buttonData, LabelData labelData, Vector2 position, Vector2? size, Func<bool>? clickClickFunc = null) : base(name, position, Vector2.Zero, clickClickFunc) {
        this.Texture = buttonData.Texture;
        this.Size = size ?? (this.Texture != null ? new Vector2(this.Texture.Value.width, this.Texture.Value.height) : Vector2.Zero);
        this.Rotation = buttonData.Rotation;
        this.Color = buttonData.Color;
        this.HoverColor = buttonData.HoverColor;
        
        this.Font = labelData.Font;
        this.TextRotation = labelData.Rotation;
        this.TextColor = labelData.Color;
        this.TextHoverColor = labelData.HoverColor;
        
        this._text = labelData.Text;
        this._fontSize = labelData.FontSize;
        this._spacing = labelData.Spacing;
        this.ReloadTextSize();
    }

    protected internal override void Update() {
        base.Update();
        this.CalcTextSize = new Vector2(this.TextSize.X * this.WidthScale, this.TextSize.Y * this.HeightScale);
    }

    protected internal override void Draw() {
        if (this.Texture != null) {
            Rectangle source = new Rectangle(0, 0, this.Texture.Value.width, this.Texture.Value.height);
            Rectangle dest = new Rectangle(this.CalcPos.X + (this.CalcSize.X / 2), this.CalcPos.Y + (this.CalcSize.Y / 2), this.CalcSize.X, this.CalcSize.Y);
            Vector2 origin = new Vector2(dest.width / 2, dest.height / 2);
            TextureHelper.DrawPro(this.Texture.Value, source, dest, origin, this.Rotation, this.IsHovered ? this.HoverColor : this.Color);
        }
        else {
            Rectangle rec = new Rectangle(this.CalcPos.X + (this.CalcSize.X / 2), this.CalcPos.Y + (this.CalcSize.Y / 2), this.CalcSize.X, this.CalcSize.Y);
            Vector2 origin = new Vector2(rec.width / 2, rec.height / 2);
            ShapeHelper.DrawRectangle(rec, origin, this.Rotation, this.IsHovered ? this.HoverColor : this.Color);
        }
        
        Vector2 textPos = new Vector2(this.CalcPos.X + this.CalcSize.X / 2, this.CalcPos.Y + this.CalcSize.Y / 2);
        Vector2 textOrigin = new Vector2(this.TextSize.X / 2, this.TextSize.Y / 2);
        FontHelper.DrawText(this.Font, this.Text, textPos, textOrigin, this.TextRotation, this.FontSize, this.Spacing, this.IsHovered ? this.TextHoverColor : this.TextColor);
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
        this.TextSize = FontHelper.MeasureText(this.Font, this.Text, this.FontSize, this.Spacing);
    }
}