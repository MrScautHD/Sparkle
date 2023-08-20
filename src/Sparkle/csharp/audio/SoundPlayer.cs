using Raylib_cs;

namespace Sparkle.csharp.audio; 

public static class SoundPlayer {

    /// <inheritdoc cref="Raylib.LoadSound(string)"/>
    public static Sound Load(string path) => Raylib.LoadSound(path);
    
    /// <inheritdoc cref="Raylib.LoadSoundFromWave"/>
    public static Sound LoadFromWave(Wave wave) => Raylib.LoadSoundFromWave(wave);
    
    /// <inheritdoc cref="Raylib.UnloadSound"/>
    public static void Unload(Sound sound) => Raylib.UnloadSound(sound);

    
    /// <inheritdoc cref="Raylib.IsSoundReady"/>
    public static bool IsReady(Sound sound) => Raylib.IsSoundReady(sound);
    
    /// <inheritdoc cref="Raylib.IsSoundPlaying"/>
    public static bool IsPlaying(Sound sound) => Raylib.IsSoundPlaying(sound);
    
    /// <inheritdoc cref="Raylib.UpdateSound"/>
    public static unsafe void Update(Sound sound, void* data, int sampleCount) => Raylib.UpdateSound(sound, data, sampleCount);

    
    /// <inheritdoc cref="Raylib.PlaySound"/>
    public static void Play(Sound sound) => Raylib.PlaySound(sound);
    
    /// <inheritdoc cref="Raylib.StopSound"/>
    public static void Stop(Sound sound) => Raylib.StopSound(sound);
    
    /// <inheritdoc cref="Raylib.PauseSound"/>
    public static void Pause(Sound sound) => Raylib.PauseSound(sound);
    
    /// <inheritdoc cref="Raylib.ResumeSound"/>
    public static void Resume(Sound sound) => Raylib.ResumeSound(sound);
    
    
    /// <inheritdoc cref="Raylib.SetSoundVolume"/>
    public static void SetVolume(Sound sound, float volume) => Raylib.SetSoundVolume(sound, volume);
    
    /// <inheritdoc cref="Raylib.SetSoundPitch"/>
    public static void SetPitch(Sound sound, float pitch) => Raylib.SetSoundPitch(sound, pitch);
    
    /// <inheritdoc cref="Raylib.SetSoundPan"/>
    public static void SetPan(Sound sound, float pan) => Raylib.SetSoundPan(sound, pan);
}