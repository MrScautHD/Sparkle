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
    /// Constructor for creating a Sprite object.
    /// </summary>
    /// <param name="texture">Texture of the sprite.</param>
    /// <param name="offsetPos">Offset position of the sprite.</param>
    /// <param name="size">Size of the sprite.</param>
    /// <param name="color">Color of the sprite.</param>
    /// <param name="rotation">Rotation of the sprite.</param>
    public Sprite(Texture2D texture, Vector3 offsetPos, Vector2? size = default, Color? color = default, float rotation = 0) : base(offsetPos) {
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