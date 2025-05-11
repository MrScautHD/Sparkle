using System.Numerics;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Scenes;
using Veldrid;
using Vortice.Mathematics;
using Color = Bliss.CSharp.Colors.Color;

namespace Sparkle.CSharp.Entities.Components;

public class Sprite : InterpolatedComponent {

    /// <summary>
    /// The texture used to render the sprite.
    /// </summary>
    public Texture2D Texture;
    
    /// <summary>
    /// The size of the sprite in pixels. If not specified, defaults to the texture's dimensions.
    /// </summary>
    public Vector2 Size;
    
    /// <summary>
    /// The tint color applied when rendering the sprite. Defaults to white (no tint).
    /// </summary>
    public Color Color;
    
    /// <summary>
    /// The flip mode of the sprite (e.g., horizontal, vertical, or none).
    /// </summary>
    public SpriteFlip Flip;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Sprite"/> class.
    /// </summary>
    /// <param name="texture">The texture to display for this sprite.</param>
    /// <param name="offsetPos">The local position offset of the sprite relative to its entity.</param>
    /// <param name="size">The optional size of the sprite. If null, uses the texture's dimensions.</param>
    /// <param name="color">The optional color tint. If null, defaults to white.</param>
    /// <param name="flip">The sprite flip mode. Defaults to <see cref="SpriteFlip.None"/>.</param>
    public Sprite(Texture2D texture, Vector2 offsetPos, Vector2? size = null, Color? color = null, SpriteFlip flip = SpriteFlip.None) : base(new Vector3(offsetPos, 0)) {
        this.Texture = texture;
        this.Size = size ?? new Vector2(texture.Width, texture.Height);
        this.Color = color ?? Color.White;
        this.Flip = flip;
    }
    
    /// <summary>
    /// Draws the sprite using the active 2D camera and the specified graphics context and framebuffer.
    /// Applies transformations like scale, rotation, and flip.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    /// <param name="framebuffer">The framebuffer to draw the sprite onto.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        Camera2D? cam2D = SceneManager.ActiveCam2D;
        
        if (cam2D == null) {
            return;
        }
        
        Rectangle source = new Rectangle(0, 0, (int) this.Texture.Width, (int) this.Texture.Height);
        Rectangle dest = new Rectangle((int) this.LerpedGlobalPosition.X, (int) this.LerpedGlobalPosition.Y, (int) this.Size.X, (int) this.Size.Y);
        Vector2 origin = new Vector2(dest.Width / 2.0F, dest.Height / 2.0F);
        Vector2 scale = new Vector2(this.LerpedScale.X, this.LerpedScale.Y);
        float rotation = float.RadiansToDegrees(this.LerpedRotation.ToEuler().Z);
        
        context.SpriteBatch.SetOutput(framebuffer.OutputDescription);
        context.SpriteBatch.SetView(cam2D.GetView());
        context.SpriteBatch.DrawTexture(this.Texture, new Vector2(this.LerpedGlobalPosition.X, this.LerpedGlobalPosition.Y), 0.5F, source, scale, origin, rotation, this.Color, this.Flip);
        context.SpriteBatch.ResetSettings();
    }
}