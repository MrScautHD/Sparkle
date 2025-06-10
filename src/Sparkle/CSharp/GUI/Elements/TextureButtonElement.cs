using System.Numerics;
using Bliss.CSharp.Colors;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

public class TextureButtonElement : GuiElement {
    
    /// <summary>
    /// The associated data for a texture-based button in the GUI.
    /// </summary>
    public TextureButtonData ButtonData { get; private set; }
    
    /// <summary>
    /// The data and properties necessary for rendering and handling text on a GUI element.
    /// </summary>
    public LabelData LabelData { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureButtonElement"/> class.
    /// </summary>
    /// <param name="buttonData">The texture and visual configuration for the button.</param>
    /// <param name="labelData">The label configuration to be drawn over the button.</param>
    /// <param name="anchor">The anchor point determining the element's relative position.</param>
    /// <param name="offset">The offset from the anchor point.</param>
    /// <param name="size">Optional override for the size. If not provided, defaults to the texture size.</param>
    /// <param name="origin">Optional origin point for transformations like rotation and scaling.</param>
    /// <param name="rotation">Optional rotation angle in radians.</param>
    /// <param name="clickFunc">Optional function to execute when the button is clicked. Should return true if handled.</param>
    public TextureButtonElement(TextureButtonData buttonData, LabelData labelData, Anchor anchor, Vector2 offset, Vector2? size = null, Vector2? origin = null, float rotation = 0.0F, Func<bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, origin, rotation, clickFunc) {
        this.ButtonData = buttonData;
        this.LabelData = labelData;
        this.Size = size ?? new Vector2(buttonData.Texture.Width, buttonData.Texture.Height);
    }
    
    /// <summary>
    /// Draws the texture button element and its label onto the specified framebuffer.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer where the element will be drawn.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        // Draw texture.
        Color buttonColor = this.IsHovered ? this.ButtonData.HoverColor : this.ButtonData.Color;
        context.SpriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, this.ButtonData.SourceRect, this.ButtonData.Scale * this.Gui.ScaleFactor, this.Origin, this.Rotation, buttonColor, this.ButtonData.Flip);
        
        // Draw text.
        if (this.LabelData.Text != string.Empty) {
            Vector2 textSize = this.LabelData.Font.MeasureText(this.LabelData.Text, this.LabelData.Size, this.LabelData.Scale, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Effect, this.LabelData.EffectAmount);
            Vector2 textOrigin = textSize / 2.0F - (this.Size / 2.0F - this.Origin);
            Vector2 textPos = this.Position;
            
            Color textColor = this.IsHovered ? this.LabelData.HoverColor : this.LabelData.Color;
            context.SpriteBatch.DrawText(this.LabelData.Font, this.LabelData.Text, textPos, this.LabelData.Size, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Scale * this.Gui.ScaleFactor, 0.5F, textOrigin, this.Rotation, textColor, this.LabelData.Style, this.LabelData.Effect, this.LabelData.EffectAmount);
        }
        
        context.SpriteBatch.End();
    }
}