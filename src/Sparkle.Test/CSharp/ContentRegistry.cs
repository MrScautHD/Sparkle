using Bliss.CSharp.Fonts;
using Bliss.CSharp.Geometry.Meshes;
using Bliss.CSharp.Geometry.Models;
using Bliss.CSharp.Graphics.Rendering;
using Bliss.CSharp.Images;
using Bliss.CSharp.Textures;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Graphics.Animations;
using Sparkle.CSharp.Registries;

namespace Sparkle.Test.CSharp;

public class ContentRegistry : Registry {
    
    // Fonts:
    public static Font Fontoe { get; private set; }
    
    // Textures:
    public static Texture2D Sprite { get; private set; }
    public static Texture2D Button { get; private set; }
    public static Texture2D TextBox { get; private set; }
    public static Texture2D ToggleBackground { get; private set; }
    public static Texture2D ToggleCheckmark { get; private set; }
    public static Texture2D SlideBar { get; private set; }
    public static Texture2D FilledSlideBar { get; private set; }
    public static Texture2D Slider { get; private set; }
    public static Texture2D UiBannerTexture { get; private set; }
    public static Texture2D UiBannerEdgeLessTexture { get; private set; }
    public static Texture2D UiSliderTexture { get; private set; }
    public static Texture2D UiArrowTexture { get; private set; }
    public static Texture2D TerrainGrass { get; private set; }
    public static Texture2D TerrainDirt { get; private set; }
    public static Texture2D TerrainRock { get; private set; }
    
    // Models:
    public static Model PlayerModel { get; private set; }
    
    // Gifs:
    public static AnimatedImage AnimatedImage { get; private set; }
    public static Texture2D Gif { get; private set; }
    
    // Animator Controllers:
    public static AnimatorController PlayerBasicAnimatorController { get; private set; }
    public static AnimatorController PlayerUpperBodyAnimatorController { get; private set; }
    
    /// <summary>
    /// Loads the content for the registry, including fonts, textures, models, and other assets.
    /// This method initializes the necessary assets required by the registry, utilizing the provided
    /// <see cref="ContentManager"/> to load resources.
    /// </summary>
    /// <param name="content">The <see cref="ContentManager"/> instance used to load resources.</param>
    protected override void Load(ContentManager content) {
        base.Load(content);
        
        // Fonts:
        Fontoe = content.Load(new FontContent("content/fontoe.ttf"));
        
        // Textures:
        Sprite = content.Load(new TextureContent("content/sprite.png"));
        Button = content.Load(new TextureContent("content/button.png"));
        TextBox = content.Load(new TextureContent("content/text-box.png"));
        ToggleBackground = content.Load(new TextureContent("content/toggle_background.png"));
        ToggleCheckmark = content.Load(new TextureContent("content/toggle_checkmark.png"));
        SlideBar = content.Load(new TextureContent("content/bar.png"));
        FilledSlideBar = content.Load(new TextureContent("content/filled_bar.png"));
        Slider = content.Load(new TextureContent("content/slider.png"));
        UiBannerTexture = content.Load(new TextureContent("content/ui_banner.png"));
        UiBannerEdgeLessTexture = content.Load(new TextureContent("content/ui_banner_edgeless.png"));
        UiSliderTexture = content.Load(new TextureContent("content/slider_high_res.png"));
        UiArrowTexture = content.Load(new TextureContent("content/ui_arrow.png"));
        TerrainGrass = content.Load(new TextureContent("content/terrain_grass.png"));
        TerrainDirt = content.Load(new TextureContent("content/terrain_dirt.png"));
        TerrainRock = content.Load(new TextureContent("content/terrain_rock.png"));
        
        // Models:
        PlayerModel = content.Load(new ModelContent("content/model.glb", true, false, true).Do(model => {
            foreach (IMesh mesh in model.Meshes) {
                mesh.Material.RenderMode = RenderMode.Cutout;
            }
        }));
        
        // Gifs:
        AnimatedImage = new AnimatedImage("content/test.gif");
        Gif = new Texture2D(content.GraphicsDevice, AnimatedImage.SpriteSheet);
        
        // Animator Controllers:
        PlayerBasicAnimatorController = new AnimatorController();
        
        // Create animator states.
        AnimatorState idleState = new AnimatorState("Idle", PlayerModel.Animations[0], true);
        AnimatorState walkState = new AnimatorState("Walk", PlayerModel.Animations[1], true, 1.1F);
        AnimatorState runState = new AnimatorState("Run", PlayerModel.Animations[2], true, 1.2F);
        AnimatorState jumpState = new AnimatorState("Jump", PlayerModel.Animations[3], false, 0.8F);
        
        // Idle => Walk
        idleState.AddTransition(new AnimatorTransition("Walk", 0.2F).AddCondition(animator => ((Player) animator.Entity).Speed is > 0.1F and <= 3.5F));
        
        // Idle => Run
        idleState.AddTransition(new AnimatorTransition("Run", 0.2F).AddCondition(animator => ((Player) animator.Entity).Speed > 3.5F));
        
        // Walk => Idle
        walkState.AddTransition(new AnimatorTransition("Idle", 0.2F).AddCondition(animator => ((Player) animator.Entity).Speed <= 0.1F));
        
        // Walk => Run
        walkState.AddTransition(new AnimatorTransition("Run", 0.2F).AddCondition(animator => ((Player) animator.Entity).Speed > 3.5F));
        
        // Run => Idle
        runState.AddTransition(new AnimatorTransition("Idle", 0.2F).AddCondition(animator => ((Player) animator.Entity).Speed <= 0.1F));
        
        // Run => Walk
        runState.AddTransition(new AnimatorTransition("Walk", 0.2F).AddCondition(animator => ((Player) animator.Entity).Speed is > 0.1F and <= 3.5F));
        
        // Any => Jump.
        AnimatorTransition jumpTransition = new AnimatorTransition("Jump", 0.1F).AddCondition(animator => !((Player) animator.Entity).IsOnGround);
        idleState.AddTransition(jumpTransition);
        walkState.AddTransition(jumpTransition);
        runState.AddTransition(jumpTransition);
        
        // Jump => Landing.
        jumpState.AddTransition(new AnimatorTransition("Idle", 0.4F).AddCondition(animator => ((Player) animator.Entity).IsOnGround));
        jumpState.AddTransition(new AnimatorTransition("Walk", 0.4F).AddCondition(animator => ((Player) animator.Entity).IsOnGround));
        jumpState.AddTransition(new AnimatorTransition("Run", 0.4F).AddCondition(animator => ((Player) animator.Entity).IsOnGround));
        
        // Adding animation states to the controller.
        PlayerBasicAnimatorController.AddState(idleState);
        PlayerBasicAnimatorController.AddState(walkState);
        PlayerBasicAnimatorController.AddState(runState);
        PlayerBasicAnimatorController.AddState(jumpState);
        
        // Create a player upper body controller.
        PlayerUpperBodyAnimatorController = new AnimatorController();
        
        // Create an animator state.
        AnimatorState upperSwingState = new AnimatorState("SwingArms", PlayerModel.Animations[2], true, 1.2F);
        
        // Adding animation state to the controller.
        PlayerUpperBodyAnimatorController.AddState(upperSwingState);
    }
    
    protected override void Dispose(bool disposing) { }
}
