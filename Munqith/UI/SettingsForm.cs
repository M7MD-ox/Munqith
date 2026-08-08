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

        // Ultra-Modern Obsidian & Pearl Palette
        private Color _windowBg;
        private Color _sidebarBg;
        private Color _cardBg;
        private Color _cardBorder;
        private Color _textPrimary;
        private Color _textSecondary;
        private Color _navItemActive;
        private readonly Color _accentColor = Color.FromArgb(59, 130, 246);      // Electric Modern Blue
        private readonly Color _emeraldGreen = Color.FromArgb(16, 185, 129);     // Vibrant Success Green
        private readonly Color _roseRed = Color.FromArgb(239, 68, 68);           // Clean Red

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
            BuildModernUI();
        }

        private void InitializeForm()
        {
            this.Text = _isArabic ? "مُنقِذ — Munqith" : "Munqith";
            this.ClientSize = new Size(760, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.RightToLeft = _isArabic ? RightToLeft.Yes : RightToLeft.No;
            this.RightToLeftLayout = false;
            this.Font = new Font("Segoe UI", 9.5f);

            UpdateThemeTokens();
            this.BackColor = _windowBg;
        }

        private void UpdateThemeTokens()
        {
            if (_isDark)
            {
                _windowBg = Color.FromArgb(18, 20, 26);          // Deep Obsidian
                _sidebarBg = Color.FromArgb(24, 27, 36);         // Sleek Sidebar
                _cardBg = Color.FromArgb(30, 34, 45);            // Elevated Card Surface
                _cardBorder = Color.FromArgb(45, 52, 68);        // Subtle Border
                _textPrimary = Color.FromArgb(248, 250, 252);    // Crisp Pure Text
                _textSecondary = Color.FromArgb(148, 163, 184);  // Slate Muted
                _navItemActive = Color.FromArgb(37, 43, 58);
            }
            else
            {
                _windowBg = Color.FromArgb(244, 246, 250);       // Pearl Canvas
                _sidebarBg = Color.FromArgb(255, 255, 255);       // Pure White Sidebar
                _cardBg = Color.FromArgb(255, 255, 255);          // Elevated White Card
                _cardBorder = Color.FromArgb(226, 232, 240);      // Slate 200 Border
                _textPrimary = Color.FromArgb(15, 23, 42);        // Deep Slate Text
                _textSecondary = Color.FromArgb(100, 116, 139);   // Slate 500 Muted
                _navItemActive = Color.FromArgb(238, 242, 255);   // Indigo Light Active
            }
        }

        private void BuildModernUI()
        {
            this.Controls.Clear();

            int sidebarWidth = 190;
            int totalWidth = this.ClientSize.Width;
            int totalHeight = this.ClientSize.Height;
            int contentWidth = totalWidth - sidebarWidth;

            if (_isArabic)
            {
                _sidebarPanel = new Panel
                {
                    Location = new Point(contentWidth, 0),
                    Size = new Size(sidebarWidth, totalHeight),
                    BackColor = _sidebarBg,
                    Padding = new Padding(10)
                };

                _contentPanel = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(contentWidth, totalHeight),
                    BackColor = _windowBg,
                    AutoScroll = true,
                    Padding = new Padding(24, 20, 24, 20)
                };
            }
            else
            {
                _sidebarPanel = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(sidebarWidth, totalHeight),
                    BackColor = _sidebarBg,
                    Padding = new Padding(10)
                };

                _contentPanel = new Panel
                {
                    Location = new Point(sidebarWidth, 0),
                    Size = new Size(contentWidth, totalHeight),
                    BackColor = _windowBg,
                    AutoScroll = true,
                    Padding = new Padding(24, 20, 24, 20)
                };
            }

            BuildSidebar(_sidebarPanel);
            this.Controls.Add(_sidebarPanel);
            this.Controls.Add(_contentPanel);

            RenderSelectedView();
        }

        private void BuildSidebar(Panel sidebar)
        {
            sidebar.Controls.Clear();

            // Brand Header with Icon Badge
            var brandCard = new Panel
            {
                Location = new Point(8, 14),
                Size = new Size(174, 60),
                BackColor = Color.Transparent
            };

            var appTitle = new Label
            {
                Text = "🛡️ مُنقِذ Munqith",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = _accentColor,
                AutoSize = true,
                Location = new Point(_isArabic ? 10 : 4, 4)
            };
            brandCard.Controls.Add(appTitle);

            var verBadge = new Label
            {
                Text = "v1.0.0 • Background Utility",
                Font = new Font("Segoe UI", 7.8f, FontStyle.Regular),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(_isArabic ? 22 : 6, 28)
            };
            brandCard.Controls.Add(verBadge);

            sidebar.Controls.Add(brandCard);

            // Modern Navigation Items
            string[] navItems = _isArabic
                ? new[] { "📊  الإحصائيات", "⚙️  الإعدادات", "⌨️  الاختصارات", "ℹ️  عن الأداة" }
                : new[] { "📊  Dashboard", "⚙️  Settings", "⌨️  Hotkeys", "ℹ️  About" };

            int y = 90;
            for (int i = 0; i < navItems.Length; i++)
            {
                int index = i;
                bool isSelected = (i == _selectedNavIndex);

                var navBtn = new Button
                {
                    Text = navItems[i],
                    Font = new Font("Segoe UI", 9.5f, isSelected ? FontStyle.Bold : FontStyle.Regular),
                    Size = new Size(172, 44),
                    Location = new Point(8, y),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = isSelected ? _navItemActive : Color.Transparent,
                    ForeColor = isSelected ? _accentColor : _textPrimary,
                    TextAlign = _isArabic ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand,
                    Padding = new Padding(_isArabic ? 0 : 12, 0, _isArabic ? 12 : 0, 0)
                };
                navBtn.FlatAppearance.BorderSize = 0;

                navBtn.Click += (s, e) =>
                {
                    _selectedNavIndex = index;
                    BuildModernUI();
                };

                sidebar.Controls.Add(navBtn);
                y += 50;
            }

            // Bottom Quick Controls: Theme (Dark/Light) & Language Switcher
            int bottomY = sidebar.Height - 105;

            var themeBtn = new Button
            {
                Text = _isDark ? "☀️  Light Mode" : "🌙  Dark Mode",
                Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                Size = new Size(172, 36),
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
                UpdateThemeTokens();
                this.BackColor = _windowBg;
                BuildModernUI();
            };
            sidebar.Controls.Add(themeBtn);

            var langBtn = new Button
            {
                Text = _isArabic ? "🌐  Switch to English" : "🌐  التحويل للعربية",
                Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                Size = new Size(172, 36),
                Location = new Point(8, bottomY + 44),
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
                this.Text = _isArabic ? "مُنقِذ — Munqith" : "Munqith";
                BuildModernUI();
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
        // 1. DASHBOARD & STATS VIEW
        // ==========================================
        private void BuildDashboardView(Panel container)
        {
            int yPos = 10;
            int cardWidth = 515;

            AddPageHeader(container, ref yPos,
                _isArabic ? "لوحة الإحصائيات والإنقاذ" : "Rescue Statistics & Live Test",
                _isArabic ? "سجل مباشر لعدد المرات والكلمات التي أنقذك فيها البرنامج مع الوقت المُوفَّر" : "Real-time tracker of converted keystrokes, words saved, and typing time gained");

            // Hero Metric Card
            var heroCard = CreateCard(yPos, cardWidth, 145);

            var heroTitle = new Label
            {
                Text = _isArabic ? "إجمالي عدد مرات الإنقاذ حتى الآن 🚀" : "Total Typing Conversions 🚀",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(_isArabic ? cardWidth - 230 : 18, 12)
            };
            heroCard.Controls.Add(heroTitle);

            _heroCountLabel = new Label
            {
                Text = _statsService.TotalConversions.ToString("N0"),
                Font = new Font("Segoe UI Variable Display", 32, FontStyle.Bold),
                ForeColor = _accentColor,
                AutoSize = true,
                Location = new Point(_isArabic ? cardWidth - 140 : 16, 30)
            };
            heroCard.Controls.Add(_heroCountLabel);

            var heroUnit = new Label
            {
                Text = _isArabic ? "مرة تحويل ناجحة" : "conversions done",
                Font = new Font("Segoe UI", 10.5f),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(_isArabic ? cardWidth - 280 : 130, 48)
            };
            heroCard.Controls.Add(heroUnit);

            int cardW = 150;
            int cardH = 46;
            int yMetrics = 84;

            if (_isArabic)
            {
                _heroCharsLabel = AddMiniStatTile(heroCard, cardWidth - 18 - cardW, yMetrics, cardW, cardH,
                    _statsService.TotalCharacters.ToString("N0"), "حرف تم تصحيحه");

                _heroWordsLabel = AddMiniStatTile(heroCard, cardWidth - 18 - (cardW * 2) - 10, yMetrics, cardW, cardH,
                    _statsService.TotalWords.ToString("N0"), "كلمة تم إنقاذها");

                string timeSavedStr = _statsService.GetTimeSavedDisplayArabic();
                _heroTimeLabel = AddMiniStatTile(heroCard, cardWidth - 18 - (cardW * 3) - 20, yMetrics, cardW, cardH,
                    timeSavedStr, "وقت مُوفَّر");
            }
            else
            {
                _heroCharsLabel = AddMiniStatTile(heroCard, 18, yMetrics, cardW, cardH,
                    _statsService.TotalCharacters.ToString("N0"), "Chars Corrected");

                _heroWordsLabel = AddMiniStatTile(heroCard, 180, yMetrics, cardW, cardH,
                    _statsService.TotalWords.ToString("N0"), "Words Saved");

                string timeSavedStr = _statsService.GetTimeSavedDisplay();
                _heroTimeLabel = AddMiniStatTile(heroCard, 342, yMetrics, cardW, cardH,
                    timeSavedStr, "Time Saved");
            }

            container.Controls.Add(heroCard);
            yPos += 158;

            // Live Playground Card
            var testCard = CreateCard(yPos, cardWidth, 180);

            var testTitle = new Label
            {
                Text = _isArabic ? "🧪  منطقة التجربة الفورية (Live Test)" : "🧪  Instant Converter Playground",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(_isArabic ? cardWidth - 280 : 18, 12)
            };
            testCard.Controls.Add(testTitle);

            var testDesc = new Label
            {
                Text = _isArabic
                    ? "اكتب أي نص إنجليزي ملخبط هنا واضغط الزر لاختبار التحويل والتقليب فوراً:"
                    : "Type garbled English text below and click button to test instant bidirectional conversion:",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(_isArabic ? cardWidth - 480 : 18, 36)
            };
            testCard.Controls.Add(testDesc);

            var testBox = new TextBox
            {
                Text = "hgf,hfm ,hpim",
                Font = new Font("Segoe UI", 10.5f),
                Size = new Size(475, 32),
                Location = new Point(18, 64),
                BackColor = _isDark ? Color.FromArgb(40, 45, 58) : Color.FromArgb(248, 250, 252),
                ForeColor = _textPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                RightToLeft = _isArabic ? RightToLeft.Yes : RightToLeft.No
            };
            testCard.Controls.Add(testBox);

            var manualTestBtn = new Button
            {
                Text = _isArabic ? "🔄  شقلب النص الآن" : "🔄  Convert Text Now",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(185, 36),
                Location = new Point(_isArabic ? cardWidth - 203 : 18, 118),
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
                    string converted = LayoutConverter.Convert(testBox.Text, KeyboardLayout.WindowsArabicPC);
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
                BackColor = _isDark ? Color.FromArgb(42, 48, 62) : Color.FromArgb(241, 245, 249)
            };

            var vLbl = new Label
            {
                Text = val,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(_isArabic ? w - 70 : 8, 4)
            };
            tile.Controls.Add(vLbl);

            var dLbl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 7.8f),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(_isArabic ? w - 110 : 8, 24)
            };
            tile.Controls.Add(dLbl);

            parent.Controls.Add(tile);
            return vLbl;
        }

        // ==========================================
        // 2. SETTINGS CARDS
        // ==========================================
        private void BuildSettingsCardsView(Panel container)
        {
            int yPos = 10;
            int cardWidth = 515;

            AddPageHeader(container, ref yPos,
                _isArabic ? "إعدادات الأداة والنظام" : "General Settings & Options",
                _isArabic ? "تخصيص سلوك التحويل التلقائي والتشغيل في الخلفية" : "Configure smart word detection and startup options");

            // Smart Detection Toggle Card (Default ENABLED)
            yPos = AddToggleCard(container, yPos, cardWidth,
                _isArabic ? "🧠  الكشف الذكي للكلمات (Smart Vocabulary) [مفعّل افتراضياً]" : "🧠  Smart Vocabulary Detection [Default Enabled]",
                _isArabic ? "يحافظ على الكلمات الإنجليزية الصحيحة تلقائياً ويصحح فقط أخطاء الكتابة" : "Preserves real English words and only flips actual typing mistakes",
                _settingsService.Settings.SmartDetectionEnabled,
                val =>
                {
                    _settingsService.Settings.SmartDetectionEnabled = val;
                    _settingsService.Save();
                });

            // Windows Startup Toggle Card
            yPos = AddToggleCard(container, yPos, cardWidth,
                _isArabic ? "🚀  التشغيل التلقائي مع فتح الجهاز" : "🚀  Launch on Windows Startup",
                _isArabic ? "تشغيل مُنقِذ تلقائياً في الخلفية في شريط المهام عند فتح الويندوز" : "Automatically launch Munqith in the system tray when Windows boots up",
                _settingsService.Settings.StartWithWindows,
                val =>
                {
                    _settingsService.Settings.StartWithWindows = val;
                    StartupService.SetStartup(val);
                    _settingsService.Save();
                });

            // Instant Save Card
            var saveCard = CreateCard(yPos, cardWidth, 68);
            var saveBtn = new Button
            {
                Text = _isArabic ? "💾  حفظ وتطبيق الإعدادات فوراً" : "💾  Save & Apply Instantly",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(230, 40),
                Location = new Point(_isArabic ? cardWidth - 246 : 16, 14),
                FlatStyle = FlatStyle.Flat,
                BackColor = _emeraldGreen,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            saveBtn.FlatAppearance.BorderSize = 0;

            _saveToastLabel = new Label
            {
                Text = _isArabic ? "✅ تم الحفظ والتطبيق فوراً!" : "✅ Saved & Applied Instantly!",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = _emeraldGreen,
                AutoSize = true,
                Location = new Point(_isArabic ? 20 : 260, 24),
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
        // 3. KEYBOARD HOTKEYS & RECORDER
        // ==========================================
        private void BuildHotkeysCardsView(Panel container)
        {
            int yPos = 10;
            int cardWidth = 515;

            AddPageHeader(container, ref yPos,
                _isArabic ? "إدارة اختصارات الكيبورد" : "Keyboard Shortcuts & Binder",
                _isArabic ? "تخصيص المفاتيح التي تقوم بتحويل وشقلبة النص في أي برنامج" : "Manage active hotkeys or record new custom keyboard combinations");

            // Hotkeys List Card
            var listCard = CreateCard(yPos, cardWidth, 195);
            var listTitle = new Label
            {
                Text = _isArabic ? "⌨️  الاختصارات النشطة حالياً:" : "⌨️  Currently Active Shortcuts:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(_isArabic ? cardWidth - 230 : 16, 12)
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
                    Location = new Point(_isArabic ? cardWidth - 140 : 18, yOffset + 2)
                };
                listCard.Controls.Add(badge);

                var toggle = new WinUIToggleSwitch
                {
                    Checked = hk.IsEnabled,
                    IsDark = _isDark,
                    Location = new Point(_isArabic ? cardWidth - 260 : 220, yOffset)
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
                    Location = new Point(_isArabic ? 20 : 420, yOffset - 1),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = _roseRed,
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand
                };
                delBtn.FlatAppearance.BorderSize = 0;
                delBtn.Click += (s, e) =>
                {
                    _settingsService.Settings.Hotkeys.RemoveAt(index);
                    _settingsService.Save();
                    _trayContext.RegisterConfiguredHotkeys();
                    BuildModernUI();
                };
                listCard.Controls.Add(delBtn);

                yOffset += 38;
            }

            container.Controls.Add(listCard);
            yPos += 207;

            // Dedicated Keyboard Recorder Card (NO MOUSE, NO AUDIO)
            var recCard = CreateCard(yPos, cardWidth, 160);
            var recTitle = new Label
            {
                Text = _isArabic ? "⌨️  مسجل اختصارات الكيبورد التفاعلي" : "⌨️  Interactive Keyboard Recorder",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(_isArabic ? cardWidth - 280 : 16, 12)
            };
            recCard.Controls.Add(recTitle);

            var recDesc = new Label
            {
                Text = _isArabic
                    ? "اضغط الزر بالماوس، ثم اضغط تركيبة المفاتيح بالكيبورد (مثلاً Alt+Q أو Ctrl+Shift+Z):"
                    : "Click below, then press your desired keyboard hotkey (e.g. Alt+Q or Ctrl+Shift+Z):",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(_isArabic ? cardWidth - 480 : 16, 36)
            };
            recCard.Controls.Add(recDesc);

            _recordButton = new Button
            {
                Text = _isArabic ? "⌨️ اضغط هنا لتسجيل اختصار كيبورد جديد..." : "⌨️ Click here to record keyboard hotkey...",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(480, 44),
                Location = new Point(16, 64),
                FlatStyle = FlatStyle.Flat,
                BackColor = _isDark ? Color.FromArgb(40, 45, 58) : Color.FromArgb(241, 245, 249),
                ForeColor = _accentColor,
                Cursor = Cursors.Hand
            };
            _recordButton.FlatAppearance.BorderColor = _accentColor;
            _recordButton.FlatAppearance.BorderSize = 2;

            _recordButton.Click += (s, e) =>
            {
                _isRecordingHotkey = true;
                _recordButton.Text = _isArabic ? "⏳ جاري الاستماع... اضغط مفاتيح الكيبورد الآن!" : "⏳ Listening... Press keyboard keys now!";
                _recordButton.BackColor = Color.FromArgb(254, 243, 199);
                _recordButton.ForeColor = Color.FromArgb(146, 64, 14);
                _recordButton.Focus();
            };

            // BLOCK ALL MOUSE CLICKS FROM BEING RECORDED
            _recordButton.KeyDown += (s, e) =>
            {
                if (!_isRecordingHotkey) return;

                e.SuppressKeyPress = true;

                // Ignore mouse keys or solo modifier releases
                if (e.KeyCode == Keys.LButton || e.KeyCode == Keys.RButton || e.KeyCode == Keys.MButton ||
                    e.KeyCode == Keys.XButton1 || e.KeyCode == Keys.XButton2 ||
                    e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Menu || e.KeyCode == Keys.None)
                {
                    return;
                }

                int mods = 0;
                if (e.Alt) mods |= 1;
                if (e.Control) mods |= 2;
                if (e.Shift) mods |= 4;

                _isRecordingHotkey = false;

                var newBinding = new HotkeyBinding
                {
                    ModifierKeysValue = mods,
                    KeyValue = (int)e.KeyCode,
                    IsEnabled = true
                };

                _recordButton.Text = $"✅ تم تسجيل الاختصار: {newBinding.DisplayName}";
                _recordButton.BackColor = Color.FromArgb(209, 250, 229);
                _recordButton.ForeColor = _emeraldGreen;

                _settingsService.Settings.Hotkeys.Add(newBinding);
                _settingsService.Save();
                _trayContext.RegisterConfiguredHotkeys();

                Task.Delay(1000).ContinueWith(_ =>
                {
                    if (this.IsHandleCreated)
                    {
                        this.Invoke(() => BuildModernUI());
                    }
                });
            };

            recCard.Controls.Add(_recordButton);
            container.Controls.Add(recCard);
        }

        // ==========================================
        // 4. ABOUT & PROJECT INFO
        // ==========================================
        private void BuildAboutView(Panel container)
        {
            int yPos = 10;
            int cardWidth = 515;

            AddPageHeader(container, ref yPos,
                _isArabic ? "عن أداة مُنقِذ" : "About Munqith",
                _isArabic ? "معلومات الأداة والتقنية المستخدمة" : "Application details, tech stack, and open source repository");

            var aboutCard = CreateCard(yPos, cardWidth, 310);

            var title = new Label
            {
                Text = "🛡️ مُنقِذ — Munqith v1.0.0",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = _accentColor,
                AutoSize = true,
                Location = new Point(_isArabic ? cardWidth - 270 : 20, 18)
            };
            aboutCard.Controls.Add(title);

            var desc = new Label
            {
                Text = _isArabic
                    ? "أداة ويندوز ذكية وفائقة السرعة تعمل في الخلفية لتصحيح لغة الكتابة تلقائياً في كافة البرامج والمتصفحات بضغطة اختصار واحدة، دون الحاجة لإعادة كتابة النص من جديد."
                    : "Fast, lightweight Windows utility running in the background to instantly auto-correct mistyped keyboard layouts across all Windows apps and browsers with a single hotkey.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = _textPrimary,
                AutoSize = true,
                MaximumSize = new Size(475, 0),
                Location = new Point(_isArabic ? 20 : 20, 60)
            };
            aboutCard.Controls.Add(desc);

            var techInfo = new Label
            {
                Text = _isArabic
                    ? "• بيئة التشغيل: Windows 10 / Windows 11 (64-bit)\n" +
                      "• التقنية: C# .NET 8.0 (LTS) — تطبيق مدمج مستقل بالكامل\n" +
                      "• محرك الالتقاط: Win32 Low-Level Hook + Native Clipboard (<30ms)"
                    : "• OS: Windows 10 / Windows 11 (64-bit)\n" +
                      "• Engine: C# .NET 8.0 (LTS) — Self-contained standalone executable\n" +
                      "• Latency: Win32 Low-Level Hook + Native Clipboard (<30ms)",
                Font = new Font("Segoe UI", 8.8f),
                ForeColor = _textSecondary,
                AutoSize = true,
                Location = new Point(_isArabic ? cardWidth - 480 : 20, 150)
            };
            aboutCard.Controls.Add(techInfo);

            var githubBtn = new Button
            {
                Text = "🌐  GitHub Repository",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(185, 38),
                Location = new Point(_isArabic ? cardWidth - 205 : 20, 245),
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
        // HELPERS: Cards, Headers, Toggles
        // ==========================================
        private void AddPageHeader(Panel container, ref int yPos, string title, string subtitle)
        {
            var headerLbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI Variable Display", 16, FontStyle.Bold),
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
                Location = new Point(6, yPos + 30)
            };
            container.Controls.Add(subLbl);

            yPos += 62;
        }

        private Panel CreateCard(int y, int width, int height)
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

                using var path = CreateRoundedRectPath(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 10);
                e.Graphics.DrawPath(pen, path);
            };

            return card;
        }

        private int AddToggleCard(Panel container, int y, int cardWidth, string title, string subtitle, bool isChecked, Action<bool> onChange)
        {
            int height = string.IsNullOrEmpty(subtitle) ? 54 : 64;
            var card = CreateCard(y, cardWidth, height);

            var titleLbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.8f, FontStyle.Bold),
                ForeColor = _textPrimary,
                AutoSize = true,
                Location = new Point(_isArabic ? cardWidth - 320 : 16, string.IsNullOrEmpty(subtitle) ? 14 : 10)
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
                    Location = new Point(_isArabic ? cardWidth - 480 : 16, 32)
                };
                card.Controls.Add(subLbl);
            }

            var toggle = new WinUIToggleSwitch
            {
                Checked = isChecked,
                IsDark = _isDark,
                Location = new Point(_isArabic ? 20 : cardWidth - 65, string.IsNullOrEmpty(subtitle) ? 14 : 18)
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
