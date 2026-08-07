namespace Munqith.UI
{
    public class AboutForm : Form
    {
        public AboutForm(string language, bool darkMode)
        {
            bool isArabic = language == "ar";
            var bgColor = darkMode ? Color.FromArgb(30, 30, 30) : Color.White;
            var textColor = darkMode ? Color.FromArgb(224, 224, 224) : Color.FromArgb(26, 26, 26);
            var secondaryColor = darkMode ? Color.FromArgb(160, 160, 160) : Color.FromArgb(100, 100, 100);
            var accentColor = Color.FromArgb(0, 120, 212);

            this.Text = isArabic ? "عن مُنقِذ" : "About Munqith";
            this.Size = new Size(420, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 10);

            int xStart = 30;

            var nameLabel = new Label
            {
                Text = "مُنقِذ Munqith",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = accentColor,
                AutoSize = true,
                Location = new Point(xStart, 25)
            };
            this.Controls.Add(nameLabel);

            var versionLabel = new Label
            {
                Text = "v1.0.0",
                Font = new Font("Segoe UI", 10),
                ForeColor = secondaryColor,
                AutoSize = true,
                Location = new Point(xStart, 65)
            };
            this.Controls.Add(versionLabel);

            var descLabel = new Label
            {
                Text = isArabic
                    ? "أداة ويندوز خفيفة لتحويل النص المكتوب بتخطيط كيبورد غلط.\nمستوحاة من إضافة شقلب لكروم."
                    : "Lightweight Windows tool for converting text\ntyped with the wrong keyboard layout.\nInspired by Sha2lab Chrome extension.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = textColor,
                AutoSize = true,
                MaximumSize = new Size(360, 0),
                Location = new Point(xStart, 100)
            };
            this.Controls.Add(descLabel);

            var shortcutLabel = new Label
            {
                Text = isArabic
                    ? "الاختصارات الافتراضية: Ctrl+Q أو Alt+Q"
                    : "Default shortcuts: Ctrl+Q or Alt+Q",
                Font = new Font("Segoe UI", 9),
                ForeColor = secondaryColor,
                AutoSize = true,
                Location = new Point(xStart, 170)
            };
            this.Controls.Add(shortcutLabel);

            var linkLabel = new LinkLabel
            {
                Text = "GitHub Repository",
                Font = new Font("Segoe UI", 9.5f),
                AutoSize = true,
                Location = new Point(xStart, 200),
                LinkColor = accentColor,
                ActiveLinkColor = Color.FromArgb(0, 90, 180)
            };
            linkLabel.Click += (s, e) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://github.com",
                        UseShellExecute = true
                    });
                }
                catch { }
            };
            this.Controls.Add(linkLabel);

            var closeBtn = new Button
            {
                Text = isArabic ? "إغلاق" : "Close",
                Size = new Size(110, 38),
                Location = new Point((420 - 110) / 2, 240),
                FlatStyle = FlatStyle.Flat,
                BackColor = accentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += (s, e) => this.Close();
            this.Controls.Add(closeBtn);
        }
    }
}
