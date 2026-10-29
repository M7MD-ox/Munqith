using System.Drawing.Drawing2D;
using Munqith.Hooks;
using Munqith.Models;
using Munqith.Services;

namespace Munqith.UI
{
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly ISettingsService _settingsService;
        private readonly IStatsService _statsService;
        private readonly IConversionService _conversionService;
        private readonly IKeyboardHook _keyboardHook;
        private readonly IServiceProvider _serviceProvider;
        private SettingsForm? _settingsForm;

        public TrayApplicationContext(
            ISettingsService settingsService,
            IStatsService statsService,
            IConversionService conversionService,
            IKeyboardHook keyboardHook,
            IServiceProvider serviceProvider)
        {
            _settingsService = settingsService;
            _statsService = statsService;
            _conversionService = conversionService;
            _keyboardHook = keyboardHook;
            _serviceProvider = serviceProvider;

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

            _trayIcon.Text = "مُنقِذ Munqith";
            _trayIcon.Visible = true;

            CreateContextMenu();

            _trayIcon.DoubleClick += (s, e) => ShowSettings();
        }

        public void CreateContextMenu()
        {
            var menu = new ContextMenuStrip
            {
                ShowImageMargin = false,
                ShowCheckMargin = false,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Padding = new Padding(4),
                DropShadowEnabled = true
            };

            bool isDark = _settingsService.Settings.DarkMode;
            bool isAr = _settingsService.Settings.Language == "ar";

            menu.Renderer = new ClaudeTrayMenuRenderer(isDark);

            // 1. Settings Header / Dashboard
            var settingsItem = new ToolStripMenuItem(isAr ? "⚙️  لوحة التحكم والإعدادات" : "⚙️  Dashboard & Settings", null, (s, e) => ShowSettings())
            {
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Padding = new Padding(8, 6, 8, 6)
            };
            menu.Items.Add(settingsItem);

            menu.Items.Add(new CustomMenuSeparator(isDark));

            // 2. Exit
            var exitItem = new ToolStripMenuItem(isAr ? "❌  إغلاق البرنامج" : "❌  Exit Munqith", null, (s, e) => ExitApplication())
            {
                Padding = new Padding(8, 6, 8, 6)
            };
            menu.Items.Add(exitItem);

            _trayIcon.ContextMenuStrip = menu;
        }

        public void RegisterConfiguredHotkeys()
        {
            _keyboardHook.UpdateBindings(_settingsService.Settings.Hotkeys);
        }

        private async void OnHotkeyTriggered(HotkeyBinding binding)
        {
            try
            {
                await _conversionService.PerformConversion();
            }
            catch { }
        }

        public void ShowSettings()
        {
            if (_settingsForm == null || _settingsForm.IsDisposed)
            {
                _settingsForm = new SettingsForm(_serviceProvider);
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

            if (_settingsForm != null && !_settingsForm.IsDisposed)
            {
                _settingsForm.AllowClose = true;
                _settingsForm.Close();
                _settingsForm.Dispose();
            }

            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            Application.Exit();
        }
    }

    /// <summary>
    /// Custom Separator with sleek padding
    /// </summary>
    public class CustomMenuSeparator : ToolStripSeparator
    {
        private readonly bool _isDark;

        public CustomMenuSeparator(bool isDark)
        {
            _isDark = isDark;
            this.Margin = new Padding(4, 4, 4, 4);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color sepColor = _isDark ? Color.FromArgb(50, 48, 44) : Color.FromArgb(230, 226, 221);
            using var pen = new Pen(sepColor, 1);
            int y = this.Height / 2;
            e.Graphics.DrawLine(pen, 8, y, this.Width - 8, y);
        }
    }

    /// <summary>
    /// Ultra-Modern Claude AI Aesthetic Renderer for System Tray Context Menu
    /// </summary>
    public class ClaudeTrayMenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly bool _isDark;
        private readonly Color _bg;
        private readonly Color _border;
        private readonly Color _hoverBg;
        private readonly Color _textPrimary;
        private readonly Color _terracotta = Color.FromArgb(217, 119, 87);

        public ClaudeTrayMenuRenderer(bool isDark) : base(new ClaudeColorTable(isDark))
        {
            _isDark = isDark;
            if (_isDark)
            {
                _bg = Color.FromArgb(32, 30, 28);
                _border = Color.FromArgb(60, 56, 52);
                _hoverBg = Color.FromArgb(48, 44, 40);
                _textPrimary = Color.FromArgb(250, 247, 242);
            }
            else
            {
                _bg = Color.FromArgb(253, 248, 243);
                _border = Color.FromArgb(230, 226, 221);
                _hoverBg = Color.FromArgb(244, 239, 234);
                _textPrimary = Color.FromArgb(31, 31, 31);
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(_bg);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using var pen = new Pen(_border, 1.5f);
            var rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawRectangle(pen, rect);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                var rect = new Rectangle(4, 2, e.Item.Width - 8, e.Item.Height - 4);
                using var path = CreateRoundedPath(rect, 6);
                using var brush = new SolidBrush(_hoverBg);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Selected ? _terracotta : _textPrimary;
            base.OnRenderItemText(e);
        }

        private static GraphicsPath CreateRoundedPath(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class ClaudeColorTable : ProfessionalColorTable
    {
        private readonly bool _isDark;

        public ClaudeColorTable(bool isDark)
        {
            _isDark = isDark;
        }

        public override Color ToolStripDropDownBackground => _isDark ? Color.FromArgb(32, 30, 28) : Color.FromArgb(253, 248, 243);
        public override Color ImageMarginGradientBegin => ToolStripDropDownBackground;
        public override Color ImageMarginGradientMiddle => ToolStripDropDownBackground;
        public override Color ImageMarginGradientEnd => ToolStripDropDownBackground;
        public override Color MenuBorder => _isDark ? Color.FromArgb(60, 56, 52) : Color.FromArgb(230, 226, 221);
        public override Color MenuItemSelected => _isDark ? Color.FromArgb(48, 44, 40) : Color.FromArgb(244, 239, 234);
        public override Color MenuItemBorder => Color.Transparent;
    }
}
