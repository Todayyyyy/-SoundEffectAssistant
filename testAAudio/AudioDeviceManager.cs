using System.Collections.Generic;
using NAudio.Wave;

public class AudioDeviceManager
{
    public List<WaveOutCapabilities> GetAudioDevices()
    {
        List<WaveOutCapabilities> devices = new List<WaveOutCapabilities>();
        for (int i = 0; i < WaveOut.DeviceCount; i++)
        {
            devices.Add(WaveOut.GetCapabilities(i));
        }
        return devices;
    }
}
