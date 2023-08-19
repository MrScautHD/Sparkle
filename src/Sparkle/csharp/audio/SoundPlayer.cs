using Raylib_cs;

namespace Sparkle.csharp.audio; 

public static class SoundPlayer {

    /// <summary> See <see cref="Raylib.LoadSound(string)"/> </summary>
    public static Sound Load(string path) => Raylib.LoadSound(path);
    
    /// <summary> See <see cref="Raylib.LoadSoundFromWave"/> </summary>
    public static Sound LoadFromWave(Wave wave) => Raylib.LoadSoundFromWave(wave);
    
    /// <summary> See <see cref="Raylib.UnloadSound"/> </summary>
    public static void Unload(Sound sound) => Raylib.UnloadSound(sound);

    
    /// <summary> See <see cref="Raylib.IsSoundReady"/> </summary>
    public static bool IsReady(Sound sound) => Raylib.IsSoundReady(sound);
    
    /// <summary> See <see cref="Raylib.IsSoundPlaying"/> </summary>
    public static bool IsPlaying(Sound sound) => Raylib.IsSoundPlaying(sound);
    
    /// <summary> See <see cref="Raylib.UpdateSound"/> </summary>
    public static unsafe void Update(Sound sound, void* data, int sampleCount) => Raylib.UpdateSound(sound, data, sampleCount);

    
    /// <summary> See <see cref="Raylib.PlaySound"/> </summary>
    public static void Play(Sound sound) => Raylib.PlaySound(sound);
    
    /// <summary> See <see cref="Raylib.StopSound"/> </summary>
    public static void Stop(Sound sound) => Raylib.StopSound(sound);
    
    /// <summary> See <see cref="Raylib.PauseSound"/> </summary>
    public static void Pause(Sound sound) => Raylib.PauseSound(sound);
    
    /// <summary> See <see cref="Raylib.ResumeSound"/> </summary>
    public static void Resume(Sound sound) => Raylib.ResumeSound(sound);
    
    
    /// <summary> See <see cref="Raylib.SetSoundVolume"/> </summary>
    public static void SetVolume(Sound sound, float volume) => Raylib.SetSoundVolume(sound, volume);
    
    /// <summary> See <see cref="Raylib.SetSoundPitch"/> </summary>
    public static void SetPitch(Sound sound, float pitch) => Raylib.SetSoundPitch(sound, pitch);
    
    /// <summary> See <see cref="Raylib.SetSoundPan"/> </summary>
    public static void SetPan(Sound sound, float pan) => Raylib.SetSoundPan(sound, pan);
}