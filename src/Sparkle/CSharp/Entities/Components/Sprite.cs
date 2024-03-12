using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Entities.Components;

public class Sprite : Component {

    public Texture2D Texture;
    public Vector2 Size;
    public Color Color;
    public float Rotation;

    /// <summary>
    /// Represents a sprite object.
    /// </summary>
    /// <param name="texture">The texture of the sprite.</param>
    /// <param name="size">The size of the sprite. If null, the size will be the same as the texture.</param>
    /// <param name="color">The color of the sprite. If null, the color will be white.</param>
    /// <param name="rotation">The rotation angle of the sprite. Default value is 0.</param>
    public Sprite(Texture2D texture, Vector2? size = default, Color? color = default, float rotation = 0) {
        this.Texture = texture;
        this.Size = size ?? new Vector2(texture.Width, texture.Height);
        this.Color = color ?? Color.White;
        this.Rotation = rotation;
    }

    protected internal override void Draw() {
        base.Draw();
        
        Rectangle source = new Rectangle(0, 0, this.Texture.Width, this.Texture.Height);
        Rectangle dest = new Rectangle(this.Entity.Position.X, this.Entity.Position.Y, this.Size.X, this.Size.Y);
        Vector2 origin = new Vector2(dest.Width / 2, dest.Height / 2);
        TextureHelper.DrawPro(this.Texture, source, dest, origin, this.Rotation, this.Color);
    }

    protected override void Dispose(bool disposing) { }
}