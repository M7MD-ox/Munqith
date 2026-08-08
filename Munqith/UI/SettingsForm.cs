using System.Drawing.Drawing2D;
using Munqith.Core;
using Munqith.Models;
using Munqith.Services;
using Munqith.UI.Controls;

namespace Munqith.UI
{
    public class SettingsForm : Form
    {
        private readonly SettingsService _settingsService;
        private readonly StatsService _statsService;
        private readonly TrayApplicationContext _trayContext;

        private bool _isDark;
        private bool _isArabic;

        // Windows 11 Fluent Tokens
        private Color _windowBg;
        private Color _sidebarBg;
        private Color _cardBg;
        private Color _cardBorder;
        private Color _textPrimary;
        private Color _textSecondary;
        private Color _navItemActive;
        private readonly Color _accentColor = Color.FromArgb(0, 120, 212);
        private readonly Color _successColor = Color.FromArgb(46, 125, 50);

        private int _selectedNavIndex = 0; // 0: Stats, 1: Settings, 2: Hotkeys, 3: About
        private Panel _sidebarPanel = null!;
        private Panel _contentPanel = null!;
        private Label? _saveToastLabel;

        // Stats labels
        private Label? _heroCountLabel;
        private Label? _heroCharsLabel;
        private Label? _heroWordsLabel;
        private Label? _heroTimeLabel;

        // Hotkey Recorder
        private bool _isRecordingHotkey = false;
        private Button? _recordButton;

        public SettingsForm(SettingsService settingsService, StatsService statsService, TrayApplicationContext trayContext)
        {
            _settingsService = settingsService;
            _statsService = statsService;
            _trayContext = trayContext;

            _isDark = _settingsService.Settings.DarkMode;
            _isArabic = _settingsService.Settings.Language == "ar";

            InitializeForm();
            BuildFluentUI();
        }

        private void InitializeForm()
        {
            this.Text = _isArabic ? "مُنقِذ — إعدادات ويندوز 11" : "Munqith — Windows 11 Settings";
            this.ClientSize = new Size(740, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.RightToLeft = _isArabic ? RightToLeft.Yes : RightToLeft.No;
            this.RightToLeftLayout = false; // Keep coordinate system strictly deterministic
            this.Font = new Font("Segoe UI", 9.5f);

            UpdateFluentTokens();
            this.BackColor = _windowBg;
        }

        private void UpdateFluentTokens()
        {
            if (_isDark)
            {
                _windowBg = Color.FromArgb(32, 32, 32);          // Win11 Dark Canvas
                _sidebarBg = Color.FromArgb(26, 26, 26);         // Win11 Dark Sidebar
                _cardBg = Color.FromArgb(44, 44, 44);            // SettingsCard Background
                _cardBorder = Color.FromArgb(60, 60, 60);        // Subtle Card Stroke
                _textPrimary = Color.FromArgb(246, 246, 246);    // TextPrimary
                _textSecondary = Color.FromArgb(162, 162, 162);  // TextSecondary
                _navItemActive = Color.FromArgb(48, 48, 48);
            }
            else
            {
                _windowBg = Color.FromArgb(243, 243, 243);        // Win11 Light Canvas
                _sidebarBg = Color.FromArgb(234, 234, 234);       // Win11 Light Sidebar
                _cardBg = Color.White;                            // SettingsCard Background
                _cardBorder = Color.FromArgb(226, 226, 226);      // Subtle Card Stroke
                _textPrimary = Color.FromArgb(27, 27, 27);        // TextPrimary
                _textSecondary = Color.FromArgb(94, 94, 94);      // TextSecondary
                _navItemActive = Color.FromArgb(255, 255, 255);
            }
        }

        private void BuildFluentUI()
        {
            this.Controls.Clear();

            int sidebarWidth = 185;
            int totalWidth = this.ClientSize.Width;
            int totalHeight = this.ClientSize.Height;
            int contentWidth = totalWidth - sidebarWidth;

            if (_isArabic)
            {
                // Sidebar on the Right, Content on the Left
                _sidebarPanel = new Panel
                {
                    Location = new Point(contentWidth, 0),
                    Size = new Size(sidebarWidth, totalHeight),
                    BackColor = _sidebarBg,
                    Padding = new Padding(8)
                };

                _contentPanel = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(contentWidth, totalHeight),
                    BackColor = _windowBg,
                    AutoScroll = true,
                    Padding = new Padding(25, 20, 25, 20)
                };
            }
            else
            {
                // Sidebar on the Left, Content on the Right
                _sidebarPanel = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(sidebarWidth, totalHeight),
                    BackColor = _sidebarBg,
                    Padding = new Padding(8)
                };

                _contentPanel = new Point(sidebarWidth, 0).X > 0 ? new Panel
                {
                    Location = new Point(sidebarWidth, 0),
                    Size = new Size(contentWidth, totalHeight),
                    BackColor = _windowBg,
                    AutoScroll = true,
                    Padding = new Padding(25, 20, 25, 20)
                } : null!;
            }

            BuildSidebar(_sidebarPanel);
            this.Controls.Add(_sidebarPanel);
            this.Controls.Add(_contentPanel);

            RenderSelectedView();
        }

        private void BuildSidebar(Panel sidebar)
        {
            sidebar.Controls.Clear();

            // App Brand Header
            var logoPanel = new Panel
            {
                Location = new Point(8, 16),
                Size = new Size(168, 55),
                BackColor = Color.Transparent
            };

            var appTitle = new Label
            {
                Text = "🛡️ مُنقِذ Munqith",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = _accentColor,
                AutoSize = true,
                Location = new Point(4, 2)
            };
            logoPanel.Controls.Add(appTitle);

            var verLbl = new Label
            {
                Text = "Windows 11 Fluent v1.0.0",
                Font = new Font("Segoe UI", 8),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(6, 26)
            };
            logoPanel.Controls.Add(verLbl);

            sidebar.Controls.Add(logoPanel);

            // Navigation Items (WinUI 3 NavigationView items)
            string[] navItems = _isArabic
                ? new[] { "📊  الإحصائيات", "⚙️  الإعدادات", "⌨️  الاختصارات", "ℹ️  عن الأداة" }
                : new[] { "📊  Dashboard", "⚙️  Settings", "⌨️  Hotkeys", "ℹ️  About" };

            int y = 85;
            for (int i = 0; i < navItems.Length; i++)
            {
                int index = i;
                bool isSelected = (i == _selectedNavIndex);

                var navBtn = new Button
                {
                    Text = navItems[i],
                    Font = new Font("Segoe UI", 9.5f, isSelected ? FontStyle.Bold : FontStyle.Regular),
                    Size = new Size(168, 42),
                    Location = new Point(8, y),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = isSelected ? _navItemActive : Color.Transparent,
                    ForeColor = isSelected ? _accentColor : _textPrimary,
                    TextAlign = _isArabic ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand,
                    Padding = new Padding(_isArabic ? 0 : 8, 0, _isArabic ? 8 : 0, 0)
                };
                navBtn.FlatAppearance.BorderSize = 0;

                navBtn.Click += (s, e) =>
                {
                    _selectedNavIndex = index;
                    BuildFluentUI();
                };

                sidebar.Controls.Add(navBtn);
                y += 48;
            }

            // Bottom Tools: Theme Switcher & Language Switcher
            int bottomY = sidebar.Height - 100;

            var themeBtn = new Button
            {
                Text = _isDark ? "☀️  Light" : "🌙  Dark",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(168, 34),
                Location = new Point(8, bottomY),
                FlatStyle = FlatStyle.Flat,
                BackColor = _cardBg,
                ForeColor = _textPrimary,
                Cursor = Cursors.Hand
            };
            themeBtn.FlatAppearance.BorderColor = _cardBorder;
            themeBtn.Click += (s, e) =>
            {
                _isDark = !_isDark;
                _settingsService.Settings.DarkMode = _isDark;
                _settingsService.Save();
                UpdateFluentTokens();
                this.BackColor = _windowBg;
                BuildFluentUI();
            };
            sidebar.Controls.Add(themeBtn);

            var langBtn = new Button
            {
                Text = _isArabic ? "🌐  English" : "🌐  العربية",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(168, 34),
                Location = new Point(8, bottomY + 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = _cardBg,
                ForeColor = _textPrimary,
                Cursor = Cursors.Hand
            };
            langBtn.FlatAppearance.BorderColor = _cardBorder;
            langBtn.Click += (s, e) =>
            {
                string newLang = _isArabic ? "en" : "ar";
                _settingsService.Settings.Language = newLang;
                _settingsService.Save();
                _isArabic = newLang == "ar";
                this.RightToLeft = _isArabic ? RightToLeft.Yes : RightToLeft.No;
                this.Text = _isArabic ? "مُنقِذ — إعدادات ويندوز 11" : "Munqith — Windows 11 Settings";
                BuildFluentUI();
            };
            sidebar.Controls.Add(langBtn);
        }

        private void RenderSelectedView()
        {
            _contentPanel.Controls.Clear();

            switch (_selectedNavIndex)
            {
                case 0:
                    BuildDashboardView(_contentPanel);
                    break;
                case 1:
                    BuildSettingsCardsView(_contentPanel);
                    break;
                case 2:
                    BuildHotkeysCardsView(_contentPanel);
                    break;
                case 3:
                    BuildAboutView(_contentPanel);
                    break;
            }
        }

        // ==========================================
        // 1. DASHBOARD & RESCUE METRICS
        // ==========================================
        private void BuildDashboardView(Panel container)
        {
            int yPos = 10;
            int cardWidth = 490;

            // Page Header
            AddPageHeader(container, ref yPos,
                _isArabic ? "لوحة الإحصائيات والإنقاذ" : "Dashboard & Rescue Stats",
                _isArabic ? "متابعة عدد مرات تصحيح الكلمات وتوفير وقت الكتابة في ويندوز" : "Real-time statistics of corrected keystrokes and typing time saved");

            // Hero Metric Card
            var heroCard = CreateSettingsCard(yPos, cardWidth, 140);

            var heroTitle = new Label
            {
                Text = _isArabic ? "إجمالي عدد مرات الإنقاذ 🚀" : "Total Times Rescued 🚀",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(18, 12)
            };
            heroCard.Controls.Add(heroTitle);

            _heroCountLabel = new Label
            {
                Text = _statsService.TotalConversions.ToString("N0"),
                Font = new Font("Segoe UI Variable Display", 34, FontStyle.Bold),
                ForeColor = _accentColor,
                AutoSize = true,
                Location = new Point(14, 30)
            };
            heroCard.Controls.Add(_heroCountLabel);

            var heroUnit = new Label
            {
                Text = _isArabic ? "عملية تحويل ناجحة" : "conversions completed",
                Font = new Font("Segoe UI", 10.5f),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(_heroCountLabel.PreferredWidth + 22, 50)
            };
            heroCard.Controls.Add(heroUnit);

            int cardW = 145;
            int cardH = 46;
            int yMetrics = 82;

            _heroCharsLabel = AddMiniStatTile(heroCard, 18, yMetrics, cardW, cardH,
                _statsService.TotalCharacters.ToString("N0"),
                _isArabic ? "حرف تم تصحيحه" : "Chars Fixed");

            _heroWordsLabel = AddMiniStatTile(heroCard, 172, yMetrics, cardW, cardH,
                _statsService.TotalWords.ToString("N0"),
                _isArabic ? "كلمة مُنقَذة" : "Words Saved");

            string timeSavedStr = _isArabic ? _statsService.GetTimeSavedDisplayArabic() : _statsService.GetTimeSavedDisplay();
            _heroTimeLabel = AddMiniStatTile(heroCard, 326, yMetrics, cardW, cardH,
                timeSavedStr,
                _isArabic ? "وقت مُوفَّر" : "Time Saved");

            container.Controls.Add(heroCard);
            yPos += 152;

            // Live Playground SettingsCard
            var testCard = CreateSettingsCard(yPos, cardWidth, 175);

            var testTitle = new Label
            {
                Text = _isArabic ? "🧪 منطقة تجربة الأداة الفورية (Live Test)" : "🧪 Live Converter Playground",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(18, 12)
            };
            testCard.Controls.Add(testTitle);

            var testDesc = new Label
            {
                Text = _isArabic
                    ? "اكتب أي نص إنجليزي لخبطة هنا واضغط الزر لتجربة التحويل والتقليب فوراً:"
                    : "Type garbled text below and click button to test bidirectional conversion instantly:",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(18, 36)
            };
            testCard.Controls.Add(testDesc);

            var testBox = new TextBox
            {
                Text = "hgf,hfm ,hpim",
                Font = new Font("Segoe UI", 10.5f),
                Size = new Size(450, 32),
                Location = new Point(18, 62),
                BackColor = _isDark ? Color.FromArgb(52, 52, 52) : Color.FromArgb(248, 249, 250),
                ForeColor = _textPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            testCard.Controls.Add(testBox);

            var manualTestBtn = new Button
            {
                Text = _isArabic ? "🔄  شقلب النص الآن" : "🔄  Convert Text Now",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(190, 36),
                Location = new Point(18, 115),
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

        private Label AddMiniStatTile(Panel parent, int x, int y, int w, int h, string val, string label)
        {
            var tile = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = _isDark ? Color.FromArgb(52, 52, 52) : Color.FromArgb(244, 245, 248)
            };

            var vLbl = new Label
            {
                Text = val,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(8, 4)
            };
            tile.Controls.Add(vLbl);

            var dLbl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 7.8f),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(8, 24)
            };
            tile.Controls.Add(dLbl);

            parent.Controls.Add(tile);
            return vLbl;
        }

        // ==========================================
        // 2. SETTINGS CARDS (Official Windows 11 Pattern)
        // ==========================================
        private void BuildSettingsCardsView(Panel container)
        {
            int yPos = 10;
            int cardWidth = 490;

            AddPageHeader(container, ref yPos,
                _isArabic ? "إعدادات الأداة والتخطيط" : "General Settings & Layouts",
                _isArabic ? "تخصيص تخطيط لوحة المفاتيح وسلوك التحويل التلقائي في ويندوز" : "Configure keyboard layout mapping and conversion scope behavior");

            // SettingsCard 1: Keyboard Layout
            var layoutCard = CreateSettingsCard(yPos, cardWidth, 68);
            AddCardTexts(layoutCard,
                _isArabic ? "⌨️  تخطيط لوحة المفاتيح" : "⌨️  Keyboard Layout",
                _isArabic ? "تحديد تخطيط الحروف (ويندوز العربي القياسي أو الماك)" : "Target keyboard mapping layout (Windows PC or Mac Arabic)");

            var layoutCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(185, 30),
                Location = new Point(285, 18),
                BackColor = _isDark ? Color.FromArgb(52, 52, 52) : Color.White,
                ForeColor = _textPrimary,
                FlatStyle = FlatStyle.Flat
            };
            layoutCombo.Items.AddRange(new[] { "Windows Arabic PC", "Mac Arabic" });
            layoutCombo.SelectedIndex = (int)_settingsService.Settings.SelectedLayout;
            layoutCombo.SelectedIndexChanged += (s, e) =>
            {
                _settingsService.Settings.SelectedLayout = (KeyboardLayout)layoutCombo.SelectedIndex;
                _settingsService.Save();
            };
            layoutCard.Controls.Add(layoutCombo);
            container.Controls.Add(layoutCard);
            yPos += 80;

            // SettingsCard 2: Conversion Scope Mode
            var modeCard = CreateSettingsCard(yPos, cardWidth, 68);
            AddCardTexts(modeCard,
                _isArabic ? "🎯  وضع ونطاق التحويل" : "🎯  Conversion Scope Mode",
                _isArabic ? "التحويل التلقائي للنص المحدد أو السطر أو الكلمة" : "Target text scope (Selected text, whole line, or last word)");

            var modeCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f),
                Size = new Size(185, 30),
                Location = new Point(285, 18),
                BackColor = _isDark ? Color.FromArgb(52, 52, 52) : Color.White,
                ForeColor = _textPrimary,
                FlatStyle = FlatStyle.Flat
            };
            var modeItems = _isArabic
                ? new[] { "النص المحدد (تلقائي)", "السطر/الجملة الحالية", "آخر كلمة فقط" }
                : new[] { "Selected Text (Auto)", "Current Line", "Last Word Only" };
            modeCombo.Items.AddRange(modeItems);
            modeCombo.SelectedIndex = (int)_settingsService.Settings.ConversionMode;
            modeCombo.SelectedIndexChanged += (s, e) =>
            {
                _settingsService.Settings.ConversionMode = (ConversionMode)modeCombo.SelectedIndex;
                _settingsService.Save();
            };
            modeCard.Controls.Add(modeCombo);
            container.Controls.Add(modeCard);
            yPos += 80;

            // SettingsCard 3: Smart Detection (ToggleSwitch)
            yPos = AddToggleSettingsCard(container, yPos, cardWidth,
                _isArabic ? "🧠  الكشف الذكي (Smart Detection)" : "🧠  Smart Word Detection",
                _isArabic ? "الحفاظ على الكلمات الإنجليزية الصحيحة وتصحيح الحروف الخطأ فقط" : "Preserves valid English vocabulary and only converts typos",
                _settingsService.Settings.SmartDetectionEnabled,
                val =>
                {
                    _settingsService.Settings.SmartDetectionEnabled = val;
                    _settingsService.Save();
                });

            // SettingsCard 4: Launch with Windows (ToggleSwitch)
            yPos = AddToggleSettingsCard(container, yPos, cardWidth,
                _isArabic ? "🚀  بدء التشغيل مع الويندوز" : "🚀  Launch on Windows Startup",
                _isArabic ? "تشغيل الأداة تلقائياً في الخلفية عند فتح الكمبيوتر" : "Automatically runs Munqith in the system tray when Windows boots",
                _settingsService.Settings.StartWithWindows,
                val =>
                {
                    _settingsService.Settings.StartWithWindows = val;
                    StartupService.SetStartup(val);
                    _settingsService.Save();
                });

            // SettingsCard 5: Enable/Pause Tool (ToggleSwitch)
            yPos = AddToggleSettingsCard(container, yPos, cardWidth,
                _isArabic ? "⚡  تفعيل عمل الأداة" : "⚡  Enable Utility Hooks",
                _isArabic ? "تفعيل أو إيقاف استجابة الأداة لاختصارات الكيبورد مؤقتاً" : "Master switch to enable or pause keyboard shortcuts globally",
                _settingsService.Settings.IsEnabled,
                val =>
                {
                    _settingsService.Settings.IsEnabled = val;
                    _settingsService.Save();
                    _trayContext.CreateContextMenu();
                });

            // Instant Save Card with Fluent Button
            var saveCard = CreateSettingsCard(yPos, cardWidth, 68);
            var saveBtn = new Button
            {
                Text = _isArabic ? "💾  حفظ وتطبيق الإعدادات فوراً" : "💾  Save & Apply Instantly",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Size = new Size(240, 40),
                Location = new Point(18, 14),
                FlatStyle = FlatStyle.Flat,
                BackColor = _successColor,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            saveBtn.FlatAppearance.BorderSize = 0;

            _saveToastLabel = new Label
            {
                Text = _isArabic ? "✅ تم التطبيق فوراً بنجاح!" : "✅ Applied successfully!",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = _successColor,
                AutoSize = true,
                Location = new Point(275, 24),
                Visible = false
            };

            saveBtn.Click += (s, e) =>
            {
                _settingsService.Save();
                _trayContext.RegisterConfiguredHotkeys();
                _trayContext.CreateContextMenu();

                _saveToastLabel.Visible = true;
                Task.Delay(2500).ContinueWith(_ =>
                {
                    if (this.IsHandleCreated)
                    {
                        this.Invoke(() => { _saveToastLabel.Visible = false; });
                    }
                });
            };

            saveCard.Controls.Add(saveBtn);
            saveCard.Controls.Add(_saveToastLabel);
            container.Controls.Add(saveCard);
        }

        // ==========================================
        // 3. HOTKEYS & INTERACTIVE RECORDER
        // ==========================================
        private void BuildHotkeysCardsView(Panel container)
        {
            int yPos = 10;
            int cardWidth = 490;

            AddPageHeader(container, ref yPos,
                _isArabic ? "إدارة الاختصارات ومُسجّل المفاتيح" : "Global Hotkeys & Key Recorder",
                _isArabic ? "إضافة وتخصيص مفاتيح الاختصار لتشغيل التحويل في أي مكان" : "Manage active global hotkey combinations or record new shortcuts");

            // Hotkeys List Card
            var listCard = CreateSettingsCard(yPos, cardWidth, 190);
            var listTitle = new Label
            {
                Text = _isArabic ? "⌨️  الاختصارات النشطة حالياً" : "⌨️  Currently Active Hotkeys",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(18, 12)
            };
            listCard.Controls.Add(listTitle);

            int yOffset = 44;
            for (int i = 0; i < _settingsService.Settings.Hotkeys.Count; i++)
            {
                int index = i;
                var hk = _settingsService.Settings.Hotkeys[i];

                var badge = new Label
                {
                    Text = $"  {hk.DisplayName}  ",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = _accentColor,
                    AutoSize = true,
                    Location = new Point(20, yOffset + 2)
                };
                listCard.Controls.Add(badge);

                var toggle = new WinUIToggleSwitch
                {
                    Checked = hk.IsEnabled,
                    IsDark = _isDark,
                    Location = new Point(240, yOffset)
                };
                toggle.CheckedChanged += (s, e) =>
                {
                    hk.IsEnabled = toggle.Checked;
                    _settingsService.Save();
                    _trayContext.RegisterConfiguredHotkeys();
                };
                listCard.Controls.Add(toggle);

                var delBtn = new Button
                {
                    Text = _isArabic ? "حذف" : "Delete",
                    Font = new Font("Segoe UI", 8.5f),
                    Size = new Size(60, 26),
                    Location = new Point(410, yOffset - 1),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(198, 40, 40),
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand
                };
                delBtn.FlatAppearance.BorderSize = 0;
                delBtn.Click += (s, e) =>
                {
                    _settingsService.Settings.Hotkeys.RemoveAt(index);
                    _settingsService.Save();
                    _trayContext.RegisterConfiguredHotkeys();
                    BuildFluentUI();
                };
                listCard.Controls.Add(delBtn);

                yOffset += 38;
            }

            container.Controls.Add(listCard);
            yPos += 202;

            // Interactive Recorder Card
            var recCard = CreateSettingsCard(yPos, cardWidth, 160);
            var recTitle = new Label
            {
                Text = _isArabic ? "🎙️  مسجل الاختصارات التفاعلي (Key Recorder)" : "🎙️  Interactive Key Recorder",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(18, 12)
            };
            recCard.Controls.Add(recTitle);

            var recDesc = new Label
            {
                Text = _isArabic
                    ? "اضغط على الحقل ثم اضغط أي تركيبة مفاتيح على الكيبورد لتسجيلها فوراً:"
                    : "Click below and press any key combination on keyboard to bind instantly:",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(18, 36)
            };
            recCard.Controls.Add(recDesc);

            _recordButton = new Button
            {
                Text = _isArabic ? "اضغط هنا لبدء تسجيل الاختصار..." : "Click here to record shortcut...",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(450, 42),
                Location = new Point(18, 62),
                FlatStyle = FlatStyle.Flat,
                BackColor = _isDark ? Color.FromArgb(52, 52, 52) : Color.FromArgb(242, 244, 248),
                ForeColor = _accentColor,
                Cursor = Cursors.Hand
            };
            _recordButton.FlatAppearance.BorderColor = _accentColor;
            _recordButton.FlatAppearance.BorderSize = 2;

            _recordButton.Click += (s, e) =>
            {
                _isRecordingHotkey = true;
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
                    _isRecordingHotkey = false;

                    var newBinding = new HotkeyBinding
                    {
                        ModifierKeysValue = mods,
                        KeyValue = (int)e.KeyCode,
                        IsEnabled = true
                    };

                    _recordButton.Text = $"✅ تم تسجيل: {newBinding.DisplayName}";
                    _recordButton.BackColor = Color.FromArgb(232, 245, 233);
                    _recordButton.ForeColor = Color.FromArgb(46, 125, 50);

                    _settingsService.Settings.Hotkeys.Add(newBinding);
                    _settingsService.Save();
                    _trayContext.RegisterConfiguredHotkeys();

                    Task.Delay(1000).ContinueWith(_ =>
                    {
                        if (this.IsHandleCreated)
                        {
                            this.Invoke(() => BuildFluentUI());
                        }
                    });
                }
            };

            recCard.Controls.Add(_recordButton);
            container.Controls.Add(recCard);
        }

        // ==========================================
        // 4. ABOUT & SPECS VIEW
        // ==========================================
        private void BuildAboutView(Panel container)
        {
            int yPos = 10;
            int cardWidth = 490;

            AddPageHeader(container, ref yPos,
                _isArabic ? "عن أداة مُنقِذ" : "About Munqith",
                _isArabic ? "معلومات الأداة ومصدر المشروع والترخيص" : "Project specifications, open source repository, and license");

            var aboutCard = CreateSettingsCard(yPos, cardWidth, 330);

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
                    ? "أداة ويندوز سريعة ولحظية تصحح لغة الكتابة تلقائياً في جميع برامج الجهاز بضغطة اختصار واحدة.\n\n" +
                      "مصممة وفق معايير مايكروسوفت الرسمية لتصميم تطبيقات ويندوز 11 (Fluent Design System & WinUI 3)."
                    : "Fast, instantaneous Windows utility that auto-corrects text typed with the wrong keyboard layout across all Windows apps.\n\n" +
                      "Crafted adhering to Microsoft's official Windows 11 Fluent Design System guidelines.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = _textPrimary,
                AutoSize = true,
                MaximumSize = new Size(450, 0),
                Location = new Point(20, 65)
            };
            aboutCard.Controls.Add(desc);

            var techInfo = new Label
            {
                Text = _isArabic
                    ? "• التقنية: C# .NET 8.0 (LTS) — Self-contained Single Executable\n" +
                      "• نمط التصميم: Windows 11 Fluent SettingsCard & WinUI 3 Navigation\n" +
                      "• الاستجابة: Win32 Native Clipboard with Micro-Delays (<30ms)"
                    : "• Tech Stack: C# .NET 8.0 (LTS) — Self-contained Single Executable\n" +
                      "• Design System: Windows 11 Fluent SettingsCard & WinUI 3 Navigation\n" +
                      "• Performance: Win32 Native Clipboard with Micro-Delays (<30ms)",
                Font = new Font("Segoe UI", 8.8f),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(20, 185)
            };
            aboutCard.Controls.Add(techInfo);

            var githubBtn = new Button
            {
                Text = "🌐 GitHub Repository",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(180, 38),
                Location = new Point(20, 265),
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
        // HELPERS: Fluent SettingsCard & Controls
        // ==========================================
        private void AddPageHeader(Panel container, ref int yPos, string title, string subtitle)
        {
            var headerLbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Variable Display", 17, FontStyle.Bold),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(4, yPos)
            };
            container.Controls.Add(headerLbl);

            var subLbl = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 8.8f),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(6, yPos + 32)
            };
            container.Controls.Add(subLbl);

            yPos += 64;
        }

        private Panel CreateSettingsCard(int y, int width, int height)
        {
            var card = new Panel
            {
                Location = new Point(4, y),
                Size = new Size(width, height),
                BackColor = _cardBg,
                Padding = new Padding(12)
            };

            card.Paint += (s, e) =>
            {
                using var pen = new Pen(_cardBorder, 1);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // 8px Corner Radius Path
                using var path = CreateRoundedRectPath(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 8);
                e.Graphics.DrawPath(pen, path);
            };

            return card;
        }

        private void AddCardTexts(Panel card, string title, string subtitle)
        {
            var titleLbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.8f, FontStyle.Bold),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(14, string.IsNullOrEmpty(subtitle) ? 14 : 10)
            };
            card.Controls.Add(titleLbl);

            if (!string.IsNullOrEmpty(subtitle))
            {
                var subLbl = new Label
                {
                    Text = subtitle,
                    Font = new Font("Segoe UI", 8.2f),
                    ForeColor = _textSecondary,
                    AutoSize = true,
                    Location = new Point(14, 32)
                };
                card.Controls.Add(subLbl);
            }
        }

        private int AddToggleSettingsCard(Panel container, int y, int cardWidth, string title, string subtitle, bool isChecked, Action<bool> onChange)
        {
            int height = string.IsNullOrEmpty(subtitle) ? 52 : 64;
            var card = CreateSettingsCard(y, cardWidth, height);
            AddCardTexts(card, title, subtitle);

            var toggle = new WinUIToggleSwitch
            {
                Checked = isChecked,
                IsDark = _isDark,
                Location = new Point(420, string.IsNullOrEmpty(subtitle) ? 14 : 18)
            };
            toggle.CheckedChanged += (s, e) => onChange(toggle.Checked);
            card.Controls.Add(toggle);

            container.Controls.Add(card);
            return y + height + 12;
        }

        private static GraphicsPath CreateRoundedRectPath(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            var size = new Size(diameter, diameter);
            var arc = new Rectangle(r.Location, size);

            path.AddArc(arc, 180, 90);
            arc.X = r.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = r.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = r.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
