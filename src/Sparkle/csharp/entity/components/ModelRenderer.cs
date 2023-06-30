using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.scene;

namespace Sparkle.csharp.entity.components; 

public class ModelRenderer : Component {
    
    private Model _model;
    private Texture2D _texture;
    private MaterialMapIndex _materialMap;
    private Color _color;
    
    private bool _drawWires;
    
    public ModelRenderer(Model model, Texture2D texture, MaterialMapIndex materialMap = MaterialMapIndex.MATERIAL_MAP_ALBEDO, Color? color = null, bool drawWires = false) {
        this._model = model;
        this._texture = texture;
        this._materialMap = materialMap;
        this._color = color ?? Color.WHITE;
        this._drawWires = drawWires;
        
        Raylib.SetMaterialTexture(ref this._model, 0, this._materialMap, ref this._texture);
    }
    
    protected internal override unsafe void Draw() {
        base.Draw();
        
        SceneManager.MainCamera!.BeginMode3D();
        
        Vector3 axis;
        float angle;
        
        Raymath.QuaternionToAxisAngle(this.Entity.Rotation, &axis, &angle);
        
        if (this._drawWires) {
            Raylib.DrawModelWiresEx(this._model, this.Entity.Position, axis, angle, this.Entity.Scale, this._color);
        }
        else {
            Raylib.DrawModelEx(this._model, this.Entity.Position, axis, angle, this.Entity.Scale, Color.WHITE);
        }
        
        SceneManager.MainCamera.EndMode3D();
    }
}