using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Geometry.Animation;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Mathematics;
using Sparkle.CSharp.Graphics.Animations;

namespace Sparkle.CSharp.Entities.Components;

public class Animator : InterpolatedComponent {
    
    /// <summary>
    /// Controls the global playback speed multiplier applied to all animation layers.
    /// </summary>
    public float GlobalPlaybackSpeed;
    
    /// <summary>
    /// Gets or sets a value indicating whether frame interpolation between keyframes is enabled.
    /// </summary>
    public bool Interpolation;
    
    /// <summary>
    /// Gets the positional offset applied by this component. Always returns <see cref="Vector3.Zero"/>.
    /// </summary>
    public new Vector3 OffsetPosition => Vector3.Zero;
    
    /// <summary>
    /// Occurs after final local bone transforms are evaluated but before hierarchy reconstruction, allowing external modification of bone matrices.
    /// </summary>
    public event Action<Dictionary<string, Matrix4x4>>? OnBoneTransformsReady;
    
    /// <summary>
    /// Stores the current local bone matrices indexed by bone name.
    /// </summary>
    private Dictionary<string, Matrix4x4> _currentBoneMatrices;
    
    /// <summary>
    /// Contains all animation layers managed by this animator, indexed by layer name.
    /// </summary>
    private Dictionary<string, AnimationLayer> _layers;
    
    /// <summary>
    /// Cached reference to the associated <see cref="ModelRenderer"/> component.
    /// </summary>
    private ModelRenderer? _modelRenderer;
    
    /// <summary>
    /// Cache storing the currently evaluated local poses for a layer.
    /// </summary>
    private Matrix4x4[]? _currentLocalPosesCache;
    
    /// <summary>
    /// Cache storing the previously evaluated local poses for blending.
    /// </summary>
    private Matrix4x4[]? _previousLocalPosesCache;
    
    /// <summary>
    /// Cache storing the blended local poses for a specific layer.
    /// </summary>
    private Matrix4x4[]? _layerLocalPosesCache;
    
    /// <summary>
    /// Cache storing the final blended local poses across all layers.
    /// </summary>
    private Matrix4x4[]? _finalLocalPosesCache;
    
    /// <summary>
    /// Cache storing reconstructed global bone transforms.
    /// </summary>
    private Matrix4x4[]? _globalPosesCache;
    
    /// <summary>
    /// Cache storing the final baked bone matrices ready for rendering.
    /// </summary>
    private Matrix4x4[]? _bakedMatricesCache;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Animator"/> class.
    /// </summary>
    /// <param name="controller">The base animator controller assigned to the default layer.</param>
    /// <param name="globalPlaybackSpeed">The global playback speed multiplier.</param>
    /// <param name="interpolation">If true, enables interpolation between animation frames.</param>
    public Animator(AnimatorController controller, float globalPlaybackSpeed = 1.0F, bool interpolation = true) : base(Vector3.Zero) {
        this.GlobalPlaybackSpeed = globalPlaybackSpeed;
        this.Interpolation = interpolation;
        this._currentBoneMatrices = new Dictionary<string, Matrix4x4>();
        this._layers = new Dictionary<string, AnimationLayer>();
        this.AddLayer(new AnimationLayer("Base", controller));
    }
    
    /// <summary>
    /// Initializes the animator and validates required components.
    /// </summary>
    protected internal override void Init() {
        base.Init();
        
        if (!this.Entity.TryGetComponent(out this._modelRenderer)) {
            Logger.Error($"The Animator component requires a ModelRenderer component to function properly. Please ensure a ModelRenderer is attached to Entity with the id: [{this.Entity.Id}]");
        }
    }
    
    /// <summary>
    /// Updates the animator.
    /// </summary>
    /// <param name="delta">The time in seconds since the last update, used for advancing animation playback.</param>
    protected internal override void Update(double delta) {
        base.Update(delta);
        
        // Check if the model has a skeleton.
        if (this._modelRenderer?.Model.Skeleton is not { } skeleton) {
            return;
        }
        
        // Create cache arrays.
        int boneCount = skeleton.Bones.Count;
        this._finalLocalPosesCache = this.EnsureArraySize(this._finalLocalPosesCache, boneCount);
        this._globalPosesCache = this.EnsureArraySize(this._globalPosesCache, boneCount);
        this._bakedMatricesCache = this.EnsureArraySize(this._bakedMatricesCache, boneCount);
        this._layerLocalPosesCache = this.EnsureArraySize(this._layerLocalPosesCache, boneCount);
        this._currentLocalPosesCache = this.EnsureArraySize(this._currentLocalPosesCache, boneCount);
        this._previousLocalPosesCache = this.EnsureArraySize(this._previousLocalPosesCache, boneCount);
        
        bool hasFinalPoses = false;
        
        // Evaluate all layers and blend them together.
        foreach (AnimationLayer layer in this._layers.Values) {
            this.EvaluateTransitions(layer);
            this.UpdateBlending(layer, delta);
            
            if (layer.CurrentState != null) {
                layer.CurrentTime = this.AdvanceTime(layer.CurrentState, layer.CurrentTime, delta);
            }
            
            if (layer.PreviousState != null && layer.BlendWeight < 1.0F) {
                layer.PreviousTime = this.AdvanceTime(layer.PreviousState, layer.PreviousTime, delta);
            }
            
            if (layer.CurrentState != null) {
                this.GetFrameLocalPoses(skeleton, layer.CurrentState, layer.CurrentTime, this.Interpolation, this._currentLocalPosesCache);
                Array.Copy(this._currentLocalPosesCache, this._layerLocalPosesCache, boneCount);
                
                // Apply blending if transitioning states.
                if (layer.BlendWeight < 1.0F) {
                    Matrix4x4[]? previousLocalPoses = null;
                    
                    if (layer.UseSnapshotBlend && layer.BlendStartSnapshot != null) {
                        previousLocalPoses = layer.BlendStartSnapshot; 
                    }
                    else if (layer.PreviousState != null) {
                        this.GetFrameLocalPoses(skeleton, layer.PreviousState, layer.PreviousTime, this.Interpolation, this._previousLocalPosesCache);
                        previousLocalPoses = this._previousLocalPosesCache;
                    }
                    
                    if (previousLocalPoses != null) {
                        for (int i = 0; i < boneCount; i++) {
                            Matrix4x4 previousMatrix = i < previousLocalPoses.Length ? previousLocalPoses[i] : Matrix4x4.Identity;
                            this._layerLocalPosesCache[i] = Matrix4x4.LerpSrt(previousMatrix, this._currentLocalPosesCache[i], layer.BlendWeight);
                        }
                    }
                }
                
                // Save a snapshot for future interruptions.
                if (layer.SnapshotLocalPoses == null || layer.SnapshotLocalPoses.Length != boneCount) {
                    layer.SnapshotLocalPoses = new Matrix4x4[boneCount];
                }
                
                Array.Copy(this._layerLocalPosesCache, layer.SnapshotLocalPoses, boneCount);
                
                // Blend layer onto the final output based on BoneMask and Weight.
                if (!hasFinalPoses) {
                    Array.Copy(this._layerLocalPosesCache, this._finalLocalPosesCache, boneCount);
                    hasFinalPoses = true;
                }
                else {
                    for (int i = 0; i < boneCount; i++) {
                        if (layer.BoneMask.Count == 0 || layer.BoneMask.Contains(skeleton.Bones[i].Name)) {
                            this._finalLocalPosesCache[i] = Matrix4x4.LerpSrt(this._finalLocalPosesCache[i], this._layerLocalPosesCache[i], layer.Weight);
                        }
                    }
                }
            }
            
            // Apply custom procedural modifiers and reconstruct global space.
            if (hasFinalPoses) {
                
                // Populate the dictionary with the raw animation matrices.
                this._currentBoneMatrices.Clear();
                
                for (int i = 0; i < boneCount; i++) {
                    string boneName = skeleton.Bones[i].Name;
                    this._currentBoneMatrices[boneName] = this._finalLocalPosesCache[i];
                }
                
                // Fire the event, Let the user override or modify the matrices directly.
                this.OnBoneTransformsReady?.Invoke(this._currentBoneMatrices);
                
                // Reconstruct and apply to the hierarchy.
                for (int i = 0; i < boneCount; i++) {
                    string boneName = skeleton.Bones[i].Name;
                    int parentId = skeleton.Bones[i].ParentId;
                    
                    // Grab the potentially modified matrix from the dictionary.
                    Matrix4x4 localPose = this._currentBoneMatrices.TryGetValue(boneName, out Matrix4x4 modifiedMatrix) ? modifiedMatrix : this._finalLocalPosesCache[i];
                    
                    // Reconstruct global pose hierarchy.
                    if (parentId >= 0 && parentId < boneCount) {
                        this._globalPosesCache[i] = localPose * this._globalPosesCache[parentId];
                    }
                    else {
                        this._globalPosesCache[i] = localPose;
                    }
                    
                    // Bake matrix.
                    Matrix4x4 inverseBindPose = skeleton.Bones[i].Transformation;
                    this._bakedMatricesCache[i] = inverseBindPose * this._globalPosesCache[i];
                }
                
                // Apply to the renderer meshes.
                foreach (Mesh mesh in this._modelRenderer.Model.Meshes) {
                    Matrix4x4[]? renderableMatrices = this._modelRenderer.GetRenderableBoneMatricesByMesh(mesh);
                    
                    if (renderableMatrices != null) {
                        for (int boneId = 0; boneId < boneCount && boneId < renderableMatrices.Length; boneId++) {
                            renderableMatrices[boneId] = this._bakedMatricesCache[boneId];
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Plays an animation state on the specified layer with optional blending.
    /// </summary>
    /// <param name="stateName">The name of the animation state to play.</param>
    /// <param name="blendDuration">The duration in seconds for cross-fading between states.</param>
    /// <param name="normalizedTime">The normalized start time (0.0–1.0) within the animation.</param>
    /// <param name="layerName">The name of the animation layer.</param>
    public void Play(string stateName, float blendDuration = 0.0F, float normalizedTime = 0.0F, string layerName = "Base") {
        AnimationLayer? layer = this.GetLayer(layerName);
        
        if (layer == null) {
            Logger.Warn($"Failed to play state with the name: [{stateName}]. Animation layer with the name: [{layerName}] was not found.");
            return;
        }
        
        if (layer.Controller.GetStates().TryGetValue(stateName, out AnimatorState? newState)) {
            if (layer.CurrentState != newState) {
                if (blendDuration > 0.0F && layer.CurrentState != null) {
                    if (layer.BlendWeight < 1.0F) {
                        layer.UseSnapshotBlend = true;
                        
                        if (layer.SnapshotLocalPoses != null) {
                            if (layer.BlendStartSnapshot == null || layer.BlendStartSnapshot.Length != layer.SnapshotLocalPoses.Length) {
                                layer.BlendStartSnapshot = new Matrix4x4[layer.SnapshotLocalPoses.Length];
                            }
                            
                            Array.Copy(layer.SnapshotLocalPoses, layer.BlendStartSnapshot, layer.SnapshotLocalPoses.Length);
                        }
                    }
                    else {
                        layer.UseSnapshotBlend = false;
                        layer.PreviousState = layer.CurrentState;
                        layer.PreviousTime = layer.CurrentTime;
                    }
                    
                    layer.CurrentBlendDuration = blendDuration;
                    layer.BlendTimer = 0.0F;
                    layer.BlendWeight = 0.0F;
                }
                else {
                    layer.UseSnapshotBlend = false;
                    layer.PreviousState = null;
                    layer.BlendWeight = 1.0F;
                }
                
                if (newState.AnimationClip.BoneFrameTransformations.Count > 0) {
                    if (layer.PreviousState == newState && newState.IsLooping) {
                        layer.CurrentTime = layer.PreviousTime;
                    }
                    else {
                        layer.CurrentTime = Math.Clamp(normalizedTime, 0.0F, 1.0F) * (newState.AnimationClip.BoneFrameTransformations.Count - 1);
                    }
                }
                else {
                    layer.CurrentTime = 0.0;
                }
                
                layer.CurrentState = newState;
            }
        }
    }
    
    /// <summary>
    /// Stops animation playback on the specified layer.
    /// </summary>
    /// <param name="layerName">The name of the animation layer.</param>
    public void Stop(string layerName = "Base") {
        AnimationLayer? layer = this.GetLayer(layerName);
        
        if (layer == null) {
            Logger.Warn($"Failed to stop animation. Animation layer with the name: [{layerName}] was not found.");
            return;
        }
        
        layer.CurrentState = null;
        layer.PreviousState = null;
        layer.BlendWeight = 1.0F;
    }
    
    /// <summary>
    /// Gets all animation layers managed by this animator.
    /// </summary>
    /// <returns>A read-only collection of animation layers.</returns>
    public IReadOnlyCollection<AnimationLayer> GetLayers() {
        return this._layers.Values;
    }
    
    /// <summary>
    /// Gets the animation layer with the specified name.
    /// </summary>
    /// <param name="name">The name of the layer.</param>
    /// <returns>The layer if found; otherwise, <c>null</c>.</returns>
    public AnimationLayer? GetLayer(string name) {
        if (!this.TryGetLayer(name, out AnimationLayer? result)) {
            return null;
        }
        
        return result;
    }
    
    /// <summary>
    /// Attempts to retrieve the animation layer with the specified name.
    /// </summary>
    /// <param name="name">The name of the layer.</param>
    /// <param name="layer">When this method returns, contains the layer if found.</param>
    /// <returns><c>true</c> if the layer exists; otherwise, <c>false</c>.</returns>
    public bool TryGetLayer(string name, [NotNullWhen(true)] out AnimationLayer? layer) {
        return this._layers.TryGetValue(name, out layer);
    }
    
    /// <summary>
    /// Adds a new animation layer to the animator.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    public void AddLayer(AnimationLayer layer) {
        if (!this.TryAddLayer(layer)) {
            throw new Exception($"The layer with the name: [{layer.Name}] is already added!");
        }
    }
    
    /// <summary>
    /// Attempts to add a new animation layer.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    /// <returns><c>true</c> if the layer was added successfully; otherwise, <c>false</c>.</returns>
    public bool TryAddLayer(AnimationLayer layer) {
        return this._layers.TryAdd(layer.Name, layer);
    }
    
    /// <summary>
    /// Advances animation time based on delta time, playback speed, and looping settings.
    /// </summary>
    /// <param name="state">The animation state being advanced.</param>
    /// <param name="time">The current animation time.</param>
    /// <param name="delta">The frame delta time.</param>
    /// <returns>The updated animation time.</returns>
    private double AdvanceTime(AnimatorState state, double time, double delta) {
        ModelAnimation clip = state.AnimationClip;
        
        double ticksPerSecond = clip.TicksPerSecond > 0.0 ? clip.TicksPerSecond : 24.0;
        double durationInTicks = clip.DurationInTicks > 0.0 ? clip.DurationInTicks : 1.0;
        int frameCount = clip.FrameCount > 0 ? clip.FrameCount : clip.BoneFrameTransformations.Count;
        
        double durationInSeconds = durationInTicks / ticksPerSecond;
        double framesPerSecond = frameCount / durationInSeconds;
        
        time += delta * framesPerSecond * state.Speed * this.GlobalPlaybackSpeed;
        double maxTime = clip.BoneFrameTransformations.Count - 1;
        
        if (maxTime <= 0.0) {
            return 0.0;
        }
        
        if (time > maxTime) {
            time = state.IsLooping ? time % maxTime : maxTime;
        }
        
        return time;
    }
    
    /// <summary>
    /// Updates blending weight for the specified animation layer.
    /// </summary>
    /// <param name="layer">The layer to update.</param>
    /// <param name="delta">The frame delta time.</param>
    private void UpdateBlending(AnimationLayer layer, double delta) {
        if (layer.PreviousState == null || layer.BlendWeight >= 1.0F) {
            layer.BlendWeight = 1.0F;
            return;
        }
        
        layer.BlendTimer += (float) delta;
        layer.BlendWeight = Math.Clamp(layer.BlendTimer / layer.CurrentBlendDuration, 0.0F, 1.0F);
        
        if (layer.BlendWeight >= 1.0F) {
            layer.PreviousState = null;
        }
    }
    
    /// <summary>
    /// Evaluates and triggers animation state transitions for the specified layer.
    /// </summary>
    /// <param name="layer">The animation layer to evaluate.</param>
    private void EvaluateTransitions(AnimationLayer layer) {
        if (layer.CurrentState == null) {
            return;
        }
        
        foreach (AnimatorTransition transition in layer.CurrentState.GetTransitions()) {
            bool allConditionsMet = true;
            
            foreach (Func<Animator, bool> condition in transition.GetConditions()) {
                if (!condition.Invoke(this)) {
                    allConditionsMet = false;
                    break;
                }
            }
            
            if (allConditionsMet) {
                this.Play(transition.TargetState, transition.BlendDuration, 0.0F, layer.Name);
                break;
            }
        }
    }
    
    /// <summary>
    /// Evaluates the local bone poses for a specific animation state at a given time, optionally interpolating between frames.
    /// </summary>
    /// <param name="skeleton">The skeleton being evaluated.</param>
    /// <param name="state">The animation state.</param>
    /// <param name="time">The current animation time.</param>
    /// <param name="enableInterpolation">If true, interpolates between frames.</param>
    /// <param name="localPoses">The output array storing computed local poses.</param>
    private void GetFrameLocalPoses(Skeleton skeleton, AnimatorState state, double time, bool enableInterpolation, Matrix4x4[] localPoses) {
        ModelAnimation animation = state.AnimationClip;
        int maxFrames = animation.BoneFrameTransformations.Count - 1;
        int currentFrame = Math.Clamp((int) Math.Floor(time), 0, maxFrames);
        int nextFrame = Math.Clamp(currentFrame + 1, 0, maxFrames);
        
        // Wrap around to the start if the animation is looping.
        if (nextFrame == maxFrames && state.IsLooping) {
            nextFrame = 0;
        }
        
        // Calculate how far along we are between the current frame and the next frame (0.0 to 1.0).
        float fraction = enableInterpolation ? (float) (time - Math.Floor(time)) : 0.0F;
        
        if (!enableInterpolation) {
            nextFrame = currentFrame;
        }
        
        Matrix4x4[] currentFrameMatrices = animation.BoneFrameTransformations[currentFrame];
        Matrix4x4[] nextFrameMatrices = animation.BoneFrameTransformations[nextFrame];
        
        for (int i = 0; i < currentFrameMatrices.Length; i++) {
            
            // Un-bake into local spaces for easier layered blending
            Matrix4x4 inverseBindPose = skeleton.Bones[i].Transformation;
            Matrix4x4.Invert(inverseBindPose, out Matrix4x4 bindPose);
            
            // Reconstruct global pose.
            Matrix4x4 currentGlobalPose = bindPose * currentFrameMatrices[i];
            Matrix4x4 nextGlobalPose = bindPose * nextFrameMatrices[i];
            
            int parentId = skeleton.Bones[i].ParentId;
            
            Matrix4x4 currentLocalPose = currentGlobalPose;
            Matrix4x4 nextLocalPose = nextGlobalPose;
            
            // Extract local pose relative to parent.
            if (parentId >= 0 && parentId < currentFrameMatrices.Length) {
                Matrix4x4 parentInverseBind = skeleton.Bones[parentId].Transformation;
                Matrix4x4.Invert(parentInverseBind, out Matrix4x4 parentBind);
                
                Matrix4x4 parentGlobalA = parentBind * currentFrameMatrices[parentId];
                Matrix4x4 parentGlobalB = parentBind * nextFrameMatrices[parentId];
                
                Matrix4x4.Invert(parentGlobalA, out Matrix4x4 invParentA);
                Matrix4x4.Invert(parentGlobalB, out Matrix4x4 invParentB);
                
                currentLocalPose = currentGlobalPose * invParentA;
                nextLocalPose = nextGlobalPose * invParentB;
            }
            
            // Interpolate between frames.
            localPoses[i] = enableInterpolation ? Matrix4x4.LerpSrt(currentLocalPose, nextLocalPose, fraction) : currentLocalPose;
        }
    }
    
    /// <summary>
    /// Ensures the specified matrix array matches the required size,
    /// allocating a new array if necessary.
    /// </summary>
    /// <param name="array">The array to validate.</param>
    /// <param name="size">The required size.</param>
    /// <returns>A matrix array of the specified size.</returns>
    private Matrix4x4[] EnsureArraySize(Matrix4x4[]? array, int size) {
        if (array == null || array.Length != size) {
            return new Matrix4x4[size];
        }
        
        return array;
    }
}