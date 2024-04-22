using System.Numerics;
using Raylib_cs;
using Sparkle.CSharp.Effects;

namespace Sparkle.CSharp.Particles;

public class ParticleBuilder {

    private ParticleData _data;

    /// <summary>
    /// Represents a builder class for creating ParticleData objects.
    /// </summary>
    public ParticleBuilder() {
        this._data = new ParticleData();
    }

    /// <summary>
    /// Sets the effect of the particle to the specified value.
    /// </summary>
    /// <param name="effect">The effect to set.</param>
    /// <returns>The particle builder object with the updated effect.</returns>
    public ParticleBuilder SetEffect(Effect effect) {
        this._data.Effect = effect;
        return this;
    }

    /// <summary>
    /// Sets the size of the particle to the specified value.
    /// </summary>
    /// <param name="size">The size of the particle.</param>
    /// <returns>The particle builder object with the updated size.</returns>
    public ParticleBuilder SetSize(Vector2 size) {
        this._data.StartSize = size;
        this._data.EndSize = size;
        return this;
    }

    /// <summary>
    /// Sets the size of the particle over its lifetime to the specified values.
    /// </summary>
    /// <param name="startSize">The starting size of the particle.</param>
    /// <param name="endSize">The ending size of the particle.</param>
    /// <returns>The particle builder object with the updated size.</returns>
    public ParticleBuilder SetSizeOverLifeTime(Vector2 startSize, Vector2 endSize) {
        this._data.StartSize = startSize;
        this._data.EndSize = endSize;
        return this;
    }

    /// <summary>
    /// Sets the rotation of the particle to the specified value.
    /// </summary>
    /// <param name="rotation">The rotation of the particle.</param>
    /// <returns>The particle builder object with the updated rotation.</returns>
    public ParticleBuilder SetRotation(float rotation) {
        this._data.StartRotation = rotation;
        this._data.EndRotation = rotation;
        return this;
    }
    
    /// <summary>
    /// Sets the rotation of the particle over its lifetime to the specified values.
    /// </summary>
    /// <param name="startRotation">The starting rotation of the particle.</param>
    /// <param name="endRotation">The ending rotation of the particle.</param>
    /// <returns>The particle builder object with the updated rotation.</returns>
    public ParticleBuilder SetRotationOverLifeTime(float startRotation, float endRotation) {
        this._data.StartRotation = startRotation;
        this._data.EndRotation = endRotation;
        return this;
    }

    /// <summary>
    /// Sets the color of the particle to the specified value.
    /// </summary>
    /// <param name="color">The color of the particle.</param>
    /// <returns>The particle builder object with the updated color.</returns>
    public ParticleBuilder SetColor(Color color) {
        this._data.StartColor = color;
        this._data.EndColor = color;
        return this;
    }

    /// <summary>
    /// Sets the color of the particle over its lifetime to the specified values.
    /// </summary>
    /// <param name="startColor">The starting color of the particle.</param>
    /// <param name="endColor">The ending color of the particle.</param>
    /// <returns>The particle builder object with the updated color.</returns>
    public ParticleBuilder SetColorOverLifeTime(Color startColor, Color endColor) {
        this._data.StartColor = startColor;
        this._data.EndColor = endColor;
        return this;
    }

    /// <summary>
    /// Builds a ParticleData object with the provided settings.
    /// </summary>
    /// <returns>The built ParticleData object.</returns>
    public ParticleData Build() {
        return this._data;
    }
}