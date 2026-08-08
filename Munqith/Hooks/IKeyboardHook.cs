using Munqith.Models;

namespace Munqith.Hooks
{
    public interface IKeyboardHook : IDisposable
    {
        event Action<HotkeyBinding>? HotkeyTriggered;
        void UpdateBindings(IEnumerable<HotkeyBinding> bindings);
        void Start();
        void Stop();
    }
}
