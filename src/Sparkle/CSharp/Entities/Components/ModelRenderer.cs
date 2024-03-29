using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Effects;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.Rendering.Util;
using Sparkle.CSharp.Scenes;
using BoundingBox = Raylib_cs.BoundingBox;

namespace Sparkle.CSharp.Entities.Components;

public class ModelRenderer : Component {

    public ModelAnimationPlayer AnimationPlayer { get; }
    
    private Model _model;
    private BoundingBox _box;
    private Material[] _materials;
    private Effect _effect;
    private Color _color;
    private bool _drawWires;
    
    public ModelRenderer(Model model, Material[] materials, Effect? effect = default, Color? color = default, ModelAnimation[]? animations = default, bool drawWires = false) {
        this.AnimationPlayer = new ModelAnimationPlayer(model, animations ?? Array.Empty<ModelAnimation>());
        this._model = model;
        this._box = ModelHelper.GetBoundingBox(this._model);
        this._materials = materials;
        this._effect = effect ?? EffectRegistry.DiscardAlpha;
        this._color = color ?? Color.White;
        this._drawWires = drawWires;
        this.SetupMaterial();
    }

    protected internal override void Update() {
        base.Update();
        
        this._effect.UpdateMaterialParameters(this._materials);
        
        Vector3 dimension = this._box.Max - this._box.Min;
        this._box.Min.X = this.Entity.Position.X - dimension.X / 2;
        this._box.Min.Y = this.Entity.Position.Y;
        this._box.Min.Z = this.Entity.Position.Z - dimension.Z / 2;
        
        this._box.Max.X = this.Entity.Position.X + dimension.X / 2;
        this._box.Max.Y = this.Entity.Position.Y + dimension.Y;
        this._box.Max.Z = this.Entity.Position.Z + dimension.Z / 2;
    }

    protected internal override void FixedUpdate() {
        base.FixedUpdate();
        this.AnimationPlayer.FixedUpdate();
    }
    
    protected internal override unsafe void Draw() {
        base.Draw();
        
        if (SceneManager.MainCam3D!.GetFrustum().ContainsOrientedBox(this._box, this.Entity.Position, this.Entity.Rotation)) {
            Vector3 axis;
            float angle;
           
            Raymath.QuaternionToAxisAngle(this.Entity.Rotation, &axis, &angle);
            
            if (this._drawWires) {
                ModelHelper.DrawModelWires(this._model, this.Entity.Position, axis, angle * Raylib.RAD2DEG, this.Entity.Scale, this._color);
            }
            else {
                ModelHelper.DrawModel(this._model, this.Entity.Position, axis, angle * Raylib.RAD2DEG, this.Entity.Scale, this._color);
            }
        }
    }

    private unsafe void SetupMaterial() {
        for (int i = 0; i < this._model.MaterialCount; i++) {
            this._model.Materials[i] = this._materials[i];
            this._model.Materials[i].Shader = this._effect.Shader;
        }
    }

    protected override void Dispose(bool disposing) { }
}