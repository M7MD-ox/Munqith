using Munqith.Hooks;
using Munqith.Models;

namespace Munqith.Core
{
    public static class TextSelectionHelper
    {
        private const int MICRO_DELAY_MS = 6;

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
        }

        public static async Task SimulateSelectAll()
        {
            ReleaseModifiers();
            await Task.Delay(MICRO_DELAY_MS);

            // Press Ctrl + A
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_A_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);
            NativeMethods.keybd_event(NativeMethods.VK_A_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(MICRO_DELAY_MS);
        }

        public static async Task SimulateSelectLastWord()
        {
            ReleaseModifiers();
            await Task.Delay(MICRO_DELAY_MS);

            // Press Ctrl + Shift + Left
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, 0, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_LEFT_B, 0, 0, UIntPtr.Zero);
            await Task.Delay(MICRO_DELAY_MS);
            NativeMethods.keybd_event(NativeMethods.VK_LEFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            NativeMethods.keybd_event(NativeMethods.VK_CONTROL_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);

            await Task.Delay(MICRO_DELAY_MS);
        }

        /// <summary>
        /// Selects N characters backward from the current cursor position.
        /// </summary>
        public static async Task SimulateSelectCharsBackward(int count)
        {
            ReleaseModifiers();
            await Task.Delay(MICRO_DELAY_MS);

            for (int i = 0; i < count; i++)
            {
                NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, 0, UIntPtr.Zero);
                NativeMethods.keybd_event(NativeMethods.VK_LEFT_B, 0, 0, UIntPtr.Zero);
                NativeMethods.keybd_event(NativeMethods.VK_LEFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
                NativeMethods.keybd_event(NativeMethods.VK_SHIFT_B, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            }

            await Task.Delay(MICRO_DELAY_MS);
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

            return NativeClipboard.GetText(maxRetries: 5, delayMs: 8);
        }
    }
}
