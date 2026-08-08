using Munqith.Interop;

namespace Munqith.Services
{
    public class TextSelectionService : ITextSelectionService
    {
        private const int MicroDelayMs = 6;
        private readonly INativeClipboard _clipboard;

        public TextSelectionService(INativeClipboard? clipboard = null)
        {
            _clipboard = clipboard ?? new NativeClipboard();
        }

        public void ReleaseModifiers()
        {
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_MENU_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_Q_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public async Task<string?> TryCopyCurrentSelection()
        {
            _clipboard.Clear();
            ReleaseModifiers();
            await Task.Delay(MicroDelayMs);

            // Press Ctrl + C
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_C_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MicroDelayMs);
            NativeMethods.keybd_event(NativeMethods.VK_C_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(MicroDelayMs * 2);
            return _clipboard.GetText(maxRetries: 4, delayMs: 6);
        }

        public async Task SimulatePaste()
        {
            ReleaseModifiers();
            await Task.Delay(MicroDelayMs);

            // Press Ctrl + V
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_V_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MicroDelayMs);
            NativeMethods.keybd_event(NativeMethods.VK_V_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(MicroDelayMs);
        }

        public async Task<string?> TrySelectAndCopyLine()
        {
            _clipboard.Clear();
            ReleaseModifiers();
            await Task.Delay(MicroDelayMs);

            // Press Shift + Home (Select entire line from start to cursor)
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_HOME_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MicroDelayMs);
            NativeMethods.keybd_event(NativeMethods.VK_HOME_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            await Task.Delay(MicroDelayMs);

            return await TryCopyCurrentSelection();
        }

        public async Task<string?> TrySelectAndCopyLastWord()
        {
            _clipboard.Clear();
            ReleaseModifiers();
            await Task.Delay(MicroDelayMs);

            // Press Ctrl + Shift + Left (Select last word before cursor)
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_LEFT_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MicroDelayMs);
            NativeMethods.keybd_event(NativeMethods.VK_LEFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            await Task.Delay(MicroDelayMs);

            return await TryCopyCurrentSelection();
        }

        public async Task<string?> GetTextForConversion()
        {
            // 1. Try copying selected text first
            string? text = await TryCopyCurrentSelection();

            // 2. Fallback to line
            if (string.IsNullOrWhiteSpace(text))
            {
                text = await TrySelectAndCopyLine();
            }

            // 3. Fallback to last word
            if (string.IsNullOrWhiteSpace(text))
            {
                text = await TrySelectAndCopyLastWord();
            }

            return text;
        }
    }
}
