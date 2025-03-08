using Bliss.CSharp.Effects;
using Veldrid;

namespace Sparkle.CSharp.Content.Types;

public class EffectContent : IContentType<Effect> {
    
    /// <summary>
    /// The vertex layout description for the effect.
    /// </summary>
    public VertexLayoutDescription VertexLayout { get; }
    
    /// <summary>
    /// The file path of the vertex shader.
    /// </summary>
    public string Path { get; }
    
    /// <summary>
    /// The file path of the fragment shader.
    /// </summary>
    public string FragPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectContent"/> class.
    /// </summary>
    /// <param name="vertexLayout">The vertex layout description.</param>
    /// <param name="vertPath">The file path of the vertex shader.</param>
    /// <param name="fragPath">The file path of the fragment shader.</param>
    public EffectContent(VertexLayoutDescription vertexLayout, string vertPath, string fragPath) {
        this.VertexLayout = vertexLayout;
        this.Path = vertPath;
        this.FragPath = fragPath;
    }
}