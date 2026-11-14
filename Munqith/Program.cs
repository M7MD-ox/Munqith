using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Munqith.Core;
using Munqith.Hooks;
using Munqith.Interop;
using Munqith.Services;
using Munqith.UI;

namespace Munqith
{
    internal static class Program
    {
        private const string SingleInstanceMutexName = "MunqithSingleInstance";

        [STAThread]
        static void Main(string[] args)
        {
            bool isRestart = args.Any(a => string.Equals(a, "--restart", StringComparison.OrdinalIgnoreCase));

            // Ensure only one instance runs (wait briefly if restarting as Administrator)
            Mutex? mutex = null;
            bool isNewInstance = false;
            int attempts = isRestart ? 25 : 1;
            for (int i = 0; i < attempts; i++)
            {
                mutex?.Dispose();
                mutex = new Mutex(true, SingleInstanceMutexName, out isNewInstance);
                if (isNewInstance) break;
                Thread.Sleep(100);
            }

            using (mutex)
            {
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

                var services = new ServiceCollection();
                ConfigureServices(services);

                var serviceProvider = services.BuildServiceProvider();

                Application.Run(serviceProvider.GetRequiredService<TrayApplicationContext>());
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // WinForms Blazor WebView
            services.AddWindowsFormsBlazorWebView();

            // Native Interop
            services.AddSingleton<INativeClipboard, NativeClipboard>();

            // Domain Core
            services.AddSingleton<ILayoutConverter, LayoutConverter>();
            services.AddSingleton<ISmartDetector, SmartDetector>();
            services.AddSingleton<ITextSelectionService, TextSelectionService>();

            // Application Services
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IStatsService, StatsService>();
            services.AddSingleton<IStartupService, StartupService>();
            services.AddSingleton<IConversionService, ConversionService>();

            // System Hooks
            services.AddSingleton<IKeyboardHook, LowLevelKeyboardHook>();

            // UI Context
            services.AddSingleton<TrayApplicationContext>();
            services.AddSingleton<IServiceProvider>(sp => sp);
        }
    }
}