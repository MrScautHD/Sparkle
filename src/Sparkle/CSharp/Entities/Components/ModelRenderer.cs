using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Collision;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Geometry;
using Raylib_CSharp.Materials;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Unsafe.Spans.Data;
using Sparkle.CSharp.Effects;
using Sparkle.CSharp.Models;
using Sparkle.CSharp.Scenes;

namespace Sparkle.CSharp.Entities.Components;

public class ModelRenderer : Component {

    public ModelAnimationPlayer? AnimationPlayer { get; }
    
    private Model _model;
    private BoundingBox _box;
    private Color _color;
    private bool _drawWires;
    
    /// <summary>
    /// Constructor for creating a ModelRenderer object.
    /// </summary>
    /// <param name="model">The 3D model to render.</param>
    /// <param name="offsetPos">Offset position of the model renderer.</param>
    /// <param name="color">Color of the model.</param>
    /// <param name="animations">Array of animations for the model.</param>
    /// <param name="drawWires">Flag indicating whether to draw wires.</param>
    public ModelRenderer(Model model, Vector3 offsetPos, Color? color = default, ReadOnlySpanData<ModelAnimation>? animations = default, bool drawWires = false) : base(offsetPos) {
        if (animations != null) {
            this.AnimationPlayer = new ModelAnimationPlayer(model, animations);
        }
        this._model = model;
        this._box = model.GetBoundingBox();
        this._color = color ?? Color.White;
        this._drawWires = drawWires;
    }

    protected internal override void Update() {
        base.Update();
        
        Vector3 dimension = this._box.Max - this._box.Min;
        this._box.Min.X = this.GlobalPos.X - dimension.X / 2;
        this._box.Min.Y = this.GlobalPos.Y;
        this._box.Min.Z = this.GlobalPos.Z - dimension.Z / 2;
        
        this._box.Max.X = this.GlobalPos.X + dimension.X / 2;
        this._box.Max.Y = this.GlobalPos.Y + dimension.Y;
        this._box.Max.Z = this.GlobalPos.Z + dimension.Z / 2;
    }
    
    protected internal override void FixedUpdate() {
        base.FixedUpdate();
        this.AnimationPlayer?.FixedUpdate();
    }
    
    protected internal override void Draw() {
        base.Draw();
        Cam3D? cam = SceneManager.ActiveCam3D;
        if (cam == null) return;
        
        foreach (Material material in this._model.Materials) {
            foreach (var effect in EffectManager.Effects) {
                if (effect.Shader.Id == material.Shader.Id) {
                    effect.Apply(material);
                    break;
                }
            }
        }
        
        if (cam.GetFrustum().ContainsOrientedBox(this._box, this.GlobalPos, this.Entity.Rotation)) {
            RayMath.QuaternionToAxisAngle(this.Entity.Rotation, out Vector3 axis, out float angle);
            
            if (this._drawWires) {
                Graphics.DrawModelWiresEx(this._model, this.GlobalPos, axis, angle * RayMath.Rad2Deg, this.Entity.Scale, this._color);
            }
            else {
                Graphics.DrawModelEx(this._model, this.GlobalPos, axis, angle * RayMath.Rad2Deg, this.Entity.Scale, this._color);
            }
        }
    }

    public override Component Clone() {
        return new ModelRenderer(this._model, this.OffsetPos, this._color, default, this._drawWires);
    }

    protected override void Dispose(bool disposing) { }
}