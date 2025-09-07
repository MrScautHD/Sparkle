using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class ToggleData {
    
    /// <summary>
    /// The texture used for the background of the toggle box.
    /// </summary>
    public Texture2D BackgroundTexture;
    
    /// <summary>
    /// The texture used for the checkmark when the toggle is active.
    /// </summary>
    public Texture2D CheckmarkTexture;
    
    /// <summary>
    /// The sampler used when rendering the background texture.
    /// </summary>
    public Sampler? BackgroundSampler;
    
    /// <summary>
    /// The sampler used when rendering the checkmark texture.
    /// </summary>
    public Sampler? CheckmarkSampler;
    
    /// <summary>
    /// The source rectangle of the background texture.
    /// </summary>
    public Rectangle BackgroundSourceRect;
    
    /// <summary>
    /// The source rectangle of the checkmark texture.
    /// </summary>
    public Rectangle CheckmarkSourceRect;
    
    /// <summary>
    /// The scale applied to the background texture.
    /// </summary>
    public Vector2 BackgroundScale;
    
    /// <summary>
    /// The scale applied to the checkmark texture.
    /// </summary>
    public Vector2 CheckmarkScale;
    
    /// <summary>
    /// The base color applied to the background when not hovered.
    /// </summary>
    public Color BackgroundColor;
    
    /// <summary>
    /// The base color applied to the checkmark when not hovered.
    /// </summary>
    public Color CheckmarkColor;
    
    /// <summary>
    /// The color applied to the background when the toggle is hovered.
    /// </summary>
    public Color BackgroundHoverColor;
    
    /// <summary>
    /// The color applied to the checkmark when the toggle is hovered.
    /// </summary>
    public Color CheckmarkHoverColor;
    
    /// <summary>
    /// The flip transformation applied to the background texture.
    /// </summary>
    public SpriteFlip BackgroundFlip;
    
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
    /// <param name="backgroundTexture">The texture for the toggle background.</param>
    /// <param name="checkmarkTexture">The texture for the toggle checkmark.</param>
    /// <param name="backgroundSampler">Optional sampler for the background texture.</param>
    /// <param name="checkmarkSampler">Optional sampler for the checkmark texture.</param>
    /// <param name="backgroundSourceRect">Optional source rectangle for the background texture. Defaults to full texture.</param>
    /// <param name="checkmarkSourceRect">Optional source rectangle for the checkmark texture. Defaults to full texture.</param>
    /// <param name="backgroundScale">Optional scale for the background texture. Defaults to <see cref="Vector2.One"/>.</param>
    /// <param name="checkmarkScale">Optional scale for the checkmark texture. Defaults to <see cref="Vector2.One"/>.</param>
    /// <param name="backgroundColor">Optional base color for the background. Defaults to white.</param>
    /// <param name="checkmarkColor">Optional base color for the checkmark. Defaults to white.</param>
    /// <param name="backgroundHoverColor">Optional hover color for the background. Defaults to <paramref name="backgroundColor"/>.</param>
    /// <param name="checkmarkHoverColor">Optional hover color for the checkmark. Defaults to <paramref name="checkmarkColor"/>.</param>
    /// <param name="backgroundFlip">Optional flip setting for the background texture. Defaults to none.</param>
    /// <param name="checkmarkFlip">Optional flip setting for the checkmark texture. Defaults to none.</param>
    /// <param name="offStateColor">Optional color for the toggle when disabled. Defaults to gray.</param>
    public ToggleData(Texture2D backgroundTexture, Texture2D checkmarkTexture, Sampler? backgroundSampler = null, Sampler? checkmarkSampler = null, Rectangle? backgroundSourceRect = null, Rectangle? checkmarkSourceRect = null, Vector2? backgroundScale = null, Vector2? checkmarkScale = null, Color? backgroundColor = null, Color? checkmarkColor = null, Color? backgroundHoverColor = null, Color? checkmarkHoverColor = null, SpriteFlip backgroundFlip = SpriteFlip.None, SpriteFlip checkmarkFlip = SpriteFlip.None, Color? offStateColor = null) {
        this.BackgroundTexture = backgroundTexture;
        this.CheckmarkTexture = checkmarkTexture;
        this.BackgroundSampler = backgroundSampler;
        this.CheckmarkSampler = checkmarkSampler;
        this.BackgroundSourceRect = backgroundSourceRect ?? new Rectangle(0, 0, (int) backgroundTexture.Width, (int) backgroundTexture.Height);
        this.CheckmarkSourceRect = checkmarkSourceRect ?? new Rectangle(0, 0, (int) checkmarkTexture.Width, (int) checkmarkTexture.Height);
        this.BackgroundScale = backgroundScale ?? Vector2.One;
        this.CheckmarkScale = checkmarkScale ?? Vector2.One;
        this.BackgroundColor = backgroundColor ?? Color.White;
        this.CheckmarkColor = checkmarkColor ?? Color.White;
        this.BackgroundHoverColor = backgroundHoverColor ?? this.BackgroundColor;
        this.CheckmarkHoverColor = checkmarkHoverColor ?? this.CheckmarkColor;
        this.BackgroundFlip = backgroundFlip;
        this.CheckmarkFlip = checkmarkFlip;
        this.OffStateColor = offStateColor ?? Color.Gray;
    }
}