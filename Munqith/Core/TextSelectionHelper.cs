using Munqith.Hooks;
using Munqith.Models;

namespace Munqith.Core
{
    public static class TextSelectionHelper
    {
        private const int MICRO_DELAY_MS = 6;
        private const byte VK_HOME_B = 0x24;

        public static void ReleaseModifiers()
        {
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_MENU_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_Q_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public static async Task<string?> TryCopyCurrentSelection()
        {
            NativeClipboard.Clear();
            ReleaseModifiers();
            await Task.Delay(MICRO_DELAY_MS);

            // Press Ctrl + C
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_C_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);
            NativeMethods.keybd_event(NativeMethods.VK_C_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(MICRO_DELAY_MS * 2);
            return NativeClipboard.GetText(maxRetries: 4, delayMs: 6);
        }

        public static async Task SimulatePaste()
        {
            ReleaseModifiers();
            await Task.Delay(MICRO_DELAY_MS);

            // Press Ctrl + V
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_V_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);
            NativeMethods.keybd_event(NativeMethods.VK_V_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(MICRO_DELAY_MS);
        }

        public static async Task<string?> TrySelectAndCopyLine()
        {
            NativeClipboard.Clear();
            ReleaseModifiers();
            await Task.Delay(MICRO_DELAY_MS);

            // Press Shift + Home (Select entire line from start to cursor)
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(VK_HOME_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);
            NativeMethods.keybd_event(VK_HOME_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);

            // Copy
            return await TryCopyCurrentSelection();
        }

        public static async Task<string?> TrySelectAndCopyLastWord()
        {
            NativeClipboard.Clear();
            ReleaseModifiers();
            await Task.Delay(MICRO_DELAY_MS);

            // Press Ctrl + Shift + Left (Select last word before cursor)
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_LEFT_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);
            NativeMethods.keybd_event(NativeMethods.VK_LEFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);

            // Copy
            return await TryCopyCurrentSelection();
        }

        public static async Task<string?> TrySelectAndCopyAll()
        {
            NativeClipboard.Clear();
            ReleaseModifiers();
            await Task.Delay(MICRO_DELAY_MS);

            // Press Ctrl + A
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_A_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);
            NativeMethods.keybd_event(NativeMethods.VK_A_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);

            return await TryCopyCurrentSelection();
        }

        /// <summary>
        /// Captures text based on conversion mode with intelligent empty clipboard detection.
        /// </summary>
        public static async Task<string?> GetTextForConversion(ConversionMode mode)
        {
            switch (mode)
            {
                case ConversionMode.SelectedText:
                    // 1. Try copying selected text
                    string? text = await TryCopyCurrentSelection();
                    // 2. If nothing was selected, fallback to line then word
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        text = await TrySelectAndCopyLine();
                    }
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        text = await TrySelectAndCopyLastWord();
                    }
                    return text;

                case ConversionMode.AllText:
                    // Current line mode
                    string? lineText = await TrySelectAndCopyLine();
                    if (string.IsNullOrWhiteSpace(lineText))
                    {
                        lineText = await TrySelectAndCopyAll();
                    }
                    return lineText;

                case ConversionMode.LastWord:
                    // Last word mode
                    return await TrySelectAndCopyLastWord();

                default:
                    return await TryCopyCurrentSelection();
            }
        }
    }
}
