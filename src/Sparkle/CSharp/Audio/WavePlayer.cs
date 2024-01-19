using Raylib_cs;

namespace Sparkle.CSharp.Audio; 

public class WavePlayer {
    
    /// <inheritdoc cref="Raylib.LoadWave(string)"/>
    public static Wave Load(string path) => Raylib.LoadWave(path);
    
    /// <inheritdoc cref="Raylib.LoadWaveFromMemory(string, byte[])"/>
    public static Wave LoadFromMemory(string fileType, byte[] fileData) => Raylib.LoadWaveFromMemory(fileType, fileData);
    
    /// <inheritdoc cref="Raylib.UnloadWave"/>
    public static void Unload(Wave wave) => Raylib.UnloadWave(wave);
    
    
    /// <inheritdoc cref="Raylib.IsWaveReady"/>
    public static bool IsReady(Wave wave) => Raylib.IsWaveReady(wave);
    
    /// <inheritdoc cref="Raylib.ExportWave(Wave, string)"/>
    public static bool Export(Wave wave, string path) => Raylib.ExportWave(wave, path);
    
    /// <inheritdoc cref="Raylib.ExportWaveAsCode(Wave, string)"/>
    public static bool ExportAsCode(Wave wave, string path) => Raylib.ExportWaveAsCode(wave, path);
    
    
    /// <inheritdoc cref="Raylib.WaveCopy"/>
    public static Wave Copy(Wave wave) => Raylib.WaveCopy(wave);
    
    /// <inheritdoc cref="Raylib.WaveCrop(ref Wave, int, int)"/>
    public static void Crop(ref Wave wave, int initSample, int finalSample) => Raylib.WaveCrop(ref wave, initSample, finalSample);
    
    /// <inheritdoc cref="Raylib.WaveFormat(ref Wave, int, int, int)"/>
    public static void Format(ref Wave wave, int sampleRate, int sampleSize, int channels) => Raylib.WaveFormat(ref wave, sampleRate, sampleSize, channels);
    
    /// <inheritdoc cref="Raylib.LoadWaveSamples"/>
    public static unsafe float* LoadSamples(Wave wave) => Raylib.LoadWaveSamples(wave);
    
    /// <inheritdoc cref="Raylib.UnloadWaveSamples"/>
    public static unsafe void UnloadSamples(float* samples) => Raylib.UnloadWaveSamples(samples);
}