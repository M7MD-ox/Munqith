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

            // Emit a Ctrl tap before releasing Alt so Windows suppresses SC_KEYMENU menu-bar focus activation.
            var inputs = new NativeMethods.INPUT[keysToRelease.Length + 1];
            inputs[0] = CreateKeyInput(NativeMethods.VK_CONTROL, keyUp: false);
            for (int i = 0; i < keysToRelease.Length; i++)
            {
                inputs[i + 1] = CreateKeyInput(keysToRelease[i], keyUp: true);
            }

            uint sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
            if (sent == 0)
            {
                NativeMethods.keybd_event((byte)NativeMethods.VK_CONTROL, 0, 0, UIntPtr.Zero);
                foreach (var vk in keysToRelease)
                {
                    NativeMethods.keybd_event((byte)vk, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
                }
            }
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
        /// Captures highlighted text; when autoSelectLastWord is enabled and nothing is selected,
        /// selects the word immediately before the caret via Ctrl+Shift+Left and copies it.
        /// </summary>
        public async Task<string?> GetTextForConversion(bool autoSelectLastWord = false)
        {
            string? selected = await TryCopyCurrentSelection();
            if (!string.IsNullOrEmpty(selected) || !autoSelectLastWord)
            {
                return selected;
            }

            // Select the previous word without stealing focus, then copy it.
            ReleaseModifiers();
            await Task.Delay(KeyPressDelayMs);
            SendCtrlShiftLeft();
            await Task.Delay(KeyPressDelayMs);
            return await TryCopyCurrentSelection();
        }

        private static void SendCtrlShiftLeft()
        {
            var inputs = new NativeMethods.INPUT[]
            {
                CreateKeyInput(NativeMethods.VK_CONTROL, keyUp: false),
                CreateKeyInput(NativeMethods.VK_SHIFT, keyUp: false),
                CreateKeyInput(NativeMethods.VK_LEFT, keyUp: false, isExtended: true),
                CreateKeyInput(NativeMethods.VK_LEFT, keyUp: true, isExtended: true),
                CreateKeyInput(NativeMethods.VK_SHIFT, keyUp: true),
                CreateKeyInput(NativeMethods.VK_CONTROL, keyUp: true)
            };

            uint sent = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
            if (sent == 0)
            {
                NativeMethods.keybd_event((byte)NativeMethods.VK_CONTROL, 0, 0, UIntPtr.Zero);
                NativeMethods.keybd_event((byte)NativeMethods.VK_SHIFT, 0, 0, UIntPtr.Zero);
                NativeMethods.keybd_event((byte)NativeMethods.VK_LEFT, 0, NativeMethods.KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
                NativeMethods.keybd_event((byte)NativeMethods.VK_LEFT, 0, NativeMethods.KEYEVENTF_EXTENDEDKEY | NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
                NativeMethods.keybd_event((byte)NativeMethods.VK_SHIFT, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
                NativeMethods.keybd_event((byte)NativeMethods.VK_CONTROL, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
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

        private static NativeMethods.INPUT CreateKeyInput(ushort vk, bool keyUp, bool isExtended = false)
        {
            uint flags = keyUp ? NativeMethods.KEYEVENTF_KEYUP : 0;
            if (isExtended) flags |= NativeMethods.KEYEVENTF_EXTENDEDKEY;

            return new NativeMethods.INPUT
            {
                type = NativeMethods.INPUT_KEYBOARD,
                U = new NativeMethods.INPUTUNION
                {
                    ki = new NativeMethods.KEYBDINPUT
                    {
                        wVk = vk,
                        wScan = 0,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };
        }
    }
}
