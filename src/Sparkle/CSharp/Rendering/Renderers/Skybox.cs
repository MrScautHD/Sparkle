using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Rendering.Renderers;

public class Skybox : Disposable {

    public readonly Image CubeMap;
    
    private Texture2D _cubeMap;
    private Model _box;

    /// <summary>
    /// Represents a skybox in a 3D scene.
    /// </summary>
    public Skybox(Image cubeMap) {
        this.CubeMap = cubeMap;
    }

    protected internal void Init() {
        this._cubeMap = TextureHelper.LoadCubemap(this.CubeMap, CubemapLayout.AutoDetect);
        this._box = ModelHelper.LoadFromMesh(MeshHelper.GenCube(1, 1, 1));
        MaterialHelper.SetTexture(ref this._box, 0, MaterialMapIndex.Cubemap, ref this._cubeMap);

        Shader shader = EffectRegistry.Skybox.Shader;
        MaterialHelper.SetShader(ref this._box, 0, ref shader);
    }
    
    protected internal void Draw() {
        Rlgl.DisableBackfaceCulling();
        Rlgl.DisableDepthMask();

        ModelHelper.DrawModel(this._box, SceneManager.MainCam3D!.Position, 1, Color.White);
        
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