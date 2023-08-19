using Raylib_cs;

namespace Sparkle.csharp.audio; 

public static class MusicPlayer {

    /// <summary> See <see cref="Raylib.LoadMusicStream(string)"/> </summary>
    public static Music LoadStream(string path) => Raylib.LoadMusicStream(path);
    
    /// <summary> See <see cref="Raylib.LoadMusicStreamFromMemory(string, byte[])"/> </summary>
    public static Music LoadStreamFromMemory(string fileType, byte[] fileData) => Raylib.LoadMusicStreamFromMemory(fileType, fileData);
    
    /// <summary> See <see cref="Raylib.UnloadMusicStream"/> </summary>
    public static void UnloadStream(Music music) => Raylib.UnloadMusicStream(music);
    
    
    /// <summary> See <see cref="Raylib.IsMusicReady"/> </summary>
    public static bool IsReady(Music music) => Raylib.IsMusicReady(music);
    
    /// <summary> See <see cref="Raylib.IsMusicStreamPlaying"/> </summary>
    public static bool IsStreamPlaying(Music music) => Raylib.IsMusicStreamPlaying(music);

    
    /// <summary> See <see cref="Raylib.GetMusicTimeLength"/> </summary>
    public static float GetTimeLength(Music music) => Raylib.GetMusicTimeLength(music);
    
    /// <summary> See <see cref="Raylib.GetMusicTimePlayed"/> </summary>
    public static float GetTimePlayed(Music music) => Raylib.GetMusicTimePlayed(music);

    
    /// <summary> See <see cref="Raylib.PlayMusicStream"/> </summary>
    public static void PlayStream(Music music) => Raylib.PlayMusicStream(music);
    
    /// <summary> See <see cref="Raylib.UpdateMusicStream"/> </summary>
    public static void UpdateStream(Music music) => Raylib.UpdateMusicStream(music);
    
    /// <summary> See <see cref="Raylib.StopMusicStream"/> </summary>
    public static void StopStream(Music music) => Raylib.StopMusicStream(music);
    
    /// <summary> See <see cref="Raylib.PauseMusicStream"/> </summary>
    public static void PauseStream(Music music) => Raylib.PauseMusicStream(music);
    
    /// <summary> See <see cref="Raylib.ResumeMusicStream"/> </summary>
    public static void ResumeStream(Music music) => Raylib.ResumeMusicStream(music);
    
    /// <summary> See <see cref="Raylib.SeekMusicStream"/> </summary>
    public static void SeekStream(Music music, float pos) => Raylib.SeekMusicStream(music, pos);
    
    /// <summary> See <see cref="Raylib.SetMusicVolume"/> </summary>
    public static void SetVolume(Music music, float volume) => Raylib.SetMusicVolume(music, volume);
    
    /// <summary> See <see cref="Raylib.SetMusicPitch"/> </summary>
    public static void SetPitch(Music music, float pitch) => Raylib.SetMusicPitch(music, pitch);
    
    /// <summary> See <see cref="Raylib.SetMusicPan"/> </summary>
    public static void SetPan(Music music, float pan) => Raylib.SetMusicPan(music, pan);
}