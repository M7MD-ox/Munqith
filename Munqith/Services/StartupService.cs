using Microsoft.Win32;

namespace Munqith.Services
{
    public class StartupService : IStartupService
    {
        private const string AppName = "Munqith";
        private const string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public bool IsRegistered()
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

        public void Register()
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

        public void Unregister()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
                key?.DeleteValue(AppName, false);
            }
            catch { }
        }

        public void SetStartup(bool enabled)
        {
            if (enabled) Register();
            else Unregister();
        }
    }
}
