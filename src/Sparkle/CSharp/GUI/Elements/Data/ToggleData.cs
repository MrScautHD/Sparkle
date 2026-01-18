using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class ToggleData {
    
    /// <summary>
    /// The texture used for the checkbox of the toggle box.
    /// </summary>
    public Texture2D CheckboxTexture;
    
    /// <summary>
    /// The texture used for the checkmark when the toggle is active.
    /// </summary>
    public Texture2D CheckmarkTexture;
    
    /// <summary>
    /// The sampler used when rendering the checkbox texture.
    /// </summary>
    public Sampler? CheckboxSampler;
    
    /// <summary>
    /// The sampler used when rendering the checkmark texture.
    /// </summary>
    public Sampler? CheckmarkSampler;
    
    /// <summary>
    /// The source rectangle of the checkbox texture.
    /// </summary>
    public Rectangle CheckboxSourceRect;
    
    /// <summary>
    /// The source rectangle of the checkmark texture.
    /// </summary>
    public Rectangle CheckmarkSourceRect;
    
    /// <summary>
    /// The base color applied to the checkbox when not hovered.
    /// </summary>
    public Color CheckboxColor;
    
    /// <summary>
    /// The base color applied to the checkmark when not hovered.
    /// </summary>
    public Color CheckmarkColor;
    
    /// <summary>
    /// The color applied to the checkbox when the toggle is hovered.
    /// </summary>
    public Color CheckboxHoverColor;
    
    /// <summary>
    /// The color applied to the checkmark when the toggle is hovered.
    /// </summary>
    public Color CheckmarkHoverColor;
    
    /// <summary>
    /// The flip transformation applied to the checkbox texture.
    /// </summary>
    public SpriteFlip CheckboxFlip;
    
    /// <summary>
    /// The flip transformation applied to the checkmark texture.
    /// </summary>
    public SpriteFlip CheckmarkFlip;
    
    /// <summary>
    /// The color applied when the toggle is in an inactive or disabled state.
    /// </summary>
    public Color OffStateColor;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleData"/> class with optional customization.
    /// </summary>
    /// <param name="checkboxTexture">The texture for the toggle Checkbox.</param>
    /// <param name="checkmarkTexture">The texture for the toggle checkmark.</param>
    /// <param name="checkboxSampler">Optional sampler for the Checkbox texture.</param>
    /// <param name="checkmarkSampler">Optional sampler for the checkmark texture.</param>
    /// <param name="checkboxSourceRect">Optional source rectangle for the Checkbox texture. Defaults to full texture.</param>
    /// <param name="checkmarkSourceRect">Optional source rectangle for the checkmark texture. Defaults to full texture.</param>
    /// <param name="checkboxColor">Optional base color for the Checkbox. Defaults to white.</param>
    /// <param name="checkmarkColor">Optional base color for the checkmark. Defaults to white.</param>
    /// <param name="checkboxHoverColor">Optional hover color for the Checkbox. Defaults to <paramref name="checkboxColor"/>.</param>
    /// <param name="checkmarkHoverColor">Optional hover color for the checkmark. Defaults to <paramref name="checkmarkColor"/>.</param>
    /// <param name="checkboxFlip">Optional flip setting for the Checkbox texture. Defaults to none.</param>
    /// <param name="checkmarkFlip">Optional flip setting for the checkmark texture. Defaults to none.</param>
    /// <param name="offStateColor">Optional color for the toggle when disabled. Defaults to gray.</param>
    public ToggleData(Texture2D checkboxTexture, Texture2D checkmarkTexture, Sampler? checkboxSampler = null, Sampler? checkmarkSampler = null, Rectangle? checkboxSourceRect = null, Rectangle? checkmarkSourceRect = null, Color? checkboxColor = null, Color? checkmarkColor = null, Color? checkboxHoverColor = null, Color? checkmarkHoverColor = null, SpriteFlip checkboxFlip = SpriteFlip.None, SpriteFlip checkmarkFlip = SpriteFlip.None, Color? offStateColor = null) {
        this.CheckboxTexture = checkboxTexture;
        this.CheckmarkTexture = checkmarkTexture;
        this.CheckboxSampler = checkboxSampler;
        this.CheckmarkSampler = checkmarkSampler;
        this.CheckboxSourceRect = checkboxSourceRect ?? new Rectangle(0, 0, (int) checkboxTexture.Width, (int) checkboxTexture.Height);
        this.CheckmarkSourceRect = checkmarkSourceRect ?? new Rectangle(0, 0, (int) checkmarkTexture.Width, (int) checkmarkTexture.Height);
        this.CheckboxColor = checkboxColor ?? Color.White;
        this.CheckmarkColor = checkmarkColor ?? Color.White;
        this.CheckboxHoverColor = checkboxHoverColor ?? this.CheckboxColor;
        this.CheckmarkHoverColor = checkmarkHoverColor ?? this.CheckmarkColor;
        this.CheckboxFlip = checkboxFlip;
        this.CheckmarkFlip = checkmarkFlip;
        this.OffStateColor = offStateColor ?? Color.Gray;
    }
}