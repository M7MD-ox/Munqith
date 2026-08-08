using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Munqith.Services;

namespace Munqith.UI
{
    public class SettingsForm : Form
    {
        private readonly BlazorWebView _blazorWebView;

        public SettingsForm(IServiceProvider serviceProvider)
        {
            this.Text = "مُنقِذ Munqith";
            this.ClientSize = new Size(780, 740);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(253, 248, 243);

            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "app.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
            }
            catch { }

            _blazorWebView = new BlazorWebView
            {
                Dock = DockStyle.Fill,
                HostPage = "wwwroot/index.html",
                Services = serviceProvider
            };

            _blazorWebView.RootComponents.Add<Munqith.UI.Blazor.MunqithDashboard>("#app");
            this.Controls.Add(_blazorWebView);
        }
    }
}
