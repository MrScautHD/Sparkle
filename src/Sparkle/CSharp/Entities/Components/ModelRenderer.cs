using System.Numerics;
using Bliss.CSharp.Colors;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Graphics.Rendering.Renderers.Forward;
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
    /// Whether the bounding box should be rendered for the associated model.
    /// </summary>
    public bool DrawBox;
    
    /// <summary>
    /// The color used for rendering the bounding box of the model.
    /// </summary>
    public Color BoxColor;
    
    /// <summary>
    /// The original bounding box of the model before being transformed.
    /// </summary>
    private BoundingBox _baseBox;
    
    /// <summary>
    /// The bounding box of the model used for visibility checks.
    /// </summary>
    private BoundingBox _frustumBox;
    
    /// <summary>
    /// A collection that maps each mesh in the model to its corresponding renderable representation.
    /// </summary>
    private Dictionary<Mesh, Renderable> _renderables;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelRenderer"/> class.
    /// </summary>
    /// <param name="model">The model containing the meshes to be rendered.</param>
    /// <param name="offsetPosition">The positional offset applied to the model renderer.</param>
    /// <param name="copyModelMaterials">Whether to clone the model's materials so they can be modified independently.</param>
    /// <param name="drawBox">Whether to render the bounding box for debugging.</param>
    /// <param name="boxColor">The color used to render the bounding box.</param>
    public ModelRenderer(Model model, Vector3 offsetPosition, bool copyModelMaterials = false, bool drawBox = false, Color? boxColor = null) : base(offsetPosition) { // TODO: ADD LOCAL TRANSFORM
        this.Model = model;
        this.DrawBox = drawBox;
        this.BoxColor = boxColor ?? Color.White;
        this._baseBox = this._frustumBox = model.GenBoundingBox();
        this._renderables = new Dictionary<Mesh, Renderable>();
        
        foreach (Mesh mesh in this.Model.Meshes) {
            this._renderables.Add(mesh, new Renderable(mesh, new Transform(), copyModelMaterials));
        }
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
        
        // Updates frustum box.
        this.UpdateFrustumBox();
        
        if (cam3D.GetFrustum().ContainsOrientedBox(this._frustumBox, this.LerpedGlobalPosition, this.LerpedRotation)) {
            Transform modelTransform = new Transform() {
                Translation = this.LerpedGlobalPosition,
                Rotation = this.LerpedRotation,
                Scale = this.LerpedScale
            };
            
            // Draw the model.
            foreach (Renderable renderable in this._renderables.Values) {
                renderable.Transforms[0] = modelTransform;
                this.Entity.Scene.Renderer.DrawRenderable(renderable);
            }
            
            // Draw the bounding box.
            if (this.DrawBox) {
                Transform boxTransform = new Transform() {
                    Translation = this.LerpedGlobalPosition,
                    Rotation = this.LerpedRotation,
                    Scale = this.LerpedScale
                };
                
                // Draw box.
                context.ImmediateRenderer.DrawBoundingBox(context.CommandList, framebuffer.OutputDescription, boxTransform, this._baseBox, this.BoxColor);
            }
        }
    }
    
    /// <summary>
    /// Retrieves the material associated with the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh for which the material is to be retrieved.</param>
    /// <returns>A reference to the material associated with the specified mesh.</returns>
    public ref Material GetRenderableMaterialByMesh(Mesh mesh) {
        return ref this._renderables[mesh].Material;
    }
    
    /// <summary>
    /// Retrieves the bone matrices for a specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh for which the bone matrices are to be retrieved.</param>
    /// <returns>A reference to an array of bone matrices associated with the specified mesh, or null if no matrices exist.</returns>
    public Matrix4x4[]? GetRenderableBoneMatricesByMesh(Mesh mesh) {
        return this._renderables[mesh].BoneMatrices;
    }

    /// <summary>
    /// Updates the frustum-aligned bounding box.
    /// </summary>
    private void UpdateFrustumBox() {
        
        // Calculate original dimensions and center offset in local space.
        Vector3 originalCenter = (this._baseBox.Min + this._baseBox.Max) / 2.0F;
        Vector3 originalDimension = this._baseBox.Max - this._baseBox.Min;
        
        // Scale everything.
        Vector3 dimension = originalDimension * this.LerpedScale;
        Vector3 centerOffset = originalCenter * this.LerpedScale;
        
        // Normalize X and Z dimensions to the maximum width.
        float maxSide = Math.Max(dimension.X, dimension.Z);
        dimension.X = maxSide;
        dimension.Z = maxSide;
        
        // Calculate bounding box position using the lerped global position and the center offset.
        Vector3 lerpedPos = this.LerpedGlobalPosition;
        Vector3 finalCenter = lerpedPos + centerOffset;
        
        this._frustumBox.Min.X = finalCenter.X - dimension.X / 2.0F;
        this._frustumBox.Min.Y = lerpedPos.Y + (this._baseBox.Min.Y * this.LerpedScale.Y);
        this._frustumBox.Min.Z = finalCenter.Z - dimension.Z / 2.0F;
        
        this._frustumBox.Max.X = finalCenter.X + dimension.X / 2.0F;
        this._frustumBox.Max.Y = lerpedPos.Y + (this._baseBox.Max.Y * this.LerpedScale.Y);
        this._frustumBox.Max.Z = finalCenter.Z + dimension.Z / 2.0F;
    }
}