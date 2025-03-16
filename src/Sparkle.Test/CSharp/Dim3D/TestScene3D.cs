using System.Numerics;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Scenes;

namespace Sparkle.Test.CSharp.Dim3D;

public class TestScene3D : Scene {
    
    public TestScene3D(string name) : base(name, SceneType.Scene3D) {
        
    }

    protected override void Init() {
        base.Init();

        float aspectRatio = (float) Game.Instance.MainWindow.GetWidth() / (float) Game.Instance.MainWindow.GetHeight();
        
        // CAMERA
        Camera3D camera3D = new Camera3D(new Vector3(10, 10, 10), Vector3.UnitY, aspectRatio, mode: CameraMode.Orbital);
        this.AddEntity(camera3D);
        
        // PLAYER
        Entity player = new Entity(new Transform());
        player.AddComponent(new ModelRenderer(ContentRegistry.PlayerModel, Vector3.Zero));
        this.AddEntity(player);
    }
}