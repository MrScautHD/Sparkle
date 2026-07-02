using Bliss.CSharp;
using Bliss.CSharp.Transformations;
using Bliss.ImGUI.CSharp;

namespace Sparkle.CSharp.ImGUI;

public abstract class ImGuiOverlay : Disposable {
    
    /// <summary>
    /// The unique name used to register, look up and remove this overlay.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Whether this overlay is updated and drawn. Toggle this to show or hide the overlay.
    /// </summary>
    public bool Enabled;
    
    /// <summary>
    /// Initializes a new <see cref="ImGuiOverlay"/>.
    /// </summary>
    /// <param name="name">The unique name of the overlay.</param>
    /// <param name="enabled">Whether the overlay starts enabled. Defaults to <see langword="false"/>.</param>
    protected ImGuiOverlay(string name, bool enabled = false) {
        this.Name = name;
        this.Enabled = enabled;
    }
    
    /// <summary>
    /// Updates the overlay each frame.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected internal virtual void Update(double delta) { }
    
    /// <summary>
    /// Executes logic after the update step.
    /// </summary>
    /// <param name="delta">The time delta since the last update.</param>
    protected internal virtual void AfterUpdate(double delta) { }
    
    /// <summary>
    /// Executes fixed-step updates for the overlay.
    /// </summary>
    /// <param name="timeStep">The fixed time step interval for the update.</param>
    protected internal virtual void FixedUpdate(double timeStep) { }
    
    /// <summary>
    /// Emits the ImGui draw commands for this overlay. Called between <c>ImGui.NewFrame</c> and <c>ImGui.Render</c>.
    /// </summary>
    /// <param name="controller">The controller driving this overlay, for access to IO, style and texture bindings.</param>
    protected internal abstract void Draw(ImGuiController controller);
    
    /// <summary>
    /// Executes when the window is resized.
    /// </summary>
    /// <param name="rectangle">The rectangle specifying the window's updated size.</param>
    protected internal virtual void Resize(Rectangle rectangle) { }
}