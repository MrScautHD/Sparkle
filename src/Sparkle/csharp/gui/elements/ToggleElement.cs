using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.elements.data;

namespace Sparkle.csharp.gui.elements; 

public class ToggleElement : GuiElement {
    
    public Texture2D? Texture;
    public Texture2D? ToggledTexture;
    public float Rotation;
    public Color Color;
    public Color HoverColor;
    public Color ToggledColor;
    
    public Font Font;
    public float TextRotation;
    public Vector2 TextSize;
    public Vector2 CalcTextSize;
    public Color TextColor;
    public Color TextHoverColor;
    public Color ToggledTextColor;
    
    private string _text;
    private string _toggledText;
    private int _fontSize;
    private int _spacing;
    
    public bool IsToggled { get; private set; }
    
    public ToggleElement(string name, ToggleData toggleData, LabelData labelData, Vector2 position, Vector2? size, Func<bool>? clickClickFunc = null) : base(name, position, Vector2.Zero, clickClickFunc) {
        this.Texture = toggleData.Texture;
        this.ToggledTexture = toggleData.ToggledTexture;
        this.Size = size ?? (this.Texture != null ? new Vector2(this.Texture.Value.width, this.Texture.Value.height) : Vector2.Zero);
        this.Rotation = toggleData.Rotation;
        this.Color = toggleData.Color;
        this.HoverColor = toggleData.HoverColor;
        this.ToggledColor = toggleData.ToggledColor;
        
        this.Font = labelData.Font;
        this.TextRotation = labelData.Rotation;
        this.TextColor = labelData.Color;
        this.TextHoverColor = labelData.HoverColor;
        this.ToggledTextColor = toggleData.ToggledTextColor;
        
        this._text = labelData.Text;
        this._toggledText = toggleData.ToggledText;
        this._fontSize = labelData.FontSize;
        this._spacing = labelData.Spacing;
        this.ReloadTextSize();
    }

    protected internal override void Update() {
        base.Update();
        this.CalcTextSize = new Vector2(this.TextSize.X * this.WidthScale, this.TextSize.Y * this.HeightScale);
        
        if (this.IsClicked) {
            this.IsToggled = !this.IsToggled;
            this.ReloadTextSize();
        }
    }
    
    protected internal override void Draw() {
        Texture2D? texture = this.IsToggled ? this.ToggledTexture : this.Texture;
        
        if (texture != null) {
            Rectangle source = new Rectangle(0, 0, texture.Value.width, texture.Value.height);
            Rectangle dest = new Rectangle(this.CalcPos.X + (this.CalcSize.X / 2), this.CalcPos.Y + (this.CalcSize.Y / 2), this.CalcSize.X, this.CalcSize.Y);
            Vector2 origin = new Vector2(dest.width / 2, dest.height / 2);
            Color color = this.IsHovered ? this.HoverColor : (this.IsToggled ? this.ToggledColor : this.Color);
            TextureHelper.DrawPro(texture.Value, source, dest, origin, this.Rotation, color);
        }
        else {
            Rectangle rec = new Rectangle(this.CalcPos.X + (this.CalcSize.X / 2), this.CalcPos.Y + (this.CalcSize.Y / 2), this.CalcSize.X, this.CalcSize.Y);
            Vector2 origin = new Vector2(rec.width / 2, rec.height / 2);
            Color color = this.IsHovered ? this.HoverColor : (this.IsToggled ? this.ToggledColor : this.Color);
            ShapeHelper.DrawRectangle(rec, origin, this.Rotation, color);
        }
        
        Vector2 textPos = new Vector2(this.CalcPos.X + this.CalcSize.X / 2, this.CalcPos.Y + this.CalcSize.Y / 2);
        Vector2 textOrigin = new Vector2(this.TextSize.X / 2, this.TextSize.Y / 2);
        Color textColor = this.IsHovered ? this.TextHoverColor : (this.IsToggled ? this.ToggledTextColor : this.TextColor);
        FontHelper.DrawText(this.Font, this.IsToggled ? this.ToggledText : this.Text, textPos, textOrigin, this.TextRotation, this.FontSize, this.Spacing, textColor);
    }
    
    public string Text {
        get => this._text;
        set {
            this._text = value;
            this.ReloadTextSize();
        }
    }
    
    public string ToggledText {
        get => this._toggledText;
        set {
            this._toggledText = value;
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
        this.TextSize = FontHelper.MeasureText(this.Font, this.IsToggled ? this.ToggledText : this.Text, this.FontSize, this.Spacing);
    }
}