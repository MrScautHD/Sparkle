using System.Numerics;
using Sparkle.CSharp.Entities;
using Sparkle.CSharp.Entities.Components;
using Sparkle.CSharp.Scenes;
using Veldrid;
using BatchOrderType = Sparkle.CSharp.Entities.Components.BatchComponent.BatchOrderType;

namespace Sparkle.CSharp.Graphics.Rendering;

public class BatchRenderer {
    
    /// <summary>
    /// A dictionary that categorizes batch components by their <see cref="BatchOrderType"/>.
    /// </summary>
    private Dictionary<BatchOrderType, List<BatchComponent>> _batchComponents;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchRenderer"/> class.
    /// </summary>
    public BatchRenderer() {
        this._batchComponents = new Dictionary<BatchOrderType, List<BatchComponent>>() {
            { BatchOrderType.DrawSpritesFirst, [] },
            { BatchOrderType.DrawPrimitivesFirst , [] }
        };
    }

    /// <summary>
    /// Adds a <see cref="BatchComponent"/> to the appropriate rendering group.
    /// </summary>
    /// <param name="component">The batch component to add.</param>
    internal void Add(BatchComponent component) {
        this._batchComponents[component.OrderType].Add(component);
    }

    /// <summary>
    /// Removes a <see cref="BatchComponent"/> from its rendering group.
    /// </summary>
    /// <param name="component">The batch component to remove.</param>
    /// <returns><c>true</c> if the component was successfully removed; otherwise, <c>false</c>.</returns>
    internal bool Remove(BatchComponent component) {
        return this._batchComponents[component.OrderType].Remove(component);
    }

    /// <summary>
    /// Executes the rendering of all registered batch components, sorting them by rendering order.
    /// </summary>
    /// <param name="context">The graphics context used for issuing rendering commands.</param>
    /// <param name="framebuffer">The target framebuffer to render to.</param>
    protected internal void Draw(GraphicsContext context, Framebuffer framebuffer) {
        Camera2D? cam2D = SceneManager.ActiveCam2D;
        
        if (cam2D == null) {
            return;
        }

        Matrix4x4 view = cam2D.GetView();
        
        // Draw sprites in the "DrawSpritesFirst" group.
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription, view: view);
        
        foreach (BatchComponent component in this._batchComponents[BatchOrderType.DrawSpritesFirst]) {
            component.DrawSprite(context.SpriteBatch);
        }
        
        context.SpriteBatch.End();
        
        // Draw primitives in the "DrawSpritesFirst" group.
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription, view: view);
        
        foreach (BatchComponent component in this._batchComponents[BatchOrderType.DrawSpritesFirst]) {
            component.DrawPrimitive(context.PrimitiveBatch);
        }
        
        context.PrimitiveBatch.End();
        
        // Draw primitives in the "DrawPrimitivesFirst" group.
        context.PrimitiveBatch.Begin(context.CommandList, framebuffer.OutputDescription, view: view);
        
        foreach (BatchComponent component in this._batchComponents[BatchOrderType.DrawPrimitivesFirst]) {
            component.DrawPrimitive(context.PrimitiveBatch);
        }
        
        context.PrimitiveBatch.End();
        
        // Draw sprites in the "DrawPrimitivesFirst" group.
        context.SpriteBatch.Begin(context.CommandList, framebuffer.OutputDescription, view: view);
        
        foreach (BatchComponent component in this._batchComponents[BatchOrderType.DrawPrimitivesFirst]) {
            component.DrawSprite(context.SpriteBatch);
        }
        
        context.SpriteBatch.End();
    }
}