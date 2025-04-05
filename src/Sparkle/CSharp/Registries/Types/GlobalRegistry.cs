using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Graphics.Pipelines.Textures;
using Bliss.CSharp.Graphics.VertexTypes;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Sparkle.CSharp.Graphics.VertexTypes;
using Veldrid;

namespace Sparkle.CSharp.Registries.Types;

public class GlobalRegistry : Registry {
    
    /// <summary>
    /// Gets the list of buffer layouts used in the graphics pipeline.
    /// </summary>
    public static List<SimpleBufferLayout> BufferLayouts { get; private set; }
    
    /// <summary>
    /// Gets the list of texture layouts used in the graphics pipeline.
    /// </summary>
    public static List<SimpleTextureLayout> TextureLayouts { get; private set; }
    
    /// <summary>
    /// Gets the effect used for rendering skyboxes.
    /// </summary>
    public static Effect SkyboxEffect { get; private set; }

    /// <summary>
    /// Gets the effect used for rendering physics debugging visuals in the graphics pipeline.
    /// </summary>
    public static Effect PhysicsDebugEffect { get; private set; }
    
    /// <summary>
    /// Gets the graphics device associated with this registry.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalRegistry"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    public GlobalRegistry(GraphicsDevice graphicsDevice) {
        this.GraphicsDevice = graphicsDevice;
        BufferLayouts = new List<SimpleBufferLayout>();
        TextureLayouts = new List<SimpleTextureLayout>();
    }
    
    /// <summary>
    /// Loads the necessary resources and effects for the registry.
    /// </summary>
    /// <param name="content">The content manager used for loading assets.</param>
    protected internal override void Load(ContentManager content) {
        base.Load(content);
        
        // Skybox effect.
        SkyboxEffect = content.Load(new EffectContent(CubemapVertex3D.VertexLayout, "content/shaders/skybox.vert", "content/shaders/skybox.frag"));
        SkyboxEffect.AddBufferLayout(this.CreateBufferLayout("ProjectionViewBuffer", SimpleBufferType.Uniform, ShaderStages.Vertex));
        SkyboxEffect.AddTextureLayout(this.CreateTextureLayout("fCubemap"));
        
        // Physics debug effect.
        PhysicsDebugEffect = content.Load(new EffectContent(PhysicsDebugVertex3D.VertexLayout, "content/shaders/physics_debug_drawer.vert", "content/shaders/physics_debug_drawer.frag"));
        PhysicsDebugEffect.AddBufferLayout(this.CreateBufferLayout("ProjectionViewBuffer", SimpleBufferType.Uniform, ShaderStages.Vertex));
    }
    
    /// <summary>
    /// Creates and registers a new buffer layout.
    /// </summary>
    /// <param name="name">The name of the buffer layout.</param>
    /// <param name="bufferType">The type of buffer.</param>
    /// <param name="stages">The shader stages where the buffer will be used.</param>
    /// <returns>A new <see cref="SimpleBufferLayout"/> instance.</returns>
    public SimpleBufferLayout CreateBufferLayout(string name, SimpleBufferType bufferType, ShaderStages stages) {
        SimpleBufferLayout bufferLayout = new SimpleBufferLayout(this.GraphicsDevice, name, bufferType, stages);
        BufferLayouts.Add(bufferLayout);
        return bufferLayout;
    }
    
    /// <summary>
    /// Creates and registers a new texture layout.
    /// </summary>
    /// <param name="name">The name of the texture layout.</param>
    /// <returns>A new <see cref="SimpleTextureLayout"/> instance.</returns>
    public SimpleTextureLayout CreateTextureLayout(string name) {
        SimpleTextureLayout textureLayout = new SimpleTextureLayout(this.GraphicsDevice, name);
        TextureLayouts.Add(textureLayout);
        return textureLayout;
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (disposing) {
            foreach (SimpleBufferLayout bufferLayout in BufferLayouts) {
                bufferLayout.Dispose();
            }
            
            BufferLayouts.Clear();

            foreach (SimpleTextureLayout textureLayout in TextureLayouts) {
                textureLayout.Dispose();
            }
            
            TextureLayouts.Clear();
        }
    }
}