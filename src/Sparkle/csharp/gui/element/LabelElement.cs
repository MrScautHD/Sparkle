using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.gui.element.data;

namespace Sparkle.csharp.gui.element; 

public class LabelElement : GuiElement {
    
    public Font Font;
    public float Rotation;
    public Color Color;
    public Color HoverColor;
    
    public string Text;
    public float FontSize;
    public int Spacing;
    
    protected float CalcFontSize { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelElement"/> with the given parameters, inheriting from a base class and setting various properties related to the label data.
    /// </summary>
    /// <param name="name">The name of the LabelElement.</param>
    /// <param name="data">Data for initializing label-specific properties like Font, Rotation, and Colors.</param>
    /// <param name="position">Position of the LabelElement on the screen.</param>
    /// <param name="clickClickFunc">Optional click function to be executed when the label is clicked.</param>
    public LabelElement(string name, LabelData data, Vector2 position, Func<bool>? clickClickFunc = null) : base(name, position, data.Size, clickClickFunc) {
        this.Font = data.Font;
        this.Rotation = data.Rotation;
        this.Color = data.Color;
        this.HoverColor = data.HoverColor;
        
        this.Text = data.Text;
        this.FontSize = data.FontSize;
        this.Spacing = data.Spacing;
    }

    protected internal override void Update() {
        this.CalcFontSize = this.FontSize * GuiManager.Scale;
        this.Size = FontHelper.MeasureText(this.Font, this.Text, this.CalcFontSize, this.Spacing);
        base.Update();
    }

    protected internal override void Draw() {
        if (this.Text != string.Empty) {
            Vector2 textPos = new Vector2(this.CalcPos.X + this.CalcSize.X / 2, this.CalcPos.Y + this.CalcSize.Y / 2);
            Vector2 textOrigin = new Vector2(this.CalcSize.X / 2, this.CalcSize.Y / 2);
            FontHelper.DrawText(this.Font, this.Text, textPos, textOrigin, this.Rotation, this.CalcFontSize, this.Spacing, this.IsHovered ? this.HoverColor : this.Color);
        }
    }
}