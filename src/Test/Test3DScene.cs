using System.Numerics;
using Raylib_cs;
using Sparkle;
using Sparkle.CSharp;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.SceneManagement;

namespace Test; 

public class Test3DScene : Scene {
    
    public Test3DScene(string name) : base(name) { }
    
    protected override void Init() {
        // CAMERA
        Vector3 pos = new Vector3(10.0f, 10.0f, 10.0f);
        Cam3D cam3D = new Cam3D(pos, Vector3.Zero, Vector3.UnitY, 70, CameraProjection.CAMERA_PERSPECTIVE, CameraMode.CAMERA_FREE);
        this.AddEntity(cam3D);

        Entity light = new Entity(new Vector3(1, 3, 0));
        light.AddComponent(new Light(Light.LightType.Point, new Vector3(0, 2, 0), Color.RED, Color.BLUE));
        this.AddEntity(light);

        // LIGHT
        /*
        Entity light = new Entity(Vector3.Zero);
        light.AddComponent(new Light(Light.LightType.Pointed, Vector3.Zero, Color.RED));
        this.AddEntity(light);*/

        for (int i = 0; i < 12; i++) {
            for (int j = 0; j < 12; j++) {
                TestEntity test = new TestEntity(new Vector3(i, 3, j));
                this.AddEntity(test);
            }
        }
        
        // TEST ENTITY
        //TestEntity entity = new TestEntity(new Vector3(0, 20, 0));
        //this.AddEntity(entity);

        // GROUND
        this.AddEntity(new GroundEntity(Vector3.Zero));
    }

    protected override void Update() {
        base.Update();
        
        if (Input.IsKeyPressed(KeyboardKey.KEY_E)) {
            GuiManager.SetGui(new TestGui("Test"));
        }

        if (Input.IsKeyPressed(KeyboardKey.KEY_R)) {
            GuiManager.SetGui(null);
        }
    }

    protected override void Draw() {
        base.Draw();
        
        SceneManager.MainCam3D!.BeginMode3D();
        Graphics.BeginShaderMode(ShaderRegistry.Pbr);
        
        ModelHelper.DrawGrid(100, 1);
        ModelHelper.DrawCube(SceneManager.MainCam3D.Target, 2, 2, 2, Color.RED);
        
        Graphics.EndShaderMode();
        SceneManager.MainCam3D.EndMode3D();
    }
}