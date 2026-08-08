using Munqith.Core;
using Munqith.Hooks;
using Munqith.Models;
using Munqith.Services;

namespace Munqith.UI
{
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly SettingsService _settingsService;
        private readonly StatsService _statsService;
        private readonly ConversionService _conversionService;
        private readonly LowLevelKeyboardHook _keyboardHook;
        private SettingsForm? _settingsForm;

        public TrayApplicationContext(
            SettingsService settingsService,
            StatsService statsService,
            ConversionService conversionService,
            LowLevelKeyboardHook keyboardHook)
        {
            _settingsService = settingsService;
            _statsService = statsService;
            _conversionService = conversionService;
            _keyboardHook = keyboardHook;

            _trayIcon = new NotifyIcon();
            InitializeTrayIcon();
            RegisterConfiguredHotkeys();

            _keyboardHook.HotkeyTriggered += OnHotkeyTriggered;
            _keyboardHook.Start();
        }

        private void InitializeTrayIcon()
        {
            // Load embedded application logo icon
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "app.ico");
                if (File.Exists(iconPath))
                {
                    _trayIcon.Icon = new Icon(iconPath);
                }
                else
                {
                    _trayIcon.Icon = SystemIcons.Shield;
                }
            }
            catch
            {
                _trayIcon.Icon = SystemIcons.Shield;
            }

            _trayIcon.Text = "مُنقِذ Munqith — أداة تصحيح لغة الكتابة";
            _trayIcon.Visible = true;

            CreateContextMenu();

            _trayIcon.DoubleClick += (s, e) => ShowSettings();
        }

        public void CreateContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Font = new Font("Segoe UI", 9.5f);
            menu.Renderer = new ToolStripProfessionalRenderer();

            // 1. Settings Item (Default bold)
            var settingsItem = new ToolStripMenuItem("⚙️  الإعدادات ولوحة التحكم (Settings)", null, (s, e) => ShowSettings());
            settingsItem.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            menu.Items.Add(settingsItem);

            menu.Items.Add(new ToolStripSeparator());

            // 2. Conversion Scope summary
            string scopeText = _settingsService.Settings.ConversionMode switch
            {
                ConversionMode.SelectedText => "🎯  النطاق: النص المحدد (Auto)",
                ConversionMode.AllText => "🎯  النطاق: السطر الحالي بالكامل",
                ConversionMode.LastWord => "🎯  النطاق: آخر كلمة قبل المؤشر",
                _ => "🎯  النطاق: ذكي"
            };
            var scopeItem = new ToolStripMenuItem(scopeText, null, (s, e) => ShowSettings());
            menu.Items.Add(scopeItem);

            menu.Items.Add(new ToolStripSeparator());

            // 3. About
            var aboutItem = new ToolStripMenuItem("ℹ️  عن أداة مُنقِذ (About)", null, (s, e) =>
            {
                ShowSettings();
            });
            menu.Items.Add(aboutItem);

            menu.Items.Add(new ToolStripSeparator());

            // 4. Exit
            var exitItem = new ToolStripMenuItem("❌  إغلاق البرنامج (Exit)", null, (s, e) => ExitApplication());
            menu.Items.Add(exitItem);

            _trayIcon.ContextMenuStrip = menu;
        }

        public void RegisterConfiguredHotkeys()
        {
            _keyboardHook.UpdateBindings(_settingsService.Settings.Hotkeys);
        }

        private async void OnHotkeyTriggered(HotkeyBinding binding)
        {
            await _conversionService.PerformConversion();
        }

        public void ShowSettings()
        {
            if (_settingsForm == null || _settingsForm.IsDisposed)
            {
                _settingsForm = new SettingsForm(_settingsService, _statsService, this);
            }

            _settingsForm.Show();
            _settingsForm.WindowState = FormWindowState.Normal;
            _settingsForm.BringToFront();
            _settingsForm.Activate();
        }

        private void ExitApplication()
        {
            _keyboardHook.Stop();
            _keyboardHook.Dispose();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }
    }
}
