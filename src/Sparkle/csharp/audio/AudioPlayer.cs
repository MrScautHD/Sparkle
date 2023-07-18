using Raylib_cs;

namespace Sparkle.csharp.audio; 

public static class AudioPlayer {

    public static Wave LoadWave(string path) => Raylib.LoadWave(path);
    public static Wave LoadWaveFromMemory(string fileType, byte[] fileData) => Raylib.LoadWaveFromMemory(fileType, fileData);
    public static void UnloadWave(Wave wave) => Raylib.UnloadWave(wave);

    public static Sound LoadSound(string path) => Raylib.LoadSound(path);
    public static Sound LoadSoundFromWave(Wave wave) => Raylib.LoadSoundFromWave(wave);
    public static void UnloadSound(Sound sound) => Raylib.UnloadSound(sound);
    
    public static bool IsWaveReady(Wave wave) => Raylib.IsWaveReady(wave);
    public static bool IsSoundReady(Sound sound) => Raylib.IsSoundReady(sound);
    public static unsafe void UpdateSound(Sound sound, void* data, int sampleCount) => Raylib.UpdateSound(sound, data, sampleCount);
    public static bool ExportWave(Wave wave, string path) => Raylib.ExportWave(wave, path);
    public static bool ExportWaveAsCode(Wave wave, string path) => Raylib.ExportWaveAsCode(wave, path);

    public static void PlaySound(Sound sound) => Raylib.PlaySound(sound);
    public static void StopSound(Sound sound) => Raylib.StopSound(sound);
    public static void PauseSound(Sound sound) => Raylib.PauseSound(sound);
    public static void ResumeSound(Sound sound) => Raylib.ResumeSound(sound);
    
    public static bool IsSoundPlaying(Sound sound) => Raylib.IsSoundPlaying(sound);
    
    public static void SetSoundVolume(Sound sound, float volume) => Raylib.SetSoundVolume(sound, volume);
    public static void SetSoundPitch(Sound sound, float pitch) => Raylib.SetSoundPitch(sound, pitch);
    public static void SetSoundPan(Sound sound, float pan) => Raylib.SetSoundPan(sound, pan);
    
    public static Wave WaveCopy(Wave wave) => Raylib.WaveCopy(wave);
    public static void WaveCrop(ref Wave wave, int initSample, int finalSample) => Raylib.WaveCrop(ref wave, initSample, finalSample);
    public static void WaveFormat(ref Wave wave, int sampleRate, int sampleSize, int channels) => Raylib.WaveFormat(ref wave, sampleRate, sampleSize, channels);
    public static unsafe float* LoadWaveSamples(Wave wave) => Raylib.LoadWaveSamples(wave);
    public static unsafe void UnloadWaveSamples(float* samples) => Raylib.UnloadWaveSamples(samples);
}