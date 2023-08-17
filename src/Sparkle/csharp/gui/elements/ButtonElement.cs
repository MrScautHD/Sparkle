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
    
    protected internal override void Draw() {
        if (this.Texture != null) {
            Rectangle source = new Rectangle(0, 0, this.Texture.Value.width, this.Texture.Value.height);
            Rectangle dest = new Rectangle(this.Position.X + (this.Size.X / 2), this.Position.Y + (this.Size.Y / 2), this.Size.X, this.Size.Y);
            Vector2 origin = new Vector2(dest.width / 2, dest.height / 2);
            TextureHelper.DrawPro(this.Texture.Value, source, dest, origin, this.Rotation, this.IsHovered ? this.HoverColor : this.Color);
        }
        else {
            Rectangle rec = new Rectangle(this.Position.X + (this.Size.X / 2), this.Position.Y + (this.Size.Y / 2), this.Size.X, this.Size.Y);
            Vector2 origin = new Vector2(rec.width / 2, rec.height / 2);
            ShapeHelper.DrawRectangle(rec, origin, this.Rotation, this.IsHovered ? this.HoverColor : this.Color);
        }
        
        Vector2 textPos = new Vector2(this.Position.X + this.Size.X / 2 - this.TextSize.X / 2F, this.Position.Y + this.Size.Y / 2 - this.TextSize.Y / 2F);
        FontHelper.DrawText(this.Font, this.Text, textPos, Vector2.Zero, this.TextRotation, this.FontSize, this.Spacing, this.IsHovered ? this.TextHoverColor : this.TextColor);
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