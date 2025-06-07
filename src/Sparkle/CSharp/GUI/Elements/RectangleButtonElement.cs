using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

public class RectangleButtonElement : GuiElement {
    
    public RectangleButtonData ButtonData { get; private set; }
    
    public LabelData LabelData { get; private set; }
    
    public RectangleButtonElement(RectangleButtonData buttonData, LabelData labelData, Anchor anchor, Vector2 offset, Vector2 size, Vector2? origin = null, float rotation = 0.0F, Func<bool>? clickFunc = null) : base(anchor, offset, size, origin, rotation, clickFunc) {
        this.ButtonData = buttonData;
        this.LabelData = labelData;
    }
    
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        // Draw texture.
        Color buttonColor = this.IsHovered ? this.ButtonData.HoverColor : this.ButtonData.Color;
        
        // Draw filled rectangle.
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(this.Position.X, this.Position.Y, this.ScaledSize.X, this.ScaledSize.Y), this.Origin, this.Rotation, 0.5F, buttonColor);
        
        // Draw outline.
        //context.PrimitiveBatch.DrawEmptyRectangle();
        
        context.PrimitiveBatch.End();
        
        // Draw text.
        if (this.LabelData.Text != string.Empty) {
            Vector2 textSize = this.LabelData.Font.MeasureText(this.LabelData.Text, this.LabelData.Size, this.LabelData.Scale, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Effect, this.LabelData.EffectAmount);
            Vector2 textOrigin = textSize / 2.0F - (this.Size / 2.0F - this.Origin);
            Vector2 textPos = this.Position;
            Color textColor = this.IsHovered ? this.LabelData.HoverColor : this.LabelData.Color;
            
            context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
            context.SpriteBatch.DrawText(this.LabelData.Font, this.LabelData.Text, textPos, this.LabelData.Size, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Scale * GuiManager.ScaleFactor, 0.5F, textOrigin, this.Rotation, textColor, this.LabelData.Style, this.LabelData.Effect, this.LabelData.EffectAmount);
            context.SpriteBatch.End();
        }
    }
}