using Raylib_cs;

namespace Sparkle.csharp.audio; 

public static class MusicPlayer {
    
    public static Music LoadStream(string path) => Raylib.LoadMusicStream(path);
    public static Music LoadStreamFromMemory(string fileType, byte[] fileData) => Raylib.LoadMusicStreamFromMemory(fileType, fileData);
    public static void UnloadStream(Music music) => Raylib.UnloadMusicStream(music);
    
    public static bool IsReady(Music music) => Raylib.IsMusicReady(music);
    public static bool IsStreamPlaying(Music music) => Raylib.IsMusicStreamPlaying(music);

    public static float GetTimeLength(Music music) => Raylib.GetMusicTimeLength(music);
    public static float GetTimePlayed(Music music) => Raylib.GetMusicTimePlayed(music);

    public static void PlayStream(Music music) => Raylib.PlayMusicStream(music);
    public static void UpdateStream(Music music) => Raylib.UpdateMusicStream(music);
    public static void StopStream(Music music) => Raylib.StopMusicStream(music);
    public static void PauseStream(Music music) => Raylib.PauseMusicStream(music);
    public static void ResumeStream(Music music) => Raylib.ResumeMusicStream(music);
    public static void SeekStream(Music music, float pos) => Raylib.SeekMusicStream(music, pos);
    public static void SetVolume(Music music, float volume) => Raylib.SetMusicVolume(music, volume);
    public static void SetPitch(Music music, float pitch) => Raylib.SetMusicPitch(music, pitch);
    public static void SetPan(Music music, float pan) => Raylib.SetMusicPan(music, pan);
}