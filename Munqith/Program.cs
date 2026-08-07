using Munqith.UI;

namespace Munqith
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Ensure only one instance runs
            using var mutex = new Mutex(true, "MunqithSingleInstance", out bool isNewInstance);
            if (!isNewInstance)
            {
                MessageBox.Show(
                    "مُنقِذ Munqith is already running!",
                    "Munqith",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new TrayApplicationContext());
        }
    }
}