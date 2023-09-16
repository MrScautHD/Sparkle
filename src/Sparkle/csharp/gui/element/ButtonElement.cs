using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.element.data;

namespace Sparkle.csharp.gui.element; 

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
    
    public string Text;
    public float FontSize;
    public int Spacing;
    
    protected float CalcFontSize { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonElement"/> with the given parameters. Inherits from a base class and sets various properties related to button and label data.
    /// </summary>
    /// <param name="name">The name of the ButtonElement.</param>
    /// <param name="buttonData">Data for initializing button-specific properties like Texture, Rotation, and Colors.</param>
    /// <param name="labelData">Data for initializing label-specific properties like Font, Text, and Colors.</param>
    /// <param name="position">Position of the ButtonElement on the screen.</param>
    /// <param name="size">Optional size of the ButtonElement. Will default to the texture size if not provided and a texture exists.</param>
    /// <param name="clickClickFunc">Optional click function to be executed when the button is clicked.</param>
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
        
        this.Text = labelData.Text;
        this.FontSize = labelData.FontSize;
        this.Spacing = labelData.Spacing;
    }

    protected internal override void Update() {
        base.Update();
        
        this.CalcFontSize = this.FontSize * GuiManager.Scale;
        this.TextSize = FontHelper.MeasureText(this.Font, this.Text, this.CalcFontSize, this.Spacing);
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

        if (this.Text != string.Empty) {
            Vector2 textPos = new Vector2(this.CalcPos.X + this.CalcSize.X / 2, this.CalcPos.Y + this.CalcSize.Y / 2);
            Vector2 textOrigin = new Vector2(this.TextSize.X / 2, this.TextSize.Y / 2);
            FontHelper.DrawText(this.Font, this.Text, textPos, textOrigin, this.TextRotation, this.CalcFontSize, this.Spacing, this.IsHovered ? this.TextHoverColor : this.TextColor);
        }
    }
}