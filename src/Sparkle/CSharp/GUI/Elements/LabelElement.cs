using System.Numerics;
using Bliss.CSharp.Colors;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

public class LabelElement : GuiElement {
    
    /// <summary>
    /// The data used to render the label.
    /// </summary>
    public LabelData Data { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LabelElement"/> class.
    /// </summary>
    /// <param name="data">The label data containing text and rendering settings.</param>
    /// <param name="anchor">The anchor point that determines the label’s alignment.</param>
    /// <param name="offset">The offset from the anchor point.</param>
    /// <param name="scale">Optional scaling factor for the element. Defaults to (1, 1).</param>
    /// <param name="origin">Optional origin point used for rotation and scaling (default is null).</param>
    /// <param name="rotation">The rotation of the label in radians (default is 0).</param>
    /// <param name="clickFunc">Optional function to be called when the label is clicked (default is null).</param>
    public LabelElement(LabelData data, Anchor anchor, Vector2 offset, Vector2? scale = null, Vector2? origin = null, float rotation = 0.0F, Func<GuiElement, bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, clickFunc) {
        this.Data = data;
    }
    
    /// <summary>
    /// Updates the state of the LabelElement during each frame with the given time delta.
    /// </summary>
    /// <param name="delta">The time elapsed between the current and the previous frame, in seconds.</param>
    /// <param name="interactionHandled">A reference to a boolean tracking whether interaction has already been handled by another element.</param>
    protected internal override void Update(double delta, ref bool interactionHandled) {
        this.Size = this.Data.Font.MeasureText(this.Data.Text, this.Data.Size, Vector2.One, this.Data.CharacterSpacing, this.Data.LineSpacing, this.Data.Effect, this.Data.EffectAmount);
        base.Update(delta, ref interactionHandled);
    }
    
    /// <summary>
    /// Draws the LabelElement on the specified framebuffer using the provided graphics context.
    /// </summary>
    /// <param name="context">The graphics context used for rendering the LabelElement.</param>
    /// <param name="framebuffer">The framebuffer in which the LabelElement will be drawn.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        if (this.Data.Text == string.Empty) {
            return;
        }

        Color color = this.IsHovered ? this.Data.HoverColor : this.Data.Color;
        
        if (!this.Interactable) {
            color = this.Data.DisabledColor;
        }
        
        // Draw text.
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription, this.Data.Sampler);
        context.SpriteBatch.DrawText(this.Data.Font, this.Data.Text, this.Position, this.Data.Size, this.Data.CharacterSpacing, this.Data.LineSpacing, this.Scale * this.Gui.ScaleFactor, 0.5F, this.Origin, this.Rotation, color, this.Data.Style, this.Data.Effect, this.Data.EffectAmount);
        context.SpriteBatch.End();
    }
}