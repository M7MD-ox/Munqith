using System.Runtime.InteropServices;
using Munqith.Interop;

namespace Munqith.Services
{
    public class TextSelectionService : ITextSelectionService
    {
        private const int KeyPressDelayMs = 15;
        private readonly INativeClipboard _clipboard;

        public TextSelectionService(INativeClipboard? clipboard = null)
        {
            _clipboard = clipboard ?? new NativeClipboard();
        }

        public void ReleaseModifiers()
        {
            ushort[] keysToRelease =
            {
                NativeMethods.VK_CONTROL,
                NativeMethods.VK_LCONTROL,
                NativeMethods.VK_RCONTROL,
                NativeMethods.VK_MENU,
                NativeMethods.VK_LMENU,
                NativeMethods.VK_RMENU,
                NativeMethods.VK_SHIFT,
                NativeMethods.VK_LSHIFT,
                NativeMethods.VK_RSHIFT,
                NativeMethods.VK_Q
            };

            SendKeyBatch(keysToRelease, keyUp: true);
        }

        public async Task<string?> TryCopyCurrentSelection()
        {
            // Wait briefly if the user is still physically holding the Alt key so it doesn't collide with Ctrl+C
            await WaitForPhysicalAltReleaseAsync(maxWaitMs: 120);

            _clipboard.Clear();
            ReleaseModifiers();
            await Task.Delay(KeyPressDelayMs);

            // Send Ctrl + C atomically via SendInput
            SendKeyChord(NativeMethods.VK_CONTROL, NativeMethods.VK_C);

            await Task.Delay(KeyPressDelayMs);
            return _clipboard.GetText(maxRetries: 8, delayMs: 10);
        }

        public async Task SimulatePaste()
        {
            ReleaseModifiers();
            await Task.Delay(KeyPressDelayMs);

            // Send Ctrl + V atomically via SendInput
            SendKeyChord(NativeMethods.VK_CONTROL, NativeMethods.VK_V);

            await Task.Delay(KeyPressDelayMs);
        }

        /// <summary>
        /// Only converts text explicitly highlighted by the user.
        /// Does not perform destructive line or word auto-selection fallbacks.
        /// </summary>
        public async Task<string?> GetTextForConversion()
        {
            return await TryCopyCurrentSelection();
        }

        private static async Task WaitForPhysicalAltReleaseAsync(int maxWaitMs)
        {
            int elapsed = 0;
            const int stepMs = 10;

            while (elapsed < maxWaitMs)
            {
                bool altDown = (NativeMethods.GetAsyncKeyState(NativeMethods.VK_MENU) & 0x8000) != 0 ||
                               (NativeMethods.GetAsyncKeyState(NativeMethods.VK_LMENU) & 0x8000) != 0 ||
                               (NativeMethods.GetAsyncKeyState(NativeMethods.VK_RMENU) & 0x8000) != 0;

                if (!altDown) break;

                await Task.Delay(stepMs);
                elapsed += stepMs;
            }
        }

        private static void SendKeyChord(ushort modifierVk, ushort keyVk)
        {
            var inputs = new NativeMethods.INPUT[]
            {
                CreateKeyInput(modifierVk, keyUp: false),
                CreateKeyInput(keyVk, keyUp: false),
                CreateKeyInput(keyVk, keyUp: true),
                CreateKeyInput(modifierVk, keyUp: true)
            };

            uint sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
            if (sent == 0)
            {
                // Fallback if SendInput is blocked by UIPI
                NativeMethods.keybd_event((byte)modifierVk, 0, 0, UIntPtr.Zero);
                NativeMethods.keybd_event((byte)keyVk, 0, 0, UIntPtr.Zero);
                NativeMethods.keybd_event((byte)keyVk, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
                NativeMethods.keybd_event((byte)modifierVk, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
        }

        private static void SendKeyBatch(ushort[] vks, bool keyUp)
        {
            var inputs = new NativeMethods.INPUT[vks.Length];
            for (int i = 0; i < vks.Length; i++)
            {
                inputs[i] = CreateKeyInput(vks[i], keyUp);
            }

            uint sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
            if (sent == 0)
            {
                uint flags = keyUp ? NativeMethods.KEYEVENTF_KEYUP : 0;
                foreach (var vk in vks)
                {
                    NativeMethods.keybd_event((byte)vk, 0, flags, UIntPtr.Zero);
                }
            }
        }

        private static NativeMethods.INPUT CreateKeyInput(ushort vk, bool keyUp)
        {
            return new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                U = new NativeMethods.INPUTUNION
                {
                    ki = new NativeMethods.KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = 0,
                        dwFlags = keyUp ? NativeMethods.KEYEVENTF_KEYUP : 0,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };
        }
    }
}
