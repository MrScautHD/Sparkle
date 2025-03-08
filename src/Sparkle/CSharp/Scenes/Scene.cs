using Sparkle.CSharp.Physics;

namespace Sparkle.CSharp.Scenes;

public abstract class Scene {
    
    public string Name { get; private set; }
    public SceneType Type { get; private set; }
    
    public Simulation Simulation { get; private set; }
    // TODO: Add Skybox
    // TODO: Add FilterEffect

    //internal Dictionary<uint, Entity> Entities;
    //private uint _entityIds;
    
    //public bool HasInitialized { get; private set; }
}