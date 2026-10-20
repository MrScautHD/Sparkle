using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Geometry.Models;
using Bliss.CSharp.Graphics.Pipelines;
using Bliss.CSharp.Graphics.Rendering;
using Bliss.CSharp.Graphics.Rendering.Renderers;
using Bliss.CSharp.Graphics.VertexTypes;
using Bliss.CSharp.Images;
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Interact.Mice;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Effects.Terrain;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Graphics.Rendering.Forward;
using Sparkle.CSharp.Scenes;
using Sparkle.CSharp.Terrain;
using Sparkle.CSharp.Terrain.Chunks;
using Sparkle.CSharp.Terrain.Generators;
using Sparkle.CSharp.Terrain.Painting;
using Veldrith;

namespace Sparkle.Test.CSharp.Dim3D;

public class LightTestScene : Scene {
    
    public Texture2D OldCarDTexture { get; private set; }
    public Texture2D OldCarNTexture { get; private set; }
    public Texture2D OldCarMraTexture { get; private set; }
    public Texture2D OldCarETexture { get; private set; }
    
    public Texture2D RoadATexture { get; private set; }
    public Texture2D RoadNTexture { get; private set; }
    public Texture2D RoadMraTexture { get; private set; }
    
    public Model OldCarModel { get; private set; }
    public Model PlaneModel { get; private set; }
    
    public MultiInstanceRenderer PlaneMultiInstanceRenderer { get; private set; }
    
    public SkyBox CloudySkybox { get; private set; }
    
    private const float _brushRadius = 8.0F;
    private const float _brushStrength = 4.0F;
    private const float _brushMaxDistance = 200.0F;
    private const float _brushStepSize = 0.5F;
    
    private ITerrain<IHeightmapChunk>? _terrain;
    
    public LightTestScene() : base("Light-Test-Scene", SceneType.Scene3D, (graphicsDevice) => new PbrForwardRenderer(graphicsDevice, null, Color.Black, enableSsr: true)) { }
    
    protected override void Load(ContentManager content) {
        base.Load(content);
        
        // Textures:
        this.OldCarDTexture = content.Load(new TextureContent("content/old_car_d.png"), false);
        this.OldCarNTexture = content.Load(new TextureContent("content/old_car_n.png"), false);
        this.OldCarMraTexture = content.Load(new TextureContent("content/old_car_mra.png"), false);
        this.OldCarETexture = content.Load(new TextureContent("content/old_car_e.png"), false);
        
        this.RoadATexture = content.Load(new TextureContent("content/road_a.png"), false);
        this.RoadNTexture = content.Load(new TextureContent("content/road_n.png"), false);
        this.RoadMraTexture = content.Load(new TextureContent("content/road_mra.png"), false);
        
        // Models:
        this.OldCarModel = content.Load(new ModelContent("content/old_car_new.glb", false).Do(model => {
            foreach (IMesh mesh in model.Meshes) {
                mesh.Material.Effect = mesh.IsSkinned ? GlobalGraphicsAssets.SkinnedPbrModelEffect : GlobalGraphicsAssets.PbrModelEffect;
                
                // Set albedo map.
                mesh.Material.SetMapTexture(MaterialMapType.Albedo, this.OldCarDTexture);
                mesh.Material.SetMapColor(MaterialMapType.Albedo, Color.White);
                mesh.Material.SetMapValue(MaterialMapType.Albedo, 1.0F);
                
                // Set normal map.
                mesh.Material.AddMaterialMap(MaterialMapType.Normal, 1, new MaterialMap {
                    Texture = this.OldCarNTexture,
                    Color = Color.White,
                    Value = 1.0F
                });
                
                // Set metallic map.
                mesh.Material.AddMaterialMap(MaterialMapType.Metallic, 2, new MaterialMap {
                    Texture = this.OldCarMraTexture,
                    Color = Color.White,
                    Value = 1.0F
                });
                
                // Set roughness map.
                mesh.Material.AddMaterialMap(MaterialMapType.Roughness, 3, new MaterialMap {
                    Color = Color.White,
                    Value = 0.0F
                });
                
                // Set occlusion map.
                mesh.Material.AddMaterialMap(MaterialMapType.Occlusion, 4, new MaterialMap {
                    Color = Color.White,
                    Value = 1.0F
                });
                
                // Set emission map.
                mesh.Material.AddMaterialMap(MaterialMapType.Emission, 5, new MaterialMap {
                    Texture = this.OldCarETexture,
                    Color = Color.White,
                    Value = 1.0F
                });
                
                // Set reflection map.
                mesh.Material.AddMaterialMap(MaterialMapType.Reflection, 6, new MaterialMap {
                    Color = Color.White,
                    Value = 1.0F
                });
                
                // Set SSR reflection map.
                mesh.Material.AddMaterialMap("SsrReflection", 7, new MaterialMap {
                    Color = Color.White,
                    Value = 1.0F
                });
                
                mesh.Material.RenderMode = RenderMode.Solid;
                mesh.GenTangents();
            }
        }), false);
        
        this.PlaneModel = content.Load(new ModelContent("content/plane.glb", false).Do(model => {
            foreach (IMesh mesh in model.Meshes) {
                mesh.Material.Effect = mesh.IsSkinned ? GlobalGraphicsAssets.SkinnedPbrModelEffect : GlobalGraphicsAssets.PbrModelEffect;
                
                // Set albedo map.
                mesh.Material.SetMapTexture(MaterialMapType.Albedo, this.RoadATexture);
                mesh.Material.SetMapColor(MaterialMapType.Albedo, Color.White);
                mesh.Material.SetMapValue(MaterialMapType.Albedo, 1.0F);
                
                // Set normal map.
                mesh.Material.AddMaterialMap(MaterialMapType.Normal, 1, new MaterialMap {
                    Texture = this.RoadNTexture,
                    Color = Color.White,
                    Value = 1.0F
                });
                
                // Set metallic map.
                mesh.Material.AddMaterialMap(MaterialMapType.Metallic, 2, new MaterialMap {
                    Texture = this.RoadMraTexture,
                    Color = Color.White,
                    Value = 1.0F
                });
                
                // Set roughness map.
                mesh.Material.AddMaterialMap(MaterialMapType.Roughness, 3, new MaterialMap {
                    Color = Color.White,
                    Value = 1.0F
                });
                
                // Set occlusion map.
                mesh.Material.AddMaterialMap(MaterialMapType.Occlusion, 4, new MaterialMap {
                    Color = Color.White,
                    Value = 1.0F
                });
                
                // Set emission map.
                mesh.Material.AddMaterialMap(MaterialMapType.Emission, 5, new MaterialMap {
                    Texture = GlobalResource.DefaultModelTexture,
                    Color = Color.Black,
                    Value = 0.0F
                });
                
                // Set reflection map.
                mesh.Material.AddMaterialMap(MaterialMapType.Reflection, 6, new MaterialMap {
                    Color = Color.White,
                    Value = 1.0F
                });
                
                // Set SSR reflection map.
                mesh.Material.AddMaterialMap("SsrReflection", 7, new MaterialMap {
                    Color = Color.White,
                    Value = 0.2F
                });
                
                mesh.Material.RenderMode = RenderMode.Solid;
                mesh.GenTangents();
            }
        }), false);
        
        // Plane instance renderer.
        this.PlaneMultiInstanceRenderer = new MultiInstanceRenderer(this.PlaneModel);
        
        foreach (IMesh planeMesh in this.PlaneMultiInstanceRenderer.Meshes) {
            this.PlaneMultiInstanceRenderer.GetRenderableMaterialByMesh(planeMesh).Effect = GlobalGraphicsAssets.PbrModelEffect.GetEffectVariant([("USE_INSTANCING", "1")]).Effect;
            
            if (planeMesh is Mesh<Vertex3D> mesh) {
                mesh.MeshData.VertexFormat = new VertexFormat(
                    Vertex3D.VertexLayout.Name,
                    [
                        ..Vertex3D.VertexLayout.Layouts,
                        ..Vertex3D.InstanceMatrixLayout.Layouts
                    ]
                );
            }
        }
        
        // Skybox's:
        this.CloudySkybox = new SkyBox(content.GraphicsDevice, content.Load(new CubemapContent("content/skybox.png"), false));
    }
    
    protected override void Init() {
        base.Init();
        
        if (this.Renderer is PbrForwardRenderer forwardRenderer) {
            forwardRenderer.FogDensity = 0.018F;
            forwardRenderer.FogFalloff = 0.03F;
            forwardRenderer.FogStartDistance = 0;
            forwardRenderer.FogEndDistance = 70;
            forwardRenderer.FogColor = new Vector4(0.65F, 0.72F, 0.78F, 1.0F);
        }
        
        // RELATIVE MOUSE MODE.
        Input.EnableRelativeMouseMode();
        
        // Set skybox.
        this.Renderer.SkyBox = this.CloudySkybox;
        
        // CAMERA
        float aspectRatio = (float) GlobalGraphicsAssets.Window.GetWidth() / (float) GlobalGraphicsAssets.Window.GetHeight();
        Camera3D camera3D = new Camera3D(new Vector3(10, 10, 10), Vector3.UnitY, aspectRatio, mode: CameraMode.Free);
        this.AddEntity(camera3D);
        
        // CAR
        Entity car = new Entity(new Transform() { Translation = new Vector3(0, 5, 0)} );
        RigidBody3D carBody = new RigidBody3D(new TransformedShape(new BoxShape(4, 2, 8), new Vector3(0, 0.5F, 0))) {
            DrawDebug = false,
            DebugDrawColor = Color.Green
        };
        ModelRenderer carModelRenderer = new ModelRenderer(this.OldCarModel, -Vector3.UnitY);
        car.AddComponent(carBody);
        car.AddComponent(carModelRenderer);
        this.AddEntity(car);
        
        // ROAD
        const int gridSize = 16;
        const float tileSize = 40.0F;
        float offset = (gridSize - 1) * tileSize / 2.0F;
        
        for (int x = 0; x < gridSize; x++) {
            for (int z = 0; z < gridSize; z++) {
                Entity road = new Entity(new Transform() {
                    Translation = new Vector3(x * tileSize - offset, 0, z * tileSize - offset),
                    Scale = new Vector3(20, 20, 20)
                });
                
                road.AddComponent(new RigidBody3D(new BoxShape(96, 1, 96), MassInertiaUpdateMode.Update, MotionType.Static) {
                    DrawDebug = false,
                    DebugDrawColor = Color.Green
                });
                
                //road.AddComponent(new InstancedRenderProxy(this.PlaneMultiInstanceRenderer, Vector3.Zero));
                this.AddEntity(road);
            }
        }
        
        // LIGHT SUN
        Entity sunLight = new Entity(new Transform() { Translation = new Vector3(0, 0, 0) }, "sun");
        Light sunLightComp = Light.CreateDirectional(new Color(255, 244, 214, 255), 10.0F);
        sunLightComp.CastShadows = true;
        //sunLightComp.DebugDrawEnabled = true;
        sunLight.AddComponent(sunLightComp);
        this.AddEntity(sunLight);
        
        // LIGHT MOON
        Entity moonLight = new Entity(new Transform() { Translation = new Vector3(0, 0, 0) }, "moon");
        Light moonLightComp = Light.CreateDirectional(new Color(120, 160, 255, 255), 0.2F);
        //moonLightComp.DebugDrawEnabled = true;
        moonLight.AddComponent(moonLightComp);
        this.AddEntity(moonLight);
        
        // LIGHT RED
        Entity redLight = new Entity(new Transform() { Translation = new Vector3(7, 5, 7) }, "red");
        Light redLightComp = Light.CreatePoint(Vector3.Zero, Color.Red, 54.2F, 160);
        redLightComp.DebugDrawEnabled = true;
        redLight.AddComponent(redLightComp);
        this.AddEntity(redLight);
        
        // LIGHT GREEN
        Entity greenLight = new Entity(new Transform() { Translation = new Vector3(-7, 5, 7) }, "green");
        Light greenLightComp = Light.CreatePoint(Vector3.Zero, Color.Green, 54.2F, 160);
        greenLightComp.DebugDrawEnabled = true;
        greenLight.AddComponent(greenLightComp);
        this.AddEntity(greenLight);
        
        // LIGHT BLUE
        Entity blueLight = new Entity(new Transform() { Translation = new Vector3(-7, 5, -7) }, "blue");
        Light blueLightComp = Light.CreatePoint(Vector3.Zero, Color.Blue, 54.2F, 160);
        blueLightComp.DebugDrawEnabled = true;
        blueLight.AddComponent(blueLightComp);
        this.AddEntity(blueLight);
        
        // LIGHT YELLOW
        Entity yellowLight = new Entity(new Transform() { Translation = new Vector3(7, 5, -7) }, "yellow");
        Light yellowLightComp = Light.CreatePoint(Vector3.Zero, Color.Yellow, 54.2F, 160);
        yellowLightComp.DebugDrawEnabled = true;
        yellowLight.AddComponent(yellowLightComp);
        this.AddEntity(yellowLight);
        
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
        
        Entity terrainEntity = new Entity(new Transform() { Translation = new Vector3(-4096.0F, -128.0F, -4096.0F) }, "terrain");
        terrainEntity.AddComponent(new Terrain3D<IHeightmapChunk>(this.CreateTerrainAsync, terrainSettings, Vector3.Zero, frustumCulling: true));
        this.AddEntity(terrainEntity);
    }
    
    protected override void Update(double delta) {
        base.Update(delta);
        
        Entity? sun = this.GetEntitiesWithTag("sun").FirstOrDefault();
        Entity? moon = this.GetEntitiesWithTag("moon").FirstOrDefault();
        
        if (sun != null) {
            sun.LocalTransform.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float) delta * 0.1F);
            
            Vector3 sunDirection = Vector3.Transform(Vector3.UnitZ, sun.LocalTransform.Rotation);
            float sunHeight = sunDirection.Y;
            float daylight = this.Smooth01((sunHeight + 0.35F) / 0.9F);
            float sunHorizonFade = this.Smooth01(sunHeight / 0.2F);
            
            Light? sunLight = sun.GetComponent<Light>();
            
            if (sunLight != null) {
                float sunAboveHorizon = Math.Clamp((sunHeight + 0.08F) / 0.18F, 0.0F, 1.0F);
                float sunNearHorizon = 1.0F - Math.Clamp(MathF.Abs(sunHeight) / 0.45F, 0.0F, 1.0F);
                float sunriseAmount = this.Smooth01(sunAboveHorizon) * this.Smooth01(sunNearHorizon);
    
                Vector3 daySunColor = new Vector3(255.0F, 244.0F, 214.0F);
                Vector3 sunriseSunColor = new Vector3(255.0F, 45.0F, 18.0F);
                Vector3 sunColor = Vector3.Lerp(daySunColor, sunriseSunColor, sunriseAmount);
    
                byte red = (byte) Math.Clamp(sunColor.X, 0.0F, 255.0F);
                byte green = (byte) Math.Clamp(sunColor.Y, 0.0F, 255.0F);
                byte blue = (byte) Math.Clamp(sunColor.Z, 0.0F, 255.0F);
    
                sunLight.Enabled = true;
                sunLight.Color = new Color(red, green, blue, 255);
                sunLight.Intensity = 10.0F * daylight * sunHorizonFade;
            }
            
            if (moon != null) {
                moon.LocalTransform.Rotation = sun.LocalTransform.Rotation * Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI);
                
                Light? moonLight = moon.GetComponent<Light>();
                if (moonLight != null) {
                    float moonHeight = -sunHeight;
                    float moonHorizonFade = this.Smooth01(moonHeight / 0.3F);
                    
                    moonLight.Enabled = true;
                    moonLight.Color = new Color(70, 95, 160, 255);
                    moonLight.Intensity = 1.0F * moonHorizonFade;
                }
            }
            
            if (this.Renderer is PbrForwardRenderer pbrRenderer) {
                float dayAmbient = 0.08F;
                float nightR = 0.012F;
                float nightG = 0.016F;
                float nightB = 0.03F;
                
                pbrRenderer.AmbientColor = new Vector4(
                    nightR + ((dayAmbient - nightR) * daylight),
                    nightG + ((dayAmbient - nightG) * daylight),
                    nightB + ((dayAmbient - nightB) * daylight),
                    1.0F
                );
                
                float skyBrightness = 4.0F + (251.0F * daylight);
                byte skyTint = (byte) Math.Clamp(skyBrightness, 0.0F, 255.0F);
                
                this.CloudySkybox.Color = new Color(skyTint, skyTint, skyTint, 255);
            }
        }
        
        if (Input.IsKeyPressed(KeyboardKey.Number1)) {
            this.GetEntitiesWithTag("red").FirstOrDefault()?.GetComponent<Light>()?.Enabled ^= true;
            this.GetEntitiesWithTag("red").FirstOrDefault()?.GetComponent<Light>()?.DebugDrawEnabled ^= true;
        }
        
        if (Input.IsKeyPressed(KeyboardKey.Number2)) {
            this.GetEntitiesWithTag("green").FirstOrDefault()?.GetComponent<Light>()?.Enabled ^= true;
            this.GetEntitiesWithTag("green").FirstOrDefault()?.GetComponent<Light>()?.DebugDrawEnabled ^= true;
        }
        
        if (Input.IsKeyPressed(KeyboardKey.Number3)) {
            this.GetEntitiesWithTag("blue").FirstOrDefault()?.GetComponent<Light>()?.Enabled ^= true;
            this.GetEntitiesWithTag("blue").FirstOrDefault()?.GetComponent<Light>()?.DebugDrawEnabled ^= true;
        }
        
        if (Input.IsKeyPressed(KeyboardKey.Number4)) {
            this.GetEntitiesWithTag("yellow").FirstOrDefault()?.GetComponent<Light>()?.Enabled ^= true;
            this.GetEntitiesWithTag("yellow").FirstOrDefault()?.GetComponent<Light>()?.DebugDrawEnabled ^= true;
        }
        
        // Toggle SSR.
        if (Input.IsKeyPressed(KeyboardKey.U)) {
            if (this.Renderer is PbrForwardRenderer pbrRenderer) {
                pbrRenderer.EnableSsr ^= true;
            }
        }
        
        if (Input.IsKeyPressed(KeyboardKey.U)) {
            Terrain3D<IHeightmapChunk>? terrain3D = this.GetEntitiesWithTag("terrain").First().GetComponent<Terrain3D<IHeightmapChunk>>();
            
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
        
        Camera3D? cam = SceneManager.ActiveCam3D;
        
        if (cam != null) {
            if (this._terrain?.Painter is TileTerrainPainter painter) {
                if (painter.Material.Effect is TerrainEffect terrainEffect) {
                    
                    // Convert camera world position into terrain local space
                    Vector3 terrainOffset = new Vector3(-4096.0F, -128.0F, -4096.0F);
                    Vector3 localCamPos = cam.Position - terrainOffset;
                    
                    if (!this._terrain.RaycastSurface(localCamPos, cam.GetForward(), _brushMaxDistance, _brushStepSize, out Vector3 hitPosition, out _)) {
                        terrainEffect.DisableBrushPreview(painter.Material);
                        return;
                    }
                    
                    terrainEffect.SetBrushPreview(painter.Material, hitPosition, _brushRadius, TerrainBrushType.Circle, Color.Red.ToRgbaFloatVec4(), 1.0F, 0.5F);
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
        Vector3 terrainOffset = new Vector3(-4096.0F, -128.0F, -4096.0F);
        Vector3 localCamPos = cam.Position - terrainOffset;
        
        if (!this._terrain.RaycastSurface(localCamPos, cam.GetForward(), _brushMaxDistance, _brushStepSize, out Vector3 hitPosition, out _)) {
            return;
        }
        
        float strength = (addMaterial ? _brushStrength : -_brushStrength) * (float) delta;
        //this._terrain.ApplyBrush(hitPosition, _brushRadius, strength, TerrainBrushType.Circle);
        this._terrain.Painter.ApplyTextureLayerBrush(hitPosition, _brushRadius, strength, 1, TerrainBrushType.Circle);
    }
    
    private async Task<ITerrain<IHeightmapChunk>> CreateTerrainAsync() {
        const int terrainWidth = 8192;
        const int terrainHeight = 256;
        const int terrainDepth = 8192;
        const int chunkSize = 256;
        const int surfaceHeight = 128;
        
        // Create chunk generator.
        FlatHeightmapGenerator chunkGenerator = new FlatHeightmapGenerator(chunkSize, surfaceHeight);
        
        // Create material.
        Material material = new Material(GlobalGraphicsAssets.PbrTileTerrainEffect) {
            BlendState = BlendStateDescription.SINGLE_ALPHA_BLEND,
            RenderMode = RenderMode.Cutout
        };
        
        // Set albedo map.
        material.AddMaterialMap(MaterialMapType.Albedo, 0, new MaterialMap {
            Texture = GlobalResource.DefaultModelTexture,
            Color = Color.White,
            Value = 1.0F
        });
        
        // Set normal map.
        material.AddMaterialMap(MaterialMapType.Normal, 1, new MaterialMap {
            Texture = GlobalResource.DefaultModelTexture,
            Color = Color.White,
            Value = 0.0F
        });
        
        // Set metallic map.
        material.AddMaterialMap(MaterialMapType.Metallic, 2, new MaterialMap {
            Texture = GlobalResource.DefaultModelTexture,
            Color = Color.White,
            Value = 0.0F
        });
        
        // Set roughness map.
        material.AddMaterialMap(MaterialMapType.Roughness, 3, new MaterialMap {
            Color = Color.White,
            Value = 1.0F
        });
        
        // Set occlusion map.
        material.AddMaterialMap(MaterialMapType.Occlusion, 4, new MaterialMap {
            Color = Color.White,
            Value = 1.0F
        });
        
        // Set emission map.
        material.AddMaterialMap(MaterialMapType.Emission, 5, new MaterialMap {
            Texture = GlobalResource.DefaultModelTexture,
            Color = Color.Black,
            Value = 0.0F
        });
        
        // Set reflection map.
        material.AddMaterialMap(MaterialMapType.Reflection, 6, new MaterialMap {
            Color = Color.White,
            Value = 0.0F
        });
        
        // Set SSR reflection map.
        material.AddMaterialMap("SsrReflection", 7, new MaterialMap {
            Color = Color.White,
            Value = 0.0F
        });
        
        TileTerrainPainter painter = new TileTerrainPainter(GlobalGraphicsAssets.GraphicsDevice, material, terrainWidth, terrainDepth, 32, 8, 0, true, true);
        painter.AddLayer(ContentRegistry.TerrainGrass);
        painter.AddLayer(ContentRegistry.TerrainDirt);
        painter.AddLayer(ContentRegistry.TerrainRock);
        painter.AddLayer(new Image(32, 32, new Color(0, 0, 0, 0)));
        
        // Create/Load terrain.
        HeightmapTerrain terrain = await HeightmapTerrain.CreateAsync(chunkGenerator, painter, terrainWidth, terrainHeight, terrainDepth, chunkSize);
        
        this._terrain = terrain;
        return terrain;
    }
    
    private float Smooth01(float value) {
        value = Math.Clamp(value, 0.0F, 1.0F);
        return value * value * (3.0F - (2.0F * value));
    }
    
    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        
        if (disposing) {
            this.CloudySkybox.Dispose();
        }
    }
}