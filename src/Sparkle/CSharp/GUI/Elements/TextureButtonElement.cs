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
    /// The alignment of text within a GUI element.
    /// </summary>
    public TextAlignment TextAlignment;
    
    /// <summary>
    /// The offset of the text relative to its position.
    /// </summary>
    public Vector2 TextOffset;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureButtonElement"/> class.
    /// </summary>
    /// <param name="buttonData">The texture and visual configuration for the button.</param>
    /// <param name="labelData">The label configuration to be drawn over the button.</param>
    /// <param name="anchor">The anchor point determining the element's relative position.</param>
    /// <param name="offset">The offset from the anchor point.</param>
    /// <param name="textAlignment">The alignment of text within a GUI element.</param>
    /// <param name="textOffset">The offset of the text relative to its position.</param>
    /// <param name="size">Optional override for the size. If not provided, defaults to the texture size.</param>
    /// <param name="origin">Optional origin point for transformations like rotation and scaling.</param>
    /// <param name="rotation">Optional rotation angle in radians.</param>
    /// <param name="clickFunc">Optional function to execute when the button is clicked. Should return true if handled.</param>
    public TextureButtonElement(TextureButtonData buttonData, LabelData labelData, Anchor anchor, Vector2 offset, TextAlignment textAlignment = TextAlignment.Center, Vector2? textOffset = null, Vector2? size = null, Vector2? origin = null, float rotation = 0.0F, Func<bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, origin, rotation, clickFunc) {
        this.ButtonData = buttonData;
        this.LabelData = labelData;
        this.TextAlignment = textAlignment;
        this.TextOffset = textOffset ?? Vector2.Zero;
        this.Size = size ?? new Vector2(buttonData.SourceRect.Width * buttonData.Scale.X, buttonData.SourceRect.Height * buttonData.Scale.Y);
    }
    
    /// <summary>
    /// Draws the texture button element and its label onto the specified framebuffer.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer where the element will be drawn.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        // Draw button texture.
        Color buttonColor = this.IsHovered ? this.ButtonData.HoverColor : this.ButtonData.Color;
        
        if (this.ButtonData.Sampler != null) context.SpriteBatch.PushSampler(this.ButtonData.Sampler);
        context.SpriteBatch.DrawTexture(this.ButtonData.Texture, this.Position, 0.5F, this.ButtonData.SourceRect, this.ButtonData.Scale * this.Gui.ScaleFactor, this.Origin, this.Rotation, buttonColor, this.ButtonData.Flip);
        if (this.ButtonData.Sampler != null) context.SpriteBatch.PopSampler();
        
        // Draw text.
        if (this.LabelData.Text != string.Empty) {
            Vector2 textPos = this.Position + (this.TextOffset * this.Gui.ScaleFactor);
            Vector2 textSize = this.LabelData.Font.MeasureText(this.LabelData.Text, this.LabelData.Size, this.LabelData.Scale, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Effect, this.LabelData.EffectAmount);
            Vector2 textOrigin = this.TextAlignment switch {
                TextAlignment.Left => new Vector2(this.Size.X, this.LabelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin),
                TextAlignment.Right => new Vector2(-this.Size.X / 2.0F + (textSize.X - 2.0F), this.LabelData.Size / 2.0F) - (this.Size / 2.0F - this.Origin),
                TextAlignment.Center => new Vector2(textSize.X, this.LabelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin),
                _ => Vector2.Zero
            };
            
            Color textColor = this.IsHovered ? this.LabelData.HoverColor : this.LabelData.Color;
            
            if (this.LabelData.Sampler != null) context.SpriteBatch.PushSampler(this.LabelData.Sampler);
            context.SpriteBatch.DrawText(this.LabelData.Font, this.LabelData.Text, textPos, this.LabelData.Size, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Scale * this.Gui.ScaleFactor, 0.5F, textOrigin, this.Rotation, textColor, this.LabelData.Style, this.LabelData.Effect, this.LabelData.EffectAmount);
            if (this.LabelData.Sampler != null) context.SpriteBatch.PopSampler();
        }
        
        context.SpriteBatch.End();
    }
}