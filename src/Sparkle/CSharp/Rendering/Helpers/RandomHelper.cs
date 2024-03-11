using Raylib_cs;

namespace Sparkle.CSharp.Rendering.Helpers;

public class RandomHelper {
    
    /// <inheritdoc cref="Raylib.GetRandomValue"/>
    public static int GetRandomValue(int min, int max) => Raylib.GetRandomValue(min, max);
    
    /// <inheritdoc cref="Raylib.SetRandomSeed"/>
    public static int SetRandomSeed(uint seed) => Raylib.SetRandomSeed(seed);
    
    /// <inheritdoc cref="Raylib.LoadRandomSequence"/>
    public static unsafe int* LoadRandomSequence(int count, int min, int max) => Raylib.LoadRandomSequence(count, min, max);
    
    /// <inheritdoc cref="Raylib.UnloadRandomSequence"/>
    public static unsafe void UnloadRandomSequence(int* sequence) => Raylib.UnloadRandomSequence(sequence);
}