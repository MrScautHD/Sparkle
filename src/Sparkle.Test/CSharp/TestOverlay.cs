using System.Numerics;
using Sparkle.CSharp;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.Test.CSharp;

public class TestOverlay : Overlay {
    
    private float _timeAccumulator;
    private int _frameCounter;
    private int _fps;
    
    public TestOverlay(string name, bool enabled = false) : base(name, enabled) { }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        this._timeAccumulator += (float) Time.Delta;
        this._frameCounter++;

        if (this._timeAccumulator >= 1.0F) {
            this._fps = this._frameCounter;
            this._frameCounter = 0;
            this._timeAccumulator -= 1.0F;
        }
        
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription);
        context.SpriteBatch.DrawText(ContentRegistry.Fontoe, $"FPS: {this._fps}", new Vector2(10, 10), 18, scale: new Vector2(2, 2));
        
        if (SceneManager.ActiveScene?.Name == "Terrain3D-Scene") {
            context.SpriteBatch.DrawText(ContentRegistry.Fontoe, $"Terrain Vertices: {SceneManager.ActiveScene.GetEntitiesWithTag("terrain").FirstOrDefault()?.GetComponent<Terrain3D>()?.TotalVertexCount}", new Vector2(10, 42), 18, scale: new Vector2(2, 2));
            context.SpriteBatch.DrawText(ContentRegistry.Fontoe, $"Terrain Chunks: {SceneManager.ActiveScene.GetEntitiesWithTag("terrain").FirstOrDefault()?.GetComponent<Terrain3D>()?.Terrain.GetChunks().Count}", new Vector2(10, 84), 18, scale: new Vector2(2, 2));
            context.SpriteBatch.DrawText(ContentRegistry.Fontoe, $"Terrain Chunk size: {SceneManager.ActiveScene.GetEntitiesWithTag("terrain").FirstOrDefault()?.GetComponent<Terrain3D>()?.Terrain.ChunkSize}", new Vector2(10, 126), 18, scale: new Vector2(2, 2));
        }
        
        context.SpriteBatch.End();
    }
}