using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Terrain;
using Sparkle.CSharp.Terrain.Generators;
using Sparkle.CSharp.Terrain.Heightmap;
using Veldrid;

namespace Sparkle.Test.CSharp.Dim3D;

public class TerrainScene : Scene {
    
    private const float BrushRadius = 8.0F;
    private const float BrushStrength = 4.0F;
    private const float BrushMaxDistance = 200.0F;
    private const float BrushStepSize = 0.5F;
    
    private ITerrain? _terrain;
    
    public TerrainScene() : base("Terrain3D-Scene", SceneType.Scene3D) { }
    
    protected override void Init() {
        base.Init();
        
        // Relative mouse mode.
        Input.EnableRelativeMouseMode();
        
        // Create camera.
        float aspectRatio = (float) GlobalGraphicsAssets.Window.GetWidth() / (float) GlobalGraphicsAssets.Window.GetHeight();
        Camera3D camera3D = new Camera3D(new Vector3(0, 6, 3), Vector3.UnitY, aspectRatio, mode: CameraMode.Free, farPlane: 10000F) {
            MovementSpeed = 10
        };
        this.AddEntity(camera3D);
        
        // Create terrain.
        TerrainSettings terrainSettings = new TerrainSettings() {
            EnableLod = true,
            LodDistances = [400.0F, 600.0F, 900.0F, 1500.0F, 2500],
            LodHysteresis = 0.2F,
            CullChunksBeyondLastLod = false,
            MaxChunkBuildsPerFrame = 24,
            MaxConcurrentChunkBuilds = 12,
            MaxChunkUploadsPerFrame = 16
        };
        
        Entity terrainEntity = new Entity(new Transform() { Translation = new Vector3(0.0F, -64.0F, 0.0F)}, "terrain");
        terrainEntity.AddComponent(new Terrain3D(this.CreateTerrainAsync, terrainSettings, Vector3.Zero, frustumCulling: true));
        this.AddEntity(terrainEntity);
    }
    
    protected override void Update(double delta) {
        base.Update(delta);
        
        if (Input.IsKeyPressed(KeyboardKey.U)) {
            Terrain3D? terrain3D = this.GetEntitiesWithTag("terrain").First().GetComponent<Terrain3D>();
            
            if (terrain3D != null) {
                
                // Toggle debug drawing.
                terrain3D.DebugDrawEnabled = !terrain3D.DebugDrawEnabled;
                
                // Toggle wireframe.
                if (GlobalGraphicsAssets.GraphicsDevice.Features.FillModeWireframe) {
                    RasterizerStateDescription rasterizerState = terrain3D.Terrain.Material.RasterizerState;
                    rasterizerState.FillMode = rasterizerState.FillMode == PolygonFillMode.Solid ? PolygonFillMode.Wireframe : PolygonFillMode.Solid;
                    terrain3D.Terrain.Material.RasterizerState = rasterizerState;
                }
                else {
                    Logger.Warn("This backend to not support fill mode [Wireframe]");
                }
            }
        }
    }
    
    protected override void FixedUpdate(double delta) {
        base.FixedUpdate(delta);
        
        if (this._terrain == null) {
            return;
        }
        
        bool addMaterial = Input.IsMouseButtonDown(MouseButton.Left);
        bool removeMaterial = Input.IsMouseButtonDown(MouseButton.Right);
        
        if (!addMaterial && !removeMaterial) {
            return;
        }
        
        Camera3D? cam = SceneManager.ActiveCam3D;
        
        if (cam == null) {
            return;
        }
        
        // Convert camera world position into terrain local space
        Vector3 terrainOffset = new Vector3(0.0F, -64, 0.0F);
        Vector3 localCamPos = cam.Position - terrainOffset;
        
        if (!this._terrain.RaycastSurface(localCamPos, cam.GetForward(), BrushMaxDistance, BrushStepSize, out Vector3 hitPosition, out _)) {
            return;
        }
        
        float strength = (addMaterial ? BrushStrength : -BrushStrength) * (float) delta;
        this._terrain.ApplyBrush(hitPosition, BrushRadius, strength);
    }
    
    private async Task<ITerrain> CreateTerrainAsync() {
        const int terrainWidth = 8192;
        const int terrainHeight = 128;
        const int terrainDepth = 8192;
        const int chunkSize = 256;
        const int surfaceHeight = 64;
        
        // Create chunk generator.
        FlatChunkGenerator chunkGenerator = new FlatChunkGenerator(chunkSize, terrainHeight, surfaceHeight);
        
        // Create material.
        Material material = new Material(GlobalResource.DefaultModelEffect);
        
        material.AddMaterialMap(MaterialMapType.Albedo, 0, new MaterialMap {
            Texture = GlobalResource.DefaultModelTexture,
            Color = Color.White
        });
        
        // Create/Load terrain.
        HeightmapTerrain terrain = await HeightmapTerrain.CreateAsync(chunkGenerator, material, terrainWidth, terrainHeight, terrainDepth, chunkSize);
        
        this._terrain = terrain;
        return terrain;
    }
}
