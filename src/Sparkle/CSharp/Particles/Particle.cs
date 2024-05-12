using System.Drawing;
using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Collision;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Scenes;
using Color = Raylib_CSharp.Colors.Color;

namespace Sparkle.CSharp.Particles;

public class Particle : Disposable {
    
    public int Id { get; internal set; }

    public Texture2D Texture;
    public Vector3 Position;
    
    private float _lifeTime;
    private ParticleData _data;

    private float _timer;
    private float _interpolationFactor;
    
    public bool HasInitialized { get; private set; }

    /// <summary>
    /// Constructor for creating a Particle object.
    /// </summary>
    /// <param name="texture">Texture of the particle.</param>
    /// <param name="position">Position of the particle.</param>
    /// <param name="lifeTime">Lifetime of the particle.</param>
    /// <param name="data">Particle data.</param>
    public Particle(Texture2D texture, Vector3 position, float lifeTime, ParticleData data) {
        this.Texture = texture;
        this.Position = position;
        this._lifeTime = lifeTime;
        this._data = data;
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
    protected internal virtual void Update() {
        this._timer += Time.GetFrameTime();
        this._interpolationFactor = RayMath.Clamp(this._timer / this._lifeTime, 0.0F, 1.0F);

        if (this._timer >= this._lifeTime) {
            SceneManager.ActiveScene?.RemoveParticle(this);
        }

        Vector3 velocity = RayMath.Vector3Lerp(this._data.StartVelocity, this._data.EndVelocity, this._interpolationFactor);
        this.Position += velocity * Time.GetFrameTime();
    }
    
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
    protected internal virtual void Draw() {
        Cam3D? cam = SceneManager.ActiveCam3D;
        if (cam == null) return;
        
        Vector2 size = RayMath.Vector2Lerp(this._data.StartSize, this._data.EndSize, this._interpolationFactor);
        BoundingBox box = new BoundingBox(this.Position - new Vector3(size.X / 2, size.Y / 2, size.X / 2), this.Position + new Vector3(size.X / 2, size.Y / 2, size.X / 2));
        
        if (SceneManager.ActiveCam3D!.GetFrustum().ContainsBox(box)) {
            float rotation = RayMath.Lerp(this._data.StartRotation, this._data.EndRotation, this._interpolationFactor);
        
            Color color = new Color() {
                R = (byte) RayMath.Lerp(this._data.StartColor.R, this._data.EndColor.R, this._interpolationFactor),
                G = (byte) RayMath.Lerp(this._data.StartColor.G, this._data.EndColor.G, this._interpolationFactor),
                B = (byte) RayMath.Lerp(this._data.StartColor.B, this._data.EndColor.B, this._interpolationFactor),
                A = (byte) RayMath.Lerp(this._data.StartColor.A, this._data.EndColor.A, this._interpolationFactor)
            };
        
            RectangleF source = new RectangleF(0, 0, this.Texture.Width, this.Texture.Height);
            RectangleF dest = new RectangleF(this.Position.X + (source.X / 2), this.Position.Y + (source.Y / 2), source.X, source.Y);
            Vector2 origin = new Vector2(dest.Width / 2.0F, dest.Height / 2.0F);
            
            Graphics.BeginShaderMode(this._data.Effect.Shader);
            Graphics.DrawBillboardPro(cam.GetCamera3D(), this.Texture, source, this.Position, cam.Up, size, origin, rotation, color);
            Graphics.EndShaderMode();
        }
    }

    protected override void Dispose(bool disposing) { }
}