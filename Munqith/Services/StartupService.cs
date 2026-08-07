using Microsoft.Win32;

namespace Munqith.Services
{
    public static class StartupService
    {
        private const string AppName = "Munqith";
        private const string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public static bool IsRegistered()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);
            return key?.GetValue(AppName) != null;
        }

        public static void Register()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
            key?.SetValue(AppName, $"\"{Application.ExecutablePath}\"");
        }

        public static void Unregister()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
            key?.DeleteValue(AppName, false);
        }

        public static void SetStartup(bool enabled)
        {
            if (enabled) Register();
            else Unregister();
        }
    }
}
