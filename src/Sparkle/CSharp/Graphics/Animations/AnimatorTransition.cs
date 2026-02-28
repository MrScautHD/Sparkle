using Sparkle.CSharp.Entities.Components;

namespace Sparkle.CSharp.Graphics.Animations;

public class AnimatorTransition {
    
    /// <summary>
    /// The name of the animation state to transition to.
    /// </summary>
    public string TargetState { get; private set; }
    
    /// <summary>
    /// How long the blend should take in seconds. 0 means instant snapping.
    /// </summary>
    public float BlendDuration { get; private set; }
    
    /// <summary>
    /// A list of conditions that must all evaluate to true for this transition to occur.
    /// </summary>
    private List<Func<Animator, bool>> _conditions;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatorTransition"/> class.
    /// </summary>
    /// <param name="targetState">The name of the animation state to transition to.</param>
    /// <param name="blendDuration">The duration, in seconds, of the blend between states.</param>
    public AnimatorTransition(string targetState, float blendDuration = 0.2F) {
        this.TargetState = targetState;
        this.BlendDuration = blendDuration;
        this._conditions = new List<Func<Animator, bool>>();
    }
    
    /// <summary>
    /// Retrieves the list of conditions that must evaluate to true for the transition to occur.
    /// </summary>
    /// <returns>A read-only list of functions representing the conditions associated with the transition.</returns>
    public IReadOnlyList<Func<Animator, bool>> GetConditions() {
        return this._conditions;
    }
    
    /// <summary>
    /// Adds a condition that must evaluate to true for this transition to occur.
    /// </summary>
    public AnimatorTransition AddCondition(Func<Animator, bool> condition) {
        this._conditions.Add(condition);
        return this;
    }
}