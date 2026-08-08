using System.Diagnostics;
using System.Runtime.InteropServices;
using Munqith.Interop;
using Munqith.Models;

namespace Munqith.Hooks
{
    public class LowLevelKeyboardHook : IKeyboardHook
    {
        private NativeMethods.HookProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public event Action<HotkeyBinding>? HotkeyTriggered;

        private readonly List<HotkeyBinding> _bindings = new();

        public LowLevelKeyboardHook()
        {
            _proc = HookCallback;
        }

        public void UpdateBindings(IEnumerable<HotkeyBinding> bindings)
        {
            lock (_bindings)
            {
                _bindings.Clear();
                _bindings.AddRange(bindings.Where(b => b.IsEnabled));
            }
        }

        public void Start()
        {
            if (_hookID == IntPtr.Zero)
            {
                IntPtr hInstance = NativeMethods.GetModuleHandle("user32.dll");
                if (hInstance == IntPtr.Zero)
                {
                    using Process curProcess = Process.GetCurrentProcess();
                    using ProcessModule curModule = curProcess.MainModule!;
                    hInstance = NativeMethods.GetModuleHandle(curModule.ModuleName);
                }

                _hookID = NativeMethods.SetWindowsHookEx(
                    NativeMethods.WH_KEYBOARD_LL,
                    _proc,
                    hInstance,
                    0);
            }
        }

        public void Stop()
        {
            if (_hookID != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            int message = (int)wParam;
            if (nCode >= 0 && (message == NativeMethods.WM_KEYDOWN || message == NativeMethods.WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                // Check physical modifier state asynchronously via GetAsyncKeyState
                bool ctrl = (NativeMethods.GetAsyncKeyState(NativeMethods.VK_CONTROL_B) & 0x8000) != 0;
                bool alt = (NativeMethods.GetAsyncKeyState(NativeMethods.VK_MENU_B) & 0x8000) != 0 || message == NativeMethods.WM_SYSKEYDOWN;
                bool shift = (NativeMethods.GetAsyncKeyState(NativeMethods.VK_SHIFT_B) & 0x8000) != 0;

                int currentModifiers = 0;
                if (alt) currentModifiers |= 1;
                if (ctrl) currentModifiers |= 2;
                if (shift) currentModifiers |= 4;

                HotkeyBinding? matchedBinding = null;

                lock (_bindings)
                {
                    foreach (var binding in _bindings)
                    {
                        if (binding.IsEnabled &&
                            binding.ModifierKeysValue == currentModifiers &&
                            binding.KeyValue == (int)key)
                        {
                            matchedBinding = binding;
                            break;
                        }
                    }
                }

                if (matchedBinding != null)
                {
                    Task.Run(() => HotkeyTriggered?.Invoke(matchedBinding));
                    return (IntPtr)1; // Consume key
                }
            }

            return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
