using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Helpers;

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
        this.HasInitialized = true;
        
        this._cubeMap = TextureHelper.LoadCubemap(this.CubeMap, CubemapLayout.AutoDetect);
        this._box = ModelHelper.LoadFromMesh(MeshHelper.GenCube(1, 1, 1));
        MaterialHelper.SetTexture(ref this._box, 0, MaterialMapIndex.Cubemap, ref this._cubeMap);

        Shader shader = EffectRegistry.Skybox.Shader;
        MaterialHelper.SetShader(ref this._box, 0, ref shader);
    }
    
    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal void Draw() {
        Rlgl.DisableBackfaceCulling();
        Rlgl.DisableDepthMask();
        
        ModelHelper.DrawModel(this._box, Vector3.Zero, 1, this.Color);
        
        Rlgl.EnableBackfaceCulling();
        Rlgl.EnableDepthMask();
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            ModelHelper.Unload(this._box);
            TextureHelper.Unload(this._cubeMap);
        }
    }
}