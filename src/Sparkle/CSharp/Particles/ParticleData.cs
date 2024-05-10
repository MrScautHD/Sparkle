using System.Numerics;
using Raylib_CSharp.Colors;
using Sparkle.CSharp.Effects;
using Sparkle.CSharp.Registries.Types;

namespace Sparkle.CSharp.Particles;

public struct ParticleData {

    public Effect Effect { get; internal set; }

    public Vector2 StartSize { get; internal set; }
    public Vector2 EndSize { get; internal set; }
    
    public float StartRotation { get; internal set; }
    public float EndRotation { get; internal set; }
    
    public Color StartColor { get; internal set; }
    public Color EndColor { get; internal set; }
    
    public Vector3 StartVelocity { get; internal set; }
    public Vector3 EndVelocity { get; internal set; }

    /// <summary>
    /// Default constructor for creating a ParticleData struct.
    /// </summary>
    public ParticleData() {
        this.Effect = EffectRegistry.DiscardAlpha;
        this.StartSize = Vector2.Zero;
        this.EndSize = Vector2.Zero;
        this.StartRotation = 0f;
        this.EndRotation = 0f;
        this.StartColor = Color.White;
        this.EndColor = Color.White;
        this.StartVelocity = Vector3.Zero;
        this.EndVelocity = Vector3.Zero;
    }
}