using System.Numerics;
using Bliss.CSharp.Colors;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements;

// TODO: Done it.
public class TextureSliderBarElement : GuiElement {
    
    public TextureSliderBarData Data { get; private set; }
    
    
    public float MinValue;
    
    public float MaxValue;
    
    // TODO: Add this everywhere (With the color)!
    public bool Interactable; // TODO: ADD INTERACTABLE MAYBE AS DEFAULT VALUE IN GUIELEMENT if not look for a new param sorting.
    
    public float Value;
    
    public bool WholeNumbers;
    
    public TextureSliderBarElement(TextureSliderBarData data, Anchor anchor, Vector2 offset, float minValue, float maxValue, bool interactable = true, float value = 0.0F, bool wholeNumbers = false, Vector2? size = null, Vector2? scale = null, Vector2? origin = null, float rotation = 0.0F, Func<bool>? clickFunc = null) : base(anchor, offset, Vector2.Zero, scale, origin, rotation, clickFunc) {
        this.Data = data;
        this.Size = size ?? new Vector2(data.EmptyBarSourceRect.Width, data.EmptyBarSourceRect.Height);
        this.MinValue = minValue;
        this.MaxValue = maxValue;
        this.Interactable = interactable;
        this.Value = value;
        this.WholeNumbers = wholeNumbers;
    }

    protected internal override void Update(double delta) {
        base.Update(delta);
        
    }
    
    /// <summary>
    /// Draws the texture button element and its label onto the specified framebuffer.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer where the element will be drawn.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);

        // Draw empty bar texture.
        Color emptyBarColor = this.IsHovered ? this.Data.EmptyBarHoverColor : this.Data.EmptyBarColor;
        
        if (!this.Interactable) {
            emptyBarColor = this.Data.OffStateColor;
        }
        
        if (this.Data.EmptyBarSampler != null) context.SpriteBatch.PushSampler(this.Data.EmptyBarSampler);
        context.SpriteBatch.DrawTexture(this.Data.EmptyBarTexture, this.Position, 0.5F, this.Data.EmptyBarSourceRect, this.Data.EmptyBarScale * this.Gui.ScaleFactor, this.Origin, this.Rotation, emptyBarColor, this.Data.EmptyBarFlip);
        if (this.Data.EmptyBarSampler != null) context.SpriteBatch.PopSampler();
        
        context.SpriteBatch.End();
    }
}