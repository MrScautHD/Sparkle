using Raylib_cs;

namespace Sparkle.csharp.audio; 

public static class AudioStreamer {
    
    public static AudioStream Load(uint sampleRate, uint sampleSize, uint channels) => Raylib.LoadAudioStream(sampleRate, sampleSize, channels);
    public static void Unload(AudioStream stream) => Raylib.UnloadAudioStream(stream);

    public static bool IsReady(AudioStream stream) => Raylib.IsAudioStreamReady(stream);
    public static unsafe void Update(AudioStream stream, void* data, int frameCount) => Raylib.UpdateAudioStream(stream, data, frameCount);

    public static bool IsProcessed(AudioStream stream) => Raylib.IsAudioStreamProcessed(stream);
    public static bool IsPlaying(AudioStream stream) => Raylib.IsAudioStreamPlaying(stream);
    
    public static void Play(AudioStream stream) => Raylib.PlayAudioStream(stream);
    public static void Pause(AudioStream stream) => Raylib.PauseAudioStream(stream);
    public static void Resume(AudioStream stream) => Raylib.ResumeAudioStream(stream);
    public static void Stop(AudioStream stream) => Raylib.StopAudioStream(stream);
    public static void SetVolume(AudioStream stream, float volume) => Raylib.SetAudioStreamVolume(stream, volume);
    public static void SetPitch(AudioStream stream, float pitch) => Raylib.SetAudioStreamPitch(stream, pitch);
    public static void SetPan(AudioStream stream, float pan) => Raylib.SetAudioStreamPan(stream, pan);
    public static void SetBufferSizeDefault(int size) => Raylib.SetAudioStreamBufferSizeDefault(size);

    //public static void SetAudioStreamCallback(AudioStream stream, __FnPtr<void (void*, uint)> callback) => Raylib.SetAudioStreamCallback(stream);
    //public static void AttachAudioStreamProcessor(AudioStream stream) => Raylib.AttachAudioStreamProcessor(stream);
    //public static void DetachAudioStreamProcessor(AudioStream stream) => Raylib.DetachAudioStreamProcessor(stream);
    //public static void AttachAudioMixedProcessor(AudioStream stream) => Raylib.AttachAudioMixedProcessor(stream);
    //public static void DetachAudioMixedProcessor(AudioStream stream) => Raylib.DetachAudioMixedProcessor(stream);
}