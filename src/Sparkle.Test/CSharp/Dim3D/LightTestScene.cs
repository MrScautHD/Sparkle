using System.Numerics;
using Bliss.CSharp;
using Bliss.CSharp.Camera.Dim3;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Geometry.Models;
using Bliss.CSharp.Graphics.Rendering;
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
using Sparkle.CSharp.Graphics.Rendering.Forward.Pbr;
using Sparkle.CSharp.Scenes;
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
    
    public LightTestScene() : base("Light-Test-Scene", SceneType.Scene3D, (graphicsDevice) => new PbrForwardRenderer(graphicsDevice, Color.Black)) { }
    
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
                    Value = 1.0F
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
                
                mesh.Material.RenderMode = RenderMode.Solid;
                mesh.GenTangents();
            }
        }), false);
    }
    
    protected override void Update(double delta) {
        base.Update(delta);
        
        Entity? sun = this.GetEntitiesWithTag("sun").FirstOrDefault();
        Entity? moon = this.GetEntitiesWithTag("moon").FirstOrDefault();
        
        if (sun != null) {
            sun.LocalTransform.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float) delta * 0.2F);
            
            Vector3 sunDirection = Vector3.Transform(Vector3.UnitZ, sun.LocalTransform.Rotation);
            float sunHeight = sunDirection.Y;
            float daylight = this.Smooth01((sunHeight + 0.35F) / 0.9F);
            float sunHorizonFade = this.Smooth01(sunHeight / 0.2F);
            
            Light? sunLight = sun.GetComponent<Light>();
            
            if (sunLight != null) {
                float sunriseAmount = 1.0F - this.Smooth01(daylight / 0.5F);
                
                byte red = 255;
                byte green = (byte) Math.Clamp(120 + (124 * (1.0F - sunriseAmount)), 0.0F, 255.0F);
                byte blue = (byte) Math.Clamp(80 + (134 * (1.0F - sunriseAmount)), 0.0F, 255.0F);
                
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
                float nightR = 0.012F, nightG = 0.016F, nightB = 0.03F;
                
                pbrRenderer.AmbientColor = new Vector4(
                    nightR + ((dayAmbient - nightR) * daylight),
                    nightG + ((dayAmbient - nightG) * daylight),
                    nightB + ((dayAmbient - nightB) * daylight),
                    1.0F
                );
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
    }
    
    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        context.CommandList.ClearColorTarget(0, Color.Black.ToRgbaFloat());
        base.Draw(context, framebuffer);
    }

    protected override void Init() {
        base.Init();
        
        // RELATIVE MOUSE MODE.
        Input.EnableRelativeMouseMode();
        
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
        Entity road = new Entity(new Transform() { Translation = new Vector3(0, 0, 0), Scale = new Vector3(20, 20, 20)});
        road.AddComponent(new RigidBody3D(new BoxShape(96, 1, 96), MassInertiaUpdateMode.Update, MotionType.Static) {
            DrawDebug = false,
            DebugDrawColor = Color.Green
        });
        road.AddComponent(new ModelRenderer(this.PlaneModel, Vector3.Zero));
        this.AddEntity(road);
        
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
    
    private float Smooth01(float value) {
        value = Math.Clamp(value, 0.0F, 1.0F);
        return value * value * (3.0F - (2.0F * value));
    }
}