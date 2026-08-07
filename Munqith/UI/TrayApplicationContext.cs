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
        private SettingsForm? _openSettingsForm;

        public TrayApplicationContext()
        {
            _settingsService = new SettingsService();
            _settingsService.Load();
            _statsService = new StatsService(_settingsService);
            _conversionService = new ConversionService(_settingsService, _statsService);

            _trayIcon = new NotifyIcon
            {
                Icon = LoadAppIcon(),
                Text = "مُنقِذ Munqith",
                Visible = true,
                ContextMenuStrip = CreateContextMenu()
            };

            _trayIcon.MouseClick += TrayIcon_MouseClick;
            _trayIcon.DoubleClick += (s, e) => ShowSettings();

            // Initialize Low Level Keyboard Hook
            _keyboardHook = new LowLevelKeyboardHook();
            _keyboardHook.HotkeyTriggered += OnHotkeyTriggered;
            RegisterConfiguredHotkeys();
            _keyboardHook.Start();
        }

        private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var method = typeof(NotifyIcon).GetMethod("ShowContextMenu", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (method != null)
                {
                    method.Invoke(_trayIcon, null);
                }
                else
                {
                    ShowSettings();
                }
            }
        }

        public void RegisterConfiguredHotkeys()
        {
            _keyboardHook.UpdateBindings(_settingsService.Settings.Hotkeys);
        }

        private async void OnHotkeyTriggered(HotkeyBinding binding)
        {
            await _conversionService.PerformConversion();
        }

        public ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();
            var isArabic = _settingsService.Settings.Language == "ar";
            menu.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;
            menu.Font = new Font("Segoe UI", 9.5f);

            var settingsItem = new ToolStripMenuItem(isArabic ? "⚙️  الإعدادات والتحكم" : "⚙️  Settings & Dashboard");
            settingsItem.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            settingsItem.Click += (s, e) => ShowSettings();
            menu.Items.Add(settingsItem);

            menu.Items.Add(new ToolStripSeparator());

            var toggleText = _settingsService.Settings.IsEnabled
                ? (isArabic ? "⏸️  إيقاف مؤقت" : "⏸️  Pause")
                : (isArabic ? "▶️  تشغيل" : "▶️  Resume");
            var toggleItem = new ToolStripMenuItem(toggleText);
            toggleItem.Click += (s, e) =>
            {
                _settingsService.Settings.IsEnabled = !_settingsService.Settings.IsEnabled;
                _settingsService.Save();
                _trayIcon.ContextMenuStrip = CreateContextMenu();
            };
            menu.Items.Add(toggleItem);

            menu.Items.Add(new ToolStripSeparator());

            var aboutItem = new ToolStripMenuItem(isArabic ? "ℹ️  عن مُنقِذ" : "ℹ️  About Munqith");
            aboutItem.Click += (s, e) => ShowAbout();
            menu.Items.Add(aboutItem);

            menu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem(isArabic ? "❌  خروج" : "❌  Exit");
            exitItem.Click += (s, e) => ExitApp();
            menu.Items.Add(exitItem);

            return menu;
        }

        private Icon LoadAppIcon()
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "app.ico");
                if (File.Exists(iconPath))
                {
                    return new Icon(iconPath);
                }
            }
            catch { }

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

        public void ShowSettings()
        {
            if (_openSettingsForm != null && !_openSettingsForm.IsDisposed)
            {
                if (_openSettingsForm.WindowState == FormWindowState.Minimized)
                    _openSettingsForm.WindowState = FormWindowState.Normal;
                _openSettingsForm.BringToFront();
                _openSettingsForm.Activate();
                NativeMethods.SetForegroundWindow(_openSettingsForm.Handle);
                return;
            }

            _openSettingsForm = new SettingsForm(_settingsService, _statsService, this);
            _openSettingsForm.Icon = LoadAppIcon();
            _openSettingsForm.FormClosed += (s, e) =>
            {
                _openSettingsForm = null;
                RegisterConfiguredHotkeys();
                _trayIcon.ContextMenuStrip = CreateContextMenu();
            };

            _openSettingsForm.Show();
            _openSettingsForm.BringToFront();
            _openSettingsForm.Activate();
            NativeMethods.SetForegroundWindow(_openSettingsForm.Handle);
        }

        private void ShowAbout()
        {
            var form = new AboutForm(_settingsService.Settings.Language, _settingsService.Settings.DarkMode);
            form.Icon = LoadAppIcon();
            form.ShowDialog();
        }

        private void ExitApp()
        {
            _keyboardHook.Stop();
            _keyboardHook.Dispose();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _keyboardHook.Dispose();
                _trayIcon.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
