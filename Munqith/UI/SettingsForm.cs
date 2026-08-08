using System.Drawing.Drawing2D;
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
        private Color _surfaceColor;
        private Color _cardColor;
        private Color _textColor;
        private Color _subTextColor;
        private Color _borderColor;
        private readonly Color _accentColor = Color.FromArgb(0, 120, 212);

        // Tab state
        private int _selectedTabIndex = 0;
        private Panel _headerPanel = null!;
        private Panel _navTabBar = null!;
        private Panel _contentPanel = null!;

        // Stats Labels for dynamic update
        private Label? _heroCountLabel;
        private Label? _heroCharsLabel;
        private Label? _heroWordsLabel;
        private Label? _heroTimeLabel;

        // Hotkey Recorder state
        private bool _isRecordingHotkey = false;
        private int _recordedModifiers = 0;
        private int _recordedKey = 0;
        private Button? _recordButton;

        public SettingsForm(SettingsService settingsService, StatsService statsService, TrayApplicationContext trayContext)
        {
            _settingsService = settingsService;
            _statsService = statsService;
            _trayContext = trayContext;

            _isDark = _settingsService.Settings.DarkMode;
            _isArabic = _settingsService.Settings.Language == "ar";

            InitializeForm();
            BuildFullUI();
        }

        private void InitializeForm()
        {
            this.Text = _isArabic ? "مُنقِذ — لوحة التحكم والإعدادات" : "Munqith — Dashboard & Settings";
            this.Size = new Size(560, 720);
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
                _bgColor = Color.FromArgb(24, 24, 24);
                _surfaceColor = Color.FromArgb(34, 34, 34);
                _cardColor = Color.FromArgb(44, 44, 44);
                _textColor = Color.FromArgb(245, 245, 245);
                _subTextColor = Color.FromArgb(165, 165, 165);
                _borderColor = Color.FromArgb(60, 60, 60);
            }
            else
            {
                _bgColor = Color.FromArgb(245, 247, 250);
                _surfaceColor = Color.White;
                _cardColor = Color.FromArgb(255, 255, 255);
                _textColor = Color.FromArgb(25, 25, 25);
                _subTextColor = Color.FromArgb(115, 115, 115);
                _borderColor = Color.FromArgb(228, 231, 236);
            }
        }

        private void BuildFullUI()
        {
            this.Controls.Clear();

            _headerPanel = CreateTopHeader();
            this.Controls.Add(_headerPanel);

            _navTabBar = CreateNavTabBar();
            this.Controls.Add(_navTabBar);

            _contentPanel = new Panel
            {
                Location = new Point(0, 115),
                Size = new Size(545, 560),
                AutoScroll = true,
                BackColor = _bgColor,
                Padding = new Padding(20)
            };
            this.Controls.Add(_contentPanel);

            RenderActiveTabContent();
        }

        private Panel CreateTopHeader()
        {
            var p = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(545, 65),
                BackColor = _surfaceColor,
                Padding = new Padding(15, 10, 15, 10)
            };

            var titleLbl = new Label
            {
                Text = _isArabic ? "🛡️ مُنقِذ Munqith" : "🛡️ Munqith",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = _accentColor,
                AutoSize = true,
                Location = new Point(15, 14)
            };
            p.Controls.Add(titleLbl);

            bool active = _settingsService.Settings.IsEnabled;
            var statusBadge = new Label
            {
                Text = active ? (_isArabic ? " ● نشط " : " ● Active ") : (_isArabic ? " ○ موقوف " : " ○ Paused "),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = active ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40),
                AutoSize = true,
                Location = new Point(185, 20)
            };
            p.Controls.Add(statusBadge);

            var themeBtn = new Button
            {
                Text = _isDark ? "☀️ Light" : "🌙 Dark",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(75, 30),
                Location = new Point(370, 16),
                FlatStyle = FlatStyle.Flat,
                BackColor = _cardColor,
                ForeColor = _textColor,
                Cursor = Cursors.Hand
            };
            themeBtn.FlatAppearance.BorderColor = _borderColor;
            themeBtn.Click += (s, e) =>
            {
                _isDark = !_isDark;
                _settingsService.Settings.DarkMode = _isDark;
                _settingsService.Save();
                UpdateThemeTokens();
                this.BackColor = _bgColor;
                BuildFullUI();
            };
            p.Controls.Add(themeBtn);

            var langBtn = new Button
            {
                Text = _isArabic ? "EN" : "عربي",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(60, 30),
                Location = new Point(455, 16),
                FlatStyle = FlatStyle.Flat,
                BackColor = _accentColor,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            langBtn.FlatAppearance.BorderSize = 0;
            langBtn.Click += (s, e) =>
            {
                string newLang = _isArabic ? "en" : "ar";
                _settingsService.Settings.Language = newLang;
                _settingsService.Save();
                _isArabic = newLang == "ar";
                this.RightToLeft = _isArabic ? RightToLeft.Yes : RightToLeft.No;
                this.RightToLeftLayout = _isArabic;
                this.Text = _isArabic ? "مُنقِذ — لوحة التحكم والإعدادات" : "Munqith — Dashboard & Settings";
                BuildFullUI();
            };
            p.Controls.Add(langBtn);

            return p;
        }

        private Panel CreateNavTabBar()
        {
            var p = new Panel
            {
                Location = new Point(0, 65),
                Size = new Size(545, 50),
                BackColor = _surfaceColor
            };

            string[] tabs = _isArabic
                ? new[] { "📊 الإحصائيات", "⚙️ الإعدادات", "⌨️ الاختصارات", "ℹ️ عن الأداة" }
                : new[] { "📊 Dashboard", "⚙️ Settings", "⌨️ Hotkeys", "ℹ️ About" };

            int btnWidth = 130;
            for (int i = 0; i < tabs.Length; i++)
            {
                int index = i;
                var btn = new Button
                {
                    Text = tabs[i],
                    Font = new Font("Segoe UI", 9.5f, index == _selectedTabIndex ? FontStyle.Bold : FontStyle.Regular),
                    Size = new Size(btnWidth, 42),
                    Location = new Point(12 + i * (btnWidth + 4), 4),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = index == _selectedTabIndex ? _accentColor : _surfaceColor,
                    ForeColor = index == _selectedTabIndex ? Color.White : _subTextColor,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) =>
                {
                    _selectedTabIndex = index;
                    BuildFullUI();
                };
                p.Controls.Add(btn);
            }

            return p;
        }

        private void RenderActiveTabContent()
        {
            _contentPanel.Controls.Clear();

            switch (_selectedTabIndex)
            {
                case 0:
                    BuildDashboardTab(_contentPanel);
                    break;
                case 1:
                    BuildSettingsTab(_contentPanel);
                    break;
                case 2:
                    BuildHotkeysTab(_contentPanel);
                    break;
                case 3:
                    BuildAboutTab(_contentPanel);
                    break;
            }
        }

        // ==========================================
        // TAB 1: DASHBOARD & STATS
        // ==========================================
        private void BuildDashboardTab(Panel container)
        {
            int yPos = 10;

            // Rescue Hero Card
            var heroCard = CreateCard(yPos, 140);

            var heroTitle = new Label
            {
                Text = _isArabic ? "تم إنقاذك من إعادة الكتابة" : "Rescued Typing Counter",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = _subTextColor,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            heroCard.Controls.Add(heroTitle);

            _heroCountLabel = new Label
            {
                Text = _statsService.TotalConversions.ToString("N0"),
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = _accentColor,
                AutoSize = true,
                Location = new Point(15, 32)
            };
            heroCard.Controls.Add(_heroCountLabel);

            var heroUnit = new Label
            {
                Text = _isArabic ? "مرة حتى الآن 🚀" : "times total 🚀",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(_heroCountLabel.PreferredWidth + 22, 54)
            };
            heroCard.Controls.Add(heroUnit);

            int cardW = 150;
            int cardH = 45;
            int yMetrics = 85;

            _heroCharsLabel = AddMetricTile(heroCard, 15, yMetrics, cardW, cardH,
                _statsService.TotalCharacters.ToString("N0"),
                _isArabic ? "حرف تم تصحيحه" : "Chars Fixed");

            _heroWordsLabel = AddMetricTile(heroCard, 175, yMetrics, cardW, cardH,
                _statsService.TotalWords.ToString("N0"),
                _isArabic ? "كلمة مُنقَذة" : "Words Saved");

            string timeDisplayStr = _isArabic ? _statsService.GetTimeSavedDisplayArabic() : _statsService.GetTimeSavedDisplay();
            _heroTimeLabel = AddMetricTile(heroCard, 335, yMetrics, cardW, cardH,
                timeDisplayStr,
                _isArabic ? "وقت مُوفَّر" : "Time Saved");

            container.Controls.Add(heroCard);
            yPos += 155;

            // Live Test Playground Card
            var testCard = CreateCard(yPos, 180);
            var testTitle = new Label
            {
                Text = _isArabic ? "🧪 منطقة تجربة الأداة الفورية" : "🧪 Live Instant Converter Test",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            testCard.Controls.Add(testTitle);

            var testDesc = new Label
            {
                Text = _isArabic
                    ? "اكتب أي نص إنجليزي أو عربي لخبطة هنا واضغط الزر لتجربة التحويل والتقليب فوراً:"
                    : "Type garbled text below and click button to test bidirectional conversion instantly:",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = _subTextColor,
                AutoSize = true,
                Location = new Point(15, 36)
            };
            testCard.Controls.Add(testDesc);

            var testBox = new TextBox
            {
                Text = "hgf,hfm ,hpim",
                Font = new Font("Segoe UI", 11),
                Size = new Size(465, 35),
                Location = new Point(15, 62),
                BackColor = _isDark ? Color.FromArgb(55, 55, 55) : Color.FromArgb(248, 249, 250),
                ForeColor = _textColor
            };
            testCard.Controls.Add(testBox);

            var manualTestBtn = new Button
            {
                Text = _isArabic ? "🔄 شقلب النص الآن" : "🔄 Convert Text Now",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(200, 36),
                Location = new Point(15, 115),
                FlatStyle = FlatStyle.Flat,
                BackColor = _accentColor,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            manualTestBtn.FlatAppearance.BorderSize = 0;
            manualTestBtn.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(testBox.Text))
                {
                    string converted = LayoutConverter.Convert(testBox.Text, _settingsService.Settings.SelectedLayout);
                    testBox.Text = converted;
                    var res = new ConversionResult
                    {
                        WasConverted = true,
                        CharacterCount = converted.Length,
                        WordCount = converted.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                    };
                    _statsService.RecordConversion(res);

                    if (_heroCountLabel != null) _heroCountLabel.Text = _statsService.TotalConversions.ToString("N0");
                    if (_heroCharsLabel != null) _heroCharsLabel.Text = _statsService.TotalCharacters.ToString("N0");
                    if (_heroWordsLabel != null) _heroWordsLabel.Text = _statsService.TotalWords.ToString("N0");
                    if (_heroTimeLabel != null) _heroTimeLabel.Text = _isArabic ? _statsService.GetTimeSavedDisplayArabic() : _statsService.GetTimeSavedDisplay();
                }
            };
            testCard.Controls.Add(manualTestBtn);

            container.Controls.Add(testCard);
        }

        private Label AddMetricTile(Panel parent, int x, int y, int w, int h, string val, string label)
        {
            var tile = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = _isDark ? Color.FromArgb(55, 55, 55) : Color.FromArgb(240, 243, 246)
            };

            var vLbl = new Label
            {
                Text = val,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(6, 4)
            };
            tile.Controls.Add(vLbl);

            var dLbl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 7.8f),
                ForeColor = _subTextColor,
                AutoSize = true,
                Location = new Point(6, 22)
            };
            tile.Controls.Add(dLbl);

            parent.Controls.Add(tile);
            return vLbl;
        }

        // ==========================================
        // TAB 2: SETTINGS & LAYOUTS
        // ==========================================
        private void BuildSettingsTab(Panel container)
        {
            int yPos = 10;

            var layoutCard = CreateCard(yPos, 65);
            AddComboSetting(layoutCard,
                _isArabic ? "⌨️ تخطيط لغة الكيبورد" : "⌨️ Keyboard Layout",
                new[] { "Windows Arabic PC (التخطيط القياسي)", "Mac Arabic (تخطيط الماك)" },
                (int)_settingsService.Settings.SelectedLayout,
                idx =>
                {
                    _settingsService.Settings.SelectedLayout = (KeyboardLayout)idx;
                    _settingsService.Save();
                });
            container.Controls.Add(layoutCard);
            yPos += 78;

            var modeCard = CreateCard(yPos, 65);
            var modes = _isArabic
                ? new[] { "النص المحدد (مع التحويل التلقائي الذكي)", "السطر أو الجملة الحالية كاملة", "آخر كلمة فقط قبل المؤشر" }
                : new[] { "Selected Text (with auto fallback)", "Entire Current Line / Sentence", "Last Word Only" };
            AddComboSetting(modeCard,
                _isArabic ? "🎯 وضع ونطاق التحويل" : "🎯 Conversion Scope Mode",
                modes,
                (int)_settingsService.Settings.ConversionMode,
                idx =>
                {
                    _settingsService.Settings.ConversionMode = (ConversionMode)idx;
                    _settingsService.Save();
                });
            container.Controls.Add(modeCard);
            yPos += 78;

            yPos = AddToggleSetting(container, yPos,
                _isArabic ? "🧠 الكشف الذكي (Smart Detection)" : "🧠 Smart Detection",
                _isArabic ? "الحفاظ على الكلمات الإنجليزية الصحيحة وتصحيح الكلمات الخطأ فقط" : "Preserves real English words and only converts typos",
                _settingsService.Settings.SmartDetectionEnabled,
                val =>
                {
                    _settingsService.Settings.SmartDetectionEnabled = val;
                    _settingsService.Save();
                });

            yPos = AddToggleSetting(container, yPos,
                _isArabic ? "🚀 التشغيل التلقائي مع الويندوز" : "🚀 Launch with Windows Startup",
                _isArabic ? "بدء تشغيل مُنقِذ تلقائياً عند فتح الكمبيوتر" : "Automatically runs Munqith when Windows boots up",
                _settingsService.Settings.StartWithWindows,
                val =>
                {
                    _settingsService.Settings.StartWithWindows = val;
                    StartupService.SetStartup(val);
                    _settingsService.Save();
                });

            yPos = AddToggleSetting(container, yPos,
                _isArabic ? "⚡ تفعيل الأداة" : "⚡ Enable Utility",
                _isArabic ? "تفعيل أو إيقاف استجابة الأداة للاختصارات" : "Master toggle to pause/enable hotkey conversion",
                _settingsService.Settings.IsEnabled,
                val =>
                {
                    _settingsService.Settings.IsEnabled = val;
                    _settingsService.Save();
                    _trayContext.CreateContextMenu();
                });
        }

        // ==========================================
        // TAB 3: HOTKEYS & INTERACTIVE RECORDER
        // ==========================================
        private void BuildHotkeysTab(Panel container)
        {
            int yPos = 10;

            var listCard = CreateCard(yPos, 200);
            var listTitle = new Label
            {
                Text = _isArabic ? "⌨️ الاختصارات الحالية المفعلة" : "⌨️ Active Global Hotkeys",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            listCard.Controls.Add(listTitle);

            int yOffset = 42;
            for (int i = 0; i < _settingsService.Settings.Hotkeys.Count; i++)
            {
                int index = i;
                var hk = _settingsService.Settings.Hotkeys[i];

                var badge = new Label
                {
                    Text = $"  {hk.DisplayName}  ",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = _accentColor,
                    AutoSize = true,
                    Location = new Point(20, yOffset)
                };
                listCard.Controls.Add(badge);

                var chk = new CheckBox
                {
                    Checked = hk.IsEnabled,
                    Text = _isArabic ? "مفعّل" : "Active",
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = _textColor,
                    AutoSize = true,
                    Location = new Point(200, yOffset + 2)
                };
                chk.CheckedChanged += (s, e) =>
                {
                    hk.IsEnabled = chk.Checked;
                    _settingsService.Save();
                    _trayContext.RegisterConfiguredHotkeys();
                };
                listCard.Controls.Add(chk);

                var delBtn = new Button
                {
                    Text = _isArabic ? "حذف" : "Delete",
                    Font = new Font("Segoe UI", 8f),
                    Size = new Size(55, 24),
                    Location = new Point(410, yOffset),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(198, 40, 40),
                    ForeColor = Color.White
                };
                delBtn.FlatAppearance.BorderSize = 0;
                delBtn.Click += (s, e) =>
                {
                    _settingsService.Settings.Hotkeys.RemoveAt(index);
                    _settingsService.Save();
                    _trayContext.RegisterConfiguredHotkeys();
                    BuildFullUI();
                };
                listCard.Controls.Add(delBtn);

                yOffset += 35;
            }

            container.Controls.Add(listCard);
            yPos += 215;

            var recCard = CreateCard(yPos, 170);
            var recTitle = new Label
            {
                Text = _isArabic ? "🎙️ تسجيل اختصار جديد تفاعلي" : "🎙️ Interactive Hotkey Recorder",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            recCard.Controls.Add(recTitle);

            var recDesc = new Label
            {
                Text = _isArabic
                    ? "اضغط على الزر أدناه ثم اضغط على أي مجموعة مفاتيح في الكيبورد لإنشاء اختصار جديد:"
                    : "Click button below and press any key combination on keyboard:",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = _subTextColor,
                AutoSize = true,
                Location = new Point(15, 36)
            };
            recCard.Controls.Add(recDesc);

            _recordButton = new Button
            {
                Text = _isArabic ? "اضغط هنا لبدء تسجيل الاختصار..." : "Click here to record shortcut...",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(465, 42),
                Location = new Point(15, 62),
                FlatStyle = FlatStyle.Flat,
                BackColor = _isDark ? Color.FromArgb(55, 55, 55) : Color.FromArgb(238, 242, 246),
                ForeColor = _accentColor,
                Cursor = Cursors.Hand
            };
            _recordButton.FlatAppearance.BorderColor = _accentColor;
            _recordButton.FlatAppearance.BorderSize = 2;

            _recordButton.Click += (s, e) =>
            {
                _isRecordingHotkey = true;
                _recordedModifiers = 0;
                _recordedKey = 0;
                _recordButton.Text = _isArabic ? "⏳ جاري الاستماع... اضغط مفاتيحك الآن!" : "⏳ Listening... Press your keys now!";
                _recordButton.BackColor = Color.FromArgb(255, 236, 179);
                _recordButton.ForeColor = Color.Black;
                _recordButton.Focus();
            };

            _recordButton.KeyDown += (s, e) =>
            {
                if (!_isRecordingHotkey) return;

                e.SuppressKeyPress = true;

                int mods = 0;
                if (e.Alt) mods |= 1;
                if (e.Control) mods |= 2;
                if (e.Shift) mods |= 4;

                if (e.KeyCode != Keys.ControlKey && e.KeyCode != Keys.ShiftKey && e.KeyCode != Keys.Menu && e.KeyCode != Keys.None)
                {
                    _recordedModifiers = mods;
                    _recordedKey = (int)e.KeyCode;
                    _isRecordingHotkey = false;

                    var newBinding = new HotkeyBinding
                    {
                        ModifierKeysValue = _recordedModifiers,
                        KeyValue = _recordedKey,
                        IsEnabled = true
                    };

                    _recordButton.Text = $"✅ تم تسجيل الاختصار: {newBinding.DisplayName}";
                    _recordButton.BackColor = Color.FromArgb(232, 245, 233);
                    _recordButton.ForeColor = Color.FromArgb(46, 125, 50);

                    _settingsService.Settings.Hotkeys.Add(newBinding);
                    _settingsService.Save();
                    _trayContext.RegisterConfiguredHotkeys();

                    Task.Delay(1000).ContinueWith(_ =>
                    {
                        if (this.IsHandleCreated)
                        {
                            this.Invoke(() => BuildFullUI());
                        }
                    });
                }
            };

            recCard.Controls.Add(_recordButton);
            container.Controls.Add(recCard);
        }

        // ==========================================
        // TAB 4: ABOUT & INFO
        // ==========================================
        private void BuildAboutTab(Panel container)
        {
            int yPos = 10;
            var aboutCard = CreateCard(yPos, 320);

            var title = new Label
            {
                Text = "🛡️ مُنقِذ Munqith v1.0.0",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = _accentColor,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            aboutCard.Controls.Add(title);

            var desc = new Label
            {
                Text = _isArabic
                    ? "أداة ويندوز سريعة وخفيفة جداً تصحح لغة الكتابة تلقائياً في جميع برامج الجهاز بضغطة اختصار واحدة.\n\n" +
                      "مستوحاة من فكرة إضافة 'شقلب' لمتصفح كروم، وتعمل كبرنامج مستقل على مستوى نظام ويندوز بالكامل."
                    : "Fast, lightweight Windows utility that auto-corrects text typed with the wrong keyboard layout across all Windows apps with a single hotkey.\n\n" +
                      "Inspired by Sha2lab Chrome extension, built as a system-wide native Windows app.",
                Font = new Font("Segoe UI", 9.8f),
                ForeColor = _textColor,
                AutoSize = true,
                MaximumSize = new Size(450, 0),
                Location = new Point(20, 65)
            };
            aboutCard.Controls.Add(desc);

            var techInfo = new Label
            {
                Text = _isArabic
                    ? "• التقنية: C# .NET 8.0 (LTS)\n" +
                      "• طريقة الالتقاط: Low-Level System Hook (WH_KEYBOARD_LL)\n" +
                      "• الوضع: Self-contained Executable (~67 MB)"
                    : "• Tech Stack: C# .NET 8.0 (LTS)\n" +
                      "• Hook Mechanism: Low-Level System Hook (WH_KEYBOARD_LL)\n" +
                      "• Distribution: Self-contained Executable (~67 MB)",
                Font = new Font("Segoe UI", 8.8f),
                ForeColor = _subTextColor,
                AutoSize = true,
                Location = new Point(20, 180)
            };
            aboutCard.Controls.Add(techInfo);

            var githubBtn = new Button
            {
                Text = "🌐 GitHub Repository",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(180, 38),
                Location = new Point(20, 255),
                FlatStyle = FlatStyle.Flat,
                BackColor = _accentColor,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            githubBtn.FlatAppearance.BorderSize = 0;
            githubBtn.Click += (s, e) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://github.com") { UseShellExecute = true });
                }
                catch { }
            };
            aboutCard.Controls.Add(githubBtn);

            container.Controls.Add(aboutCard);
        }

        // ==========================================
        // HELPERS & UI COMPONENTS
        // ==========================================
        private Panel CreateCard(int y, int height)
        {
            var p = new Panel
            {
                Location = new Point(15, y),
                Size = new Size(495, height),
                BackColor = _cardColor,
                Padding = new Padding(12)
            };

            p.Paint += (s, e) =>
            {
                using var pen = new Pen(_borderColor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };

            return p;
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
                Size = new Size(260, 30),
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
            int height = string.IsNullOrEmpty(subtitle) ? 50 : 62;
            var card = CreateCard(y, height);

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = _textColor,
                AutoSize = true,
                Location = new Point(15, string.IsNullOrEmpty(subtitle) ? 14 : 8)
            };
            card.Controls.Add(titleLabel);

            if (!string.IsNullOrEmpty(subtitle))
            {
                var subLabel = new Label
                {
                    Text = subtitle,
                    Font = new Font("Segoe UI", 8.2f),
                    ForeColor = _subTextColor,
                    AutoSize = true,
                    Location = new Point(15, 30)
                };
                card.Controls.Add(subLabel);
            }

            var toggle = new CheckBox
            {
                Checked = isChecked,
                AutoSize = true,
                Location = new Point(450, string.IsNullOrEmpty(subtitle) ? 14 : 18)
            };
            toggle.CheckedChanged += (s, e) => onChange(toggle.Checked);
            card.Controls.Add(toggle);

            parent.Controls.Add(card);
            return y + height + 12;
        }
    }
}
