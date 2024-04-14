using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Rendering.Helpers;

namespace Sparkle.CSharp.Rendering.Models;

public class ModelAnimationPlayer {

    private readonly Model _model;
    private readonly ModelAnimation[] _animations;
    
    private int _frameCount;
    private int _playingIndex;
    private int _oldPlayingIndex;
    private bool _isPlaying;
    private float _blendFactor;
    private bool _isLoop;
    private bool _isPause;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelAnimationPlayer"/> class.
    /// </summary>
    /// <param name="model">The 3D model associated with the animation player.</param>
    /// <param name="animations">An array of <see cref="ModelAnimation"/> objects representing different animations for the model.</param>
    public ModelAnimationPlayer(Model model, ModelAnimation[] animations) {
        this._model = model;
        this._animations = animations;
    }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="Game.AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal void FixedUpdate() {
        if (!this._isPause && this._isPlaying) {
            this._frameCount++;

            if (this._frameCount <= this._animations[this._playingIndex].FrameCount) {
                if (this._blendFactor > 0) {
                    this.BlendAnimation();
                }
                
                ModelHelper.UpdateAnimation(this._model, this._animations[this._playingIndex], this._frameCount);
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

    // TODO Check this system when Raylib-v5.1 release and the GLTF/GLB Blockbench bug is fixed.
    /// <summary>
    /// Blend two animations based on the given blend factor.
    /// </summary>
    private void BlendAnimation() {
        ModelAnimation currentAnimation = this._animations[this._playingIndex];
        ModelAnimation nextAnimation = this._animations[this._oldPlayingIndex];
        
        for (int frameIndex = 0; frameIndex < currentAnimation.FrameCount; frameIndex++) {
            FramePoses currentFramePoses = currentAnimation.FramePosesColl[frameIndex];
            FramePoses nextFramePoses = nextAnimation.FramePosesColl[frameIndex];

            for (int boneIndex = 0; boneIndex < currentAnimation.BoneCount; boneIndex++) {
                Transform interpolatedPose = this.InterpolatePoses(currentFramePoses[boneIndex], nextFramePoses[boneIndex], this._blendFactor);
                currentFramePoses[boneIndex] = interpolatedPose;
            }
        }
    }

    /// <summary>
    /// Interpolates between two poses based on a blend factor.
    /// </summary>
    /// <param name="currentPose">The current pose to interpolate from.</param>
    /// <param name="nextPose">The next pose to interpolate to.</param>
    /// <param name="blendFactor">The blend factor for interpolation. Should be a value between 0 and 1.</param>
    /// <returns>The interpolated pose.</returns>
    private Transform InterpolatePoses(Transform currentPose, Transform nextPose, float blendFactor) {
        Vector3 interpolatedTranslation = Vector3.Lerp(currentPose.Translation, nextPose.Translation, blendFactor);
        Quaternion interpolatedRotation = Quaternion.Slerp(currentPose.Rotation, nextPose.Rotation, blendFactor);
        Vector3 interpolatedScale = Vector3.Lerp(currentPose.Scale, nextPose.Scale, blendFactor);

        return new Transform() {
            Translation = interpolatedTranslation,
            Rotation = interpolatedRotation,
            Scale = interpolatedScale
        };
    }

    /// <summary>
    /// Plays the specified animation.
    /// </summary>
    /// <param name="index">The index of the animation to play.</param>
    /// <param name="loop">If set to <c>true</c>, the animation will loop; otherwise, it will play once.</param>
    /// <param name="blendFactor">The blend factor between animations.</param>
    public void Play(int index, bool loop, float blendFactor) {
        if (index > this._animations.Length - 1) {
            Logger.Error($"Unable to play the animation at index [{index}], the maximum number of available animations is [{this._animations.Length}].");
            return;
        }

        this._oldPlayingIndex = 0;
        this._playingIndex = index;
        this._isPlaying = true;
        this._blendFactor = blendFactor;
        this._isLoop = loop;
        this._isPause = false;
    }

    /// <summary>
    /// Stops the playback of the animation.
    /// </summary>
    public void Stop() {
        ModelHelper.UpdateAnimation(this._model, this._animations[this._playingIndex], 0);
        this._frameCount = 0;
        this._playingIndex = 0;
        this._oldPlayingIndex = 0;
        this._isPlaying = false;
        this._blendFactor = 0;
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