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

        public static async Task SimulateCopy()
        {
            ReleaseModifiers();
            await Task.Delay(MICRO_DELAY_MS);

            // Press Ctrl + C
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_C_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);
            NativeMethods.keybd_event(NativeMethods.VK_C_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(MICRO_DELAY_MS * 2);
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

        public static async Task SimulateSelectLine()
        {
            ReleaseModifiers();
            await Task.Delay(MICRO_DELAY_MS);

            // Press Shift + Home (Select entire line from start to cursor)
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(VK_HOME_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);
            NativeMethods.keybd_event(VK_HOME_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(MICRO_DELAY_MS);
        }

        public static async Task SimulateSelectLastWord()
        {
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
        }

        public static async Task SimulateSelectAll()
        {
            ReleaseModifiers();
            await Task.Delay(MICRO_DELAY_MS);

            // Press Ctrl + A (Select all text)
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_A_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);
            NativeMethods.keybd_event(NativeMethods.VK_A_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(MICRO_DELAY_MS);
        }

        /// <summary>
        /// Attempts to get text for conversion based on configured mode.
        /// </summary>
        public static async Task<string?> GetTextForConversion(ConversionMode mode)
        {
            switch (mode)
            {
                case ConversionMode.SelectedText:
                    // Try copying current selection first
                    await SimulateCopy();
                    break;
                case ConversionMode.AllText:
                    // Select current line or whole text
                    await SimulateSelectLine();
                    await SimulateCopy();
                    break;
                case ConversionMode.LastWord:
                    // Select last word
                    await SimulateSelectLastWord();
                    await SimulateCopy();
                    break;
            }

            return NativeClipboard.GetText(maxRetries: 5, delayMs: 8);
        }
    }
}
