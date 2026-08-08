using System.Text.Json.Serialization;
using Munqith.Core;

namespace Munqith.Models
{
    public class AppSettings
    {
        public bool SmartDetectionEnabled { get; set; } = true; // Enabled by default as requested!
        public bool StartWithWindows { get; set; } = true;
        public bool DarkMode { get; set; } = false;
        public string Language { get; set; } = "ar";

        public List<HotkeyBinding> Hotkeys { get; set; } = new()
        {
            new HotkeyBinding { ModifierKeysValue = 2, KeyValue = 0x51, IsEnabled = true },  // Ctrl+Q
            new HotkeyBinding { ModifierKeysValue = 1, KeyValue = 0x51, IsEnabled = true }   // Alt+Q
        };

        // Stats
        public int TotalConversions { get; set; } = 0;
        public int TotalCharactersConverted { get; set; } = 0;
        public int TotalWordsConverted { get; set; } = 0;
        public double TotalTimeSavedSeconds { get; set; } = 0;
    }

    public class HotkeyBinding
    {
        public int ModifierKeysValue { get; set; }
        public int KeyValue { get; set; }
        public bool IsEnabled { get; set; } = true;

        [JsonIgnore]
        public string DisplayName
        {
            get
            {
                var parts = new List<string>();
                if ((ModifierKeysValue & 2) != 0) parts.Add("Ctrl");
                if ((ModifierKeysValue & 1) != 0) parts.Add("Alt");
                if ((ModifierKeysValue & 4) != 0) parts.Add("Shift");
                var keyName = ((System.Windows.Forms.Keys)KeyValue).ToString();
                parts.Add(keyName);
                return string.Join("+", parts);
            }
        }
    }
}
