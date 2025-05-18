using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Graphics.Rendering.Sprites;

public class SpriteRenderer {
    
    /// <summary>
    /// Storing queued sprites for rendering.
    /// </summary>
    private List<SpriteData> _sprites;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteRenderer"/> class.
    /// </summary>
    public SpriteRenderer() {
        this._sprites = new List<SpriteData>();
    }

    /// <summary>
    /// Queues a sprite for rendering with specified properties.
    /// </summary>
    /// <param name="texture">The texture of the sprite to be drawn.</param>
    /// <param name="sampler">The sampler specifying how the texture will be sampled.</param>
    /// <param name="position">The position where the sprite will be rendered.</param>
    /// <param name="layerDepth">The depth value for layer sorting of the sprite.</param>
    /// <param name="sourceRect">The rectangle defining the portion of the texture to use.</param>
    /// <param name="scale">The scaling factor applied to the sprite.</param>
    /// <param name="origin">The origin point used for rotation and scaling transformations.</param>
    /// <param name="rotation">The rotation angle in radians applied to the sprite.</param>
    /// <param name="color">The color applied to blend with the sprite texture.</param>
    /// <param name="flip">The sprite flipping mode indicating horizontal or vertical flipping.</param>
    /// <param name="effect">The effect to be applied to the sprite during rendering, if any.</param>
    /// <param name="blendState">The blend state to be used for rendering, or null for the default state.</param>
    /// <param name="depthStencilState">The depth-stencil state to be used for rendering, or null for the default state.</param>
    /// <param name="rasterizerState">The rasterizer state to be used for rendering, or null for the default state.</param>
    public void DrawSprite(Texture2D texture, Sampler? sampler, Vector2 position, float layerDepth, Rectangle sourceRect, Vector2 scale, Vector2 origin, float rotation, Color color, SpriteFlip flip, Effect? effect, BlendStateDescription? blendState, DepthStencilStateDescription? depthStencilState, RasterizerStateDescription? rasterizerState) {
        this._sprites.Add(new SpriteData(texture, sampler, position, layerDepth, sourceRect, scale, origin, rotation, color, flip, effect, blendState, depthStencilState, rasterizerState));
    }

    /// <summary>
    /// Draws all queued sprites onto the specified framebuffer using the provided graphics context.
    /// </summary>
    /// <param name="context">The graphics context used for rendering operations.</param>
    /// <param name="framebuffer">The framebuffer where the sprites will be drawn.</param>
    protected internal void Draw(GraphicsContext context, Framebuffer framebuffer) {
        Camera2D? cam2D = SceneManager.ActiveCam2D;
        
        if (cam2D == null) {
            return;
        }
        
        // Sort sprites.
        this._sprites.Sort((sprite1, sprite2) => sprite1.LayerDepth.CompareTo(sprite2.LayerDepth));
        
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        
        // Draw sprites.
        foreach (SpriteData sprite in this._sprites) {
            if (this.IsSpriteVisible(cam2D, sprite)) {
                context.SpriteBatch.SetEffect(sprite.Effect);
                context.SpriteBatch.SetBlendState(sprite.BlendState);
                context.SpriteBatch.SetDepthStencilState(sprite.DepthStencilState);
                context.SpriteBatch.SetRasterizerState(sprite.RasterizerState);
                context.SpriteBatch.SetView(cam2D.GetView());
                context.SpriteBatch.SetSampler(sprite.Sampler);
            
                context.SpriteBatch.DrawTexture(sprite.Texture, sprite.Position, 0.5F, sprite.SourceRect, sprite.Scale, sprite.Origin, sprite.Rotation, sprite.Color, sprite.Flip);
                context.SpriteBatch.ResetSettings();
            }
        }
        
        context.SpriteBatch.End();
        
        // Clear sprites.
        this._sprites.Clear();
    }

    /// <summary>
    /// Determines whether the specified sprite is visible within the camera's view.
    /// </summary>
    /// <param name="camera">The camera used to define the visible area.</param>
    /// <param name="sprite">The sprite data containing position, rotation, scale, and other properties.</param>
    /// <returns>True if the sprite is visible in the camera's view; otherwise, false.</returns>
    private bool IsSpriteVisible(Camera2D camera, SpriteData sprite) {
        Vector2 spriteScale = new Vector2(sprite.SourceRect.Width, sprite.SourceRect.Height) * sprite.Scale;
        Vector2 spriteOrigin = sprite.Origin * sprite.Scale;
        
        Matrix3x2 transform = Matrix3x2.CreateRotation(float.DegreesToRadians(sprite.Rotation), sprite.Position);
        
        // Calculate the 4 corners.
        Vector2 corner1 = Vector2.Transform(new Vector2(sprite.Position.X, sprite.Position.Y) - spriteOrigin, transform);
        Vector2 corner2 = Vector2.Transform(new Vector2(sprite.Position.X + spriteScale.X, sprite.Position.Y) - spriteOrigin, transform);
        Vector2 corner3 = Vector2.Transform(new Vector2(sprite.Position.X, sprite.Position.Y + spriteScale.Y) - spriteOrigin, transform);
        Vector2 corner4 = Vector2.Transform(new Vector2(sprite.Position.X + spriteScale.X, sprite.Position.Y + spriteScale.Y) - spriteOrigin, transform);
        
        // Get cameras visible area.
        RectangleF visibleArea = camera.GetVisibleArea();
        
        return visibleArea.Contains(corner1) ||
               visibleArea.Contains(corner2) ||
               visibleArea.Contains(corner3) ||
               visibleArea.Contains(corner4);
    }
}