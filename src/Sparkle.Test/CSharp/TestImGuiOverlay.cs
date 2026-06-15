using System.Numerics;
using Bliss.CSharp.Windowing;
using Bliss.ImGUI.CSharp;
using Hexa.NET.ImGui;
using SDL3;
using Sparkle.CSharp;
using Sparkle.CSharp.ImGUI;

namespace Sparkle.Test.CSharp;

public class TestImGuiOverlay : ImGuiOverlay {
    
    private Vector4 _pickedColor;
    
    private float _appliedScale;
    
    public TestImGuiOverlay(string name, bool enabled = false) : base(name, enabled) {
        this._pickedColor = new Vector4(0.0F, 130.0F, 255.0F, 255.0F) / 255.0F;
    }
    
    public override void Draw(ImGuiController controller) {
        this.UpdateScale(controller);
        
        //ImGui.SetNextWindowPos(new Vector2(5.0F, 5.0F));
        ImGui.SetNextWindowSize(new Vector2(320.0F, 180.0F), ImGuiCond.FirstUseEver);
        
        if (ImGui.Begin("Test ImGUI Overlay")) {
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
    
    /// <summary>
    /// Applies the window's current DPI display scale to the ImGui style if it has changed since the last frame.
    /// </summary>
    /// <param name="controller">The ImGui controller whose style and window are scaled.</param>
    private void UpdateScale(ImGuiController controller) {
        if (controller.Window is not Sdl3Window) {
            throw new Exception("This window type do not support DPI scaling!");
        }
        
        float scale = SDL.GetWindowDisplayScale(controller.Window.Handle);
        
        if (scale <= 0.0F) {
            scale = 4.0F;
        }
        
        if (Math.Abs(scale - this._appliedScale) < 0.01F) {
            return;
        }
        
        // Reset to defaults before scaling, since ScaleAllSizes is cumulative.
        ImGui.StyleColorsDark(controller.Style);
        ImGui.ScaleAllSizes(controller.Style, scale);
        controller.Style.FontScaleDpi = scale;
        
        this._appliedScale = scale;
    }
    
    protected override void Dispose(bool disposing) { }
}