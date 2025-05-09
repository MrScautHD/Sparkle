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

public class Sprite : Component {

    public Texture2D Texture;
    public Vector2 Size;
    public Color Color;
    public SpriteFlip Flip;
    
    public Sprite(Texture2D texture, Vector2 offsetPos, Vector2? size = null, Color? color = null, SpriteFlip flip = SpriteFlip.None) : base(new Vector3(offsetPos, 0)) {
        this.Texture = texture;
        this.Size = size ?? new Vector2(texture.Width, texture.Height);
        this.Color = color ?? Color.White;
        this.Flip = flip;
    }

    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        
        // TODO: FIX THIS! (also use InterpolatedComponent)
        Rectangle source = new Rectangle(0, 0, (int) this.Texture.Width, (int) this.Texture.Height);
        Rectangle dest = new Rectangle((int) this.GlobalPosition.X, (int) this.GlobalPosition.Y, (int) this.Size.X, (int) this.Size.Y);
        Vector2 origin = new Vector2(dest.Width / 2.0F, dest.Height / 2.0F);
        Vector2 scale = new Vector2(this.Entity.Transform.Scale.X, this.Entity.Transform.Scale.Y);
        float rotation = float.RadiansToDegrees(this.Entity.Transform.Rotation.ToEuler().Z);
        
        context.SpriteBatch.SetOutput(framebuffer.OutputDescription);
        context.SpriteBatch.SetView(SceneManager.ActiveCam2D?.GetView());
        context.SpriteBatch.DrawTexture(this.Texture, new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y), source, scale, origin, rotation, this.Color, this.Flip);
        context.SpriteBatch.ResetSettings();
    }
}