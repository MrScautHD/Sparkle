using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Graphics.Rendering.Renderers.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Veldrid;

namespace Sparkle.CSharp.GUI.Elements.Data;

public class TextureSliderBarData {
    
    public Texture2D EmptyBarTexture;

    public Texture2D FullBarTexture;
    
    public Texture2D SliderTexture;
    
    public Sampler? EmptyBarSampler;
    
    public Sampler? FullBarSampler;
    
    public Sampler? SliderSampler;
    
    public Rectangle EmptyBarSourceRect;
    
    public Rectangle FullBarSourceRect;
    
    public Rectangle SliderSourceRect;
    
    public Vector2 EmptyBarScale;
    
    public Vector2 FullBarScale;
    
    public Vector2 SliderScale;
    
    public Color EmptyBarColor;
    
    public Color FullBarColor;
    
    public Color SliderColor;
    
    public Color EmptyBarHoverColor;
    
    public Color FullBarHoverColor;
    
    public Color SliderHoverColor;
    
    public SpriteFlip EmptyBarFlip;
    
    public SpriteFlip FullBarFlip;
    
    public SpriteFlip SliderFlip;
    
    public Color OffStateColor;
    
    public TextureSliderBarData(Texture2D emptyBarTexture,
        Texture2D fullBarTexture,
        Texture2D sliderTexture,
        Sampler? emptyBarSampler = null,
        Sampler? fullBarSampler = null,
        Sampler? sliderSampler = null,
        Rectangle? emptyBarSourceRect = null,
        Rectangle? fullBarSourceRect = null,
        Rectangle? sliderSourceRect = null,
        Vector2? emptyBarScale = null,
        Vector2? fullBarScale = null,
        Vector2? sliderScale = null,
        Color? emptyBarColor = null,
        Color? fullBarColor = null,
        Color? sliderColor = null,
        Color? emptyBarHoverColor = null,
        Color? fullBarHoverColor = null,
        Color? sliderHoverColor = null,
        SpriteFlip emptyBarFlip = SpriteFlip.None,
        SpriteFlip fullBarFlip = SpriteFlip.None,
        SpriteFlip sliderFlip = SpriteFlip.None,
        Color? offStateColor = null) {
        this.EmptyBarTexture = emptyBarTexture;
        this.FullBarTexture = fullBarTexture;
        this.SliderTexture = sliderTexture;
        this.EmptyBarSampler = emptyBarSampler;
        this.FullBarSampler = fullBarSampler;
        this.SliderSampler = sliderSampler;
        this.EmptyBarSourceRect = emptyBarSourceRect ?? new Rectangle(0, 0, (int) emptyBarTexture.Width, (int) emptyBarTexture.Height);
        this.FullBarSourceRect = fullBarSourceRect ?? new Rectangle(0, 0, (int) fullBarTexture.Width, (int) fullBarTexture.Height);
        this.SliderSourceRect = sliderSourceRect ?? new Rectangle(0, 0, (int) sliderTexture.Width, (int) sliderTexture.Height);
        this.EmptyBarScale = emptyBarScale ?? Vector2.One;
        this.FullBarScale = fullBarScale ?? Vector2.One;
        this.SliderScale = sliderScale ?? Vector2.One;
        this.EmptyBarColor = emptyBarColor ?? Color.White;
        this.FullBarColor = fullBarColor ?? Color.White;
        this.SliderColor = sliderColor ?? Color.White;
        this.EmptyBarHoverColor = emptyBarHoverColor ?? this.EmptyBarColor;
        this.FullBarHoverColor = fullBarHoverColor ?? this.FullBarColor;
        this.SliderHoverColor = sliderHoverColor ?? this.SliderColor;
        this.EmptyBarFlip = emptyBarFlip;
        this.FullBarFlip = fullBarFlip;
        this.SliderFlip = sliderFlip;
        this.OffStateColor = offStateColor ?? Color.Gray;
    }
}