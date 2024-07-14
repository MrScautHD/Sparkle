using Raylib_CSharp.Images;
using Raylib_CSharp.Textures;

namespace Sparkle.CSharp.Gifs;

public class Gif : Disposable {
    
    public uint Id { get; private set; }
    public int Frames { get; private set; }
    
    public bool HasInitialized { get; private set; }

    private string _path;
    
    private Image _gif;
    private Texture2D _frame;
    
    /// <summary>
    /// Constructor for creating a Gif object.
    /// </summary>
    /// <param name="path">The path to the GIF file.</param>
    public Gif(string path) {
        this._path = path;
    }

    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal void Init() {
        this._gif = Image.LoadAnim(this._path, out int frames);
        this.Frames = frames;
        
        this._frame = Texture2D.LoadFromImage(this._gif);
        this.Id = this._frame.Id;
        
        this.HasInitialized = true;
    }

    /// <summary>
    /// Returns the specified frame of the GIF as a Texture2D object.
    /// </summary>
    /// <param name="frame">The index of the frame to retrieve.</param>
    /// <returns>The specified frame of the GIF as a Texture2D object.</returns>
    public Texture2D GetFrame(int frame) {
        int dataOffset = this._gif.Width * this._gif.Height * 4 * frame;
        this._frame.Update(this._gif.Data + dataOffset);
        
        return this._frame;
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._gif.Unload();
            this._frame.Unload();
        }
    }
}