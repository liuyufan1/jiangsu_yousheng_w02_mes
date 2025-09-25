using System.IO;
using System.Text.Json;

namespace w02_mes.helpers;


public static class ConfigHelper
{
    private static readonly string ConfigPath = "config.json";

    public static AppConfig Load()
    {
        if (!File.Exists(ConfigPath))
            return new AppConfig(); // 默认配置

        string json = File.ReadAllText(ConfigPath);
        return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
    }

    public static void Save(AppConfig config)
    {
        string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(ConfigPath, json);
    }
}
