using Bliss.CSharp.Effects;
using Bliss.CSharp.Graphics.Pipelines.Buffers;
using Bliss.CSharp.Graphics.Pipelines.Textures;
using Bliss.CSharp.Graphics.VertexTypes;
using Sparkle.CSharp.Content;
using Sparkle.CSharp.Content.Types;
using Veldrid;

namespace Sparkle.CSharp.Registries.Types;

public class GlobalRegistry : Registry {
    
    public static List<SimpleBufferLayout> BufferLayouts { get; private set; }
    public static List<SimpleTextureLayout> TextureLayouts { get; private set; }
    
    public static Effect SkyboxEffect { get; private set; }
    
    public GraphicsDevice GraphicsDevice { get; private set; }

    public GlobalRegistry(GraphicsDevice graphicsDevice) {
        this.GraphicsDevice = graphicsDevice;
    }

    protected internal override void Load(ContentManager content) {
        base.Load(content);
        
        // Skybox effect.
        SkyboxEffect = content.Load(new EffectContent(CubemapVertex3D.VertexLayout, "content/shaders/skybox.vert", "content/shaders/skybox.frag"));
        SkyboxEffect.AddBufferLayout(new SimpleBufferLayout(this.GraphicsDevice, "ProjectionViewBuffer", SimpleBufferType.Uniform, ShaderStages.Vertex));
        SkyboxEffect.AddTextureLayout(new SimpleTextureLayout(this.GraphicsDevice, "fCubemap"));
    }
    
    public SimpleBufferLayout CreateBufferLayout(string name, SimpleBufferType bufferType, ShaderStages stages) {
        SimpleBufferLayout bufferLayout = new SimpleBufferLayout(this.GraphicsDevice, name, bufferType, stages);
        BufferLayouts.Add(bufferLayout);
        return bufferLayout;
    }
    
    public SimpleTextureLayout CreateTextureLayout(string name) {
        SimpleTextureLayout textureLayout = new SimpleTextureLayout(this.GraphicsDevice, name);
        TextureLayouts.Add(textureLayout);
        return textureLayout;
    }
}