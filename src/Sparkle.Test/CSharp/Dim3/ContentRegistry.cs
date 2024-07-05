using Raylib_CSharp.Colors;
using Raylib_CSharp.Geometry;
using Raylib_CSharp.Images;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Textures;
using Raylib_CSharp.Unsafe.Spans.Data;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Registries;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Gifs;
using Sparkle.CSharp.Rendering.Models;

namespace Sparkle.Test.CSharp.Dim3;

public class ContentRegistry : Registry {
    
    // TEXTURES
    public static Texture2D PlayerTexture;
    public static Texture2D SpriteTexture;
    
    // IMAGES
    public static Image Skybox;
    
    // GIF
    public static Gif Gif;
    
    // MODELS
    public static Model PlayerModel;
    
    // MODEL ANIMATIONS
    public static ReadOnlySpanData<ModelAnimation> Animations;
    
    protected override void Load(ContentManager content) {
        base.Load(content);
        
        // TEXTURES
        PlayerTexture = content.Load(new TextureContent("content/texture.png"));
        SpriteTexture = content.Load(new TextureContent("content/sprite.png"));
        
        // IMAGES
        Skybox = content.Load(new ImageContent("content/skybox.png"));
        
        // GIF
        Gif = content.Load(new GifContent("content/flame.gif"));
        
        // MODEL ANIMATIONS
        Animations = content.Load(new ModelAnimationContent("content/model.glb"));
        
        // MODELS
        MaterialManipulator manipulator = new MaterialManipulator()
            .Set(1, EffectRegistry.Pbr)
            .Set(1, MaterialMapIndex.Albedo, PlayerTexture)
            .Set(1, MaterialMapIndex.Metalness, PlayerTexture)
            .Set(1, MaterialMapIndex.Normal, PlayerTexture)
            .Set(1, MaterialMapIndex.Emission, PlayerTexture)
            
            .Set(1, MaterialMapIndex.Albedo, Color.White)
            .Set(1, MaterialMapIndex.Emission, new Color(255, 162, 0, 255))
            
            .Set(1, MaterialMapIndex.Metalness, 0.0F)
            .Set(1, MaterialMapIndex.Roughness, 0.0F)
            .Set(1, MaterialMapIndex.Occlusion, 1.0F)
            .Set(1, MaterialMapIndex.Emission, 0.01F)
            .Set(1, 0, 0.5F)
            .Set(1, 1, 0.5F);
        
        PlayerModel = content.Load(new ModelContent("content/model.glb", manipulator));
    }
}