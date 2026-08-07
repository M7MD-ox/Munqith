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
        private GlobalHotkeyManager? _hotkeyManager;
        private HotkeyListenerForm? _hotkeyForm;

        public TrayApplicationContext()
        {
            _settingsService = new SettingsService();
            _settingsService.Load();
            _statsService = new StatsService(_settingsService);
            _conversionService = new ConversionService(_settingsService, _statsService);

            _trayIcon = new NotifyIcon
            {
                Icon = CreateAppIcon(),
                Text = "مُنقِذ Munqith",
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };
            _trayIcon.DoubleClick += (s, e) => ShowSettings();

            SetupHotkeys();
        }

        private void SetupHotkeys()
        {
            _hotkeyForm = new HotkeyListenerForm();
            _hotkeyManager = new GlobalHotkeyManager(_hotkeyForm.Handle);
            _hotkeyForm.HotkeyPressed += OnHotkeyPressed;
            _hotkeyForm.SetHotkeyManager(_hotkeyManager);
            RegisterConfiguredHotkeys();
        }

        public void RegisterConfiguredHotkeys()
        {
            _hotkeyManager?.UnregisterAll();
            foreach (var binding in _settingsService.Settings.Hotkeys)
            {
                if (binding.IsEnabled)
                {
                    _hotkeyManager?.RegisterHotkey(
                        (uint)binding.ModifierKeysValue,
                        (uint)binding.KeyValue,
                        () => { }); // Action handled via WM_HOTKEY
                }
            }
        }

        private async void OnHotkeyPressed(int hotkeyId)
        {
            await _conversionService.PerformConversion();
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();
            var isArabic = _settingsService.Settings.Language == "ar";
            menu.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            var settingsItem = new ToolStripMenuItem(isArabic ? "⚙️ الإعدادات" : "⚙️ Settings");
            settingsItem.Click += (s, e) => ShowSettings();
            menu.Items.Add(settingsItem);
            menu.Items.Add(new ToolStripSeparator());

            var toggleText = _settingsService.Settings.IsEnabled
                ? (isArabic ? "⏸️ إيقاف مؤقت" : "⏸️ Pause")
                : (isArabic ? "▶️ تشغيل" : "▶️ Resume");
            var toggleItem = new ToolStripMenuItem(toggleText);
            toggleItem.Click += (s, e) =>
            {
                _settingsService.Settings.IsEnabled = !_settingsService.Settings.IsEnabled;
                _settingsService.Save();
                _trayIcon.ContextMenuStrip = CreateContextMenu();
            };
            menu.Items.Add(toggleItem);
            menu.Items.Add(new ToolStripSeparator());

            var aboutItem = new ToolStripMenuItem(isArabic ? "ℹ️ عن البرنامج" : "ℹ️ About");
            aboutItem.Click += (s, e) => ShowAbout();
            menu.Items.Add(aboutItem);
            menu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem(isArabic ? "❌ خروج" : "❌ Exit");
            exitItem.Click += (s, e) => ExitApp();
            menu.Items.Add(exitItem);

            return menu;
        }

        private Icon CreateAppIcon()
        {
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using var brush = new SolidBrush(Color.FromArgb(0, 120, 212));
                g.FillEllipse(brush, 1, 1, 30, 30);
                using var font = new Font("Segoe UI", 14, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.White);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("م", font, textBrush, new RectangleF(0, 0, 32, 32), sf);
            }
            return Icon.FromHandle(bitmap.GetHicon());
        }

        private void ShowSettings()
        {
            var form = new SettingsForm(_settingsService, _statsService, this);
            form.ShowDialog();
            RegisterConfiguredHotkeys();
            _trayIcon.ContextMenuStrip = CreateContextMenu();
        }

        private void ShowAbout()
        {
            var form = new AboutForm(_settingsService.Settings.Language, _settingsService.Settings.DarkMode);
            form.ShowDialog();
        }

        private void ExitApp()
        {
            _hotkeyManager?.Dispose();
            _hotkeyForm?.Close();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _hotkeyManager?.Dispose();
                _trayIcon.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Hidden form to receive WM_HOTKEY messages from the OS.
    /// </summary>
    internal class HotkeyListenerForm : Form
    {
        private GlobalHotkeyManager? _hotkeyManager;
        public event Action<int>? HotkeyPressed;

        public HotkeyListenerForm()
        {
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Opacity = 0;
            this.Show();
            this.Hide();
        }

        public void SetHotkeyManager(GlobalHotkeyManager manager) => _hotkeyManager = manager;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                _hotkeyManager?.ProcessHotkey(id);
                HotkeyPressed?.Invoke(id);
            }
            base.WndProc(ref m);
        }
    }
}
