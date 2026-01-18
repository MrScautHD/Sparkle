using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

public class RectangleButtonElement : GuiElement {
    
    /// <summary>
    /// Gets the button's visual configuration, including fill color, hover color, and outline properties.
    /// </summary>
    public RectangleButtonData ButtonData { get; private set; }
    
    /// <summary>
    /// Gets the label data used to render text over the button.
    /// </summary>
    public LabelData LabelData { get; private set; }
    
    /// <summary>
    /// The alignment of text within a GUI element.
    /// </summary>
    public TextAlignment TextAlignment;

    /// <summary>
    /// The offset of the text relative to its position.
    /// </summary>
    public Vector2 TextOffset;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleButtonElement"/> class.
    /// </summary>
    /// <param name="buttonData">The visual configuration of the rectangle button.</param>
    /// <param name="labelData">The text label data displayed on the button.</param>
    /// <param name="anchor">The anchor point for positioning the element.</param>
    /// <param name="offset">The offset from the anchor position.</param>
    /// <param name="size">The size of the button.</param>
    /// <param name="scale">The scale applied to the button.</param>
    /// <param name="textAlignment">The alignment of text within a GUI element.</param>
    /// <param name="textOffset">The offset of the text relative to its position.</param>
    /// <param name="origin">The origin point for rotation and alignment. Defaults to (0, 0).</param>
    /// <param name="rotation">The rotation of the button in radians. Defaults to 0.</param>
    /// <param name="clickFunc">Optional function to invoke when the button is clicked. Returns true if handled.</param>
    public RectangleButtonElement(RectangleButtonData buttonData, LabelData labelData, Anchor anchor, Vector2 offset, Vector2 size, Vector2? scale = null, TextAlignment textAlignment = TextAlignment.Center, Vector2? textOffset = null, Vector2? origin = null, float rotation = 0.0F, Func<bool>? clickFunc = null) : base(anchor, offset, size, scale, origin, rotation, clickFunc) {
        this.ButtonData = buttonData;
        this.LabelData = labelData;
        this.TextAlignment = textAlignment;
        this.TextOffset = textOffset ?? Vector2.Zero;
    }
    
    /// <summary>
    /// Renders the rectangle button element on the given framebuffer using the provided graphics context.
    /// </summary>
    /// <param name="context">The graphics context used for rendering primitives and text.</param>
    /// <param name="framebuffer">The framebuffer target for rendering.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        // Draw a filled rectangle.
        Color buttonColor = this.IsHovered ? this.ButtonData.HoverColor : this.ButtonData.Color;
        
        if (!this.Interactable) {
            buttonColor = this.ButtonData.DisabledColor;
        }
        
        context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(this.Position.X, this.Position.Y, this.ScaledSize.X, this.ScaledSize.Y), this.Origin * this.Scale * this.Gui.ScaleFactor, this.Rotation, 0.5F, buttonColor);
        
        // Draw an empty rectangle.
        if (this.ButtonData.OutlineThickness > 0.0F) {
            Color outlineColor = this.IsHovered ? this.ButtonData.OutlineHoverColor : this.ButtonData.OutlineColor;
            
            if (!this.Interactable) {
                outlineColor = this.ButtonData.DisabledOutlineColor;
            }
            
            float outlineThickness = this.ButtonData.OutlineThickness * this.Gui.ScaleFactor;
            context.PrimitiveBatch.DrawEmptyRectangle(new RectangleF(this.Position.X, this.Position.Y, this.ScaledSize.X, this.ScaledSize.Y), outlineThickness, this.Origin * this.Scale * this.Gui.ScaleFactor, this.Rotation, 0.5F, outlineColor);
        }
        
        context.PrimitiveBatch.End();
        
        // Draw text.
        if (this.LabelData.Text != string.Empty) {
            Vector2 textPos = this.Position + (this.TextOffset * this.Scale * this.Gui.ScaleFactor);
            Vector2 textSize = this.LabelData.Font.MeasureText(this.LabelData.Text, this.LabelData.Size, Vector2.One, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Effect, this.LabelData.EffectAmount);
            Vector2 textOrigin = this.TextAlignment switch {
                TextAlignment.Left => new Vector2(this.Size.X, this.LabelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin),
                TextAlignment.Right => new Vector2(-this.Size.X / 2.0F + (textSize.X - 2.0F), this.LabelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin),
                TextAlignment.Center => new Vector2(textSize.X, this.LabelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin),
                _ => Vector2.Zero
            };
            
            Color textColor = this.IsHovered ? this.LabelData.HoverColor : this.LabelData.Color;
            context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription, this.LabelData.Sampler);
            context.SpriteBatch.DrawText(this.LabelData.Font, this.LabelData.Text, textPos, this.LabelData.Size, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.Scale * this.Gui.ScaleFactor, 0.5F, textOrigin, this.Rotation, textColor, this.LabelData.Style, this.LabelData.Effect, this.LabelData.EffectAmount);
            context.SpriteBatch.End();
        }
    }
}