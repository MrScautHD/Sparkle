using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Terrain;

public class MarchingCubesChunk : Disposable {
    
    public MarchingCubes MarchingCubes { get; private set; }
    public Model Model { get; private set; }
    
    private Vector3 _position;
    private int _width;
    private int _height;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MarchingCubesChunk"/> class.
    /// </summary>
    /// <param name="marchingCubes">The MarchingCubes instance associated with this chunk.</param>
    /// <param name="position">The position of the chunk within the grid.</param>
    /// <param name="width">The width of the chunk in units.</param>
    /// <param name="height">The height of the chunk in units.</param>
    public MarchingCubesChunk(MarchingCubes marchingCubes, Vector3 position, int width, int height) {
        this.MarchingCubes = marchingCubes;
        this._position = position;
        this._width = width;
        this._height = height;
    }


    /// <summary>
    /// Generates a Marching Cubes chunk.
    /// </summary>
    public void Generate() {
        this.MarchingCubes.SetHeights(this._position, this._width, this._height);
        this.MarchingCubes.MarchCubes(this._position, this._width, this._height);
        this.Model = this.MarchingCubes.GenerateModel();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            ModelHelper.Unload(this.Model);
        }
    }
}