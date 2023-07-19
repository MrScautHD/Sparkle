using Raylib_cs;

namespace Sparkle.csharp.audio; 

public static class SoundPlayer {

    public static Sound Load(string path) => Raylib.LoadSound(path);
    public static Sound LoadFromWave(Wave wave) => Raylib.LoadSoundFromWave(wave);
    public static void Unload(Sound sound) => Raylib.UnloadSound(sound);

    public static bool IsReady(Sound sound) => Raylib.IsSoundReady(sound);
    public static bool IsPlaying(Sound sound) => Raylib.IsSoundPlaying(sound);
    public static unsafe void Update(Sound sound, void* data, int sampleCount) => Raylib.UpdateSound(sound, data, sampleCount);

    public static void Play(Sound sound) => Raylib.PlaySound(sound);
    public static void Stop(Sound sound) => Raylib.StopSound(sound);
    public static void Pause(Sound sound) => Raylib.PauseSound(sound);
    public static void Resume(Sound sound) => Raylib.ResumeSound(sound);
    
    public static void SetVolume(Sound sound, float volume) => Raylib.SetSoundVolume(sound, volume);
    public static void SetPitch(Sound sound, float pitch) => Raylib.SetSoundPitch(sound, pitch);
    public static void SetPan(Sound sound, float pan) => Raylib.SetSoundPan(sound, pan);
}