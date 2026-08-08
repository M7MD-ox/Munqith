using Munqith.Hooks;
using Munqith.Services;
using Munqith.UI;

namespace Munqith
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Ensure only one instance runs
            using var mutex = new Mutex(true, "MunqithSingleInstance", out bool isNewInstance);
            if (!isNewInstance)
            {
                MessageBox.Show(
                    "مُنقِذ Munqith is already running in the system tray!",
                    "Munqith",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ApplicationConfiguration.Initialize();

            var settingsService = new SettingsService();
            var statsService = new StatsService(settingsService);
            var conversionService = new ConversionService(settingsService, statsService);
            var keyboardHook = new LowLevelKeyboardHook();

            Application.Run(new TrayApplicationContext(settingsService, statsService, conversionService, keyboardHook));
        }
    }
}