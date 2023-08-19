using Raylib_cs;

namespace Sparkle.csharp.audio; 

public class AudioDevice {

    /// <summary>
    /// Used for Initializes objects.
    /// </summary>
    internal void Init() {
        if (!this.IsReady()) {
            Raylib.InitAudioDevice();
        }
    }
    
    /// <summary> See <see cref="Raylib.CloseAudioDevice"/> </summary>
    public void Close() => Raylib.CloseAudioDevice();
    
    /// <summary> See <see cref="Raylib.IsAudioDeviceReady"/> </summary>
    public bool IsReady() => Raylib.IsAudioDeviceReady();
    
    /// <summary> See <see cref="Raylib.SetMasterVolume"/> </summary>
    public void SetMasterVolume(float volume) => Raylib.SetMasterVolume(volume);
}