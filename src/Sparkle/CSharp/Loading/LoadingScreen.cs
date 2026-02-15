using Bliss.CSharp.Colors;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Veldrid;

namespace Sparkle.CSharp.Loading;

public abstract class LoadingScreen {
    
    public float MinTime { get; set; } = 0.5f;
    
    public static LoadingScreen? None => null;
    
    public static LoadingScreen Blank => new BlankLoadingScreen();
    
    public static LoadingScreen Loading => new TextLoadingScreen();
    
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
        
        protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
            context.CommandList.ClearColorTarget(0, Color.Gray.ToRgbaFloat());
            context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription);
            context.PrimitiveBatch.DrawFilledRectangle(new RectangleF(40, 40, 300, 300), color: Color.Magenta);
            //context.SpriteBatch.DrawText(GlobalGraphicsAssets., "Loading...", new Vector2(20, 20), 24);
            context.PrimitiveBatch.End();
        }
    }
}