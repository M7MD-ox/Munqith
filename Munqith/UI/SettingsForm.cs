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
        private readonly bool _isDark;
        private readonly bool _isArabic;

        // Theme colors
        private readonly Color _bgColor;
        private readonly Color _sectionColor;
        private readonly Color _textColor;
        private readonly Color _secondaryTextColor;
        private readonly Color _accentColor = Color.FromArgb(0, 120, 212);

        public SettingsForm(SettingsService settingsService, StatsService statsService, TrayApplicationContext trayContext)
        {
            _settingsService = settingsService;
            _statsService = statsService;
            _trayContext = trayContext;
            _isDark = settingsService.Settings.DarkMode;
            _isArabic = settingsService.Settings.Language == "ar";

            // Set theme colors
            if (_isDark)
            {
                _bgColor = Color.FromArgb(30, 30, 30);
                _sectionColor = Color.FromArgb(45, 45, 45);
                _textColor = Color.FromArgb(224, 224, 224);
                _secondaryTextColor = Color.FromArgb(160, 160, 160);
            }
            else
            {
                _bgColor = Color.White;
                _sectionColor = Color.FromArgb(245, 245, 245);
                _textColor = Color.FromArgb(26, 26, 26);
                _secondaryTextColor = Color.FromArgb(100, 100, 100);
            }

            InitializeForm();
            BuildUI();
        }

        private void InitializeForm()
        {
            this.Text = _isArabic ? "مُنقِذ — الإعدادات" : "Munqith — Settings";
            this.Size = new Size(520, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.RightToLeft = _isArabic ? RightToLeft.Yes : RightToLeft.No;
            this.RightToLeftLayout = _isArabic;
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = _bgColor;
        }

        private void BuildUI()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = _bgColor
            };

            int yPos = 10;

            // === HEADER ===
            var headerLabel = new Label
            {
                Text = _isArabic ? "مُنقِذ" : "Munqith",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = _accentColor,
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            mainPanel.Controls.Add(headerLabel);

            var subtitleLabel = new Label
            {
                Text = _isArabic ? "أداة تحويل تخطيط الكيبورد" : "Keyboard Layout Converter",
                Font = new Font("Segoe UI", 9),
                ForeColor = _secondaryTextColor,
                AutoSize = true,
                Location = new Point(22, yPos + 35)
            };
            mainPanel.Controls.Add(subtitleLabel);
            yPos += 60;

            // === STATS SECTION ===
            var statsPanel = CreateSection(yPos, 130);
            BuildStatsSection(statsPanel);
            mainPanel.Controls.Add(statsPanel);
            yPos += 145;

            // === KEYBOARD LAYOUT ===
            var layoutPanel = CreateSection(yPos, 60);
            AddComboSetting(layoutPanel,
                _isArabic ? "تخطيط الكيبورد" : "Keyboard Layout",
                new[] { "Windows Arabic PC", "Mac Arabic" },
                (int)_settingsService.Settings.SelectedLayout,
                idx =>
                {
                    _settingsService.Settings.SelectedLayout = (KeyboardLayout)idx;
                    _settingsService.Save();
                });
            mainPanel.Controls.Add(layoutPanel);
            yPos += 75;

            // === CONVERSION MODE ===
            var modePanel = CreateSection(yPos, 60);
            var modeLabels = _isArabic
                ? new[] { "النص المحدد", "كل النص", "آخر كلمة" }
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
            mainPanel.Controls.Add(modePanel);
            yPos += 75;

            // === SMART DETECTION ===
            yPos = AddToggleSetting(mainPanel, yPos,
                _isArabic ? "الكشف الذكي" : "Smart Detection",
                _isArabic ? "يحافظ على الكلمات الإنجليزية الصحيحة" : "Preserves correctly typed English words",
                _settingsService.Settings.SmartDetectionEnabled,
                val =>
                {
                    _settingsService.Settings.SmartDetectionEnabled = val;
                    _settingsService.Save();
                });

            // === START WITH WINDOWS ===
            yPos = AddToggleSetting(mainPanel, yPos,
                _isArabic ? "بدء مع الويندوز" : "Start with Windows",
                _isArabic ? "تشغيل تلقائي عند بدء الويندوز" : "Auto-start when Windows starts",
                _settingsService.Settings.StartWithWindows,
                val =>
                {
                    _settingsService.Settings.StartWithWindows = val;
                    StartupService.SetStartup(val);
                    _settingsService.Save();
                });

            // === DARK MODE ===
            yPos = AddToggleSetting(mainPanel, yPos,
                _isArabic ? "الوضع الليلي" : "Dark Mode",
                "",
                _settingsService.Settings.DarkMode,
                val =>
                {
                    _settingsService.Settings.DarkMode = val;
                    _settingsService.Save();
                    MessageBox.Show(
                        _isArabic ? "سيتم تطبيق الوضع الليلي عند إعادة فتح الإعدادات"
                                  : "Dark mode will apply when you reopen settings",
                        _isArabic ? "ملاحظة" : "Note",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                });

            // === LANGUAGE ===
            var langPanel = CreateSection(yPos, 60);
            AddComboSetting(langPanel,
                _isArabic ? "اللغة" : "Language",
                new[] { "العربية", "English" },
                _isArabic ? 0 : 1,
                idx =>
                {
                    _settingsService.Settings.Language = idx == 0 ? "ar" : "en";
                    _settingsService.Save();
                    MessageBox.Show(
                        idx == 0 ? "سيتم تطبيق اللغة عند إعادة فتح الإعدادات"
                                 : "Language will apply when you reopen settings",
                        idx == 0 ? "ملاحظة" : "Note",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                });
            mainPanel.Controls.Add(langPanel);
            yPos += 75;

            // === HOTKEYS SECTION ===
            var hotkeyPanel = CreateSection(yPos, 100);
            BuildHotkeysSection(hotkeyPanel);
            mainPanel.Controls.Add(hotkeyPanel);
            yPos += 115;

            // === ENABLE/DISABLE ===
            yPos = AddToggleSetting(mainPanel, yPos,
                _isArabic ? "تشغيل الأداة" : "Enable Tool",
                _isArabic ? "تفعيل/تعطيل التحويل" : "Enable/disable conversion",
                _settingsService.Settings.IsEnabled,
                val =>
                {
                    _settingsService.Settings.IsEnabled = val;
                    _settingsService.Save();
                });

            this.Controls.Add(mainPanel);
        }

        private void BuildStatsSection(Panel panel)
        {
            var titleLabel = new Label
            {
                Text = _isArabic ? "تم إنقاذك" : "You've been saved",
                Font = new Font("Segoe UI", 9),
                ForeColor = _secondaryTextColor,
                AutoSize = true,
                Location = new Point(15, 8)
            };
            panel.Controls.Add(titleLabel);

            // Big conversion count
            var countLabel = new Label
            {
                Text = _statsService.TotalConversions.ToString(),
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = _accentColor,
                AutoSize = true,
                Location = new Point(15, 28)
            };
            panel.Controls.Add(countLabel);

            var timesLabel = new Label
            {
                Text = _isArabic ? "مرة" : "times",
                Font = new Font("Segoe UI", 11),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(countLabel.PreferredWidth + 20, 48)
            };
            panel.Controls.Add(timesLabel);

            // Stats cards row
            int cardY = 85;
            int cardWidth = 140;
            int spacing = 15;
            int startX = 15;

            // Characters card
            AddStatCard(panel, startX, cardY, cardWidth,
                _statsService.TotalCharacters.ToString("N0"),
                _isArabic ? "حرف" : "characters");

            // Words card
            AddStatCard(panel, startX + cardWidth + spacing, cardY, cardWidth,
                _statsService.TotalWords.ToString("N0"),
                _isArabic ? "كلمة" : "words");

            // Time saved card
            var timeDisplay = _isArabic
                ? _statsService.GetTimeSavedDisplayArabic()
                : _statsService.GetTimeSavedDisplay();
            AddStatCard(panel, startX + (cardWidth + spacing) * 2, cardY, cardWidth,
                timeDisplay,
                _isArabic ? "وقت مُوفَّر" : "time saved");
        }

        private void AddStatCard(Panel parent, int x, int y, int width, string value, string label)
        {
            var valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(x, y)
            };
            parent.Controls.Add(valueLabel);

            var descLabel = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 8),
                ForeColor = _secondaryTextColor,
                AutoSize = true,
                Location = new Point(x, y + 22)
            };
            parent.Controls.Add(descLabel);
        }

        private void BuildHotkeysSection(Panel panel)
        {
            var titleLabel = new Label
            {
                Text = _isArabic ? "الاختصارات" : "Hotkeys",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(15, 8)
            };
            panel.Controls.Add(titleLabel);

            int yOffset = 35;
            foreach (var hotkey in _settingsService.Settings.Hotkeys)
            {
                var hotkeyLabel = new Label
                {
                    Text = hotkey.DisplayName,
                    Font = new Font("Segoe UI", 10),
                    ForeColor = _accentColor,
                    AutoSize = true,
                    Location = new Point(30, yOffset)
                };
                panel.Controls.Add(hotkeyLabel);

                var enabledCheck = new CheckBox
                {
                    Checked = hotkey.IsEnabled,
                    Text = _isArabic ? "مفعّل" : "Enabled",
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = _secondaryTextColor,
                    AutoSize = true,
                    Location = new Point(200, yOffset)
                };
                var currentHotkey = hotkey;
                enabledCheck.CheckedChanged += (s, e) =>
                {
                    currentHotkey.IsEnabled = enabledCheck.Checked;
                    _settingsService.Save();
                };
                panel.Controls.Add(enabledCheck);

                yOffset += 28;
            }
        }

        private Panel CreateSection(int y, int height)
        {
            return new Panel
            {
                Location = new Point(20, y),
                Size = new Size(460, height),
                BackColor = _sectionColor,
                Padding = new Padding(10)
            };
        }

        private void AddComboSetting(Panel parent, string label, string[] items, int selected, Action<int> onChange)
        {
            var lbl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 10),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(15, 10)
            };
            parent.Controls.Add(lbl);

            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(200, 30),
                Location = new Point(15, 32),
                BackColor = _isDark ? Color.FromArgb(60, 60, 60) : Color.White,
                ForeColor = _textColor
            };
            combo.Items.AddRange(items);
            if (selected >= 0 && selected < items.Length)
                combo.SelectedIndex = selected;
            combo.SelectedIndexChanged += (s, e) => onChange(combo.SelectedIndex);
            parent.Controls.Add(combo);
        }

        private int AddToggleSetting(Panel parent, int y, string title, string subtitle, bool isChecked, Action<bool> onChange)
        {
            var section = CreateSection(y, string.IsNullOrEmpty(subtitle) ? 45 : 55);

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(15, string.IsNullOrEmpty(subtitle) ? 12 : 6)
            };
            section.Controls.Add(titleLabel);

            if (!string.IsNullOrEmpty(subtitle))
            {
                var subLabel = new Label
                {
                    Text = subtitle,
                    Font = new Font("Segoe UI", 8),
                    ForeColor = _secondaryTextColor,
                    AutoSize = true,
                    Location = new Point(15, 28)
                };
                section.Controls.Add(subLabel);
            }

            var toggle = new CheckBox
            {
                Checked = isChecked,
                AutoSize = true,
                Location = new Point(420, string.IsNullOrEmpty(subtitle) ? 12 : 14)
            };
            toggle.CheckedChanged += (s, e) => onChange(toggle.Checked);
            section.Controls.Add(toggle);

            parent.Controls.Add(section);
            return y + section.Height + 10;
        }
    }
}
