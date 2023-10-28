using System.Numerics;
using Raylib_cs;
using Sparkle.csharp.graphics.util;
using Sparkle.csharp.scene;

namespace Sparkle.csharp.entity.component; 

public class ModelRenderer : Component {
    
    private Model _model;
    private Texture2D _texture;
    private MaterialMapIndex _materialMap;
    private Shader? _shader;
    private Color _color;
    
    private bool _drawWires;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelRenderer"/>, setting the model, texture, material map, optional color, and wireframe rendering option.
    /// </summary>
    /// <param name="model">The 3D model to be rendered.</param>
    /// <param name="texture">The texture to be applied to the model.</param>
    /// <param name="materialMap">The type of material map to be used. Default is MaterialMapIndex.MATERIAL_MAP_ALBEDO.</param>
    /// <param name="color">Optional color to be applied to the model. Default is white.</param>
    /// <param name="drawWires">Optional flag to indicate whether to render the model in wireframe. Default is false.</param>
    public ModelRenderer(Model model, Texture2D texture, MaterialMapIndex materialMap = MaterialMapIndex.MATERIAL_MAP_ALBEDO, Shader? shader = null, Color? color = null, bool drawWires = false) {
        this._model = model;
        this._texture = texture;
        this._materialMap = materialMap;
        this._shader = shader;
        this._color = color ?? Color.WHITE;
        this._drawWires = drawWires;
        
        MaterialHelper.SetTexture(ref this._model, 0, this._materialMap, ref this._texture);

        if (this._shader != null) {
            Shader shaderRef = this._shader.Value;
            MaterialHelper.SetShader(ref this._model, 0, ref shaderRef);
        }
    }
    
    protected internal override unsafe void Draw() {
        base.Draw();
        
        SceneManager.MainCam3D!.BeginMode3D();
        
        Vector3 axis;
        float angle;
        
        Raymath.QuaternionToAxisAngle(this.Entity.Rotation, &axis, &angle);
        
        if (this._drawWires) {
            
            ModelHelper.DrawModelWires(this._model, this.Entity.Position, axis, angle * Raylib.RAD2DEG, this.Entity.Scale, this._color);
        }
        else {
            ModelHelper.DrawModel(this._model, this.Entity.Position, axis, angle * Raylib.RAD2DEG, this.Entity.Scale, this._color);
        }
        
        SceneManager.MainCam3D.EndMode3D();
    }

    protected override void Dispose(bool disposing) { }
}