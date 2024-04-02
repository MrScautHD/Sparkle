using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Terrain;

public class MarchingCubesChunk : Disposable {
    
    public MarchingCubes MarchingCubes { get; private set; }
    public Model Model { get; private set; }
    
    public Vector3 Position { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MarchingCubesChunk"/> class.
    /// </summary>
    /// <param name="marchingCubes">The MarchingCubes instance associated with this chunk.</param>
    /// <param name="position">The position of the chunk within the grid.</param>
    /// <param name="width">The width of the chunk in units.</param>
    /// <param name="height">The height of the chunk in units.</param>
    public MarchingCubesChunk(MarchingCubes marchingCubes, Vector3 position, int width, int height) {
        this.MarchingCubes = marchingCubes;
        this.Position = position;
        this.Width = width;
        this.Height = height;
    }


    /// <summary>
    /// Generates a Marching Cubes chunk.
    /// </summary>
    public void Generate() {
        this.MarchingCubes.SetHeights(this.Position, this.Width, this.Height);
        this.MarchingCubes.MarchCubes(this.Position, this.Width, this.Height);
        this.Model = this.MarchingCubes.GenerateModel();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            ModelHelper.Unload(this.Model);
        }
    }
}