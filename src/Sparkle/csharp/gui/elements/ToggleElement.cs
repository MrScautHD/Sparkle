using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.elements.data;

namespace Sparkle.csharp.gui.elements; 

public class ToggleElement : GuiElement {
    
    public Texture2D Texture;
    public Texture2D ToggledTexture;
    public Color Color;
    public Color HoverColor;
    public Color ToggledColor;
    
    public Font Font;
    public float TextRotation;
    public Vector2 TextSize;
    public Color TextColor;
    public Color TextHoverColor;
    public Color ToggledTextColor;
    
    private string _text;
    private string _toggledText;
    private int _fontSize;
    private int _spacing;
    
    public bool IsToggled { get; private set; }
    
    public ToggleElement(string name, ToggleData toggleData, LabelData labelData, Vector2 position, Vector2 size, Func<bool>? clickClickFunc = null) : base(name, position, size, clickClickFunc) {
        this.Texture = toggleData.Texture;
        this.ToggledTexture = toggleData.ToggledTexture;
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

        if (this.IsClicked) {
            this.IsToggled = !this.IsToggled;
            this.ReloadTextSize();
        }
    }
    
    protected internal override void Draw() {
        Color color = this.IsHovered ? this.HoverColor : (this.IsToggled ? this.ToggledColor : this.Color);
        TextureHelper.Draw(this.IsToggled ? this.ToggledTexture : this.Texture, this.Position, color);
        
        Color textColor = this.IsHovered ? this.TextHoverColor : (this.IsToggled ? this.ToggledTextColor : this.TextColor);
        Vector2 textPos = new Vector2(this.Position.X + this.Size.X / 2F - this.TextSize.X / 2F, this.Position.Y + this.Size.Y / 2F - this.TextSize.Y);
        FontHelper.DrawText(this.Font, this.IsToggled ? this.ToggledText : this.Text, textPos, Vector2.Zero, this.TextRotation, this.FontSize, this.Spacing, textColor);
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