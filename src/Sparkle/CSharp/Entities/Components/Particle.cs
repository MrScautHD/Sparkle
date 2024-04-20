using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities.Components;

public class Particle : Component {
    
    public Texture2D Texture;
    public Vector2 Size;
    public float Rotation;
    public Color Color;
    
    /// <summary>
    /// Constructor for creating a Particle object.
    /// </summary>
    /// <param name="texture">Texture of the particle.</param>
    /// <param name="offsetPos">Offset position of the particle.</param>
    /// <param name="size">Size of the particle.</param>
    /// <param name="rotation">Rotation of the particle.</param>
    /// <param name="color">Color of the particle.</param>
    public Particle(Texture2D texture, Vector3 offsetPos, Vector2 size, float rotation, Color color) : base(offsetPos) {
        this.Texture = texture;
        this.Size = size;
        this.Rotation = rotation;
        this.Color = color;
    }
    
    protected internal override void Draw() {
        base.Draw();
        Cam3D? cam = SceneManager.ActiveCam3D;
        if (cam == null) return;
        
        Rectangle source = new Rectangle(0, 0, this.Texture.Width, this.Texture.Height);
        Rectangle dest = new Rectangle(this.Entity.Position.X + (this.Size.X / 2), this.Entity.Position.Y + (this.Size.Y / 2), this.Size.X, this.Size.Y);
        Vector2 origin = new Vector2(dest.Width / 2, dest.Height / 2);
        
        ModelHelper.DrawBillboardPro(cam, this.Texture, source, this.Entity.Position, cam.Up, this.Size, origin, this.Rotation, this.Color);
    }

    protected override void Dispose(bool disposing) { }
}