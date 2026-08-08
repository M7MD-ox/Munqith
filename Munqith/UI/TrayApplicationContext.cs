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
            menu.RenderMode = ToolStripRenderMode.Professional;
            menu.Renderer = new ModernTrayMenuRenderer(_settingsService.Settings.DarkMode);
            menu.Padding = new Padding(4);

            // 1. Settings Item (Default bold)
            var settingsItem = new ToolStripMenuItem("⚙️  لوحة التحكم والإعدادات (Settings)", null, (s, e) => ShowSettings());
            settingsItem.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            menu.Items.Add(settingsItem);

            menu.Items.Add(new ToolStripSeparator());

            // 2. Active Mode status
            var scopeItem = new ToolStripMenuItem("🛡️  الوضع الذكي نشط (Smart Mode Active)", null, (s, e) => ShowSettings());
            menu.Items.Add(scopeItem);

            menu.Items.Add(new ToolStripSeparator());

            // 3. About
            var aboutItem = new ToolStripMenuItem("ℹ️  عن أداة مُنقِذ (About)", null, (s, e) => ShowSettings());
            menu.Items.Add(aboutItem);

            menu.Items.Add(new ToolStripSeparator());

            // 4. Exit
            var exitItem = new ToolStripMenuItem("❌  إغلاق البرنامج بالكامل (Exit)", null, (s, e) => ExitApplication());
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

    /// <summary>
    /// Custom Modern Renderer for System Tray Context Menu
    /// </summary>
    public class ModernTrayMenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly bool _isDark;

        public ModernTrayMenuRenderer(bool isDark) : base(new ModernColorTable(isDark))
        {
            _isDark = isDark;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                var rect = new Rectangle(2, 1, e.Item.Width - 4, e.Item.Height - 2);
                Color hoverBg = _isDark ? Color.FromArgb(48, 54, 70) : Color.FromArgb(240, 235, 230);
                using var brush = new SolidBrush(hoverBg);
                e.Graphics.FillRectangle(brush, rect);
            }
            else
            {
                base.OnRenderMenuItemBackground(e);
            }
        }
    }

    public class ModernColorTable : ProfessionalColorTable
    {
        private readonly bool _isDark;

        public ModernColorTable(bool isDark)
        {
            _isDark = isDark;
        }

        public override Color ToolStripDropDownBackground => _isDark ? Color.FromArgb(28, 32, 44) : Color.FromArgb(253, 248, 243);
        public override Color ImageMarginGradientBegin => ToolStripDropDownBackground;
        public override Color ImageMarginGradientMiddle => ToolStripDropDownBackground;
        public override Color ImageMarginGradientEnd => ToolStripDropDownBackground;
        public override Color MenuBorder => _isDark ? Color.FromArgb(50, 58, 78) : Color.FromArgb(230, 226, 221);
        public override Color SeparatorDark => _isDark ? Color.FromArgb(45, 52, 68) : Color.FromArgb(230, 226, 221);
        public override Color SeparatorLight => Color.Transparent;
        public override Color MenuItemSelected => _isDark ? Color.FromArgb(48, 54, 70) : Color.FromArgb(240, 235, 230);
    }
}
