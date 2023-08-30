using Raylib_cs;

namespace Sparkle.csharp.audio; 

#if !HEADLESS
public static class AudioDevice {
    

    /// <inheritdoc cref="Raylib.InitAudioDevice"/>
    public static void Init() => Raylib.InitAudioDevice();
    
    /// <inheritdoc cref="Raylib.CloseAudioDevice"/>
    public static void Close() => Raylib.CloseAudioDevice();
    
    /// <inheritdoc cref="Raylib.IsAudioDeviceReady"/>
    public static bool IsReady() => Raylib.IsAudioDeviceReady();
    
    /// <inheritdoc cref="Raylib.SetMasterVolume"/>
    public static void SetMasterVolume(float volume) => Raylib.SetMasterVolume(volume);
}
#endif