using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Fonts;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.Loading;

public abstract class LoadingScreen {
    
    public float MinTime { get; set; } = 0.5f;
    
    public static LoadingScreen? None => null;
    
    public static LoadingScreen Blank => new BlankLoadingScreen();
    
    public static LoadingScreen Loading(Font font) => new TextLoadingScreen(font);
    
    protected internal virtual void Update(double delta) { }
    
    protected internal virtual void AfterUpdate(double delta) { }
    
    protected internal virtual void FixedUpdate(double timeStep) { }
    
    protected internal abstract void Draw(GraphicsContext context, Framebuffer framebuffer);
    
    protected internal virtual void Resize(Rectangle rectangle) { }
    
    private class BlankLoadingScreen : LoadingScreen {
        
        protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
            context.CommandList.ClearColorTarget(0, Color.Black.ToRgbaFloat());
        }
    }

    private class TextLoadingScreen : LoadingScreen {

        private Font _font;
        
        public TextLoadingScreen(Font font) {
            this._font = font;
        }
        
        protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
            context.CommandList.ClearColorTarget(0, Color.Black.ToRgbaFloat());
            context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);

            string text = "Loading...";
            float fontSize = 18;
            Vector2 scale = new Vector2(5, 5);

            Vector2 textSize = this._font.MeasureText(text, fontSize) * scale;
            Vector2 position = new Vector2(framebuffer.Width / 2.0f - textSize.X / 2.0f, framebuffer.Height / 2.0f - textSize.Y / 2.0f);

            context.SpriteBatch.DrawText(this._font, text, position, fontSize, scale: scale);
            context.SpriteBatch.End();
        }
    }
}