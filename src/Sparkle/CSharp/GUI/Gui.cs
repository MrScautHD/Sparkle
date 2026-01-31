using Bliss.CSharp;
using Bliss.CSharp.Transformations;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.GUI.Elements;
using Veldrid;

namespace Sparkle.CSharp.GUI;

public abstract class Gui : Disposable {
    
    /// <summary>
    /// The name of the GUI.
    /// </summary>
    public readonly string Name;
    
    /// <summary>
    /// The dimensions of the GUI.
    /// </summary>
    public readonly (int Width, int Height) Size;
    
    /// <summary>
    /// The scaling factor applied to the GUI.
    /// </summary>
    public int ScaleFactor => (int) MathF.Round(GuiManager.Scale * ((float) GlobalGraphicsAssets.Window.GetHeight() / (float) this.Size.Height));
    
    /// <summary>
    /// Internal dictionary storing GUI elements by name.
    /// </summary>
    private Dictionary<string, GuiElement> _elements;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Gui"/> class.
    /// </summary>
    /// <param name="name">The name of the GUI.</param>
    /// <param name="size">The width and height of the GUI.</param>
    public Gui(string name, (int, int)? size = null) {
        this.Name = name;
        this.Size = size ?? (1280, 720);
        this._elements = new Dictionary<string, GuiElement>();
    }
    
    /// <summary>
    /// Called when the GUI is initialized.
    /// </summary>
    protected internal virtual void Init() { }
    
    /// <summary>
    /// Updates all enabled elements in the GUI.
    /// </summary>
    /// <param name="delta">Elapsed time since the last frame in seconds.</param>
    protected internal virtual void Update(double delta) {
        foreach (GuiElement element in this._elements.Values) {
            if (element.Enabled) {
                element.Update(delta);
            }
        }
    }

    /// <summary>
    /// Performs post-update logic on all enabled elements.
    /// </summary>
    /// <param name="delta">Elapsed time since the last frame in seconds.</param>
    protected internal virtual void AfterUpdate(double delta) {
        foreach (GuiElement element in this._elements.Values) {
            if (element.Enabled) {
                element.AfterUpdate(delta);
            }
        }
    }
    
    /// <summary>
    /// Performs fixed-step updates on all enabled elements.
    /// </summary>
    /// <param name="fixedStep">The fixed timestep interval.</param>
    protected internal virtual void FixedUpdate(double fixedStep) {
        foreach (GuiElement element in this._elements.Values) {
            if (element.Enabled) {
                element.FixedUpdate(fixedStep);
            }
        }
    }
    
    /// <summary>
    /// Draws all enabled elements to the specified framebuffer using the provided graphics context.
    /// </summary>
    /// <param name="context">The rendering context.</param>
    /// <param name="framebuffer">The framebuffer to draw into.</param>
    protected internal virtual void Draw(GraphicsContext context, Framebuffer framebuffer) {
        foreach (GuiElement element in this._elements.Values) {
            if (element.Enabled) {
                element.Draw(context, framebuffer);
            }
        }
    }
    
    /// <summary>
    /// Resizes all elements in the GUI based on the new window or framebuffer size.
    /// </summary>
    /// <param name="rectangle">The new size rectangle.</param>
    protected internal virtual void Resize(Rectangle rectangle) {
        foreach (GuiElement element in this._elements.Values) {
            element.Resize(rectangle);
        }
    }
    
    /// <summary>
    /// Retrieves all GUI elements associated with the current GUI instance.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="GuiElement"/> objects belonging to this GUI.</returns>
    public IEnumerable<GuiElement> GetElements() {
        return this._elements.Values;
    }
    
    /// <summary>
    /// Determines whether an element with the specified name exists in the GUI.
    /// </summary>
    /// <param name="name">The name of the element to search for.</param>
    /// <returns>True if an element with the specified name exists; otherwise, false.</returns>
    public bool HasElement(string name) {
        return this._elements.ContainsKey(name);
    }
    
    /// <summary>
    /// Retrieves a GUI element by its name.
    /// </summary>
    /// <param name="name">The name of the GUI element to retrieve.</param>
    /// <returns>The <see cref="GuiElement"/> associated with the specified name if found; otherwise, null.</returns>
    public GuiElement? GetElement(string name) {
        if (!this.TryGetElement(name, out GuiElement? result)) {
            return null;
        }

        return result;
    }
    
    /// <summary>
    /// Attempts to retrieve a GUI element by its name.
    /// </summary>
    /// <param name="name">The name of the GUI element to retrieve.</param>
    /// <param name="element">When this method returns, contains the GUI element associated with the specified name, if found; otherwise, null.</param>
    /// <returns>True if the element with the specified name is found; otherwise, false.</returns>
    public bool TryGetElement(string name, out GuiElement? element) {
        return this._elements.TryGetValue(name, out element);
    }
    
    /// <summary>
    /// Adds a new GUI element to the current GUI instance.
    /// </summary>
    /// <param name="name">The unique name of the GUI element being added.</param>
    /// <param name="element">The instance of the <see cref="GuiElement"/> to add to the GUI.</param>
    /// <exception cref="Exception">Thrown if the element with the specified name is already present in the GUI or has been added to another GUI.</exception>
    public void AddElement(string name, GuiElement element) {
        if (!this.TryAddElement(name, element)) {
            throw new Exception($"The element with the name: [{name}] is already present in the GUI or has been added to another one!");
        }
    }
    
    /// <summary>
    /// Attempts to add a new GUI element to the current GUI.
    /// </summary>
    /// <param name="name">The name of the GUI element to add. Must be unique and non-empty.</param>
    /// <param name="element">The GUI element to add to the current GUI.</param>
    /// <returns> <c>true</c> if the element is successfully added; <c>false</c> if the name is empty, if an element with the same name already exists, or if the element is already associated with another GUI.</returns>
    public bool TryAddElement(string name, GuiElement element) {
        if (name == string.Empty) {
            return false;
        }
        
        if (this._elements.ContainsKey(name)) {
            return false;
        }
        
        if (element.Gui != null!) {
            return false;
        }
        
        element.Gui = this;
        element.Name = name;
        
        this._elements.Add(name, element);
        return true;
    }
    
    /// <summary>
    /// Removes the specified GUI element from the collection.
    /// </summary>
    /// <param name="element">The GUI element to be removed.</param>
    /// <exception cref="Exception">Thrown when the element could not be removed or disposed.</exception>
    public void RemoveElement(GuiElement element) {
        if (!this.TryRemoveElement(element)) {
            throw new Exception($"Failed to Remove/Dispose the element: [{element.Name}] from the GUI: [{this.Name}]");
        }
    }
    
    /// <summary>
    /// Attempts to remove the specified GUI element from the collection of elements.
    /// </summary>
    /// <param name="element">The <see cref="GuiElement"/> instance to be removed.</param>
    /// <returns><c>true</c> if the element was successfully removed; otherwise, <c>false</c>.</returns>
    public bool TryRemoveElement(GuiElement element) {
        if (!this._elements.Remove(element.Name)) {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Removes the GUI element with the specified name. Throws an exception if the element cannot be removed.
    /// </summary>
    /// <param name="name">The name of the GUI element to remove.</param>
    /// <exception cref="Exception">Thrown when the element cannot be removed or disposed of successfully.</exception>
    public void RemoveElement(string name) {
        if (!this.TryRemoveElement(name)) {
            throw new Exception($"Failed to Remove/Dispose the element: [{name}] from the GUI: [{this.Name}]");
        }
    }
    
    /// <summary>
    /// Attempts to remove a GUI element by its name.
    /// </summary>
    /// <param name="name">The name of the GUI element to remove.</param>
    /// <returns><c>true</c> if the element is successfully removed; otherwise, <c>false</c>.</returns>
    public bool TryRemoveElement(string name) {
        if (!this._elements.Remove(name)) {
            return false;
        }
        
        return true;
    }
    
    protected override void Dispose(bool disposing) {
        if (disposing) {
            this._elements.Clear();
        }
    }
}