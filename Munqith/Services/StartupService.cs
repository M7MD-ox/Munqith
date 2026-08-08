using Microsoft.Win32;

namespace Munqith.Services
{
    public static class StartupService
    {
        private const string AppName = "Munqith";
        private const string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public static bool IsRegistered()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
                return key?.GetValue(AppName) != null;
            }
            catch
            {
                return false;
            }
        }

        public static void Register()
        {
            try
            {
                string? exePath = Environment.ProcessPath ?? Application.ExecutablePath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
                    key?.SetValue(AppName, $"\"{exePath}\"");
                }
            }
            catch { }
        }

        public static void Unregister()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
                key?.DeleteValue(AppName, false);
            }
            catch { }
        }

        public static void SetStartup(bool enabled)
        {
            if (enabled) Register();
            else Unregister();
        }
    }
}
