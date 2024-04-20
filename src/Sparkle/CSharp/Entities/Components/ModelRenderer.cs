using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Effects;
using Sparkle.CSharp.Registries.Types;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.Rendering.Models;
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
    
    /// <summary>
    /// Constructor for creating a ModelRenderer object.
    /// </summary>
    /// <param name="model">The 3D model to render.</param>
    /// <param name="offsetPos">Offset position of the model renderer.</param>
    /// <param name="materials">Array of materials applied to the model.</param>
    /// <param name="effect">Effect to apply to the model.</param>
    /// <param name="color">Color of the model.</param>
    /// <param name="animations">Array of animations for the model.</param>
    /// <param name="drawWires">Flag indicating whether to draw wires.</param>
    public ModelRenderer(Model model, Vector3 offsetPos, Material[] materials, Effect? effect = default, Color? color = default, ModelAnimation[]? animations = default, bool drawWires = false) : base(offsetPos) {
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
        this._box.Min.X = this.GlobalPos.X - dimension.X / 2;
        this._box.Min.Y = this.GlobalPos.Y;
        this._box.Min.Z = this.GlobalPos.Z - dimension.Z / 2;
        
        this._box.Max.X = this.GlobalPos.X + dimension.X / 2;
        this._box.Max.Y = this.GlobalPos.Y + dimension.Y;
        this._box.Max.Z = this.GlobalPos.Z + dimension.Z / 2;
    }
    
    protected internal override void FixedUpdate() {
        base.FixedUpdate();
        this.AnimationPlayer.FixedUpdate();
    }
    
    protected internal override unsafe void Draw() {
        base.Draw();
        
        if (SceneManager.ActiveCam3D!.GetFrustum().ContainsOrientedBox(this._box, this.GlobalPos, this.Entity.Rotation)) {
            Vector3 axis;
            float angle;
           
            Raymath.QuaternionToAxisAngle(this.Entity.Rotation, &axis, &angle);
            
            if (this._drawWires) {
                ModelHelper.DrawModelWires(this._model, this.GlobalPos, axis, angle * Raylib.RAD2DEG, this.Entity.Scale, this._color);
            }
            else {
                ModelHelper.DrawModel(this._model, this.GlobalPos, axis, angle * Raylib.RAD2DEG, this.Entity.Scale, this._color);
            }
        }
    }
    
    /// <summary>
    /// Sets up the materials for the model.
    /// </summary>
    private unsafe void SetupMaterial() {
        for (int i = 0; i < this._model.MaterialCount; i++) {
            this._model.Materials[i] = this._materials[i];
            this._model.Materials[i].Shader = this._effect.Shader;
        }
    }

    protected override void Dispose(bool disposing) { }
}