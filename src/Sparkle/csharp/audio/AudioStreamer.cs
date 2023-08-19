using Raylib_cs;

namespace Sparkle.csharp.audio; 

public static class AudioStreamer {
    
    /// <summary> See <see cref="Raylib.LoadAudioStream"/> </summary>
    public static AudioStream Load(uint sampleRate, uint sampleSize, uint channels) => Raylib.LoadAudioStream(sampleRate, sampleSize, channels);
    
    /// <summary> See <see cref="Raylib.LoadAudioStream"/> </summary>
    public static void Unload(AudioStream stream) => Raylib.UnloadAudioStream(stream);

    
    /// <summary> See <see cref="Raylib.IsAudioStreamReady"/> </summary>
    public static bool IsReady(AudioStream stream) => Raylib.IsAudioStreamReady(stream);
    
    /// <summary> See <see cref="Raylib.UpdateAudioStream"/> </summary>
    public static unsafe void Update(AudioStream stream, void* data, int frameCount) => Raylib.UpdateAudioStream(stream, data, frameCount);

    
    /// <summary> See <see cref="Raylib.IsAudioStreamProcessed"/> </summary>
    public static bool IsProcessed(AudioStream stream) => Raylib.IsAudioStreamProcessed(stream);
    
    /// <summary> See <see cref="Raylib.IsAudioStreamPlaying"/> </summary>
    public static bool IsPlaying(AudioStream stream) => Raylib.IsAudioStreamPlaying(stream);
    
    /// <summary> See <see cref="Raylib.PlayAudioStream"/> </summary>
    public static void Play(AudioStream stream) => Raylib.PlayAudioStream(stream);
    
    /// <summary> See <see cref="Raylib.PauseAudioStream"/> </summary>
    public static void Pause(AudioStream stream) => Raylib.PauseAudioStream(stream);
    
    /// <summary> See <see cref="Raylib.ResumeAudioStream"/> </summary>
    public static void Resume(AudioStream stream) => Raylib.ResumeAudioStream(stream);
    
    /// <summary> See <see cref="Raylib.StopAudioStream"/> </summary>
    public static void Stop(AudioStream stream) => Raylib.StopAudioStream(stream);
    
    /// <summary> See <see cref="Raylib.SetAudioStreamVolume"/> </summary>
    public static void SetVolume(AudioStream stream, float volume) => Raylib.SetAudioStreamVolume(stream, volume);
    
    /// <summary> See <see cref="Raylib.SetAudioStreamPitch"/> </summary>
    public static void SetPitch(AudioStream stream, float pitch) => Raylib.SetAudioStreamPitch(stream, pitch);
    
    /// <summary> See <see cref="Raylib.SetAudioStreamPan"/> </summary>
    public static void SetPan(AudioStream stream, float pan) => Raylib.SetAudioStreamPan(stream, pan);
    
    /// <summary> See <see cref="Raylib.SetAudioStreamBufferSizeDefault"/> </summary>
    public static void SetBufferSizeDefault(int size) => Raylib.SetAudioStreamBufferSizeDefault(size);
    
    
    /// <summary> See <see cref="Raylib.SetAudioStreamCallback"/> </summary>
    public static unsafe void SetCallback(AudioStream stream, delegate*unmanaged[Cdecl]<void*, uint, void> callback) => Raylib.SetAudioStreamCallback(stream, callback);
    
    /// <summary> See <see cref="Raylib.AttachAudioStreamProcessor"/> </summary>
    public static unsafe void AttachProcessor(AudioStream stream, delegate*unmanaged[Cdecl]<void*, uint, void> processor) => Raylib.AttachAudioStreamProcessor(stream, processor);
    
    /// <summary> See <see cref="Raylib.DetachAudioStreamProcessor"/> </summary>
    public static unsafe void DetachProcessor(AudioStream stream, delegate*unmanaged[Cdecl]<void*, uint, void> processor) => Raylib.DetachAudioStreamProcessor(stream, processor);
    
    /// <summary> See <see cref="Raylib.AttachAudioMixedProcessor"/> </summary>
    public static unsafe void AttachAudioMixedProcessor(delegate*unmanaged[Cdecl]<void*, uint, void> processor) => Raylib.AttachAudioMixedProcessor(processor);
    
    /// <summary> See <see cref="Raylib.DetachAudioMixedProcessor"/> </summary>
    public static unsafe void DetachAudioMixedProcessor(delegate*unmanaged[Cdecl]<void*, uint, void> processor) => Raylib.DetachAudioMixedProcessor(processor);
}