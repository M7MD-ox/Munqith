namespace Munqith.Interop
{
    public interface INativeClipboard
    {
        void Clear(int maxRetries = 5, int delayMs = 5);
        string? GetText(int maxRetries = 6, int delayMs = 8);
        bool SetText(string text, int maxRetries = 5, int delayMs = 6);
    }
}
