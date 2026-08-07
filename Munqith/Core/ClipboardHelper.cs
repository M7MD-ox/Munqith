namespace Munqith.Core
{
    public static class ClipboardHelper
    {
        public static string? GetText()
        {
            try
            {
                if (Clipboard.ContainsText())
                    return Clipboard.GetText();
                return null;
            }
            catch { return null; }
        }

        public static void SetText(string text)
        {
            try { Clipboard.SetText(text); }
            catch { }
        }

        public static IDataObject? BackupClipboard()
        {
            try { return Clipboard.GetDataObject(); }
            catch { return null; }
        }

        public static void RestoreClipboard(IDataObject? backup)
        {
            try
            {
                if (backup != null)
                    Clipboard.SetDataObject(backup, true);
            }
            catch { }
        }
    }
}
