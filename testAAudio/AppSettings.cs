using System;
using System.Collections.Generic;
using System.Windows.Forms;

public class AppSettings
{
    public Dictionary<string, string> ButtonAudioFilePaths { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, Keys> ShortcutKeys { get; set; } = new Dictionary<string, Keys>();
    public float Volume { get; set; }
    public DateTime? LastLoginTime { get; set; }

    public AppSettings()
    {
        ShortcutKeys = new Dictionary<string, Keys>();
    }
}
