using Raylib_cs;

namespace Sparkle.csharp.audio; 

public static class MusicPlayer {
    
    /// <inheritdoc cref="Raylib.LoadMusicStream(string)"/>
    public static Music LoadStream(string path) => Raylib.LoadMusicStream(path);
    
    /// <inheritdoc cref="Raylib.LoadMusicStreamFromMemory(string, byte[])"/>
    public static Music LoadStreamFromMemory(string fileType, byte[] fileData) => Raylib.LoadMusicStreamFromMemory(fileType, fileData);
    
    /// <inheritdoc cref="Raylib.UnloadMusicStream"/>
    public static void UnloadStream(Music music) => Raylib.UnloadMusicStream(music);
    
    
    /// <inheritdoc cref="Raylib.IsMusicReady"/>
    public static bool IsReady(Music music) => Raylib.IsMusicReady(music);
    
    /// <inheritdoc cref="Raylib.IsMusicStreamPlaying"/>
    public static bool IsStreamPlaying(Music music) => Raylib.IsMusicStreamPlaying(music);

    
    /// <inheritdoc cref="Raylib.GetMusicTimeLength"/>
    public static float GetTimeLength(Music music) => Raylib.GetMusicTimeLength(music);
    
    /// <inheritdoc cref="Raylib.GetMusicTimePlayed"/>
    public static float GetTimePlayed(Music music) => Raylib.GetMusicTimePlayed(music);

    
    /// <inheritdoc cref="Raylib.PlayMusicStream"/>
    public static void PlayStream(Music music) => Raylib.PlayMusicStream(music);
    
    /// <inheritdoc cref="Raylib.UpdateMusicStream"/>
    public static void UpdateStream(Music music) => Raylib.UpdateMusicStream(music);
    
    /// <inheritdoc cref="Raylib.StopMusicStream"/>
    public static void StopStream(Music music) => Raylib.StopMusicStream(music);
    
    /// <inheritdoc cref="Raylib.PauseMusicStream"/>
    public static void PauseStream(Music music) => Raylib.PauseMusicStream(music);
    
    /// <inheritdoc cref="Raylib.ResumeMusicStream"/>
    public static void ResumeStream(Music music) => Raylib.ResumeMusicStream(music);
    
    /// <inheritdoc cref="Raylib.SeekMusicStream"/>
    public static void SeekStream(Music music, float pos) => Raylib.SeekMusicStream(music, pos);
    
    /// <inheritdoc cref="Raylib.SetMusicVolume"/>
    public static void SetVolume(Music music, float volume) => Raylib.SetMusicVolume(music, volume);
    
    /// <inheritdoc cref="Raylib.SetMusicPitch"/>
    public static void SetPitch(Music music, float pitch) => Raylib.SetMusicPitch(music, pitch);
    
    /// <inheritdoc cref="Raylib.SetMusicPan"/>
    public static void SetPan(Music music, float pan) => Raylib.SetMusicPan(music, pan);
}