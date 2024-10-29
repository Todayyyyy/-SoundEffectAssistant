using Newtonsoft.Json;
using System.IO;

public class SettingsManager
{
    private string filePath;

    public SettingsManager(string filePath)
    {
        this.filePath = filePath;
    }

    public void SaveSettings(AppSettings settings)
    {
        var json = JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public AppSettings LoadSettings()
    {
        if (!File.Exists(filePath))
        {
            return new AppSettings(); // 如果文件不存在，返回默认设置
        }

        var json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<AppSettings>(json);
    }
}
