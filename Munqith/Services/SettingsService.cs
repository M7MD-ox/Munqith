using System.Text.Json;
using Munqith.Models;

namespace Munqith.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsDir;
        private readonly string _settingsFile;
        private readonly object _fileLock = new();
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public AppSettings Settings { get; private set; } = new();

        public SettingsService(string? customSettingsDir = null)
        {
            _settingsDir = customSettingsDir ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Munqith");
            _settingsFile = Path.Combine(_settingsDir, "settings.json");
            Load();
        }

        public void Load()
        {
            lock (_fileLock)
            {
                try
                {
                    if (File.Exists(_settingsFile))
                    {
                        var json = File.ReadAllText(_settingsFile);
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
                        }
                    }
                }
                catch
                {
                    Settings = new AppSettings();
                }
            }
        }

        public void Save()
        {
            lock (_fileLock)
            {
                try
                {
                    Directory.CreateDirectory(_settingsDir);
                    string tempFile = _settingsFile + ".tmp";
                    var json = JsonSerializer.Serialize(Settings, JsonOptions);
                    File.WriteAllText(tempFile, json);
                    File.Move(tempFile, _settingsFile, overwrite: true);
                }
                catch { }
            }
        }
    }
}
