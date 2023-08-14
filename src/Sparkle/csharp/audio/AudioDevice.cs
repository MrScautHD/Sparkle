using Raylib_cs;

namespace Sparkle.csharp.audio; 

public class AudioDevice {

    internal void Init() {
        if (!this.IsReady()) {
            Raylib.InitAudioDevice();
        }
    }

    public void Close() => Raylib.CloseAudioDevice();
    
    public bool IsReady() => Raylib.IsAudioDeviceReady();
    
    public void SetMasterVolume(float volume) => Raylib.SetMasterVolume(volume);
}