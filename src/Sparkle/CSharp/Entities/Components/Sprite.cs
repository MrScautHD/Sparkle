using System.Numerics;
using Bliss.CSharp.Graphics.Rendering.Batches.Sprites;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Vortice.Mathematics;
using Color = Bliss.CSharp.Colors.Color;

namespace Sparkle.CSharp.Entities.Components;

public class Sprite : Component {

    public Texture2D Texture;
    public Vector2 Size;
    public Color Color;
    public SpriteFlip Flip;
    
    public Sprite(Texture2D texture, Vector3 offsetPos, Vector2? size = null, Color? color = null, SpriteFlip flip = SpriteFlip.None) : base(offsetPos) {
        this.Texture = texture;
        this.Size = size ?? new Vector2(texture.Width, texture.Height);
        this.Color = color ?? Color.White;
        this.Flip = flip;
    }

    protected internal override void Draw(GraphicsContext context) {
        base.Draw(context);
        
        Rectangle source = new Rectangle(0, 0, (int) this.Texture.Width, (int) this.Texture.Height);
        Rectangle dest = new Rectangle((int) this.GlobalPos.X, (int) this.GlobalPos.Y, (int) this.Size.X, (int) this.Size.Y);
        Vector2 origin = new Vector2(dest.Width / 2.0F, dest.Height / 2.0F);
        Vector2 scale = new Vector2(this.Entity.Transform.Scale.X, this.Entity.Transform.Scale.Y);
        float rotation = float.RadiansToDegrees(this.Entity.Transform.Rotation.ToEuler().Z);
        
        context.SpriteBatch.Begin(context.CommandList, context.Framebuffer.OutputDescription);
        context.SpriteBatch.DrawTexture(this.Texture, new Vector2(this.GlobalPos.X, this.GlobalPos.Y), source, scale, origin, rotation, this.Color, this.Flip);
        context.SpriteBatch.End();
    }
}