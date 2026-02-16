namespace Sparkle.CSharp.GUI.Loading;

public abstract class LoadingGui : Gui {
    
    /// <summary>
    /// Gets the minimum time, in seconds, that a loading operation should take.
    /// </summary>
    public float MinTime { get; private set; }
    
    /// <summary>
    /// Gets the current loading progress value.
    /// </summary>
    public float Progress { get; internal set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LoadingGui"/> class.
    /// </summary>
    /// <param name="name">The unique name of the loading GUI.</param>
    /// <param name="minTime">The minimum amount of time, in seconds, that the loading screen should remain visible.</param>
    /// <param name="size">Optional size of the GUI in pixels as a tuple (width, height). If <c>null</c>, a default size is used.</param>
    protected LoadingGui(string name, float minTime = 0.5F, (int, int)? size = null) : base(name, size) {
        this.MinTime = minTime;
    }
}