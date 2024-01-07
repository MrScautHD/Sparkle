using Raylib_cs;
using Sparkle.csharp.graphics.helper;

namespace Sparkle.csharp.graphics.model; 

public class ModelAnimationPlayer {
    
    private readonly ModelAnimation[] _animations;
    
    private int _frameCount;
    private int _playingIndex;
    private bool _isPlaying;
    private bool _isLoop;
    private bool _isPause;

    /// <summary>
    /// Initializes a new instance of the ModelAnimationPlayer class with the given animations.
    /// </summary>
    /// <param name="animations">An array of ModelAnimation objects representing the animations to be played.</param>
    public ModelAnimationPlayer(ModelAnimation[] animations) {
        this._animations = animations;
    }

    protected internal void FixedUpdate(Model model) {
        if (!this._isPause && this._isPlaying) {
            this._frameCount++;

            if (this._frameCount <= this._animations[this._playingIndex].FrameCount) {
                ModelHelper.UpdateAnimation(model, this._animations[this._playingIndex], this._frameCount);
            }
            else {
                if (this._isLoop) {
                    this._frameCount = 0;
                }
                else {
                    this.Stop();
                }
            }
        }
    }

    /// <summary>
    /// Plays the animation at the specified index.
    /// </summary>
    /// <param name="index">The index of the animation to play.</param>
    /// <param name="loop">Specifies whether the animation should loop or not.</param>
    public void Play(int index, bool loop) {
        if (index > this._animations.Length - 1) {
            Logger.Error($"Unable to play the animation at index [{index}], the maximum number of available animations is [{this._animations.Length}].");
            return;
        }

        this._playingIndex = index;
        this._isPlaying = true;
        this._isLoop = loop;
        this._isPause = false;
    }

    /// <summary>
    /// Stops the playback of the animation.
    /// </summary>
    public void Stop() {
        this._frameCount = 0;
        this._playingIndex = 0;
        this._isPlaying = false;
        this._isLoop = false;
        this._isPause = false;
    }

    /// <summary>
    /// Set the pause flag to true, indicating a pause state.
    /// </summary>
    public void Pause() {
        this._isPause = true;
    }

    /// <summary>
    /// Unpauses the application.
    /// </summary>
    public void UnPause() {
        this._isPause = false;
    }
}