using System.Numerics;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.Test.CSharp.Dim3D;

public class TestScene3D : Scene {
    
    public TestScene3D(string name) : base(name, SceneType.Scene3D) { }

    protected override void Init() {
        base.Init();
        
        // CAMERA
        float aspectRatio = (float) Game.Instance.MainWindow.GetWidth() / (float) Game.Instance.MainWindow.GetHeight();
        Camera3D camera3D = new Camera3D(new Vector3(10, 10, 10), Vector3.UnitY, aspectRatio, mode: CameraMode.Orbital);
        this.AddEntity(camera3D);
        
        // PLAYER
        Entity player = new Entity(new Transform());
        player.AddComponent(new ModelRenderer(ContentRegistry.PlayerModel, Vector3.Zero));
        this.AddEntity(player);
    }

    protected override void Update(double delta) {
        base.Update(delta);

        if (Input.IsKeyDown(KeyboardKey.Number1)) {
            this.SetFilterEffect(ContentRegistry.GrayScaleEffect);
        }

        if (Input.IsKeyDown(KeyboardKey.Number2)) {
            this.SetFilterEffect(null);
        }
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        context.ImmediateRenderer.DrawGird(context.CommandList, framebuffer.OutputDescription, new Transform(), 50, 1, Color.Gray);
    }
}