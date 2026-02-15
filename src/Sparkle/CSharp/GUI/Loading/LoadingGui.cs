namespace Sparkle.CSharp.GUI.Loading;

public abstract class LoadingGui : Gui {
    
    public float MinTime { get; set; }
    
    public float Progress { get; internal set; }
    
    protected LoadingGui(string name, float minTime = 0.5F, (int, int)? size = null) : base(name, size) {
        this.MinTime = minTime;
    }
}