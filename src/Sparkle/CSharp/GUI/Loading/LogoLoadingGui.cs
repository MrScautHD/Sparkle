using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.GUI.Loading;

public class LogoLoadingGui : LoadingGui {
    
    public Texture2D Logo { get; private set; }
    
    private float _timer;
    private float _fadeInTime;
    
    public LogoLoadingGui(string name, float minTime = 5.5F, (int, int)? size = null) : base(name, minTime, size) {
        this._fadeInTime = 3.0F;
    }
    
    protected internal override void Update(double delta) {
        base.Update(delta);
        this._timer += (float) delta;
    }
    
    protected internal override void Init() {
        base.Init();
        this.Logo = Game.Instance?.Content.Load(new TextureContent("content/sparkle/images/logo.png"))!;
    }
    
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        float bgProgress = Math.Clamp(this._timer / 0.5F, 0.0F, 1.0F);
        float easedBgAlpha = (float) Math.Pow(bgProgress, 5.0F);
        
        byte r = (byte) float.Lerp(0, 36, easedBgAlpha);
        byte g = (byte) float.Lerp(0, 36, easedBgAlpha);
        byte b = (byte) float.Lerp(0, 36, easedBgAlpha);
        
        context.CommandList.ClearColorTarget(0, new Color(r, g, b, 255).ToRgbaFloat());
        // TODO: Do a image gui element...
        
        float progress = Math.Clamp(this._timer / this._fadeInTime, 0.0F, 1.0F);
        float easedAlpha = (float) Math.Pow(progress, 5.0F);
        
        Color color = new Color(255, 255, 255, (byte) (easedAlpha * 255));
        
        Vector2 scale = new Vector2(4.0F, 4.0F);
        
        float x = (GlobalGraphicsAssets.Window.GetWidth() / 2.0F) - (this.Logo.Width * scale.X / 2.0F);
        float y = (GlobalGraphicsAssets.Window.GetHeight() / 2.0F) - (this.Logo.Height * scale.Y / 2.0F);
        
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.SpriteBatch.DrawTexture(this.Logo, new Vector2(x, y), color: color, scale: scale);
        context.SpriteBatch.End();
        
        base.Draw(context, framebuffer);
    }
}