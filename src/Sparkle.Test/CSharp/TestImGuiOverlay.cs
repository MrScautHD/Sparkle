using System.Numerics;
using Bliss.ImGUI.CSharp;
using Hexa.NET.ImGui;
using Sparkle.CSharp;
using Sparkle.CSharp.ImGUI;

namespace Sparkle.Test.CSharp;

public class TestImGuiOverlay : ImGuiOverlay {
    
    private Vector4 _pickedColor;
    
    public TestImGuiOverlay(string name, bool enabled = false) : base(name, (1280, 720), 1.0F, enabled) {
        this._pickedColor = new Vector4(0.0F, 130.0F, 255.0F, 255.0F) / 255.0F;
    }
    
    protected override void Draw(ImGuiController controller, float scaleFactor) {
        Vector2 minWindowSize = new Vector2(300.0F, 160.0F);
        Vector2 maxWindowSize = new Vector2(1000.0F, 700.0F);
        
        this.SetNextWindowScaledPlacement(controller, new Vector2(20, 20), new Vector2(320, 180), minWindowSize, maxWindowSize, ImGuiCond.FirstUseEver);
        
        if (ImGui.Begin("Test ImGUI Overlay")) {
            this.UpdateWindowScaledPlacement();
            
            ImGui.Text("Hello World!");
            ImGui.Separator();
            
            ImGui.Text("Time:");
            ImGui.Separator();
            ImGui.Text($"\tFrame: {Time.Frame}");
            ImGui.Text($"\tDelta: {Time.Delta}");
            ImGui.NewLine();
            
            ImGui.Text("Color Picker:");
            ImGui.Separator();
            ImGui.ColorEdit4("Color", ref this._pickedColor, ImGuiColorEditFlags.Uint8 | ImGuiColorEditFlags.DisplayRgb);
        }
        
        ImGui.End();
    }
    
    protected override void Dispose(bool disposing) { }
}
