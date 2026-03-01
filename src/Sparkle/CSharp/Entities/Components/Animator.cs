using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Geometry.Animation;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Mathematics;
using Sparkle.CSharp.Graphics.Animations;

namespace Sparkle.CSharp.Entities.Components;

public class Animator : InterpolatedComponent {
    
    // TODO: IDK IF I WILL LEAVE IT THERE...
    //public AnimatorController Controller => this._layers["Base"].Controller;
    //
    //public AnimatorState? CurrentState => this._layers["Base"].CurrentState;
    //
    //public AnimatorState? PreviousState => this._layers["Base"].PreviousState;
    
    public float GlobalPlaybackSpeed;
    
    public bool Interpolation;
    
    public new Vector3 OffsetPosition => Vector3.Zero;
    
    public event Action<Dictionary<string, Matrix4x4>>? OnBoneTransformsReady;
    
    private Dictionary<string, Matrix4x4> _currentBoneMatrices;
    
    private Dictionary<string, AnimationLayer> _layers;
    
    private ModelRenderer? _modelRenderer;
    
    public Animator(AnimatorController controller, float globalPlaybackSpeed = 1.0F, bool interpolation = true) : base(Vector3.Zero) {
        this.GlobalPlaybackSpeed = globalPlaybackSpeed;
        this.Interpolation = interpolation;
        this._currentBoneMatrices = new Dictionary<string, Matrix4x4>();
        this._layers = new Dictionary<string, AnimationLayer>();
        this.AddLayer(new AnimationLayer("Base", controller));
    }
    
    protected internal override void Init() {
        base.Init();
        
        if (!this.Entity.TryGetComponent(out this._modelRenderer)) {
            Logger.Error($"The Animator component requires a ModelRenderer component to function properly. Please ensure a ModelRenderer is attached to Entity with the id: [{this.Entity.Id}]");
        }
    }
    
    protected internal override void Update(double delta) {
        base.Update(delta);
        
        // Check if the model has a skeleton.
        if (this._modelRenderer?.Model.Skeleton is not { } skeleton) {
            return;
        }
        
        Matrix4x4[]? finalLocalPoses = null;
        
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
                Matrix4x4[] currentLocalPoses = this.GetFrameLocalPoses(skeleton, layer.CurrentState, layer.CurrentTime, this.Interpolation);
                Matrix4x4[] layerLocalPoses = currentLocalPoses;
                
                // Apply blending if transitioning states.
                if (layer.BlendWeight < 1.0F) {
                    Matrix4x4[]? previousLocalPoses = null;
                    
                    if (layer.UseSnapshotBlend && layer.BlendStartSnapshot != null) {
                        previousLocalPoses = layer.BlendStartSnapshot; 
                    }
                    else if (layer.PreviousState != null) {
                        previousLocalPoses = this.GetFrameLocalPoses(skeleton, layer.PreviousState, layer.PreviousTime, this.Interpolation);
                    }
                    
                    if (previousLocalPoses != null) {
                        layerLocalPoses = new Matrix4x4[currentLocalPoses.Length];
                        
                        for (int i = 0; i < layerLocalPoses.Length; i++) {
                            Matrix4x4 previousMatrix = i < previousLocalPoses.Length ? previousLocalPoses[i] : Matrix4x4.Identity;
                            layerLocalPoses[i] = Matrix4x4.LerpSrt(previousMatrix, currentLocalPoses[i], layer.BlendWeight);
                        }
                    }
                }
                
                // Save a snapshot for future interruptions.
                if (layer.SnapshotLocalPoses == null || layer.SnapshotLocalPoses.Length != layerLocalPoses.Length) {
                    layer.SnapshotLocalPoses = new Matrix4x4[layerLocalPoses.Length];
                }
                
                Array.Copy(layerLocalPoses, layer.SnapshotLocalPoses, layerLocalPoses.Length);
                
                // Blend layer onto the final output based on BoneMask and Weight.
                if (finalLocalPoses == null) {
                    finalLocalPoses = new Matrix4x4[layerLocalPoses.Length];
                    Array.Copy(layerLocalPoses, finalLocalPoses, layerLocalPoses.Length);
                }
                else {
                    for (int i = 0; i < finalLocalPoses.Length; i++) {
                        if (layer.BoneMask.Count == 0 || layer.BoneMask.Contains(skeleton.Bones[i].Name)) {
                            finalLocalPoses[i] = Matrix4x4.LerpSrt(finalLocalPoses[i], layerLocalPoses[i], layer.Weight);
                        }
                    }
                }
            }
            
            // Apply custom procedural modifiers and reconstruct global space.
            if (finalLocalPoses != null) {
                Matrix4x4[] globalPoses = new Matrix4x4[finalLocalPoses.Length];
                Matrix4x4[] bakedMatrices = new Matrix4x4[finalLocalPoses.Length];
                
                // Populate the dictionary with the raw animation matrices.
                this._currentBoneMatrices.Clear();
                
                for (int i = 0; i < finalLocalPoses.Length; i++) {
                    string boneName = skeleton.Bones[i].Name;
                    this._currentBoneMatrices[boneName] = finalLocalPoses[i];
                }
                
                // Fire the event, Let the user override or modify the matrices directly.
                this.OnBoneTransformsReady?.Invoke(this._currentBoneMatrices);
                
                // Reconstruct and apply to the hierarchy.
                for (int i = 0; i < finalLocalPoses.Length; i++) {
                    string boneName = skeleton.Bones[i].Name;
                    int parentId = skeleton.Bones[i].ParentId;
                    
                    // Grab the potentially modified matrix from the dictionary.
                    Matrix4x4 localPose = this._currentBoneMatrices.TryGetValue(boneName, out Matrix4x4 modifiedMatrix) ? modifiedMatrix : finalLocalPoses[i];
                    
                    // Reconstruct global pose hierarchy.
                    if (parentId >= 0 && parentId < finalLocalPoses.Length) {
                        globalPoses[i] = localPose * globalPoses[parentId];
                    }
                    else {
                        globalPoses[i] = localPose;
                    }
                    
                    // Bake matrix.
                    Matrix4x4 inverseBindPose = skeleton.Bones[i].Transformation;
                    bakedMatrices[i] = inverseBindPose * globalPoses[i];
                }
                
                // Apply to the renderer meshes.
                foreach (Mesh mesh in this._modelRenderer.Model.Meshes) {
                    Matrix4x4[]? renderableMatrices = this._modelRenderer.GetRenderableBoneMatricesByMesh(mesh);
                    
                    if (renderableMatrices != null) {
                        for (int boneId = 0; boneId < bakedMatrices.Length && boneId < renderableMatrices.Length; boneId++) {
                            renderableMatrices[boneId] = bakedMatrices[boneId];
                        }
                    }
                }
            }
        }
    }
    
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
    
    public IReadOnlyCollection<AnimationLayer> GetLayers() {
        return this._layers.Values;
    }
    
    public AnimationLayer? GetLayer(string name) {
        if (!this.TryGetLayer(name, out AnimationLayer? result)) {
            return null;
        }
        
        return result;
    }
    
    public bool TryGetLayer(string name, [NotNullWhen(true)] out AnimationLayer? layer) {
        return this._layers.TryGetValue(name, out layer);
    }
    
    public void AddLayer(AnimationLayer layer) {
        if (!this.TryAddLayer(layer)) {
            throw new Exception($"The layer with the name: [{layer.Name}] is already added!");
        }
    }
    
    public bool TryAddLayer(AnimationLayer layer) {
        return this._layers.TryAdd(layer.Name, layer);
    }
    
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
    
    private Matrix4x4[] GetFrameLocalPoses(Skeleton skeleton, AnimatorState state, double time, bool enableInterpolation) {
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
        Matrix4x4[] localPoses = new Matrix4x4[currentFrameMatrices.Length];
        
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
        
        return localPoses;
    }
}