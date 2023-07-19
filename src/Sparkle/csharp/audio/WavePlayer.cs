using Raylib_cs;

namespace Sparkle.csharp.audio; 

public class WavePlayer {
    
    public static Wave Load(string path) => Raylib.LoadWave(path);
    public static Wave LoadFromMemory(string fileType, byte[] fileData) => Raylib.LoadWaveFromMemory(fileType, fileData);
    public static void Unload(Wave wave) => Raylib.UnloadWave(wave);
    
    public static bool IsReady(Wave wave) => Raylib.IsWaveReady(wave);
    public static bool Export(Wave wave, string path) => Raylib.ExportWave(wave, path);
    public static bool ExportAsCode(Wave wave, string path) => Raylib.ExportWaveAsCode(wave, path);
    
    public static Wave Copy(Wave wave) => Raylib.WaveCopy(wave);
    public static void Crop(ref Wave wave, int initSample, int finalSample) => Raylib.WaveCrop(ref wave, initSample, finalSample);
    public static void Format(ref Wave wave, int sampleRate, int sampleSize, int channels) => Raylib.WaveFormat(ref wave, sampleRate, sampleSize, channels);
    public static unsafe float* LoadSamples(Wave wave) => Raylib.LoadWaveSamples(wave);
    public static unsafe void UnloadSamples(float* samples) => Raylib.UnloadWaveSamples(samples);
}