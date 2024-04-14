using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Particles;

// TODO: WIP!!!
public class Particle : Disposable {

    public Texture2D Texture;
    public Vector3 Position;
    public Vector2 Size;
    public float Rotation;
    public Color Color;
    
    public bool HasInitialized { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Particle"/> class.
    /// </summary>
    /// <param name="texture">The texture of the particle.</param>
    /// <param name="position">The position of the particle in 3D space.</param>
    /// <param name="size">The size of the particle.</param>
    /// <param name="rotation">The rotation of the particle in radians.</param>
    /// <param name="color">The color of the particle.</param>
    public Particle(Texture2D texture, Vector3 position, Vector2 size, float rotation, Color color) {
        this.Texture = texture;
        this.Position = position;
        this.Size = size;
        this.Rotation = rotation;
        this.Color = color;
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal virtual void Init() {
        this.HasInitialized = true;
    }
    
    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    protected internal virtual void Update() { }
    
    /// <summary>
    /// Called after the Update method on each tick to further update dynamic elements and game logic.
    /// </summary>
    protected internal virtual void AfterUpdate() { }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() { }

    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    public virtual void Draw() {
        Cam3D? cam = SceneManager.ActiveCam3D;
        if (cam == null) return;
        
        Rectangle source = new Rectangle(0, 0, this.Texture.Width, this.Texture.Height);
        Rectangle dest = new Rectangle(this.Position.X + (this.Size.X / 2), this.Position.Y + (this.Size.Y / 2), this.Size.X, this.Size.Y);
        Vector2 origin = new Vector2(dest.Width / 2, dest.Height / 2);
        
        ModelHelper.DrawBillboardPro(cam, this.Texture, source, this.Position, cam.Up, this.Size, origin, this.Rotation, this.Color);
    }
    
    protected override void Dispose(bool disposing) { }
}