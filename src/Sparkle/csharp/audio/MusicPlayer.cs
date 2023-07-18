using Raylib_cs;

namespace Sparkle.csharp.audio; 

public static class MusicPlayer {
    
    public static Music LoadMusicStream(string path) => Raylib.LoadMusicStream(path);
    public static Music LoadMusicStreamFromMemory(string fileType, byte[] fileData) => Raylib.LoadMusicStreamFromMemory(fileType, fileData);
    public static void UnloadMusicStream(Music music) => Raylib.UnloadMusicStream(music);
    
    public static bool IsMusicReady(Music music) => Raylib.IsMusicReady(music);
    public static bool IsMusicStreamPlaying(Music music) => Raylib.IsMusicStreamPlaying(music);
    
    public static float GetMusicTimeLength(Music music) => Raylib.GetMusicTimeLength(music);
    public static float GetMusicTimePlayed(Music music) => Raylib.GetMusicTimePlayed(music);

    public static void PlayMusicStream(Music music) => Raylib.PlayMusicStream(music);
    public static void UpdateMusicStream(Music music) => Raylib.UpdateMusicStream(music);
    public static void StopMusicStream(Music music) => Raylib.StopMusicStream(music);
    public static void PauseMusicStream(Music music) => Raylib.PauseMusicStream(music);
    public static void ResumeMusicStream(Music music) => Raylib.ResumeMusicStream(music);
    public static void SeekMusicStream(Music music, float position) => Raylib.SeekMusicStream(music, position);
    public static void SetMusicVolume(Music music, float volume) => Raylib.SetMusicVolume(music, volume);
    public static void SetMusicPitch(Music music, float pitch) => Raylib.SetMusicPitch(music, pitch);
    public static void SetMusicPan(Music music, float pan) => Raylib.SetMusicPan(music, pan);
}