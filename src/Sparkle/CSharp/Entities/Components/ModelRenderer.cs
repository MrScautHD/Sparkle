using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

public class ModelRenderer : Component {

    private Model _model;
    private BoundingBox _box;
    private Sampler? _sampler;
    private Color _color;
    private bool _drawWires;
    
    public ModelRenderer(Model model, Vector3 offsetPos, Sampler? sampler = null, bool drawWires = false, Color? color = null) : base(offsetPos) {
        this._model = model;
        this._box = model.BoundingBox;
        this._sampler = sampler;
        this._color = color ?? Color.White;
        this._drawWires = drawWires;
    }

    protected internal override void Update(double delta) {
        base.Update(delta);
        
        Vector3 dimension = this._box.Max - this._box.Min;
        this._box.Min.X = this.GlobalPos.X - dimension.X / 2;
        this._box.Min.Y = this.GlobalPos.Y;
        this._box.Min.Z = this.GlobalPos.Z - dimension.Z / 2;
        
        this._box.Max.X = this.GlobalPos.X + dimension.X / 2;
        this._box.Max.Y = this.GlobalPos.Y + dimension.Y;
        this._box.Max.Z = this.GlobalPos.Z + dimension.Z / 2;
    }

    protected internal override void Draw(GraphicsContext context) {
        base.Draw(context);
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D == null) {
            return;
        }
        
        if (cam3D.GetFrustum().ContainsOrientedBox(this._box, this.GlobalPos, this.Entity.Transform.Rotation)) {
            this._model.Draw(context.CommandList, new Transform() { Translation = this.GlobalPos }, context.Output, this._sampler, this._drawWires, this._color);
        }
    }
}