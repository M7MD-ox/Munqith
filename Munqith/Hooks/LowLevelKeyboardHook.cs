using System.Diagnostics;
using System.Runtime.InteropServices;
using Munqith.Models;

namespace Munqith.Hooks
{
    public class LowLevelKeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104; // Official Windows Win32 WM_SYSKEYDOWN (Fix for Alt+Key combinations)

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
                    WH_KEYBOARD_LL,
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
            if (nCode >= 0 && (message == WM_KEYDOWN || message == WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                // Check physical modifier state asynchronously via GetAsyncKeyState
                bool ctrl = (NativeMethods.GetAsyncKeyState(0x11) & 0x8000) != 0; // VK_CONTROL
                bool alt = (NativeMethods.GetAsyncKeyState(0x12) & 0x8000) != 0 || message == WM_SYSKEYDOWN; // VK_MENU (Alt)
                bool shift = (NativeMethods.GetAsyncKeyState(0x10) & 0x8000) != 0; // VK_SHIFT

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
                    return (IntPtr)1; // Suppress key stroke to prevent double insertion in target app
                }
            }

            return NativeMethods.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
