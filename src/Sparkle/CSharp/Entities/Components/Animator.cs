using System.Numerics;
using Bliss.CSharp.Geometry;
using Bliss.CSharp.Geometry.Animation;
using Bliss.CSharp.Logging;
using Bliss.CSharp.Mathematics;
using Sparkle.CSharp.Graphics;
using Sparkle.CSharp.Graphics.Animations;

namespace Sparkle.CSharp.Entities.Components;

public class Animator : InterpolatedComponent {
    
    public AnimatorController Controller { get; private set; }
    
    public CullingMode CullingMode { get; private set; }
    
    public float GlobalPlaybackSpeed;
    
    public bool Interpolation;
    
    public AnimatorState? CurrentState { get; private set; }
    
    public AnimatorState? PreviousState { get; private set; }
    
    public new Vector3 OffsetPosition => Vector3.Zero;
    
    private ModelRenderer? _modelRenderer;
    
    private bool _useSnapshotBlend;
    
    private float _currentBlendDuration;
    
    private float _blendTimer;
    
    private float _blendWeight;
    
    private double _currentTime;
    
    private double _previousTime;
    
    private Matrix4x4[]? _snapshotMatrices;
    
    public Animator(AnimatorController controller, CullingMode cullingMode = CullingMode.Always, float globalPlaybackSpeed = 1.0F, bool interpolation = true) : base(Vector3.Zero) {
        this.Controller = controller;
        this.CullingMode = cullingMode;
        this.GlobalPlaybackSpeed = globalPlaybackSpeed;
        this.Interpolation = interpolation;
    }
    
    protected internal override void Init() {
        base.Init();
        
        if (!this.Entity.TryGetComponent(out this._modelRenderer)) {
            Logger.Error($"The Animator component requires a ModelRenderer component to function properly. Please ensure a ModelRenderer is attached to Entity with the id: [{this.Entity.Id}]");
        }
    }

    protected internal override void Update(double delta) {
        base.Update(delta);
        
        if (this._modelRenderer?.Model.Skeleton is not { } skeleton) {
            return;
        }
        
        this.EvaluateTransitions();
        this.UpdateBlending(delta);
        
        if (this.CurrentState != null) {
            this._currentTime = this.AdvanceTime(this.CurrentState, this._currentTime, delta);
        }
        
        if (this.PreviousState != null && this._blendWeight < 1.0F) {
            this._previousTime = this.AdvanceTime(this.PreviousState, this._previousTime, delta);
        }
        
        if (this.CurrentState != null) {
            Matrix4x4[] currentMatrices = this.GetEvaluatedMatrices(skeleton, this.CurrentState, this._currentTime, this.Interpolation);
            Matrix4x4[] finalMatrices = currentMatrices;
            
            // Apply blending if we are transitioning between states.
            if (this._blendWeight < 1.0F) {
                Matrix4x4[]? previousMatrices = null;
                
                if (this._useSnapshotBlend && this._snapshotMatrices != null) {
                    previousMatrices = this._snapshotMatrices;
                }
                else if (this.PreviousState != null) {
                    previousMatrices = this.GetEvaluatedMatrices(skeleton, this.PreviousState, this._previousTime, this.Interpolation);
                }
                
                // Perform the blend if we have valid matrices to blend from.
                if (previousMatrices != null) {
                    Matrix4x4[] blendedGlobalPoses = new Matrix4x4[currentMatrices.Length];
                    
                    finalMatrices = new Matrix4x4[currentMatrices.Length];
                    
                    for (int i = 0; i < finalMatrices.Length; i++) {
                        Matrix4x4 previousMatrix = i < previousMatrices.Length ? previousMatrices[i] : Matrix4x4.Identity;
                        Matrix4x4 currentMatrix = currentMatrices[i];
                        
                        // Un-bake the matrices to get absolute global poses.
                        Matrix4x4 inverseBindPose = skeleton.Bones[i].Transformation;
                        Matrix4x4.Invert(inverseBindPose, out Matrix4x4 bindPose);
                        
                        Matrix4x4 previousGlobalPose = bindPose * previousMatrix;
                        Matrix4x4 currentGlobalPose = bindPose * currentMatrix;
                        
                        int parentId = skeleton.Bones[i].ParentId;
                        
                        Matrix4x4 previousLocalPose = previousGlobalPose;
                        Matrix4x4 currentLocalPose = currentGlobalPose;
                        
                        if (parentId >= 0 && parentId < finalMatrices.Length) {
                            
                            // Get the parent's global poses.
                            Matrix4x4 parentInverseBind = skeleton.Bones[parentId].Transformation;
                            Matrix4x4.Invert(parentInverseBind, out Matrix4x4 parentBind);
                            
                            Matrix4x4 previousParentGlobalPose = parentBind * (parentId < previousMatrices.Length ? previousMatrices[parentId] : Matrix4x4.Identity);
                            Matrix4x4 currentParentGlobalPose = parentBind * currentMatrices[parentId];
                            
                            // Invert parent global and multiply to get local space.
                            Matrix4x4.Invert(previousParentGlobalPose, out Matrix4x4 invParentA);
                            Matrix4x4.Invert(currentParentGlobalPose, out Matrix4x4 invParentB);
                            
                            previousLocalPose = previousGlobalPose * invParentA;
                            currentLocalPose = currentGlobalPose * invParentB;
                        }
                        
                        Matrix4x4 blendedLocal = Matrix4x4.LerpSrt(previousLocalPose, currentLocalPose, this._blendWeight);
                        
                        // Reconstructing the blended global hierarchy.
                        if (parentId >= 0 && parentId < finalMatrices.Length) {
                            
                            // Multiply by the parent's already-blended global pose.
                            blendedGlobalPoses[i] = blendedLocal * blendedGlobalPoses[parentId];
                        }
                        else {
                            
                            // Root bone.
                            blendedGlobalPoses[i] = blendedLocal;
                        }
                        
                        // Re-bake the matrices for the GPU.
			            finalMatrices[i] = inverseBindPose * blendedGlobalPoses[i];
                    }
                }
            }
            
            // Save the snapshot for the next frame in case we get interrupted.
            if (this._snapshotMatrices == null || this._snapshotMatrices.Length != finalMatrices.Length) {
                this._snapshotMatrices = new Matrix4x4[finalMatrices.Length];
            }
            
            Array.Copy(finalMatrices, this._snapshotMatrices, finalMatrices.Length);
            
            // Apply the evaluated bone matrices to the renderables inside the model renderer.
            foreach (Mesh mesh in this._modelRenderer.Model.Meshes) {
                Matrix4x4[]? renderableMatrices = this._modelRenderer.GetRenderableBoneMatricesByMesh(mesh);
                
                if (renderableMatrices != null) {
                    for (int boneId = 0; boneId < finalMatrices.Length && boneId < renderableMatrices.Length; boneId++) {
                        renderableMatrices[boneId] = finalMatrices[boneId];
                    }
                }
            }
        }
    }
    
    public void Play(string stateName, float blendDuration = 0.0F, float normalizedTime = 0.0F) {
        if (this.Controller.GetStates().TryGetValue(stateName, out AnimatorState? newState)) {
            if (this.CurrentState != newState) {
                if (blendDuration > 0.0F && this.CurrentState != null) {
                    if (this._blendWeight < 1.0F) {
                        this._useSnapshotBlend = true;
                    }
                    else {
                        this._useSnapshotBlend = false;
                        this.PreviousState = this.CurrentState;
                        this._previousTime = this._currentTime;
                    }
                    
                    this._currentBlendDuration = blendDuration;
                    this._blendTimer = 0.0F;
                    this._blendWeight = 0.0F;
                }
                else {
                    this._useSnapshotBlend = false;
                    this.PreviousState = null;
                    this._blendWeight = 1.0F;
                }
                
                if (newState.AnimationClip.BoneFrameTransformations.Count > 0) {
                    if (this.PreviousState == newState) {
                        this._currentTime = this._previousTime;
                    }
                    else {
                        this._currentTime = Math.Clamp(normalizedTime, 0.0F, 1.0F) * (newState.AnimationClip.BoneFrameTransformations.Count - 1);
                    }
                }
                else {
                    this._currentTime = 0.0;
                }
                
                this.CurrentState = newState;
            }
        }
    }
    
    private double AdvanceTime(AnimatorState state, double time, double delta) {
        ModelAnimation clip = state.AnimationClip;
        
        double ticksPerSecond = clip.TicksPerSecond > 0.0 ? clip.TicksPerSecond : 24.0;
        double durationInTicks = clip.DurationInTicks > 0.0 ? clip.DurationInTicks : 1.0;
        int frameCount = clip.FrameCount > 0 ? clip.FrameCount : clip.BoneFrameTransformations.Count;
        
        double durationInSeconds = durationInTicks / ticksPerSecond;
        double framesPerSecond = frameCount / durationInSeconds;
        
        // Advance the time (which is the current frame index).
        time += delta * framesPerSecond * state.Speed * this.GlobalPlaybackSpeed;
        
        double maxTime = clip.BoneFrameTransformations.Count - 1;
        
        if (maxTime <= 0.0) {
            return 0.0;
        }
        
        // Loop or clamp the animation frame.
        if (time > maxTime) {
            time = state.IsLooping ? time % maxTime : maxTime;
        }
        
        return time;
    }
    
    private void UpdateBlending(double delta) {
        if (this.PreviousState == null || this._blendWeight >= 1.0F) {
            this._blendWeight = 1.0F;
            return;
        }
        
        this._blendTimer += (float) delta;
        this._blendWeight = Math.Clamp(this._blendTimer / this._currentBlendDuration, 0.0F, 1.0F);
        
        // Complete blending.
        if (this._blendWeight >= 1.0F) {
            this.PreviousState = null;
        }
    }
    
    private void EvaluateTransitions() {
        if (this.CurrentState == null) {
            return;
        }
        
        foreach (AnimatorTransition transition in this.CurrentState.GetTransitions()) {
            bool allConditionsMet = true;
            
            // Execute the conditions directly.
            foreach (Func<Animator, bool> condition in transition.GetConditions()) {
                if (!condition.Invoke(this)) {
                    allConditionsMet = false;
                    break;
                }
            }
            
            if (allConditionsMet) {
                this.Play(transition.TargetState, transition.BlendDuration);
                break;
            }
        }
    }
    
    private Matrix4x4[] GetEvaluatedMatrices(Skeleton skeleton, AnimatorState state, double time, bool enableInterpolation) {
        ModelAnimation animation = state.AnimationClip;
        int maxFrames = animation.BoneFrameTransformations.Count - 1;
        
        if (!enableInterpolation) {
            int frame = Math.Clamp((int) Math.Floor(time), 0, maxFrames);
            return animation.BoneFrameTransformations[frame];
        }
        
        // Frame interpolation logic.
        int currentFrame = Math.Clamp((int) Math.Floor(time), 0, maxFrames);
        int nextFrame = Math.Clamp(currentFrame + 1, 0, maxFrames);
        
        // Loop nextFrame back to 0 if the animation loops, and we hit the end.
        if (nextFrame == maxFrames && state.IsLooping) {
            nextFrame = 0;
        }
        
        float fraction = (float) (time - Math.Floor(time));
        
        Matrix4x4[] currentFrameMatrices = animation.BoneFrameTransformations[currentFrame];
        Matrix4x4[] nextFrameMatrices = animation.BoneFrameTransformations[nextFrame];
        Matrix4x4[] interpolated = new Matrix4x4[currentFrameMatrices.Length];
        Matrix4x4[] blendedGlobalPoses = new Matrix4x4[currentFrameMatrices.Length];
        
        for (int i = 0; i < currentFrameMatrices.Length; i++) {
            Matrix4x4 currentMatrix = currentFrameMatrices[i];
            Matrix4x4 nextMatrix = nextFrameMatrices[i];
            
            // Un-bake the matrices to get absolute global poses.
            Matrix4x4 inverseBindPose = skeleton.Bones[i].Transformation;
            Matrix4x4.Invert(inverseBindPose, out Matrix4x4 bindPose);
            
            Matrix4x4 currentGlobalPose = bindPose * currentMatrix;
            Matrix4x4 nextGlobalPose = bindPose * nextMatrix;
            
            int parentId = skeleton.Bones[i].ParentId;
            
            Matrix4x4 currentLocalPose = currentGlobalPose;
            Matrix4x4 nextLocalPose = nextGlobalPose;
            
            if (parentId >= 0 && parentId < currentFrameMatrices.Length) {
                
                // Get the parent's global poses.
                Matrix4x4 parentInverseBind = skeleton.Bones[parentId].Transformation;
                Matrix4x4.Invert(parentInverseBind, out Matrix4x4 parentBind);
                
                Matrix4x4 parentGlobalA = parentBind * currentFrameMatrices[parentId];
                Matrix4x4 parentGlobalB = parentBind * nextFrameMatrices[parentId];
                
                // Invert parent global and multiply to get local space.
                Matrix4x4.Invert(parentGlobalA, out Matrix4x4 invParentA);
                Matrix4x4.Invert(parentGlobalB, out Matrix4x4 invParentB);
                
                currentLocalPose = currentGlobalPose * invParentA;
                nextLocalPose = nextGlobalPose * invParentB;
            }
            
            Matrix4x4 blendedLocal = Matrix4x4.LerpSrt(currentLocalPose, nextLocalPose, fraction);
            
            // Reconstructing the blended global hierarchy.
            if (parentId >= 0 && parentId < currentFrameMatrices.Length) {
                
                // Multiply by the parent's already-blended global pose.
                blendedGlobalPoses[i] = blendedLocal * blendedGlobalPoses[parentId];
            }
            else {
                
                // Root bone.
                blendedGlobalPoses[i] = blendedLocal;
            }
            
            // Re-bake the matrices for the GPU.
            interpolated[i] = inverseBindPose * blendedGlobalPoses[i];
        }
        
        return interpolated;
    }
}