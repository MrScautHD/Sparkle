using Raylib_CSharp.Images;
using Raylib_CSharp.Textures;

namespace Sparkle.CSharp.Rendering.Gifs;

public class Gif : Disposable {
    
    public Texture2D Texture { get; private set; }
    public Image Image { get; private set; }
    
    public int FrameCounter { get; private set; }
    public int FrameDelay { get; private set; }
    
    public int AnimFrames { get; private set; }
    public int CurrentAnimFrame { get; private set; }
    
    public bool HasInitialized { get; private set; }

    private readonly string _path;
    
    /// <summary>
    /// Constructor for creating a Gif object.
    /// </summary>
    /// <param name="path">Path to the GIF file.</param>
    /// <param name="frameDelay">Delay between frames in milliseconds.</param>
    public Gif(string path, int frameDelay) {
        this._path = path;
        this.FrameDelay = frameDelay;
    }

    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal void Init() {
        this.Image = Image.LoadAnim(this._path, out int frames);
        this.AnimFrames = frames;
        
        this.Texture = Texture2D.LoadFromImage(this.Image);
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
    protected internal virtual void FixedUpdate() {
        this.FrameCounter++;
        if (this.FrameCounter >= this.FrameDelay) {
            
            this.CurrentAnimFrame++;
            if (this.CurrentAnimFrame >= this.AnimFrames) {
                this.CurrentAnimFrame = 0;
            }

            int nextFrameDataOffset = this.Image.Width * this.Image.Height * 4 * this.CurrentAnimFrame;
            
            this.Texture.Update(this.Image.Data + nextFrameDataOffset);

            this.FrameCounter = 0;
        }
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal virtual void Draw() { }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this.Image.Unload();
            this.Texture.Unload();
            GifManager.Gifs.Remove(this);
        }
    }
}