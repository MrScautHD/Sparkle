using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Geometry;
using Raylib_CSharp.Images;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Textures;
using Sparkle.CSharp.Registries.Types;

namespace Sparkle.CSharp.Rendering.Renderers;

public class Skybox : Disposable {

    public readonly Image CubeMap;
    public Color Color;
    
    private Texture2D _cubeMap;
    private Model _box;
    
    public bool HasInitialized { get; private set; }

    /// <summary>
    /// Represents a skybox in a 3D scene.
    /// </summary>
    public Skybox(Image cubeMap, Color? color = default) {
        this.CubeMap = cubeMap;
        this.Color = color ?? Color.White;
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal void Init() {
        this._cubeMap = Texture2D.LoadCubemap(this.CubeMap, CubemapLayout.AutoDetect);
        this._box = Model.LoadFromMesh(Mesh.GenCube(1, 1, 1));
        
        this._box.SetMaterialTexture(0, MaterialMapIndex.Cubemap, this._cubeMap);
        this._box.SetMaterialShader(0, EffectRegistry.Skybox.Shader);
        
        this.HasInitialized = true;
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal void Draw() {
        RlGl.DisableBackfaceCulling();
        RlGl.DisableDepthMask();
        
        Graphics.DrawModel(this._box, Vector3.Zero, 1, this.Color);
        
        RlGl.EnableBackfaceCulling();
        RlGl.EnableDepthMask();
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._box.Unload();
            this._cubeMap.Unload();
        }
    }
}