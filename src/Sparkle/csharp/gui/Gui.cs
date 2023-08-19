using Sparkle.csharp.audio;
using Sparkle.csharp.content;
using Sparkle.csharp.graphics;
using Sparkle.csharp.gui.elements;
using Sparkle.csharp.window;

namespace Sparkle.csharp.gui; 

public abstract class Gui : IDisposable {
    
    protected Window Window => Game.Instance.Window;
    protected Graphics Graphics => Game.Instance.Graphics;
    protected ContentManager Content => Game.Instance.Content;
    protected AudioDevice AudioDevice => Game.Instance.AudioDevice;
    
    public readonly string Name;
    
    public bool HasInitialized { get; private set; }

    private Dictionary<string, GuiElement> _elements;

    public Gui(string name) {
        this.Name = name;
        this._elements = new Dictionary<string, GuiElement>();
    }
    
    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    protected internal virtual void Init() {
        this.HasInitialized = true;
    }
    
    /// <summary>
    /// Is invoked during each tick and is used for updating dynamic elements and game logic.
    /// </summary>
    protected internal virtual void Update() {
        foreach (GuiElement element in this._elements.Values) {
            element.Update();
        }
    }

    /// <summary>
    /// Is invoked at a fixed rate of every 60 frames following the <see cref="Update"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal virtual void FixedUpdate() {
        foreach (GuiElement element in this._elements.Values) {
            element.FixedUpdate();
        }
    }

    /// <summary>
    /// Is called every tick, used for rendering stuff.
    /// </summary>
    protected internal virtual void Draw() {
        foreach (GuiElement element in this._elements.Values) {
            if (element.Enabled) {
                element.Draw();
            }
        }
    }
    
    /// <summary>
    /// Adds a GUI element to the collection and initializes it.
    /// </summary>
    /// <param name="element">The GUI element to be added.</param>
    protected void AddElement(GuiElement element) {
        element.Init();
        
        this._elements.Add(element.Name, element);
    }
    
    /// <summary>
    /// Removes a GUI element from the collection.
    /// </summary>
    /// <param name="name">The name of the GUI element to be removed.</param>
    protected void RemoveElement(string name) {
        this._elements.Remove(name);
    }
    
    /// <summary>
    /// Removes a GUI element from the collection.
    /// </summary>
    /// <param name="element">The GUI element to be removed.</param>
    protected void RemoveElement(GuiElement element) {
        this.RemoveElement(element.Name);
    }

    /// <summary>
    /// Retrieves a GUI element from the collection by its name.
    /// </summary>
    /// <param name="name">The name of the GUI element to be retrieved.</param>
    /// <returns>The GUI element associated with the specified name.</returns>
    protected GuiElement GetElement(string name) {
        return this._elements[name];
    }

    /// <summary>
    /// Retrieves an array of all GUI elements currently in the collection.
    /// </summary>
    /// <returns>An array containing all GUI elements in the collection.</returns>
    protected GuiElement[] GetElements() {
        return this._elements.Values.ToArray();
    }

    public virtual void Dispose() { }
}