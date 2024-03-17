using Raylib_cs;

namespace Sparkle.CSharp.Terrain;

public class Perlin {
    
    private readonly Random _random;
    private readonly int _gradientSizeTable;
    private readonly int[] _perm;
    
    public bool HasInitialized { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the PerlinNoise class with the specified seed.
    /// </summary>
    /// <param name="seed">Seed value for initializing the random number generator.</param>
    public Perlin(int seed) {
        this._random = new Random(seed);
        this._gradientSizeTable = 256;
        this._perm = new int[this._gradientSizeTable * 2];
    }
    
    /// <summary>
    /// Initializes the PerlinNoise object.
    /// </summary>
    public void Init() {
        this.SetupPermutation();
        this.HasInitialized = true;
    }
    
    /// <summary>
    /// Setup the permutation array for Perlin noise generation.
    /// </summary>
    private void SetupPermutation() {
        for (int i = 0; i < this._gradientSizeTable; i++) {
            int source = this._random.Next(this._gradientSizeTable - i) + i;

            this._perm[i + this._gradientSizeTable] = this._perm[source];
            this._perm[i] = this._perm[i + this._gradientSizeTable];
            this._perm[source] = this._perm[i];
        }
    }

    /// <summary>
    /// Generates Perlin noise based on the given coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The Perlin noise value.</returns>
    public float Noise(float x, float y) {
        int xi = (int) Math.Floor(x) & 255;
        int yi = (int) Math.Floor(y) & 255;

        int g1 = this._perm[this._perm[xi] + yi];
        int g2 = this._perm[this._perm[xi + 1] + yi];
        int g3 = this._perm[this._perm[xi] + yi + 1];
        int g4 = this._perm[this._perm[xi + 1] + yi + 1];

        float xf = x - MathF.Floor(x);
        float yf = y - MathF.Floor(y);

        float u = this.Fade(xf);
        float v = this.Fade(yf);

        float x1Interpolated = Raymath.Lerp(this.Grad(g1, xf, yf), this.Grad(g2, xf - 1, yf), u);
        float x2Interpolated = Raymath.Lerp(this.Grad(g3, xf, yf - 1), this.Grad(g4, xf - 1, yf - 1), u);

        return Raymath.Lerp(x1Interpolated, x2Interpolated, v);
    }

    /// <summary>
    /// Fades a given value.
    /// </summary>
    /// <param name="t">The value to fade.</param>
    /// <returns>The faded value.</returns>
    private float Fade(float t) {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    /// <summary>
    /// Calculates the gradient value for Perlin noise generation based on the given hash, x, and y values.
    /// </summary>
    /// <param name="hash">The hash value.</param>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The gradient value.</returns>
    private float Grad(int hash, float x, float y) {
        int h = hash & 7;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? 2 * v : -2 * v);
    }
}