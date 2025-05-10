using Sparkle.CSharp;
using Sparkle.CSharp.Overlays;

namespace Sparkle.ImGUI;

public class SimpleGame(GameSettings settings) : Game(settings)
{
    private ImGuiOverlay? _imGuiOverlay;

    protected override void Init()
    {
        base.Init();
        _imGuiOverlay = new ImGuiOverlay();
        OverlayManager.AddOverlay(_imGuiOverlay);
    }

    protected override void OnClose()
    {
        _imGuiOverlay?.Dispose();
    }
}