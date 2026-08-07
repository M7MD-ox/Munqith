using System.Runtime.InteropServices;
using Munqith.Hooks;
using Munqith.Models;

namespace Munqith.Core
{
    public static class TextSelectionHelper
    {
        private const int KEY_DELAY_MS = 30;

        /// <summary>
        /// Release physical modifier keys if held so SendInput keyboard shortcuts fire cleanly
        /// </summary>
        public static void ReleaseModifiers()
        {
            var inputs = new List<NativeMethods.INPUT>();

            if ((NativeMethods.GetKeyState((int)Keys.ControlKey) & 0x8000) != 0)
                inputs.Add(CreateKeyInput(NativeMethods.VK_CONTROL, true));

            if ((NativeMethods.GetKeyState((int)Keys.Menu) & 0x8000) != 0)
                inputs.Add(CreateKeyInput(NativeMethods.VK_MENU, true));

            if ((NativeMethods.GetKeyState((int)Keys.ShiftKey) & 0x8000) != 0)
                inputs.Add(CreateKeyInput(NativeMethods.VK_SHIFT, true));

            if (inputs.Count > 0)
            {
                NativeMethods.SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<NativeMethods.INPUT>());
            }
        }

        public static async Task SimulateCopy()
        {
            ReleaseModifiers();
            await Task.Delay(KEY_DELAY_MS);
            SendKeyCombo(NativeMethods.VK_CONTROL, NativeMethods.VK_C);
            await Task.Delay(KEY_DELAY_MS * 3);
        }

        public static async Task SimulatePaste()
        {
            ReleaseModifiers();
            await Task.Delay(KEY_DELAY_MS);
            SendKeyCombo(NativeMethods.VK_CONTROL, NativeMethods.VK_V);
            await Task.Delay(KEY_DELAY_MS);
        }

        public static async Task SimulateSelectAll()
        {
            ReleaseModifiers();
            await Task.Delay(KEY_DELAY_MS);
            SendKeyCombo(NativeMethods.VK_CONTROL, NativeMethods.VK_A);
            await Task.Delay(KEY_DELAY_MS);
        }

        public static async Task SimulateSelectLastWord()
        {
            ReleaseModifiers();
            await Task.Delay(KEY_DELAY_MS);

            var inputs = new NativeMethods.INPUT[6];
            inputs[0] = CreateKeyInput(NativeMethods.VK_CONTROL, false);
            inputs[1] = CreateKeyInput(NativeMethods.VK_SHIFT, false);
            inputs[2] = CreateKeyInput(NativeMethods.VK_LEFT, false);
            inputs[3] = CreateKeyInput(NativeMethods.VK_LEFT, true);
            inputs[4] = CreateKeyInput(NativeMethods.VK_SHIFT, true);
            inputs[5] = CreateKeyInput(NativeMethods.VK_CONTROL, true);

            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
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
            return ClipboardHelper.GetText();
        }

        private static void SendKeyCombo(ushort modifier, ushort key)
        {
            var inputs = new NativeMethods.INPUT[4];
            inputs[0] = CreateKeyInput(modifier, false);
            inputs[1] = CreateKeyInput(key, false);
            inputs[2] = CreateKeyInput(key, true);
            inputs[3] = CreateKeyInput(modifier, true);
            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
        }

        private static NativeMethods.INPUT CreateKeyInput(ushort vkCode, bool keyUp)
        {
            return new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                u = new NativeMethods.INPUTUNION
                {
                    ki = new NativeMethods.KEYBDINPUT
                    {
                        wVk = vkCode,
                        wScan = 0,
                        dwFlags = keyUp ? NativeMethods.KEYEVENTF_KEYUP : 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
        }
    }
}
