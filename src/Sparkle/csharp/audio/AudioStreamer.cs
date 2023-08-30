using Raylib_cs;

namespace Sparkle.csharp.audio; 

#if !HEADLESS
public static class AudioStreamer {
    
    /// <inheritdoc cref="Raylib.LoadAudioStream"/>
    public static AudioStream Load(uint sampleRate, uint sampleSize, uint channels) => Raylib.LoadAudioStream(sampleRate, sampleSize, channels);
    
    /// <inheritdoc cref="Raylib.LoadAudioStream"/>
    public static void Unload(AudioStream stream) => Raylib.UnloadAudioStream(stream);

    
    /// <inheritdoc cref="Raylib.IsAudioStreamReady"/>
    public static bool IsReady(AudioStream stream) => Raylib.IsAudioStreamReady(stream);
    
    /// <inheritdoc cref="Raylib.UpdateAudioStream"/>
    public static unsafe void Update(AudioStream stream, void* data, int frameCount) => Raylib.UpdateAudioStream(stream, data, frameCount);

    
    /// <inheritdoc cref="Raylib.IsAudioStreamProcessed"/>
    public static bool IsProcessed(AudioStream stream) => Raylib.IsAudioStreamProcessed(stream);
    
    /// <inheritdoc cref="Raylib.IsAudioStreamPlaying"/>
    public static bool IsPlaying(AudioStream stream) => Raylib.IsAudioStreamPlaying(stream);
    
    /// <inheritdoc cref="Raylib.PlayAudioStream"/>
    public static void Play(AudioStream stream) => Raylib.PlayAudioStream(stream);
    
    /// <inheritdoc cref="Raylib.PauseAudioStream"/>
    public static void Pause(AudioStream stream) => Raylib.PauseAudioStream(stream);
    
    /// <inheritdoc cref="Raylib.ResumeAudioStream"/>
    public static void Resume(AudioStream stream) => Raylib.ResumeAudioStream(stream);
    
    /// <inheritdoc cref="Raylib.StopAudioStream"/>
    public static void Stop(AudioStream stream) => Raylib.StopAudioStream(stream);
    
    /// <inheritdoc cref="Raylib.SetAudioStreamVolume"/>
    public static void SetVolume(AudioStream stream, float volume) => Raylib.SetAudioStreamVolume(stream, volume);
    
    /// <inheritdoc cref="Raylib.SetAudioStreamPitch"/>
    public static void SetPitch(AudioStream stream, float pitch) => Raylib.SetAudioStreamPitch(stream, pitch);
    
    /// <inheritdoc cref="Raylib.SetAudioStreamPan"/>
    public static void SetPan(AudioStream stream, float pan) => Raylib.SetAudioStreamPan(stream, pan);
    
    /// <inheritdoc cref="Raylib.SetAudioStreamBufferSizeDefault"/>
    public static void SetBufferSizeDefault(int size) => Raylib.SetAudioStreamBufferSizeDefault(size);
    
    
    /// <inheritdoc cref="Raylib.SetAudioStreamCallback"/>
    public static unsafe void SetCallback(AudioStream stream, delegate*unmanaged[Cdecl]<void*, uint, void> callback) => Raylib.SetAudioStreamCallback(stream, callback);
    
    /// <inheritdoc cref="Raylib.AttachAudioStreamProcessor"/>
    public static unsafe void AttachProcessor(AudioStream stream, delegate*unmanaged[Cdecl]<void*, uint, void> processor) => Raylib.AttachAudioStreamProcessor(stream, processor);
    
    /// <inheritdoc cref="Raylib.DetachAudioStreamProcessor"/>
    public static unsafe void DetachProcessor(AudioStream stream, delegate*unmanaged[Cdecl]<void*, uint, void> processor) => Raylib.DetachAudioStreamProcessor(stream, processor);
    
    /// <inheritdoc cref="Raylib.AttachAudioMixedProcessor"/>
    public static unsafe void AttachAudioMixedProcessor(delegate*unmanaged[Cdecl]<void*, uint, void> processor) => Raylib.AttachAudioMixedProcessor(processor);
    
    /// <inheritdoc cref="Raylib.DetachAudioMixedProcessor"/>
    public static unsafe void DetachAudioMixedProcessor(delegate*unmanaged[Cdecl]<void*, uint, void> processor) => Raylib.DetachAudioMixedProcessor(processor);
}
#endif