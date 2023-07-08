using Sparkle.csharp.content;
using Sparkle.csharp.graphics;
using Sparkle.csharp.gui.elements;
using Sparkle.csharp.window;

namespace Sparkle.csharp.gui; 

public abstract class Gui : IDisposable {
    
    // TODO MAKE A RESIZE SUPPORT

    protected Window Window => Game.Instance.Window;
    
    protected Graphics Graphics => Game.Instance.Graphics;

    protected ContentManager Content => Game.Instance.Content;
    
    public readonly string Name;

    private Dictionary<string, GuiElement> _elements;

    public Gui(string name) {
        this.Name = name;
        this._elements = new Dictionary<string, GuiElement>();
    }
    
    protected internal virtual void Init() {
        
    }
    
    protected internal virtual void Update() {
        foreach (GuiElement element in this._elements.Values) {
            element.Update();
        }
    }

    protected internal virtual void FixedUpdate() {
        foreach (GuiElement element in this._elements.Values) {
            element.FixedUpdate();
        }
    }

    protected internal virtual void Draw() {
        foreach (GuiElement element in this._elements.Values) {
            element.Draw();
        }
    }
    
    protected void AddElement(GuiElement element) {
        element.Init();
        
        this._elements.Add(element.Name, element);
    }
    
    protected void RemoveElement(string name) {
        this._elements.Remove(name);
    }
    
    protected void RemoveElement(GuiElement element) {
        this.RemoveElement(element.Name);
    }

    protected GuiElement GetElement(string name) {
        return this._elements[name];
    }

    protected GuiElement[] GetElements() {
        return this._elements.Values.ToArray();
    }

    public virtual void Dispose() {
        
    }
}