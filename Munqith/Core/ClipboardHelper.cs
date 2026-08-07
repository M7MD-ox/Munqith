namespace Munqith.Core
{
    public static class ClipboardHelper
    {
        /// <summary>
        /// Gets text from clipboard safely using an STA thread.
        /// WinForms Clipboard requires STA thread apartment state.
        /// </summary>
        public static string? GetText()
        {
            string? result = null;
            var thread = new Thread(() =>
            {
                try
                {
                    if (Clipboard.ContainsText())
                        result = Clipboard.GetText();
                }
                catch { }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(500); // 500ms timeout
            return result;
        }

        /// <summary>
        /// Sets text to clipboard safely using an STA thread.
        /// </summary>
        public static void SetText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            var thread = new Thread(() =>
            {
                try
                {
                    Clipboard.SetText(text);
                }
                catch { }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(500);
        }

        public static IDataObject? BackupClipboard()
        {
            IDataObject? backup = null;
            var thread = new Thread(() =>
            {
                try
                {
                    backup = Clipboard.GetDataObject();
                }
                catch { }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(500);
            return backup;
        }

        public static void RestoreClipboard(IDataObject? backup)
        {
            if (backup == null) return;
            var thread = new Thread(() =>
            {
                try
                {
                    Clipboard.SetDataObject(backup, true);
                }
                catch { }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(500);
        }
    }
}
