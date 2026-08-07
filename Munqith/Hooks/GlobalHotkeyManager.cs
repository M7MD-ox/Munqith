namespace Munqith.Hooks
{
    public class GlobalHotkeyManager : IDisposable
    {
        private readonly IntPtr _windowHandle;
        private readonly Dictionary<int, Action> _hotkeyActions = new();
        private int _nextId = 1;

        public GlobalHotkeyManager(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        /// <summary>
        /// Register a global hotkey. Returns the hotkey ID, or -1 on failure.
        /// </summary>
        public int RegisterHotkey(uint modifiers, uint key, Action callback)
        {
            int id = _nextId++;
            if (NativeMethods.RegisterHotKey(_windowHandle, id, modifiers | NativeMethods.MOD_NOREPEAT, key))
            {
                _hotkeyActions[id] = callback;
                return id;
            }
            return -1;
        }

        public void UnregisterHotkey(int id)
        {
            NativeMethods.UnregisterHotKey(_windowHandle, id);
            _hotkeyActions.Remove(id);
        }

        public void UnregisterAll()
        {
            foreach (var id in _hotkeyActions.Keys.ToList())
            {
                NativeMethods.UnregisterHotKey(_windowHandle, id);
            }
            _hotkeyActions.Clear();
            _nextId = 1;
        }

        public void ProcessHotkey(int hotkeyId)
        {
            if (_hotkeyActions.TryGetValue(hotkeyId, out var action))
            {
                action?.Invoke();
            }
        }

        public void Dispose()
        {
            UnregisterAll();
        }
    }
}
