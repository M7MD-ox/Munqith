using System.Runtime.InteropServices;

namespace Munqith.Core
{
    public static class NativeClipboard
    {
        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVEABLE = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalFree(IntPtr hMem);

        /// <summary>
        /// Reads Unicode text from Windows clipboard with automatic retry loop.
        /// Completely avoids WinForms/WPF STA thread limitations and COMExceptions.
        /// </summary>
        public static string? GetText(int maxRetries = 10, int delayMs = 35)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        IntPtr handle = GetClipboardData(CF_UNICODETEXT);
                        if (handle != IntPtr.Zero)
                        {
                            IntPtr pointer = GlobalLock(handle);
                            if (pointer != IntPtr.Zero)
                            {
                                try
                                {
                                    string? text = Marshal.PtrToStringUni(pointer);
                                    if (!string.IsNullOrEmpty(text))
                                    {
                                        return text;
                                    }
                                }
                                finally
                                {
                                    GlobalUnlock(handle);
                                }
                            }
                        }
                    }
                    finally
                    {
                        CloseClipboard();
                    }
                }

                Thread.Sleep(delayMs);
            }

            return null;
        }

        /// <summary>
        /// Writes Unicode text directly to Windows clipboard with retry loop.
        /// </summary>
        public static bool SetText(string text, int maxRetries = 10, int delayMs = 35)
        {
            if (string.IsNullOrEmpty(text)) return false;

            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(text + "\0");
            UIntPtr size = (UIntPtr)bytes.Length;

            for (int i = 0; i < maxRetries; i++)
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        EmptyClipboard();

                        IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE, size);
                        if (hGlobal != IntPtr.Zero)
                        {
                            IntPtr target = GlobalLock(hGlobal);
                            if (target != IntPtr.Zero)
                            {
                                Marshal.Copy(bytes, 0, target, bytes.Length);
                                GlobalUnlock(hGlobal);

                                if (SetClipboardData(CF_UNICODETEXT, hGlobal) != IntPtr.Zero)
                                {
                                    return true; // Successfully placed on clipboard
                                }
                            }

                            GlobalFree(hGlobal);
                        }
                    }
                    finally
                    {
                        CloseClipboard();
                    }
                }

                Thread.Sleep(delayMs);
            }

            return false;
        }
    }
}
