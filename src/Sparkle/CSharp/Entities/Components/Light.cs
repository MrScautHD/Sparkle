using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Effects.Types;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Entities.Components;

public class Light : Component {
    
    public PbrEffect Effect { get; private set; }
    public int Id { get; private set; }

    public bool Enabled;
    public bool DrawSphere;
    
    public PbrEffect.LightType Type;
    public Vector3 Target;
    public Color Color;
    public float Intensity;

    private bool _result;

    /// <summary>
    /// Represents a light component which can be added to entities in a scene.
    /// </summary>
    public Light(PbrEffect effect, PbrEffect.LightType type, Vector3 target, Color color, float intensity = 4, bool drawSphere = false) {
        this.Enabled = true;
        this.DrawSphere = drawSphere;
        this.Effect = effect;
        this.Type = type;
        this.Target = target;
        this.Color = color;
        this.Intensity = intensity;
    }
    
    protected internal override void Init() {
        this._result = this.Effect.AddLight(this.Enabled, this.Type, this.Entity.Position, this.Target, this.Color, this.Intensity, out int id);
        this.Id = id;
        
        if (this._result) {
            base.Init();
        }
    }

    protected internal override void Update() {
        base.Update();
        this.Effect.UpdateLightParameters(this.Id, this.Enabled, this.Type, this.Entity.Position, this.Target, this.Color, this.Intensity);
    }

    protected internal override void Draw() {
        base.Draw();
        
        if (this.DrawSphere) {
            if (this.Enabled) {
                ModelHelper.DrawSphere(this.Entity.Position, 0.05F * this.Intensity, 16, 16, this.Color);
            }
            else {
                ModelHelper.DrawSphereWires(this.Entity.Position, 0.05F * this.Intensity, 16, 16, this.Color);
            }
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            if (this._result) {
                this.Effect.RemoveLight(this.Id);
            }
        }
    }
}