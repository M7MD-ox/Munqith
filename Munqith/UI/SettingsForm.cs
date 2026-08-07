using Munqith.Core;
using Munqith.Models;
using Munqith.Services;

namespace Munqith.UI
{
    public class SettingsForm : Form
    {
        private readonly SettingsService _settingsService;
        private readonly StatsService _statsService;
        private readonly TrayApplicationContext _trayContext;
        
        private bool _isDark;
        private bool _isArabic;

        // Theme colors
        private Color _bgColor;
        private Color _sectionColor;
        private Color _textColor;
        private Color _secondaryTextColor;
        private Color _borderColor;
        private readonly Color _accentColor = Color.FromArgb(0, 120, 212); // Windows 11 Blue

        private Panel _mainPanel = null!;

        public SettingsForm(SettingsService settingsService, StatsService statsService, TrayApplicationContext trayContext)
        {
            _settingsService = settingsService;
            _statsService = statsService;
            _trayContext = trayContext;
            
            _isDark = settingsService.Settings.DarkMode;
            _isArabic = settingsService.Settings.Language == "ar";

            InitializeForm();
            BuildUI();
        }

        private void InitializeForm()
        {
            this.Text = _isArabic ? "مُنقِذ — الإعدادات" : "Munqith — Settings";
            this.Size = new Size(530, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.RightToLeft = _isArabic ? RightToLeft.Yes : RightToLeft.No;
            this.RightToLeftLayout = _isArabic;
            this.Font = new Font("Segoe UI", 9.5f);
            
            UpdateThemeTokens();
            this.BackColor = _bgColor;
        }

        private void UpdateThemeTokens()
        {
            if (_isDark)
            {
                _bgColor = Color.FromArgb(32, 32, 32);           // Dark background
                _sectionColor = Color.FromArgb(44, 44, 44);       // Dark card surface
                _textColor = Color.FromArgb(243, 243, 243);       // Primary text
                _secondaryTextColor = Color.FromArgb(160, 160, 160); // Secondary text
                _borderColor = Color.FromArgb(58, 58, 58);       // Subtle border
            }
            else
            {
                _bgColor = Color.FromArgb(249, 249, 249);         // Light background
                _sectionColor = Color.White;                       // White card surface
                _textColor = Color.FromArgb(26, 26, 26);         // Primary text
                _secondaryTextColor = Color.FromArgb(110, 110, 110); // Secondary text
                _borderColor = Color.FromArgb(229, 229, 229);     // Light border
            }
        }

        private void BuildUI()
        {
            this.Controls.Clear();

            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = _bgColor
            };

            int yPos = 12;

            // === HEADER WITH LOGO / TITLE ===
            var headerPanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(470, 50),
                BackColor = Color.Transparent
            };

            var headerLabel = new Label
            {
                Text = _isArabic ? "🛡️  مُنقِذ Munqith" : "🛡️  Munqith",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = _accentColor,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            headerPanel.Controls.Add(headerLabel);

            var subtitleLabel = new Label
            {
                Text = _isArabic ? "أداة تصحيح لغة الكتابة في ويندوز" : "Windows Keyboard Layout Auto-Fixer",
                Font = new Font("Segoe UI", 9f),
                ForeColor = _secondaryTextColor,
                AutoSize = true,
                Location = new Point(4, 30)
            };
            headerPanel.Controls.Add(subtitleLabel);

            _mainPanel.Controls.Add(headerPanel);
            yPos += 62;

            // === STATS DASHBOARD ===
            var statsPanel = CreateSection(yPos, 135);
            BuildStatsSection(statsPanel);
            _mainPanel.Controls.Add(statsPanel);
            yPos += 148;

            // === KEYBOARD LAYOUT ===
            var layoutPanel = CreateSection(yPos, 62);
            AddComboSetting(layoutPanel,
                _isArabic ? "تخطيط الكيبورد" : "Keyboard Layout",
                new[] { "Windows Arabic PC", "Mac Arabic" },
                (int)_settingsService.Settings.SelectedLayout,
                idx =>
                {
                    _settingsService.Settings.SelectedLayout = (KeyboardLayout)idx;
                    _settingsService.Save();
                });
            _mainPanel.Controls.Add(layoutPanel);
            yPos += 74;

            // === CONVERSION MODE ===
            var modePanel = CreateSection(yPos, 62);
            var modeLabels = _isArabic
                ? new[] { "النص المحدد (Selected Text)", "كل النص (All Text)", "آخر كلمة (Last Word)" }
                : new[] { "Selected Text", "All Text", "Last Word" };
            AddComboSetting(modePanel,
                _isArabic ? "وضع التحويل" : "Conversion Mode",
                modeLabels,
                (int)_settingsService.Settings.ConversionMode,
                idx =>
                {
                    _settingsService.Settings.ConversionMode = (ConversionMode)idx;
                    _settingsService.Save();
                });
            _mainPanel.Controls.Add(modePanel);
            yPos += 74;

            // === SMART DETECTION ===
            yPos = AddToggleSetting(_mainPanel, yPos,
                _isArabic ? "الكشف الذكي (Smart Detection)" : "Smart Detection",
                _isArabic ? "تحويل الكلام الخطأ فقط والحفاظ على الكلمات الصحيحة" : "Preserves real English words and only converts typos",
                _settingsService.Settings.SmartDetectionEnabled,
                val =>
                {
                    _settingsService.Settings.SmartDetectionEnabled = val;
                    _settingsService.Save();
                });

            // === START WITH WINDOWS ===
            yPos = AddToggleSetting(_mainPanel, yPos,
                _isArabic ? "بدء التشغيل مع الويندوز" : "Start with Windows",
                _isArabic ? "تشغيل الأداة تلقائياً عند فتح الكمبيوتر" : "Auto-start when Windows launches",
                _settingsService.Settings.StartWithWindows,
                val =>
                {
                    _settingsService.Settings.StartWithWindows = val;
                    StartupService.SetStartup(val);
                    _settingsService.Save();
                });

            // === DARK MODE (LIVE TOGGLE!) ===
            yPos = AddToggleSetting(_mainPanel, yPos,
                _isArabic ? "الوضع الليلي (Dark Mode)" : "Dark Mode",
                _isArabic ? "تغيير مظهر الشاشة فوراً" : "Toggle dark/light theme instantly",
                _settingsService.Settings.DarkMode,
                val =>
                {
                    _settingsService.Settings.DarkMode = val;
                    _settingsService.Save();
                    _isDark = val;
                    ApplyLiveTheme();
                });

            // === LANGUAGE (LIVE TOGGLE!) ===
            var langPanel = CreateSection(yPos, 62);
            AddComboSetting(langPanel,
                _isArabic ? "لغة الواجهة (Language)" : "Interface Language",
                new[] { "العربية", "English" },
                _isArabic ? 0 : 1,
                idx =>
                {
                    string newLang = idx == 0 ? "ar" : "en";
                    if (_settingsService.Settings.Language != newLang)
                    {
                        _settingsService.Settings.Language = newLang;
                        _settingsService.Save();
                        _isArabic = newLang == "ar";
                        this.RightToLeft = _isArabic ? RightToLeft.Yes : RightToLeft.No;
                        this.RightToLeftLayout = _isArabic;
                        this.Text = _isArabic ? "مُنقِذ — الإعدادات" : "Munqith — Settings";
                        BuildUI(); // Rebuild UI layout for RTL/LTR
                    }
                });
            _mainPanel.Controls.Add(langPanel);
            yPos += 74;

            // === HOTKEYS SECTION ===
            var hotkeyPanel = CreateSection(yPos, 110);
            BuildHotkeysSection(hotkeyPanel);
            _mainPanel.Controls.Add(hotkeyPanel);
            yPos += 122;

            // === ENABLE/DISABLE TOOL ===
            yPos = AddToggleSetting(_mainPanel, yPos,
                _isArabic ? "تشغيل الأداة" : "Enable Tool",
                _isArabic ? "تفعيل أو تجميد تحويل الاختصارات" : "Enable/pause global hotkeys",
                _settingsService.Settings.IsEnabled,
                val =>
                {
                    _settingsService.Settings.IsEnabled = val;
                    _settingsService.Save();
                });

            this.Controls.Add(_mainPanel);
        }

        private void ApplyLiveTheme()
        {
            UpdateThemeTokens();
            this.BackColor = _bgColor;
            _mainPanel.BackColor = _bgColor;
            BuildUI(); // Instant rebuild with new color tokens
        }

        private void BuildStatsSection(Panel panel)
        {
            var titleLabel = new Label
            {
                Text = _isArabic ? "تم إنقاذك بإجمالي" : "Rescue Stats Summary",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = _secondaryTextColor,
                AutoSize = true,
                Location = new Point(15, 10)
            };
            panel.Controls.Add(titleLabel);

            var countLabel = new Label
            {
                Text = _statsService.TotalConversions.ToString("N0"),
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = _accentColor,
                AutoSize = true,
                Location = new Point(12, 28)
            };
            panel.Controls.Add(countLabel);

            var timesLabel = new Label
            {
                Text = _isArabic ? "مرة حتى الآن!" : "times saved!",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(countLabel.PreferredWidth + 18, 46)
            };
            panel.Controls.Add(timesLabel);

            // Row of metric cards
            int cardY = 82;
            int cardWidth = 142;
            int spacing = 10;
            int startX = 12;

            AddStatMiniCard(panel, startX, cardY, cardWidth,
                _statsService.TotalCharacters.ToString("N0"),
                _isArabic ? "حرف تم تحويله" : "Chars Converted");

            AddStatMiniCard(panel, startX + cardWidth + spacing, cardY, cardWidth,
                _statsService.TotalWords.ToString("N0"),
                _isArabic ? "كلمة مُنقَذة" : "Words Fixed");

            var timeDisplay = _isArabic
                ? _statsService.GetTimeSavedDisplayArabic()
                : _statsService.GetTimeSavedDisplay();

            AddStatMiniCard(panel, startX + (cardWidth + spacing) * 2, cardY, cardWidth,
                timeDisplay,
                _isArabic ? "وقت مُوفَّر" : "Time Saved");
        }

        private void AddStatMiniCard(Panel parent, int x, int y, int width, string value, string label)
        {
            var card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, 42),
                BackColor = _isDark ? Color.FromArgb(52, 52, 52) : Color.FromArgb(242, 244, 247),
                Padding = new Padding(6)
            };

            var valLbl = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(6, 2)
            };
            card.Controls.Add(valLbl);

            var descLbl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 7.8f),
                ForeColor = _secondaryTextColor,
                AutoSize = true,
                Location = new Point(6, 22)
            };
            card.Controls.Add(descLbl);

            parent.Controls.Add(card);
        }

        private void BuildHotkeysSection(Panel panel)
        {
            var titleLabel = new Label
            {
                Text = _isArabic ? "اختصارات المفاتيح" : "Global Hotkeys",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(15, 10)
            };
            panel.Controls.Add(titleLabel);

            int yOffset = 38;
            foreach (var hotkey in _settingsService.Settings.Hotkeys)
            {
                var badgeLabel = new Label
                {
                    Text = $"  {hotkey.DisplayName}  ",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = _accentColor,
                    AutoSize = true,
                    Location = new Point(20, yOffset)
                };
                panel.Controls.Add(badgeLabel);

                var enabledCheck = new CheckBox
                {
                    Checked = hotkey.IsEnabled,
                    Text = _isArabic ? "مفعّل" : "Active",
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = _textColor,
                    AutoSize = true,
                    Location = new Point(220, yOffset + 2)
                };
                var currentHotkey = hotkey;
                enabledCheck.CheckedChanged += (s, e) =>
                {
                    currentHotkey.IsEnabled = enabledCheck.Checked;
                    _settingsService.Save();
                };
                panel.Controls.Add(enabledCheck);

                yOffset += 32;
            }
        }

        private Panel CreateSection(int y, int height)
        {
            var panel = new Panel
            {
                Location = new Point(20, y),
                Size = new Size(470, height),
                BackColor = _sectionColor,
                Padding = new Padding(10)
            };

            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(_borderColor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            return panel;
        }

        private void AddComboSetting(Panel parent, string label, string[] items, int selected, Action<int> onChange)
        {
            var lbl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(15, 10)
            };
            parent.Controls.Add(lbl);

            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(220, 30),
                Location = new Point(15, 30),
                BackColor = _isDark ? Color.FromArgb(55, 55, 55) : Color.White,
                ForeColor = _textColor,
                FlatStyle = FlatStyle.Flat
            };
            combo.Items.AddRange(items);
            if (selected >= 0 && selected < items.Length)
                combo.SelectedIndex = selected;
            combo.SelectedIndexChanged += (s, e) => onChange(combo.SelectedIndex);
            parent.Controls.Add(combo);
        }

        private int AddToggleSetting(Panel parent, int y, string title, string subtitle, bool isChecked, Action<bool> onChange)
        {
            int height = string.IsNullOrEmpty(subtitle) ? 48 : 58;
            var section = CreateSection(y, height);

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(15, string.IsNullOrEmpty(subtitle) ? 14 : 8)
            };
            section.Controls.Add(titleLabel);

            if (!string.IsNullOrEmpty(subtitle))
            {
                var subLabel = new Label
                {
                    Text = subtitle,
                    Font = new Font("Segoe UI", 8.3f),
                    ForeColor = _secondaryTextColor,
                    AutoSize = true,
                    Location = new Point(15, 30)
                };
                section.Controls.Add(subLabel);
            }

            var toggle = new CheckBox
            {
                Checked = isChecked,
                AutoSize = true,
                Location = new Point(430, string.IsNullOrEmpty(subtitle) ? 14 : 16)
            };
            toggle.CheckedChanged += (s, e) => onChange(toggle.Checked);
            section.Controls.Add(toggle);

            parent.Controls.Add(section);
            return y + height + 12;
        }
    }
}
