using Raylib_cs;
using Sparkle.csharp.graphics.helper;

namespace Sparkle.csharp.graphics.util; 

public class ModelAnimationPlayer {

    private readonly ModelAnimation[] _animations;
    
    private int _frameCount;
    private int _playingIndex;
    private bool _isPlaying;
    private bool _isLoop;
    private bool _isPause;

    public ModelAnimationPlayer(ModelAnimation[] animations) {
        this._animations = animations;
    }

    protected internal void Update(Model model) {
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

    public void Play(int index, bool loop) {
        /*if (index > this._animations.Length - 1) {
            Logger.Warn($"This model can accommodate a maximum of [{this._animations.Length}] animations.");
            return;
        }*/

        this._playingIndex = index;
        this._isPlaying = true;
        this._isLoop = loop;
        this._isPause = false;
    }

    public void Stop() {
        this._frameCount = 0;
        this._playingIndex = 0;
        this._isPlaying = false;
        this._isLoop = false;
        this._isPause = false;
    }

    public void Pause() {
        this._isPause = true;
    }
    
    public void UnPause() {
        this._isPause = false;
    }
}