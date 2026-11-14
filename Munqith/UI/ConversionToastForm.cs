using System.Drawing.Drawing2D;

namespace Munqith.UI
{
    /// <summary>
    /// Non-activating lightweight toast overlay shown briefly after a conversion
    /// without stealing focus from the user's active application.
    /// </summary>
    public class ConversionToastForm : Form
    {
        private readonly System.Windows.Forms.Timer _closeTimer;
        private readonly Color _bgColor;
        private readonly Color _borderColor;
        private readonly Color _titleColor;
        private readonly Color _textColor;
        private readonly string _title;
        private readonly string _preview;
        private readonly bool _isRtl;

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW
                cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                return cp;
            }
        }

        public ConversionToastForm(string convertedText, bool isDark, bool isArabic)
        {
            _isRtl = isArabic;
            _title = isArabic ? "✓ تم الإنقاذ" : "✓ Rescued";

            string singleLine = convertedText.Replace("\r", " ").Replace("\n", " ").Trim();
            _preview = singleLine.Length > 44 ? singleLine[..44] + "..." : singleLine;

            if (isDark)
            {
                _bgColor = Color.FromArgb(32, 30, 28);
                _borderColor = Color.FromArgb(217, 119, 87);
                _titleColor = Color.FromArgb(217, 119, 87);
                _textColor = Color.FromArgb(250, 247, 242);
            }
            else
            {
                _bgColor = Color.FromArgb(253, 248, 243);
                _borderColor = Color.FromArgb(217, 119, 87);
                _titleColor = Color.FromArgb(193, 95, 60);
                _textColor = Color.FromArgb(31, 31, 31);
            }

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(290, 62);
            BackColor = _bgColor;
            DoubleBuffered = true;

            var workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
            Location = new Point(workingArea.Right - Width - 18, workingArea.Bottom - Height - 18);

            _closeTimer = new System.Windows.Forms.Timer { Interval = 1800 };
            _closeTimer.Tick += (_, _) =>
            {
                _closeTimer.Stop();
                Close();
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _closeTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var bgBrush = new SolidBrush(_bgColor);
            using var borderPen = new Pen(_borderColor, 1.5f);
            g.FillRectangle(bgBrush, rect);
            g.DrawRectangle(borderPen, rect);

            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine;
            if (_isRtl) flags |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;

            using var titleFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var textFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            var titleRect = new Rectangle(12, 8, Width - 24, 20);
            var textRect = new Rectangle(12, 30, Width - 24, 24);

            TextRenderer.DrawText(g, _title, titleFont, titleRect, _titleColor, flags);
            TextRenderer.DrawText(g, _preview, textFont, textRect, _textColor, flags);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _closeTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
