using Munqith.Models;

namespace Munqith.Hooks
{
    public interface IKeyboardHook : IDisposable
    {
        bool IsPaused { get; set; }
        event Action<HotkeyBinding>? HotkeyTriggered;
        void UpdateBindings(IEnumerable<HotkeyBinding> bindings);
        void Start();
        void Stop();
    }
}
