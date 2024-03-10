using System.Numerics;
using Jitter2.Collision.Shapes;
using Raylib_cs;
using Sparkle.CSharp;
using Sparkle.CSharp.Effects.Types;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.Scenes;

namespace Sparkle.Test.CSharp;

public class Test3DScene : Scene {
    
    public Test3DScene(string name) : base(name, SceneType.Scene3D) { }
    
    protected override void Init() {
        
        // CAMERA
        Vector3 pos = new Vector3(10.0f, 10.0f, 10.0f);
        Cam3D cam3D = new Cam3D(pos, Vector3.Zero, Vector3.UnitY, 70, CameraProjection.Perspective, CameraMode.Free);
        this.AddEntity(cam3D);

        Entity light = new Entity(new Vector3(1, 3, 0));
        light.AddComponent(new Light(EffectRegistry.Pbr, PbrEffect.LightType.Point, Vector3.Zero, Color.Red));
        this.AddEntity(light);
        
        for (int i = 0; i < 12; i++) {
            for (int j = 0; j < 12; j++) {
                TestEntity test = new TestEntity(new Vector3(i * 2, 30, j * 2));
                this.AddEntity(test);
            }
        }

        // GROUND
        Entity ground = new Entity(new Vector3(0, -2, 0));
        
        List<Shape> shapes = new List<Shape>();
        shapes.Add(new BoxShape(100000, 1, 100000));
        
        ground.AddComponent(new RigidBody(shapes, true, true));
        this.AddEntity(ground);
    }

    protected override void Update() {
        base.Update();
        
        if (Input.IsKeyPressed(KeyboardKey.E)) {
            GuiManager.SetGui(new TestGui("Sparkle.Test"));
        }

        if (Input.IsKeyPressed(KeyboardKey.R)) {
            GuiManager.SetGui(null);
        }
    }

    protected override void Draw() {
        base.Draw();
        Graphics.BeginShaderMode(EffectRegistry.Pbr.Shader);
        
        ModelHelper.DrawGrid(100, 1);
        ModelHelper.DrawCube(SceneManager.MainCam3D!.Target, 2, 2, 2, Color.Red);
        
        Graphics.EndShaderMode();
    }
}