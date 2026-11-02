using System.Numerics;
using Bliss.ImGUI.CSharp;
using Hexa.NET.ImGui;
using Sparkle.CSharp;
using Sparkle.CSharp.GUI;
using Sparkle.CSharp.ImGUI;

namespace Sparkle.Test.CSharp;

public class TestImGuiOverlay : ImGuiOverlay {
    
    private Vector4 _pickedColor;
    
    public TestImGuiOverlay(string name, bool enabled = false) : base(name, (1280, 720), 1.0F, enabled, true, true) {
        this._pickedColor = new Vector4(0.0F, 130.0F, 255.0F, 255.0F) / 255.0F;
    }
    
    protected override void Draw(ImGuiController controller) {
        string windowName = "Test ImGUI Overlay";
        
        this.SetNextWindowAnchoredPlacement(windowName, new Vector2(320.0F, 195.0F), Anchor.TopRight, new Vector2(-5, 5));
        ImGui.SetNextWindowSizeConstraints(new Vector2(300.0F, 160.0F), new Vector2(1000.0F, 700.0F));
        ImGui.SetNextWindowCollapsed(true, ImGuiCond.Once);
        
        if (ImGui.Begin(windowName)) {
            this.UpdateWindowPlacementState();
            
            ImGui.Text("Hello World!");
            ImGui.Separator();

            if (ImGui.Button("Reset Placement")) {
                this.ResetWindowPlacement();
            }
            
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
