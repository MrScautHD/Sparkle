using Raylib_CSharp;
using Raylib_CSharp.Geometry;
using Raylib_CSharp.Unsafe.Spans.Data;
using Sparkle.CSharp.Logging;

namespace Sparkle.CSharp.Models;

// TODO: ADD GPU ANIMATIONS
public class ModelAnimationPlayer {

    private Model _model;
    private readonly ReadOnlySpanData<ModelAnimation> _animations;
    
    private int _frameCount;
    private int _playingIndex;
    
    public bool IsPlaying { get; private set; }
    public bool IsLooped { get; private set; }
    public bool IsPaused { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelAnimationPlayer"/> class.
    /// </summary>
    /// <param name="model">The 3D model associated with the animation player.</param>
    /// <param name="animations">An array of <see cref="ModelAnimation"/> objects representing different animations for the model.</param>
    public ModelAnimationPlayer(Model model, ReadOnlySpanData<ModelAnimation> animations) {
        this._model = model;
        this._animations = animations;
    }
    
    /// <summary>
    /// Is invoked at a fixed rate of every <see cref="GameSettings.FixedTimeStep"/> frames following the <see cref="Game.AfterUpdate"/> method.
    /// It is used for handling physics and other fixed-time operations.
    /// </summary>
    protected internal void FixedUpdate() {
        if (!this.IsPaused && this.IsPlaying) {
            this._frameCount++;

            if (this._frameCount <= this._animations.GetSpan()[this._playingIndex].FrameCount) {
                this._animations.GetSpan()[this._playingIndex].Update(this._model, this._frameCount);
            }
            else {
                if (this.IsLooped) {
                    this._frameCount = 0;
                }
                else {
                    this.Stop();
                }
            }
        }
    }

    /// <summary>
    /// Plays the specified animation.
    /// </summary>
    /// <param name="index">The index of the animation to play.</param>
    /// <param name="loop">If set to <c>true</c>, the animation will loop; otherwise, it will play once.</param>
    public void Play(int index, bool loop) {
        if (index > this._animations.GetSpan().Length - 1) {
            Logger.Error($"Unable to play the animation at index [{index}], the maximum number of available animations is [{this._animations.GetSpan().Length}].");
            return;
        }

        this._frameCount = 0;
        this._playingIndex = index;
        this.IsPlaying = true;
        this.IsLooped = loop;
        this.IsPaused = false;
    }

    /// <summary>
    /// Stops the playback of the animation.
    /// </summary>
    public void Stop() {
        this._frameCount = 0;
        this._playingIndex = 0;
        this.IsPlaying = false;
        this.IsLooped = false;
        this.IsPaused = false;
        this._animations.GetSpan()[this._playingIndex].Update(this._model, 0);
        this.ResetPose();
    }
    
    /// <summary>
    /// Set the pause flag to true, indicating a pause state.
    /// </summary>
    public void Pause() {
        this.IsPaused = true;
    }

    /// <summary>
    /// Unpauses the application.
    /// </summary>
    public void UnPause() {
        this.IsPaused = false;
    }

    /// <summary>
    /// Resets the pose of the 3D model by updating the vertex and normal buffers of each mesh in the model.
    /// </summary>
    private unsafe void ResetPose() {
        foreach (Mesh mesh in this._model.Meshes) {
            RlGl.UpdateVertexBuffer(mesh.VboId[0], (nint) mesh.VerticesPtr, mesh.VertexCount * 3 * sizeof(float), 0);
            RlGl.UpdateVertexBuffer(mesh.VboId[2], (nint) mesh.NormalsPtr, mesh.VertexCount * 3 * sizeof(float), 0);
        }
    }
}