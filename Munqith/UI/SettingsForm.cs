using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using Munqith.Services;

namespace Munqith.UI
{
    public class SettingsForm : Form
    {
        private readonly BlazorWebView _blazorWebView;

        public SettingsForm(SettingsService settingsService, StatsService statsService, TrayApplicationContext trayContext)
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

            var services = new ServiceCollection();
            services.AddWindowsFormsBlazorWebView();
            services.AddSingleton(settingsService);
            services.AddSingleton(statsService);
            services.AddSingleton(trayContext);

            _blazorWebView = new BlazorWebView
            {
                Dock = DockStyle.Fill,
                HostPage = "wwwroot/index.html",
                Services = services.BuildServiceProvider()
            };

            _blazorWebView.RootComponents.Add<Munqith.UI.Blazor.MunqithDashboard>("#app");
            this.Controls.Add(_blazorWebView);
        }
    }
}
