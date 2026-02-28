using Bliss.CSharp.Geometry.Animation;

namespace Sparkle.CSharp.Graphics.Animations;

public class AnimatorState {
    
    /// <summary>
    /// The name of this animation state.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// The actual animation data associated with this state.
    /// </summary>
    public ModelAnimation AnimationClip { get; private set; }
    
    /// <summary>
    /// Determines if the animation should loop when it reaches the end.
    /// </summary>
    public bool IsLooping { get; private set; }
    
    /// <summary>
    /// The playback speed multiplier specific to this state. 1.0 is normal speed.
    /// </summary>
    public float Speed { get; private set; }
    
    /// <summary>
    /// A collection of transitions associated with this animation state.
    /// </summary>
    private List<AnimatorTransition> _transitions;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatorState"/> class.
    /// </summary>
    /// <param name="name">The unique name of the animation state.</param>
    /// <param name="animationClip">The animation clip associated with this state.</param>
    /// <param name="isLooping">Determines whether the animation should loop when it reaches the end.</param>
    /// <param name="speed">The playback speed multiplier applied to the animation clip.</param>
    public AnimatorState(string name, ModelAnimation animationClip, bool isLooping = false, float speed = 1.0F) {
        this.Name = name;
        this.AnimationClip = animationClip;
        this.IsLooping = isLooping;
        this.Speed = speed;
        this._transitions = new List<AnimatorTransition>();
    }
    
    /// <summary>
    /// Retrieves the collection of transitions associated with the current animation state.
    /// </summary>
    /// <return>A read-only list of <see cref="AnimatorTransition"/> objects linked to this animation state.</return>
    public IReadOnlyList<AnimatorTransition> GetTransitions() {
        return this._transitions;
    }
    
    /// <summary>
    /// Adds a transition to the current animation state.
    /// </summary>
    /// <param name="transition">The transition to be added to the animation state.</param>
    /// <return>The updated <see cref="AnimatorState"/> instance with the added transition.</return>
    public void AddTransition(AnimatorTransition transition) {
        this._transitions.Add(transition);
    }
}