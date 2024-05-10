using System.Drawing;
using System.Numerics;
using Raylib_CSharp.Fonts;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using Raylib_CSharp.Windowing;
using Sparkle.CSharp.GUI.Elements.Data;
using Color = Raylib_CSharp.Colors.Color;

namespace Sparkle.CSharp.GUI.Elements;

public class ToggleElement : GuiElement {
    
    public Texture2D? Texture;
    public Texture2D? ToggledTexture;
    public float Rotation;
    public Color Color;
    public Color HoverColor;
    public Color ToggledColor;
    
    public Font Font;
    public string Text;
    public string ToggledText;
    public float FontSize;
    public Vector2 TextSize;
    public Vector2 ScaledTextSize;
    public int Spacing;
    public float TextRotation;
    public Color TextColor;
    public Color TextHoverColor;
    public Color ToggledTextColor;
    
    protected float CalcFontSize { get; private set; }
    
    public bool IsToggled { get; private set; }

    /// <summary>
    /// Initializes a new toggle element with the specified parameters and associated toggle and label data.
    /// </summary>
    /// <param name="name">The name of the toggle element.</param>
    /// <param name="toggleData">Toggle-specific data including textures, rotation, and colors.</param>
    /// <param name="labelData">Label-specific data for text display on the toggle.</param>
    /// <param name="anchor">The anchor point for positioning the element.</param>
    /// <param name="offset">The offset for fine-tuning the position.</param>
    /// <param name="size">An optional size for the toggle element; if not provided, it's determined by the texture.</param>
    /// <param name="clickClickFunc">An optional function to handle click events.</param>
    public ToggleElement(string name, ToggleData toggleData, LabelData labelData, Anchor anchor, Vector2 offset, Vector2? size, Func<bool>? clickClickFunc = null) : base(name, anchor, offset, Vector2.Zero, clickClickFunc) {
        this.Texture = toggleData.Texture;
        this.ToggledTexture = toggleData.ToggledTexture;
        this.Size = size ?? (this.Texture != null ? new Vector2(this.Texture.Value.Width, this.Texture.Value.Height) : Vector2.Zero);
        this.Rotation = toggleData.Rotation;
        this.Color = toggleData.Color;
        this.HoverColor = toggleData.HoverColor;
        this.ToggledColor = toggleData.ToggledColor;
        
        this.Font = labelData.Font;
        this.Text = labelData.Text;
        this.ToggledText = toggleData.ToggledText;
        this.FontSize = labelData.FontSize;
        this.TextSize = Vector2.Zero;
        this.ScaledTextSize = Vector2.Zero;
        this.Spacing = labelData.Spacing;
        this.TextRotation = labelData.Rotation;
        this.TextColor = labelData.Color;
        this.TextHoverColor = labelData.HoverColor;
        this.ToggledTextColor = toggleData.ToggledTextColor;
    }

    protected internal override void Update() {
        base.Update();
        
        if (this.IsClicked) {
            this.IsToggled = !this.IsToggled;
        }
    }

    protected override void CalculateSize() {
        base.CalculateSize();
        
        float scale = Window.GetRenderHeight() / (float) Game.Instance.Settings.Height;
        this.CalcFontSize = this.FontSize * scale * GuiManager.Scale;
        
        string text = this.IsToggled ? this.ToggledText : this.Text;
        this.TextSize = TextManager.MeasureTextEx(this.Font, text, this.FontSize, this.Spacing);
        this.ScaledTextSize = TextManager.MeasureTextEx(this.Font, text, this.CalcFontSize, this.Spacing);
    }

    protected internal override void Draw() {
        RectangleF dest = new RectangleF(this.Position.X + (this.ScaledSize.X / 2), this.Position.Y + (this.ScaledSize.Y / 2), this.ScaledSize.X, this.ScaledSize.Y);
        Vector2 origin = new Vector2(dest.Width / 2, dest.Height / 2);
        
        Texture2D? texture = this.IsToggled ? this.ToggledTexture : this.Texture;
        string text = this.IsToggled ? this.ToggledText : this.Text;
        Color color = this.IsHovered ? this.HoverColor : (this.IsToggled ? this.ToggledColor : this.Color);
        Color textColor = this.IsHovered ? this.TextHoverColor : (this.IsToggled ? this.ToggledTextColor : this.TextColor);
        
        if (texture != null) {
            Rectangle source = new Rectangle(0, 0, texture.Value.Width, texture.Value.Height);
            this.DrawTexture(texture.Value, source, dest, origin, this.Rotation, color);
        }
        else {
            this.DrawRectangle(dest, origin, this.Rotation, color);
        }

        if (text != string.Empty) {
            this.DrawText(this.Font, text, this.TextRotation, this.CalcFontSize, this.Spacing, textColor);
        }
    }
    
    /// <summary>
    /// Draws a button with a textured background on the GUI.
    /// </summary>
    protected virtual void DrawTexture(Texture2D texture, RectangleF source, RectangleF dest, Vector2 origin, float rotation, Color color) {
        Graphics.DrawTexturePro(texture, source, dest, origin, rotation, color);
    }

    /// <summary>
    /// Draws a color button on the screen.
    /// </summary>
    protected virtual void DrawRectangle(RectangleF dest, Vector2 origin, float rotation, Color color) {
        Graphics.DrawRectanglePro(dest, origin, rotation, color);

        RectangleF rec = new RectangleF(dest.X - (dest.Width / 2), dest.Y - (dest.Height / 2), dest.Width, dest.Height);
        Graphics.DrawRectangleLinesEx(rec, 4, Color.Brightness(color, -0.5F));
    }

    /// <summary>
    /// Draws the text of the button element.
    /// </summary>
    protected virtual void DrawText(Font font, string text, float rotation, float fontSize, int spacing, Color color) {
        Vector2 pos = new Vector2(this.Position.X + (this.ScaledSize.X / 2), this.Position.Y + (this.ScaledSize.Y / 2));
        Vector2 origin = new Vector2(this.ScaledTextSize.X / 2, this.ScaledTextSize.Y / 2);
        Graphics.DrawTextPro(font, text, pos, origin, rotation, fontSize, spacing, color);
    }
}