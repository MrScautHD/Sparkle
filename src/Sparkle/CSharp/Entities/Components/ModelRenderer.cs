using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;
using Sparkle.CSharp.Rendering.Util;
using Sparkle.CSharp.Scenes;
using BoundingBox = Raylib_cs.BoundingBox;

namespace Sparkle.CSharp.Entities.Components; 

public class ModelRenderer : Component {

    public ModelAnimationPlayer AnimationPlayer { get; }
    
    private Model _model;
    private Material[] _materials;
    private Color _color;
    private bool _drawWires;
    
    public ModelRenderer(Model model, Material[] materials, Color? color = default, ModelAnimation[]? animations = default, bool drawWires = false) {
        this.AnimationPlayer = new ModelAnimationPlayer(animations ?? Array.Empty<ModelAnimation>());
        this._model = model;
        this._materials = materials;
        this._color = color ?? Color.WHITE;
        this._drawWires = drawWires;
        this.SetupMaterial();
    }
    
    protected internal override unsafe void Draw() {
        base.Draw();
        SceneManager.MainCam3D!.BeginMode3D();
        
        ShaderHelper.SetValue(this._model.Materials[0].Shader, SceneManager.ActiveScene!.GetEntity(1).GetComponent<Light>().TilingLoc, new Vector2(0.5F, 0.5F), ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        ShaderHelper.SetValue(this._model.Materials[0].Shader, SceneManager.ActiveScene!.GetEntity(1).GetComponent<Light>().EmissiveColorLoc, ColorHelper.Normalize(this._materials[0].Maps[(int) MaterialMapIndex.MATERIAL_MAP_EMISSION].Color), ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        ShaderHelper.SetValue(this._model.Materials[0].Shader, SceneManager.ActiveScene!.GetEntity(1).GetComponent<Light>().EmissivePowerLoc, 0.01F, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);
        
        BoundingBox box = ModelHelper.GetBoundingBox(this._model);
        box.Min.X += this.Entity.Position.X;
        box.Max.X += this.Entity.Position.X;
        
        box.Min.Y += this.Entity.Position.Y;
        box.Max.Y += this.Entity.Position.Y;
        
        box.Min.Z += this.Entity.Position.Z;
        box.Max.Z += this.Entity.Position.Z;
        
        // TODO IMPLEMENT OOBB BOXES!
        if (SceneManager.MainCam3D.GetFrustum().ContainsBox(box)) {
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
        SceneManager.MainCam3D.EndMode3D();
    }

    protected internal override void FixedUpdate() {
        base.FixedUpdate();
        this.AnimationPlayer.FixedUpdate(this._model);
    }

    private unsafe void SetupMaterial() {
        for (int i = 0; i < this._model.MaterialCount; i++) {
            this._model.Materials[i] = this._materials[i];
        }
    }

    protected override void Dispose(bool disposing) { }
}