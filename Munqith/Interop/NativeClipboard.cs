using System.Runtime.InteropServices;

namespace Munqith.Interop
{
    public class NativeClipboard : INativeClipboard
    {
        public void Clear(int maxRetries = 5, int delayMs = 5)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                if (NativeMethods.OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        NativeMethods.EmptyClipboard();
                        return;
                    }
                    finally
                    {
                        NativeMethods.CloseClipboard();
                    }
                }
                Thread.Sleep(delayMs);
            }
        }

        public string? GetText(int maxRetries = 6, int delayMs = 8)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                if (NativeMethods.OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        IntPtr handle = NativeMethods.GetClipboardData(NativeMethods.CF_UNICODETEXT);
                        if (handle != IntPtr.Zero)
                        {
                            IntPtr pointer = NativeMethods.GlobalLock(handle);
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
                                    NativeMethods.GlobalUnlock(handle);
                                }
                            }
                        }
                    }
                    finally
                    {
                        NativeMethods.CloseClipboard();
                    }
                }

                if (i < maxRetries - 1)
                {
                    Thread.Sleep(delayMs);
                }
            }

            return null;
        }

        public bool SetText(string text, int maxRetries = 5, int delayMs = 6)
        {
            if (string.IsNullOrEmpty(text)) return false;

            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(text + "\0");
            UIntPtr size = (UIntPtr)bytes.Length;

            for (int i = 0; i < maxRetries; i++)
            {
                if (NativeMethods.OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        NativeMethods.EmptyClipboard();

                        IntPtr hGlobal = NativeMethods.GlobalAlloc(NativeMethods.GMEM_MOVEABLE, size);
                        if (hGlobal != IntPtr.Zero)
                        {
                            IntPtr target = NativeMethods.GlobalLock(hGlobal);
                            if (target != IntPtr.Zero)
                            {
                                Marshal.Copy(bytes, 0, target, bytes.Length);
                                NativeMethods.GlobalUnlock(hGlobal);

                                if (NativeMethods.SetClipboardData(NativeMethods.CF_UNICODETEXT, hGlobal) != IntPtr.Zero)
                                {
                                    return true;
                                }
                            }

                            NativeMethods.GlobalFree(hGlobal);
                        }
                    }
                    finally
                    {
                        NativeMethods.CloseClipboard();
                    }
                }

                if (i < maxRetries - 1)
                {
                    Thread.Sleep(delayMs);
                }
            }

            return false;
        }
    }
}
