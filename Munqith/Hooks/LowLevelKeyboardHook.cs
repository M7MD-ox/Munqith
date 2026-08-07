using System.Diagnostics;
using System.Runtime.InteropServices;
using Munqith.Models;

namespace Munqith.Hooks
{
    public class LowLevelKeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0200;

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
                // In single-file .NET 8 apps, user32.dll module handle is guaranteed to be valid for global WH_KEYBOARD_LL hooks
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
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                bool ctrl = (NativeMethods.GetKeyState((int)Keys.ControlKey) & 0x8000) != 0;
                bool alt = (NativeMethods.GetKeyState((int)Keys.Menu) & 0x8000) != 0;
                bool shift = (NativeMethods.GetKeyState((int)Keys.ShiftKey) & 0x8000) != 0;

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
                    return (IntPtr)1;
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
