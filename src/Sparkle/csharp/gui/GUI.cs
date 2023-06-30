using Sparkle.csharp.content;
using Sparkle.csharp.graphics;
using Sparkle.csharp.gui.elements;
using Sparkle.csharp.window;

namespace Sparkle.csharp.gui; 

public abstract class GUI : IDisposable {
    
    protected Window Window => Game.Instance.Window;
    
    protected Graphics Graphics => Game.Instance.Graphics;

    protected ContentManager Content => Game.Instance.Content;
    
    public readonly string Name;
    
    private readonly Dictionary<string, GUIElement> _elements;

    public GUI(string name) {
        this.Name = name;
        this._elements = new Dictionary<string, GUIElement>();
    }
    
    protected internal virtual void Init() {
        foreach (GUIElement element in this._elements.Values) {
            element.Init();
        }
    }

    protected internal virtual void Update() {
        foreach (GUIElement element in this._elements.Values) {
            element.Update();
        }
    }

    protected internal virtual void Draw() {
        foreach (GUIElement element in this._elements.Values) {
            element.Draw();
        }
    }
    
    protected void AddElement(GUIElement element) {
        element.Init();
        
        this._elements.Add(element.Name, element);
    }
    
    protected void RemoveElement(string name) {
        this._elements.Remove(name);
    }
    
    protected void RemoveElement(GUIElement element) {
        this.RemoveElement(element.Name);
    }

    protected GUIElement GetElement(string name) {
        return this._elements[name];
    }

    protected GUIElement[] GetElements() {
        return this._elements.Values.ToArray();
    }

    public virtual void Dispose() {
        
    }
}