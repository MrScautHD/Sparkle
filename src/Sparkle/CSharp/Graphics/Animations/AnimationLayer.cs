using System.Numerics;

namespace Sparkle.CSharp.Graphics.Animations;

public class AnimationLayer {
    
    /// <summary>
    /// Gets the name of the animation layer.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Gets the animator controller associated with this layer.
    /// </summary>
    public AnimatorController Controller { get; private set; }

    /// <summary>
    /// Gets the blending weight of this layer, A value of <c>1.0</c> means full influence, while <c>0.0</c> disables the layer.
    /// </summary>
    public float Weight;
    
    /// <summary>
    /// Gets the set of bone names affected by this layer, If empty, the layer affects all bones. Otherwise, only bones contained in the mask are influenced.
    /// </summary>
    public HashSet<string> BoneMask;
    
    /// <summary>
    /// Gets or sets the currently active animation state.
    /// </summary>
    public AnimatorState? CurrentState { get; internal set; }
    
    /// <summary>
    /// Gets or sets the previously active animation state.
    /// </summary>
    public AnimatorState? PreviousState { get; internal set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether snapshot-based blending is used.
    /// </summary>
    public bool UseSnapshotBlend { get; internal set; }
    
    /// <summary>
    /// Gets or sets the cached local poses used for snapshot blending.
    /// </summary>
    public Matrix4x4[]? SnapshotLocalPoses { get; internal set; }
    
    /// <summary>
    /// Gets or sets the starting snapshot used for blend interpolation.
    /// </summary>
    public Matrix4x4[]? BlendStartSnapshot { get; internal set; }
    
    /// <summary>
    /// Gets or sets the total duration of the current blend.
    /// </summary>
    public float CurrentBlendDuration { get; internal set; }
    
    /// <summary>
    /// Gets or sets the elapsed time since the current blend started.
    /// </summary>
    public float BlendTimer { get; internal set; }
    
    /// <summary>
    /// Gets or sets the current blend weight between previous and current states.
    /// </summary>
    public float BlendWeight { get; internal set; }
    
    /// <summary>
    /// Gets or sets the current playback time of the active state.
    /// </summary>
    public double CurrentTime { get; internal set; }
    
    /// <summary>
    /// Gets or sets the playback time of the previous state.
    /// </summary>
    public double PreviousTime { get; internal set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationLayer"/> class.
    /// </summary>
    /// <param name="name">The unique name of the animation layer.</param>
    /// <param name="controller">The animator controller that defines the states for this layer.</param>
    /// <param name="weight">The blending weight of the layer.</param>
    /// <param name="boneMask">An optional set of bone names affected by this layer.</param>
    public AnimationLayer(string name, AnimatorController controller, float weight = 1.0F, HashSet<string>? boneMask = null) {
        this.Name = name;
        this.Controller = controller;
        this.Weight = weight;
        this.BoneMask = boneMask ?? [];
    }
}