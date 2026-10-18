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
using Bliss.CSharp.Interact;
using Bliss.CSharp.Interact.Keyboards;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Textures;
using Bliss.CSharp.Transformations;
using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Rendering;
using Sparkle.CSharp.Graphics.Rendering.Forward;
using Sparkle.CSharp.Scenes;

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
            this.PlaneMultiInstanceRenderer.GetRenderableMaterialByMesh(planeMesh).Effect = GlobalGraphicsAssets.PbrModelEffect.GetEffectVariant(["USE_INSTANCING"]).Effect;
            
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
            forwardRenderer.FogEndDistance = 50;
            forwardRenderer.FogColor = new Vector4(0.65F, 0.72F, 0.78F, 1.0F);
            
            forwardRenderer.GodRayIntensity = 0.01F;
            forwardRenderer.GodRayColor = new Vector4(1.0F, 0.78F, 0.45F, 1.0F);
            forwardRenderer.GodRayDensity = 0.85F;
            forwardRenderer.GodRayWeight = 0.045F;
            forwardRenderer.GodRayDecay = 0.93F;
            forwardRenderer.GodRayExposure = 1.1F;
            forwardRenderer.GodRaySampleCount = 48.0F;
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
                
                road.AddComponent(new InstancedRenderProxy(this.PlaneMultiInstanceRenderer, Vector3.Zero));
                this.AddEntity(road);
            }
        }
        
        // LIGHT SUN
        Entity sunLight = new Entity(new Transform() { Translation = new Vector3(0, 0, 0) }, "sun");
        Light sunLightComp = Light.CreateDirectional(new Color(255, 244, 214, 255), 54.2F);
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
                sunLight.Intensity = 54.2F * daylight * sunHorizonFade;
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