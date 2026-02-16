using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements;
using Sparkle.CSharp.GUI.Elements.Data;
using Veldrid;

namespace Sparkle.CSharp.GUI.Loading;

public class LogoLoadingGui : LoadingGui {
    
    /// <summary>
    /// Gets the loaded logo texture.
    /// </summary>
    public Texture2D Logo { get; private set; }
    
    /// <summary>
    /// The asset path used to load the logo texture.
    /// </summary>
    private string _logoPath;
    
    /// <summary>
    /// Internal timer used to drive fade animations.
    /// </summary>
    private float _timer;
    
    /// <summary>
    /// The duration, in seconds, of the logo fade-in animation.
    /// </summary>
    private float _logoFadeInTime;
    
    /// <summary>
    /// The duration, in seconds, of the background fade-in animation.
    /// </summary>
    private float _bgFadeInTime;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LogoLoadingGui"/> class.
    /// </summary>
    /// <param name="name">The unique name of the loading GUI.</param>
    /// <param name="logoPath">The asset path of the logo texture to load.</param>
    /// <param name="logoFadeInTime">The duration, in seconds, of the logo fade-in animation.</param>
    /// <param name="bgFadeInTime">The duration, in seconds, of the background fade-in animation.</param>
    /// <param name="minTime">The minimum amount of time, in seconds, that the loading screen should remain visible.</param>
    /// <param name="size">Optional size of the GUI in pixels as a tuple (width, height). If <c>null</c>, a default size is used.</param>
    public LogoLoadingGui(string name, string logoPath, float logoFadeInTime = 2.0F, float bgFadeInTime = 1.5F, float minTime = 2.5F, (int, int)? size = null) : base(name, minTime, size) {
        this._logoPath = logoPath;
        this._logoFadeInTime = logoFadeInTime;
        this._bgFadeInTime = bgFadeInTime;
    }
    
    /// <summary>
    /// Initializes the loading GUI and loads the logo texture.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        this.Logo = Game.Instance?.Content.Load(new TextureContent(this._logoPath))!;
        
        ImageData imageData = new ImageData(this.Logo, color: new Color(255, 255, 255, 0));
        this.AddElement("logo", new ImageElement(imageData, Anchor.Center, Vector2.Zero, scale: new Vector2(5, 5)));
    }
    
    /// <summary>
    /// Updates the fade-in animation of the logo.
    /// </summary>
    /// <param name="delta">The elapsed time in seconds since the last update.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);
        this._timer += (float) delta;
        
        float progress = Math.Clamp(this._timer / this._logoFadeInTime, 0.0F, 1.0F);
        float easedAlpha = MathF.Pow(progress, 5.0F);
        byte alphaByte = (byte) Math.Min(255, Math.Floor(easedAlpha * 255.5F));
        
        if (this.TryGetElement("logo", out GuiElement? element)) {
            if (element is ImageElement imageElement) {
                imageElement.Data.Color = new Color(255, 255, 255, alphaByte);
            }
        }
    }
    
    /// <summary>
    /// Renders the loading screen and applies a fading background color.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer to which the GUI is rendered.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        float progress = Math.Clamp(this._timer / this._bgFadeInTime, 0.0F, 1.0F);
        float rgb = (float) Math.Pow(progress, 5.0F);
        
        byte r = (byte) float.Lerp(0, 36, rgb);
        byte g = (byte) float.Lerp(0, 36, rgb);
        byte b = (byte) float.Lerp(0, 36, rgb);
        
        context.CommandList.ClearColorTarget(0, new Color(r, g, b, 255).ToRgbaFloat());
        
        base.Draw(context, framebuffer);
    }
}