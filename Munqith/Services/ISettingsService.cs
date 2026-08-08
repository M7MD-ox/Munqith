using Munqith.Models;

namespace Munqith.Services
{
    public interface ISettingsService
    {
        AppSettings Settings { get; }
        void Load();
        void Save();
    }
}
