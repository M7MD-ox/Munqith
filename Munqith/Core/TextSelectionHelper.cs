using Munqith.Hooks;
using Munqith.Models;

namespace Munqith.Core
{
    public static class TextSelectionHelper
    {
        private const int KEY_DELAY_MS = 40;

        /// <summary>
        /// Release physical modifier keys if held so key simulation executes cleanly.
        /// </summary>
        public static void ReleaseModifiers()
        {
            // Release Control
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            // Release Alt
            NativeMethods.keybd_event(NativeMethods.VK_MENU_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            // Release Shift
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            // Release Q
            NativeMethods.keybd_event(NativeMethods.VK_Q_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            Thread.Sleep(KEY_DELAY_MS);
        }

        public static async Task SimulateCopy()
        {
            ReleaseModifiers();
            await Task.Delay(KEY_DELAY_MS);

            // Press Ctrl + C
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_C_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(25);
            NativeMethods.keybd_event(NativeMethods.VK_C_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(KEY_DELAY_MS * 2);
        }

        public static async Task SimulatePaste()
        {
            ReleaseModifiers();
            await Task.Delay(KEY_DELAY_MS);

            // Press Ctrl + V
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_V_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(25);
            NativeMethods.keybd_event(NativeMethods.VK_V_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(KEY_DELAY_MS);
        }

        public static async Task SimulateSelectAll()
        {
            ReleaseModifiers();
            await Task.Delay(KEY_DELAY_MS);

            // Press Ctrl + A
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_A_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(25);
            NativeMethods.keybd_event(NativeMethods.VK_A_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(KEY_DELAY_MS);
        }

        public static async Task SimulateSelectLastWord()
        {
            ReleaseModifiers();
            await Task.Delay(KEY_DELAY_MS);

            // Press Ctrl + Shift + Left
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_LEFT_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(25);
            NativeMethods.keybd_event(NativeMethods.VK_LEFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(KEY_DELAY_MS);
        }

        public static async Task<string?> GetTextForConversion(ConversionMode mode)
        {
            switch (mode)
            {
                case ConversionMode.SelectedText:
                    await SimulateCopy();
                    break;
                case ConversionMode.AllText:
                    await SimulateSelectAll();
                    await SimulateCopy();
                    break;
                case ConversionMode.LastWord:
                    await SimulateSelectLastWord();
                    await SimulateCopy();
                    break;
            }

            return NativeClipboard.GetText(maxRetries: 10, delayMs: 30);
        }
    }
}
