using Raylib_cs;

namespace Sparkle.csharp.audio; 

public class WavePlayer {
    
    /// <summary> See <see cref="Raylib.LoadWave(string)"/> </summary>
    public static Wave Load(string path) => Raylib.LoadWave(path);
    
    /// <summary> See <see cref="Raylib.LoadWaveFromMemory(string, byte[])"/> </summary>
    public static Wave LoadFromMemory(string fileType, byte[] fileData) => Raylib.LoadWaveFromMemory(fileType, fileData);
    
    /// <summary> See <see cref="Raylib.UnloadWave"/> </summary>
    public static void Unload(Wave wave) => Raylib.UnloadWave(wave);
    
    
    /// <summary> See <see cref="Raylib.IsWaveReady"/> </summary>
    public static bool IsReady(Wave wave) => Raylib.IsWaveReady(wave);
    
    /// <summary> See <see cref="Raylib.ExportWave(Wave, string)"/> </summary>
    public static bool Export(Wave wave, string path) => Raylib.ExportWave(wave, path);
    
    /// <summary> See <see cref="Raylib.ExportWaveAsCode(Wave, string)"/> </summary>
    public static bool ExportAsCode(Wave wave, string path) => Raylib.ExportWaveAsCode(wave, path);
    
    
    /// <summary> See <see cref="Raylib.WaveCopy"/> </summary>
    public static Wave Copy(Wave wave) => Raylib.WaveCopy(wave);
    
    /// <summary> See <see cref="Raylib.WaveCrop(ref Wave, int, int)"/> </summary>
    public static void Crop(ref Wave wave, int initSample, int finalSample) => Raylib.WaveCrop(ref wave, initSample, finalSample);
    
    /// <summary> See <see cref="Raylib.WaveFormat(ref Wave, int, int, int)"/> </summary>
    public static void Format(ref Wave wave, int sampleRate, int sampleSize, int channels) => Raylib.WaveFormat(ref wave, sampleRate, sampleSize, channels);
    
    /// <summary> See <see cref="Raylib.LoadWaveSamples"/> </summary>
    public static unsafe float* LoadSamples(Wave wave) => Raylib.LoadWaveSamples(wave);
    
    /// <summary> See <see cref="Raylib.UnloadWaveSamples"/> </summary>
    public static unsafe void UnloadSamples(float* samples) => Raylib.UnloadWaveSamples(samples);
}