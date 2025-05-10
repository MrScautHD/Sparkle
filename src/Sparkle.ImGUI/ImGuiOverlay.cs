using Bliss.CSharp.Transformations;
using ImGuiNET;
using Sparkle.CSharp;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Overlays;
using Sparkle.CSharp.Scenes;
using Sparkle.ImGUI.Scenes;
using Veldrid;

namespace Sparkle.ImGUI;

public class ImGuiOverlay : Overlay, IDisposable
{
    private readonly ImGuiController _imGuiController;
    
    public ImGuiOverlay() : base("ImGuiOverlay", true)
    {
        var gameInstance = Game.Instance 
                        ?? throw new Exception("Game instance not found or unavailable");
            
        
        _imGuiController = new ImGuiController(
            gameInstance.GraphicsDevice, gameInstance.MsaaRenderTexture.Framebuffer.OutputDescription,
            gameInstance.MainWindow.GetWidth(), gameInstance.MainWindow.GetHeight()
        );
    }

    protected override void Update(double delta)
    {
        _imGuiController.Update((float)delta);
        
        if (SceneManager.ActiveScene is MainScene mainScene)
        {
            if (ImGui.Begin($"[{mainScene.Name}] Rectangle"))
            {
                var position = mainScene.Rectangle.Position;
                ImGui.DragFloat2("Position", ref position);
                if (position != mainScene.Rectangle.Position)
                    mainScene.Rectangle.Position = position;
            
                var size = mainScene.Rectangle.Size;
                ImGui.DragFloat2("Size", ref size);
                if (size != mainScene.Rectangle.Size)
                    mainScene.Rectangle.Size = size;
                ImGui.End();
            }
        }
    }

    protected override void Draw(GraphicsContext context, Framebuffer framebuffer)
    {
        _imGuiController.Render(context.GraphicsDevice, context.CommandList);
    }

    protected override void Resize(Rectangle rectangle)
    {
        _imGuiController.Resize(rectangle.Width, rectangle.Height);
    }

    public void Dispose()
    {
        _imGuiController.Dispose();
        GC.SuppressFinalize(this);
    }
}