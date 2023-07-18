using Raylib_cs;

namespace Sparkle.csharp.audio; 

public static class AudioStreamer {
    
    public static AudioStream LoadAudioStream(uint sampleRate, uint sampleSize, uint channels) => Raylib.LoadAudioStream(sampleRate, sampleSize, channels);
    public static void UnloadAudioStream(AudioStream stream) => Raylib.UnloadAudioStream(stream);

    public static bool IsAudioStreamReady(AudioStream stream) => Raylib.IsAudioStreamReady(stream);
    public static unsafe void UpdateAudioStream(AudioStream stream, void* data, int frameCount) => Raylib.UpdateAudioStream(stream, data, frameCount);

    public static bool IsAudioStreamProcessed(AudioStream stream) => Raylib.IsAudioStreamProcessed(stream);
    public static bool IsAudioStreamPlaying(AudioStream stream) => Raylib.IsAudioStreamPlaying(stream);
    
    public static void PlayAudioStream(AudioStream stream) => Raylib.PlayAudioStream(stream);
    public static void PauseAudioStream(AudioStream stream) => Raylib.PauseAudioStream(stream);
    public static void ResumeAudioStream(AudioStream stream) => Raylib.ResumeAudioStream(stream);
    public static void StopAudioStream(AudioStream stream) => Raylib.StopAudioStream(stream);
    public static void SetAudioStreamVolume(AudioStream stream, float volume) => Raylib.SetAudioStreamVolume(stream, volume);
    public static void SetAudioStreamPitch(AudioStream stream, float pitch) => Raylib.SetAudioStreamPitch(stream, pitch);
    public static void SetAudioStreamPan(AudioStream stream, float pan) => Raylib.SetAudioStreamPan(stream, pan);
    public static void SetAudioStreamBufferSizeDefault(int size) => Raylib.SetAudioStreamBufferSizeDefault(size);
    
    //public static void SetAudioStreamCallback(AudioStream stream, __FnPtr<void (void*, uint)> callback) => Raylib.SetAudioStreamCallback(stream);
    //public static void AttachAudioStreamProcessor(AudioStream stream) => Raylib.AttachAudioStreamProcessor(stream);
    //public static void DetachAudioStreamProcessor(AudioStream stream) => Raylib.DetachAudioStreamProcessor(stream);
    //public static void AttachAudioMixedProcessor(AudioStream stream) => Raylib.AttachAudioMixedProcessor(stream);
    //public static void DetachAudioMixedProcessor(AudioStream stream) => Raylib.DetachAudioMixedProcessor(stream);
}