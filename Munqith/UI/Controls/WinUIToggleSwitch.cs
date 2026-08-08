using System.Drawing.Drawing2D;

namespace Munqith.UI.Controls
{
    public class WinUIToggleSwitch : Control
    {
        private bool _checked = false;
        private bool _isDark = false;
        private readonly Color _accentColor = Color.FromArgb(0, 120, 212); // Windows 11 Accent Blue
        private readonly Color _accentHover = Color.FromArgb(16, 110, 190);

        public event EventHandler? CheckedChanged;

        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    this.Invalidate();
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsDark
        {
            get => _isDark;
            set
            {
                _isDark = value;
                this.Invalidate();
            }
        }

        public WinUIToggleSwitch()
        {
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.ResizeRedraw, true);

            this.Size = new Size(46, 24);
            this.Cursor = Cursors.Hand;
            this.BackColor = Color.Transparent;
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int width = 44;
            int height = 22;
            var rect = new Rectangle(1, 1, width, height);

            Color trackColor;
            Color borderColor;
            Color thumbColor;

            if (_checked)
            {
                trackColor = _accentColor;
                borderColor = _accentColor;
                thumbColor = Color.White;
            }
            else
            {
                trackColor = _isDark ? Color.FromArgb(45, 45, 45) : Color.FromArgb(240, 240, 240);
                borderColor = _isDark ? Color.FromArgb(130, 130, 130) : Color.FromArgb(140, 140, 140);
                thumbColor = _isDark ? Color.FromArgb(210, 210, 210) : Color.FromArgb(90, 90, 90);
            }

            // Draw pill track
            using var trackPath = CreatePillPath(rect, height);
            using var trackBrush = new SolidBrush(trackColor);
            g.FillPath(trackBrush, trackPath);

            using var borderPen = new Pen(borderColor, 1.2f);
            g.DrawPath(borderPen, trackPath);

            // Draw circular thumb
            int thumbRadius = height - 8;
            int thumbX = _checked ? (rect.Right - thumbRadius - 4) : (rect.Left + 4);
            int thumbY = rect.Top + 4;
            var thumbRect = new Rectangle(thumbX, thumbY, thumbRadius, thumbRadius);

            using var thumbBrush = new SolidBrush(thumbColor);
            g.FillEllipse(thumbBrush, thumbRect);
        }

        private static GraphicsPath CreatePillPath(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius;
            var arc = new Rectangle(r.Location, new Size(diameter, diameter));

            // Left arc
            path.AddArc(arc, 90, 180);

            // Top line
            arc.X = r.Right - diameter;
            // Right arc
            path.AddArc(arc, 270, 180);

            path.CloseFigure();
            return path;
        }
    }
}
