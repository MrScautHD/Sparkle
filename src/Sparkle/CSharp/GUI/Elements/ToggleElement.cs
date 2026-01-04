using System.Numerics;
using Bliss.CSharp.Colors;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

public class ToggleElement : GuiElement {
    
    /// <summary>
    /// Holds the configuration for the texture-based toggle element.
    /// </summary>
    public ToggleData ToggleData { get; private set; }
    
    /// <summary>
    /// Holds the configuration for the label text displayed within the toggle.
    /// </summary>
    public LabelData LabelData { get; private set; }
    
    /// <summary>
    /// The spacing between the toggle box and the label text.
    /// </summary>
    public float BoxTextSpacing { get; private set; }
    
    /// <summary>
    /// Indicates whether the toggle is interactable.
    /// </summary>
    public bool Interactable;
    
    /// <summary>
    /// Indicates whether the toggle is currently in the 'off' or 'on' state.
    /// </summary>
    public bool ToggleState;
    
    /// <summary>
    /// Event invoked when the toggle state changes.
    /// </summary>
    public event Action<bool>? Toggled;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleElement"/> class.
    /// </summary>
    /// <param name="toggleData">The toggle rendering data.</param>
    /// <param name="labelData">The label rendering data.</param>
    /// <param name="anchor">Anchor position of the element.</param>
    /// <param name="offset">Offset from the anchor.</param>
    /// <param name="boxTextSpacing">Spacing between the toggle box and the label text.</param>
    /// <param name="interactable">Indicates whether the toggle is interactable.</param>
    /// <param name="toggleState">Initial toggle state.</param>
    /// <param name="size">Optional custom size.</param>
    /// <param name="origin">Optional custom origin.</param>
    /// <param name="rotation">Optional rotation angle.</param>
    /// <param name="clickFunc">Optional function determining click behavior.</param>
    public ToggleElement(ToggleData toggleData, LabelData labelData, Anchor anchor, Vector2 offset, float boxTextSpacing, bool interactable = true, bool toggleState = false, Vector2? size = null, Vector2? origin = null, float rotation = 0.0F, Func<bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, origin, rotation, clickFunc) {
        this.ToggleData = toggleData;
        this.LabelData = labelData;
        this.BoxTextSpacing = boxTextSpacing;
        this.Size = size ?? this.CalculateDefaultSize(toggleData, labelData, boxTextSpacing);
        this.Interactable = interactable;
        this.ToggleState = toggleState;
    }
    
    /// <summary>
    /// Updates the toggle state each frame.
    /// </summary>
    /// <param name="delta">The elapsed time since the last frame.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);

        if (this.Interactable) {
            if (this.IsClicked) {
                this.ToggleState = !this.ToggleState;
                this.Toggled?.Invoke(this.ToggleState);
            }
        }
    }
    
    /// <summary>
    /// Draws the GUI element on the specified framebuffer using the provided graphics context.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    /// <param name="framebuffer">The framebuffer to which the element is rendered.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        // Draw background texture.
        Color backgroundColor = this.IsHovered ? this.ToggleData.BackgroundHoverColor : this.ToggleData.BackgroundColor;
        
        if (!this.Interactable) {
            backgroundColor = this.ToggleData.OffStateColor;
        }
        
        if (this.ToggleData.BackgroundSampler != null) context.SpriteBatch.PushSampler(this.ToggleData.BackgroundSampler);
        context.SpriteBatch.DrawTexture(this.ToggleData.BackgroundTexture, this.Position, 0.5F, this.ToggleData.BackgroundSourceRect, this.ToggleData.BackgroundScale * this.Gui.ScaleFactor, this.Origin, this.Rotation, backgroundColor, this.ToggleData.BackgroundFlip);
        if (this.ToggleData.BackgroundSampler != null) context.SpriteBatch.PopSampler();
        
        // Draw checkmark texture.
        if (this.ToggleState) {
            Color checkmarkColor = this.IsHovered ? this.ToggleData.CheckmarkHoverColor : this.ToggleData.CheckmarkColor;
            
            if (!this.Interactable) {
                checkmarkColor = this.ToggleData.OffStateColor;
            }
            
            if (this.ToggleData.CheckmarkSampler != null) context.SpriteBatch.PushSampler(this.ToggleData.CheckmarkSampler);
            context.SpriteBatch.DrawTexture(this.ToggleData.CheckmarkTexture, this.Position, 0.5F, this.ToggleData.CheckmarkSourceRect, this.ToggleData.CheckmarkScale * this.Gui.ScaleFactor, this.Origin, this.Rotation, checkmarkColor, this.ToggleData.CheckmarkFlip);
            if (this.ToggleData.CheckmarkSampler != null) context.SpriteBatch.PopSampler();
        }
        
        // Draw text.
        if (this.LabelData.Text != string.Empty) {
            Vector2 textPos = this.Position;
            Vector2 textSize = this.LabelData.Font.MeasureText(this.LabelData.Text, this.LabelData.Size, this.LabelData.Scale, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Effect, this.LabelData.EffectAmount);
            Vector2 textOrigin = new Vector2(textSize.X, this.LabelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin) - new Vector2(this.ToggleData.BackgroundSourceRect.Width * this.ToggleData.BackgroundScale.X + this.BoxTextSpacing, 0.0F) / 2.0F;
            
            Color textColor = this.IsHovered ? this.LabelData.HoverColor : this.LabelData.Color;
            
            if (this.LabelData.Sampler != null) context.SpriteBatch.PushSampler(this.LabelData.Sampler);
            context.SpriteBatch.DrawText(this.LabelData.Font, this.LabelData.Text, textPos, this.LabelData.Size, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Scale * this.Gui.ScaleFactor, 0.5F, textOrigin, this.Rotation, textColor, this.LabelData.Style, this.LabelData.Effect, this.LabelData.EffectAmount);
            if (this.LabelData.Sampler != null) context.SpriteBatch.PopSampler();
        }
        
        context.SpriteBatch.End();
    }
    
    /// <summary>
    /// Calculates the default size of the toggle element.
    /// </summary>
    /// <param name="toggleData">The data containing properties of the toggle background and checkmark.</param>
    /// <param name="labelData">The data containing properties of the label associated with the toggle element.</param>
    /// <param name="boxTextSpacing">The spacing between the toggle box and the label text.</param>
    /// <returns>A vector representing the calculated default size of the toggle element.</returns>
    private Vector2 CalculateDefaultSize(ToggleData toggleData, LabelData labelData, float boxTextSpacing) {
        Vector2 toggleSize = new Vector2(toggleData.BackgroundSourceRect.Width * toggleData.BackgroundScale.X, toggleData.BackgroundSourceRect.Height * toggleData.BackgroundScale.Y);
        Vector2 labelSize = labelData.Font.MeasureText(labelData.Text, labelData.Size, labelData.Scale, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
        
        return new Vector2(toggleSize.X + labelSize.X + boxTextSpacing, MathF.Max(toggleSize.Y, labelSize.Y));
    }
}