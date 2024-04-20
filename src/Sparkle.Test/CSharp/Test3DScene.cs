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
using Sparkle.CSharp.Rendering.Renderers;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Terrain;

namespace Sparkle.Test.CSharp;

public class Test3DScene : Scene {

    public MarchingCubes MarchingCubes;
    public List<MarchingCubesChunk> MarchingCubesChunks;

    public Particle Particle;

    public Test3DScene(string name) : base(name, SceneType.Scene3D) {
        this.MarchingCubes = new MarchingCubes(123, 16, 16, 0.007F, 0.5F, false);
        this.MarchingCubesChunks = new List<MarchingCubesChunk>();
    }
    
    protected override void Init() {
        int chunks = 1;
        
        for (int i = 0; i < chunks * 16; i += 16) {
            for (int j = 0; j < chunks * 16; j += 16) {
                MarchingCubesChunk chunk = new MarchingCubesChunk(this.MarchingCubes, new Vector3(i, 0, j), 16, 16);
                chunk.Generate();

                this.MarchingCubesChunks.Add(chunk);
            }
        }
        
        // SKYBOX
        this.SetSkybox(new Skybox(TestGame.Skybox, Color.Green));
        
        // CAMERA
        Vector3 pos = new Vector3(10.0f, 10.0f, 10.0f);
        Cam3D cam3D = new Cam3D(pos, Vector3.Zero, Vector3.UnitY, 70, CameraProjection.Perspective, CameraMode.Free);
        this.AddEntity(cam3D);
        
        // LIGHTS
        Entity light = new Entity(new Vector3(1, 3, 0));
        light.AddComponent(new Light(EffectRegistry.Pbr, PbrEffect.LightType.Point, Vector3.Zero, Vector3.Zero, Color.Red));
        this.AddEntity(light);
        
        // PARTICLE
        //this.Particle = new Particle(TestGame.Gif.Texture, new Vector3(40, 10, 0), new Vector2(2, 2), 0, Color.White);
        
        // TEST ENTITIES
        for (int x = 0; x < 12; x++) {
            for (int z = 0; z < 12; z++) {
                TestEntity testEntity = new TestEntity(new Vector3(x * 2, 1, z * 2));
                this.AddEntity(testEntity);
            }
        }
        
        // GROUND
        Entity ground = new Entity(new Vector3(0, -2, 0));
        
        List<Shape> shapes = new List<Shape>();
        shapes.Add(new BoxShape(100000, 1, 100000));
        
        ground.AddComponent(new RigidBody(shapes, Vector3.Zero, true, true));
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
        ModelHelper.DrawCube(SceneManager.ActiveCam3D!.Target, 2, 2, 2, Color.Red);

        foreach (MarchingCubesChunk chunk in this.MarchingCubesChunks) {
            Vector3 startPos = new Vector3(chunk.Position.X, SceneManager.ActiveCam3D.Position.Y - 50, chunk.Position.Z);
            Vector3 endPos = new Vector3(chunk.Position.X, SceneManager.ActiveCam3D.Position.Y + 50, chunk.Position.Z);

            ModelHelper.DrawLine3D(startPos, endPos, Color.Red);
            ModelHelper.DrawLine3D(new Vector3(startPos.X + 8, startPos.Y, startPos.Z + 8), new Vector3(endPos.X + 8, endPos.Y, endPos.Z + 8), Color.Red);
            ModelHelper.DrawLine3D(new Vector3(startPos.X + 8, startPos.Y, startPos.Z + 0), new Vector3(endPos.X + 8, endPos.Y, endPos.Z + 0), Color.Red);
            ModelHelper.DrawLine3D(new Vector3(startPos.X + 0, startPos.Y, startPos.Z + 8), new Vector3(endPos.X + 0, endPos.Y, endPos.Z + 8), Color.Red);
            
            ModelHelper.DrawModel(chunk.Model, Vector3.Zero, 1, Color.DarkGreen);
        }
        
        Graphics.EndShaderMode();
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        
        if (disposing) {
            foreach (MarchingCubesChunk chunk in this.MarchingCubesChunks) {
                chunk.Dispose();
            }
        }
    }
}