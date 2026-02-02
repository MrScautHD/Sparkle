using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
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
    /// <param name="toggleState">Initial toggle state.</param>
    /// <param name="scale">The scale applied to the toggle element.</param>
    /// <param name="origin">Optional custom origin.</param>
    /// <param name="rotation">Optional rotation angle.</param>
    /// <param name="clickFunc">Optional function determining click behavior.</param>
    public ToggleElement(ToggleData toggleData, LabelData labelData, Anchor anchor, Vector2 offset, float boxTextSpacing, bool toggleState = false, Vector2? scale = null, Vector2? origin = null, float rotation = 0.0F, Func<GuiElement, bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, clickFunc) {
        this.ToggleData = toggleData;
        this.LabelData = labelData;
        this.BoxTextSpacing = boxTextSpacing;
        this.ToggleState = toggleState;
    }
    
    /// <summary>
    /// Updates the toggle state each frame.
    /// </summary>
    /// <param name="delta">The elapsed time since the last frame.</param>
    /// <param name="interactionHandled">A reference to a boolean tracking whether interaction has already been handled by another element.</param>
    protected internal override void Update(double delta, ref bool interactionHandled) {
        this.Size = this.CalculateSize(this.ToggleData, this.LabelData, this.BoxTextSpacing);
        base.Update(delta, ref interactionHandled);
        
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
        
        // Draw checkbox texture.
        Color checkboxColor = this.IsHovered ? this.ToggleData.CheckboxHoverColor : this.ToggleData.CheckboxColor;
        
        if (!this.Interactable) {
            checkboxColor = this.ToggleData.DisabledColor;
        }
        
        this.DrawCheckbox(context.SpriteBatch, this.ToggleData.CheckboxTexture, this.ToggleData.CheckboxSampler, this.ToggleData.CheckboxSourceRect, checkboxColor, this.ToggleData.CheckboxFlip);
        
        // Draw checkmark texture.
        if (this.ToggleState) {
            Color checkmarkColor = this.IsHovered ? this.ToggleData.CheckmarkHoverColor : this.ToggleData.CheckmarkColor;
            
            if (!this.Interactable) {
                checkmarkColor = this.ToggleData.DisabledColor;
            }
            
            this.DrawCheckmark(context.SpriteBatch, this.ToggleData.CheckmarkTexture, this.ToggleData.CheckmarkSampler, this.ToggleData.CheckmarkSourceRect, checkmarkColor, this.ToggleData.CheckmarkFlip);
        }
        
        // Draw text.
        this.DrawText(context.SpriteBatch);
        
        context.SpriteBatch.End();
    }
    
    /// <summary>
    /// Draws the checkbox portion of the toggle element on the screen.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used for rendering textures.</param>
    /// <param name="texture">The texture of the checkbox to be drawn.</param>
    /// <param name="sampler">The optional sampler to apply for texture sampling.</param>
    /// <param name="sourceRect">The source rectangle defining the region of the texture to draw.</param>
    /// <param name="color">The color modulator applied to the checkbox.</param>
    /// <param name="flip">The flip mode applied to the texture.</param>
    private void DrawCheckbox(SpriteBatch spriteBatch, Texture2D texture, Sampler? sampler, Rectangle sourceRect, Color color, SpriteFlip flip) {
        if (sampler != null) spriteBatch.PushSampler(sampler);
        spriteBatch.DrawTexture(texture, this.Position, 0.5F, sourceRect, this.Scale * this.Gui.ScaleFactor, this.Origin, this.Rotation, color, flip);
        if (sampler != null) spriteBatch.PopSampler();
    }
    
    /// <summary>
    /// Draws the checkmark texture for the toggle element.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used for rendering textures.</param>
    /// <param name="texture">The texture of the checkmark to be drawn.</param>
    /// <param name="sampler">An optional sampler to control texture sampling behavior.</param>
    /// <param name="sourceRect">The source rectangle defining the portion of the texture to draw.</param>
    /// <param name="color">The color to apply to the checkmark texture.</param>
    /// <param name="flip">Specifies the flipping behavior for the checkmark texture.</param>
    private void DrawCheckmark(SpriteBatch spriteBatch, Texture2D texture, Sampler? sampler, Rectangle sourceRect, Color color, SpriteFlip flip) {
        Vector2 origin = new Vector2(sourceRect.Width, sourceRect.Height) / 2.0F - (new Vector2(this.ToggleData.CheckboxSourceRect.Width, this.ToggleData.CheckboxSourceRect.Height) / 2.0F - this.Origin);
        
        if (sampler != null) spriteBatch.PushSampler(sampler);
        spriteBatch.DrawTexture(texture, this.Position, 0.5F, sourceRect, this.Scale * this.Gui.ScaleFactor, origin, this.Rotation, color, flip);
        if (sampler != null) spriteBatch.PopSampler();
    }
    
    /// <summary>
    /// Renders the text associated with the current GUI element.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used for rendering text.</param>
    private void DrawText(SpriteBatch spriteBatch) {
        if (this.LabelData.Text == string.Empty) {
            return;
        }
        
        Vector2 textPos = this.Position;
        Vector2 textSize = this.LabelData.Font.MeasureText(this.LabelData.Text, this.LabelData.Size, Vector2.One, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.LabelData.Effect, this.LabelData.EffectAmount);
        Vector2 textOrigin = new Vector2(textSize.X, this.LabelData.Size) / 2.0F - (this.Size / 2.0F - this.Origin) - new Vector2(this.ToggleData.CheckboxSourceRect.Width + this.BoxTextSpacing, 0.0F) / 2.0F;
        
        Color color = this.IsHovered ? this.LabelData.HoverColor : this.LabelData.Color;
        
        if (!this.Interactable) {
            color = this.LabelData.DisabledColor;
        }
        
        if (this.LabelData.Sampler != null) spriteBatch.PushSampler(this.LabelData.Sampler);
        spriteBatch.DrawText(this.LabelData.Font, this.LabelData.Text, textPos, this.LabelData.Size, this.LabelData.CharacterSpacing, this.LabelData.LineSpacing, this.Scale * this.Gui.ScaleFactor, 0.5F, textOrigin, this.Rotation, color, this.LabelData.Style, this.LabelData.Effect, this.LabelData.EffectAmount);
        if (this.LabelData.Sampler != null) spriteBatch.PopSampler();
    }
    
    /// <summary>
    /// Calculates the size for the toggle element.
    /// </summary>
    /// <param name="toggleData">The rendering data associated with the toggle box.</param>
    /// <param name="labelData">The rendering data for the label text.</param>
    /// <param name="boxTextSpacing">The spacing between the toggle box and the label text.</param>
    /// <returns>The calculated size of the element as a <see cref="Vector2"/>.</returns>
    private Vector2 CalculateSize(ToggleData toggleData, LabelData labelData, float boxTextSpacing) {
        Vector2 toggleSize = new Vector2(toggleData.CheckboxSourceRect.Width, toggleData.CheckboxSourceRect.Height);
        Vector2 labelSize = labelData.Font.MeasureText(labelData.Text, labelData.Size, Vector2.One, labelData.CharacterSpacing, labelData.LineSpacing, labelData.Effect, labelData.EffectAmount);
        return new Vector2(toggleSize.X + labelSize.X + boxTextSpacing, MathF.Max(toggleSize.Y, labelSize.Y));
    }
}