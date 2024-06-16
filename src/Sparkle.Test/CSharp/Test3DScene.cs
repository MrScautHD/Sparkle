using System.Numerics;
using Jitter2.Collision.Shapes;
using Raylib_CSharp.Camera.Cam3D;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Sparkle.CSharp.Effects.Types;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Renderers;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Terrain;
using Logger = Sparkle.CSharp.Logging.Logger;

namespace Sparkle.Test.CSharp;

public class Test3DScene : Scene {

    public MarchingCubes MarchingCubes;
    public List<MarchingCubesChunk> MarchingCubesChunks;

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
        this.SetSkybox(new Skybox(TestGame.Skybox, Color.Blue));
        
        // FXAA
        this.SetFilterEffect(EffectRegistry.Blur);
        
        // CAMERA
        Vector3 pos = new Vector3(10.0f, 10.0f, 10.0f);
        Cam3D cam3D = new Cam3D(pos, Vector3.Zero, Vector3.UnitY, 70, CameraProjection.Perspective, CameraMode.Free);
        this.AddEntity(cam3D);
        
        // LIGHTS
        Entity light = new Entity(new Vector3(1, 6, 0));
        light.AddComponent(new Light(EffectRegistry.Pbr, PbrEffect.LightType.Point, Vector3.Zero, Vector3.Zero, Color.Red, 4, true));
        this.AddEntity(light);
        
        Entity light2 = new Entity(new Vector3(10, 6, 0));
        light2.AddComponent(new Light(EffectRegistry.Pbr, PbrEffect.LightType.Point, Vector3.Zero, Vector3.Zero, Color.Blue, 4, true));
        this.AddEntity(light2);
        
        // TEST ENTITIES
        for (int x = -3; x < 3; x++) {
            for (int z = -3; z < 3; z++) {
                TestEntity testEntity = new TestEntity(new Vector3(x * 2.5F, 1, z * 2.5F));
                this.AddEntity(testEntity);
            }
        }
        
        // GROUND
        Entity ground = new Entity(new Vector3(0, -2, 0));
        
        List<Shape> shapes = new List<Shape>();
        shapes.Add(new BoxShape(1000, 1, 1000));
        
        ground.AddComponent(new RigidBody3D(shapes, true, true));
        this.AddEntity(ground);
    }
    
    protected override void Update() {
        base.Update();

        if (Input.IsKeyPressed(KeyboardKey.O)) {
            this.GetEntity(2).GetComponent<Light>().Enabled = false;
            Logger.Error(this.GetEntity(2).GetComponent<Light>().Id + "");
        }
        
        if (Input.IsKeyPressed(KeyboardKey.I)) {
            this.GetEntity(2).GetComponent<Light>().Enabled = true;
            Logger.Error(this.GetEntity(2).GetComponent<Light>().Id + "");
        }
        
        if (Input.IsKeyPressed(KeyboardKey.E)) {
            GuiManager.SetGui(new TestGui("Sparkle.Test"));
        }

        if (Input.IsKeyPressed(KeyboardKey.R)) {
            GuiManager.SetGui(null);
        }
        
        // PARTICLE
        //for (int i = 0; i < 1; i++) {
        //    int x = new Random().Next(-2, 2);
        //    int z = new Random().Next(-2, 2);
        //    int lifeTime = new Random().Next(5, 10);
        //    
        //    ParticleData data = new ParticleBuilder()
        //        .SetEffect(EffectRegistry.DiscardAlpha)
        //        .SetSizeOverLifeTime(new Vector2(2, 2), Vector2.Zero)
        //        .SetRotationOverLifeTime(0, 360)
        //        .SetColorOverLifeTime(Color.White, Color.Red)
        //        .SetVelocityOverLifeTime(new Vector3(0, -1, 0), new Vector3(0, -10 ,0))
        //        .Build();
        //    
        //    Particle particle = new Particle(TestGame.Gif.Texture, new Vector3(SceneManager.ActiveCam3D!.Position.X + x, SceneManager.ActiveCam3D!.Position.Y - 6, SceneManager.ActiveCam3D!.Position.Z + z), lifeTime, data);
        //    this.AddParticle(particle);
        //}
    }
    
    protected override void Draw() {
        base.Draw();
        Graphics.BeginShaderMode(EffectRegistry.Pbr.Shader);
        
        Graphics.DrawGrid(100, 1);
        Graphics.DrawCube(SceneManager.ActiveCam3D!.Target, 2, 2, 2, Color.Red);

        foreach (MarchingCubesChunk chunk in this.MarchingCubesChunks) {
            Vector3 startPos = new Vector3(chunk.Position.X, SceneManager.ActiveCam3D.Position.Y - 50, chunk.Position.Z);
            Vector3 endPos = new Vector3(chunk.Position.X, SceneManager.ActiveCam3D.Position.Y + 50, chunk.Position.Z);
        
            Graphics.DrawLine3D(startPos, endPos, Color.Red);
            Graphics.DrawLine3D(new Vector3(startPos.X + 8, startPos.Y, startPos.Z + 8), new Vector3(endPos.X + 8, endPos.Y, endPos.Z + 8), Color.Red);
            Graphics.DrawLine3D(new Vector3(startPos.X + 8, startPos.Y, startPos.Z + 0), new Vector3(endPos.X + 8, endPos.Y, endPos.Z + 0), Color.Red);
            Graphics.DrawLine3D(new Vector3(startPos.X + 0, startPos.Y, startPos.Z + 8), new Vector3(endPos.X + 0, endPos.Y, endPos.Z + 8), Color.Red);
            
            Graphics.DrawModel(chunk.Model, Vector3.Zero, 1, Color.DarkGreen);
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