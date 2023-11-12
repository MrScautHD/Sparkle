using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.helper;
using Sparkle.csharp.registry.types;
using Sparkle.csharp.scene;

namespace Sparkle.csharp.entity.component; 

public class ModelRenderer : Component {
    
    private Model _model;
    private Texture2D _texture;
    private Shader _shader;
    private MaterialMapIndex _materialMap;
    private Color _color;
    
    private bool _drawWires;
    
    /// <summary>
    /// Initializes a new instance of the ModelRenderer class with the specified model and texture, allowing for optional shader, material map, color, and wireframe rendering settings.
    /// </summary>
    /// <param name="model">The 3D model to render.</param>
    /// <param name="texture">The texture to apply to the model.</param>
    /// <param name="shader">An optional custom shader to use (default is null).</param>
    /// <param name="materialMap">The material map index for the texture (default is MATERIAL_MAP_ALBEDO).</param>
    /// <param name="color">The color to apply to the model (default is Color.WHITE).</param>
    /// <param name="drawWires">A flag indicating whether to render the model in wireframe mode (default is false).</param>
    public ModelRenderer(Model model, Texture2D texture, Shader? shader = null, MaterialMapIndex materialMap = MaterialMapIndex.MATERIAL_MAP_ALBEDO, Color? color = null, bool drawWires = false) {
        this._model = model;
        this._texture = texture;
        this._shader = shader ?? ShaderRegistry.DiscardAlpha;
        this._materialMap = materialMap;
        this._color = color ?? Color.WHITE;
        this._drawWires = drawWires;
        
        for (int i = 0; i < model.MaterialCount; i++) {
            MaterialHelper.SetShader(ref this._model, i, ref this._shader);
        }
        
        for (int i = 0; i < model.MaterialCount; i++) {
            MaterialHelper.SetTexture(ref this._model, i, this._materialMap, ref this._texture);
        }
    }
    
    protected internal override unsafe void Draw() {
        base.Draw();
        SceneManager.MainCam3D!.BeginMode3D();
        
        Vector3 axis;
        float angle;
        
        Raymath.QuaternionToAxisAngle(this.Entity.Rotation, &axis, &angle);

        BoundingBox box = ModelHelper.GetBoundingBox(this._model);
        box.Min.Y += this.Entity.Position.Y;
        box.Max.Y += this.Entity.Position.Y;
        
        if (SceneManager.MainCam3D.GetFrustum().ContainsBox(box)) {
            if (this._drawWires) {
                ModelHelper.DrawModelWires(this._model, this.Entity.Position, axis, angle * Raylib.RAD2DEG, this.Entity.Scale, this._color);
            }
            else {
                ModelHelper.DrawModel(this._model, this.Entity.Position, axis, angle * Raylib.RAD2DEG, this.Entity.Scale, this._color);
            }
        }
        
        
        SceneManager.MainCam3D.EndMode3D();
    }
    
    protected override void Dispose(bool disposing) { }
}