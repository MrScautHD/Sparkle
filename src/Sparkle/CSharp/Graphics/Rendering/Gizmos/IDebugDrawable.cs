using Bliss.CSharp.Graphics.Rendering.Renderers;

namespace Sparkle.CSharp.Graphics.Rendering.Gizmos;

public interface IDebugDrawable {
    
    /// <summary>
    /// Gets or sets a value indicating whether debug drawing is enabled.
    /// </summary>
    bool DebugDrawEnabled { get; set; }
    
    /// <summary>
    /// Renders debug visuals using the specified renderer.
    /// </summary>
    /// <param name="immediateRenderer">The renderer used to draw debug primitives.</param>
    void DrawDebug(ImmediateRenderer immediateRenderer);
}