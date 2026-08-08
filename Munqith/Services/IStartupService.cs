namespace Munqith.Services
{
    public interface IStartupService
    {
        bool IsRegistered();
        void Register();
        void Unregister();
        void SetStartup(bool enabled);
    }
}
