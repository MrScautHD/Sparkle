namespace Sparkle.CSharp.Graphics.Animations;

public class AnimatorController {
    
    /// <summary>
    /// Stores all registered animation states by name.
    /// </summary>
    private Dictionary<string, AnimatorState> _states;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatorController"/> class.
    /// </summary>
    public AnimatorController() {
        this._states = new Dictionary<string, AnimatorState>();
    }
    
    /// <summary>
    /// Retrieves all registered animation states.
    /// </summary>
    /// <returns>A read-only dictionary containing animation states indexed by name.</returns>
    public IReadOnlyDictionary<string, AnimatorState> GetStates() {
        return this._states;
    }
    
    /// <summary>
    /// Adds or replaces an animation state in the controller.
    /// </summary>
    /// <param name="state">The animation state to add.</param>
    public void AddState(AnimatorState state) {
        this._states[state.Name] = state;
    }
}