using System.Text.Json;
using Munqith.Models;

namespace Munqith.Services
{
    public class SettingsService : ISettingsService
    {
        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Munqith");
        private static readonly string SettingsFile = Path.Combine(SettingsDir, "settings.json");
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public AppSettings Settings { get; private set; } = new();

        public SettingsService()
        {
            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    var json = File.ReadAllText(SettingsFile);
                    Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
                }
            }
            catch
            {
                Settings = new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDir);
                var json = JsonSerializer.Serialize(Settings, JsonOptions);
                File.WriteAllText(SettingsFile, json);
            }
            catch { }
        }
    }
}
