using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward.Renderables;
using Bliss.CSharp.Materials;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Scenes;
using Veldrid;

namespace Sparkle.CSharp.Entities.Components;

public class ModelRenderer : InterpolatedComponent {

    /// <summary>
    /// The model to be rendered.
    /// </summary>
    public Model Model { get; private set; }

    /// <summary>
    /// Indicates whether the bounding box around the model should be rendered.
    /// </summary>
    public bool DrawBoundingBox;

    /// <summary>
    /// The color used for rendering the bounding box of the model.
    /// </summary>
    public Color BoxColor;

    /// <summary>
    /// The bounding box of the model used for visibility checks.
    /// </summary>
    private BoundingBox _box;

    /// <summary>
    /// A collection that maps each mesh in the model to its corresponding renderable representation.
    /// </summary>
    private Dictionary<Mesh, Renderable> _renderables;
    
    public ModelRenderer(Model model, Vector3 offsetPosition, bool copyModelMaterials = false, bool drawBoundingBox = false, Color? boxColor = null) : base(offsetPosition) {
        this.Model = model;
        this.DrawBoundingBox = drawBoundingBox;
        this.BoxColor = boxColor ?? Color.White;
        this._box = model.BoundingBox;
        this._renderables = new Dictionary<Mesh, Renderable>();
        
        foreach (Mesh mesh in this.Model.Meshes) {
            this._renderables.Add(mesh, new Renderable(mesh, new Transform(), copyModelMaterials));
        }
    }

    /// <summary>
    /// Updates the bounding box of the model based on its position and dimensions.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);
        
        // Calculate bounding box.
        Vector3 dimension = this._box.Max - this._box.Min;
        Vector3 lerpedPos = this.LerpedGlobalPosition;
        this._box.Min.X = lerpedPos.X - dimension.X / 2.0F;
        this._box.Min.Y = lerpedPos.Y;
        this._box.Min.Z = lerpedPos.Z - dimension.Z / 2.0F;
        
        this._box.Max.X = lerpedPos.X + dimension.X / 2.0F;
        this._box.Max.Y = lerpedPos.Y + dimension.Y;
        this._box.Max.Z = lerpedPos.Z + dimension.Z / 2.0F;
    }
    
    /// <summary>
    /// Renders the model associated with this component to the specified framebuffer if it is within the camera frustum.
    /// </summary>
    /// <param name="context">The graphics context used for rendering.</param>
    /// <param name="framebuffer">The framebuffer to render into.</param>
    protected internal override void Draw(GraphicsContext context, Framebuffer framebuffer) {
        base.Draw(context, framebuffer);
        Camera3D? cam3D = SceneManager.ActiveCam3D;
        
        if (cam3D == null) {
            return;
        }
        
        if (cam3D.GetFrustum().ContainsOrientedBox(this._box, this.LerpedGlobalPosition, this.LerpedRotation)) {
            Transform transform = new Transform() {
                Translation = this.LerpedGlobalPosition,
                Rotation = this.LerpedRotation,
                Scale = this.LerpedScale
            };
            
            // Draw the model.
            foreach (Renderable renderable in this._renderables.Values) {
                renderable.Transform = transform;
                this.Entity.Scene.ForwardRenderer.DrawRenderable(renderable);
            }

            // Draw the bounding box.
            if (this.DrawBoundingBox) {
                context.ImmediateRenderer.DrawBoundingBox(context.CommandList, framebuffer.OutputDescription, new Transform(), this._box, this.BoxColor);
            }
        }
    }

    /// <summary>
    /// Retrieves the material associated with the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh for which the material is to be retrieved.</param>
    /// <returns>A reference to the material associated with the specified mesh.</returns>
    public ref Material GetMaterial(Mesh mesh) {
        return ref this._renderables[mesh].Material;
    }

    /// <summary>
    /// Retrieves the bone matrices for a specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh for which the bone matrices are to be retrieved.</param>
    /// <returns>A reference to an array of bone matrices associated with the specified mesh, or null if no matrices exist.</returns>
    public ref Matrix4x4[]? GetBoneMatrices(Mesh mesh) {
        return ref this._renderables[mesh].BoneMatrices;
    }
}