using System.Numerics;
using Bliss.CSharp.Camera.Dim2;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Scenes;

namespace Sparkle.Test.CSharp.Dim2D;

public class TestScene2D : Scene {
    
    public TestScene2D(string name) : base(name, SceneType.Scene2D) { }

    protected override void Init() {
        base.Init();
        
        // CAMERA
        Rectangle size = new Rectangle(0, 0, Game.Instance.MainWindow.GetWidth(), Game.Instance.MainWindow.GetHeight());
        Camera2D camera3D = new Camera2D(Vector2.Zero, Vector2.Zero, size, CameraFollowMode.Custom);
        this.AddEntity(camera3D);
        
        // PLAYER
        Entity player = new Entity(new Transform() { Translation = new Vector3(0, 0, 0) });
        player.AddComponent(new Sprite(ContentRegistry.PlayerSprite, Vector2.Zero));
        this.AddEntity(player);
    }
}