﻿using System.Numerics;
using Bliss.ImGUI.CSharp;
using Hexa.NET.ImGui;
using Sparkle.CSharp;
using Sparkle.CSharp.ImGUI;

namespace Sparkle.Test.CSharp;

public class TestImGuiOverlay : ImGuiOverlay {
    
    private static readonly Vector4 DefaultWindowRect = new Vector4(20.0F, 20.0F, 320.0F, 180.0F);
    
    private Vector4 _pickedColor;
    
    public TestImGuiOverlay(string name, bool enabled = false) : base(name, enabled) {
        this._pickedColor = new Vector4(0.0F, 130.0F, 255.0F, 255.0F) / 255.0F;
    }
    
    protected override void Draw(ImGuiController controller) {
        Vector2 minWindowSize = new Vector2(300.0F, 160.0F);
        Vector2 maxWindowSize = new Vector2(1000.0F, 700.0F);
        
        this.SetNextWindowPlacement(controller, "Test ImGUI Overlay", DefaultWindowRect, minWindowSize, maxWindowSize, ImGuiCond.Always);
        
        if (ImGui.Begin("Test ImGUI Overlay")) {
            this.UpdateWindowPlacement("Test ImGUI Overlay");
            
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
