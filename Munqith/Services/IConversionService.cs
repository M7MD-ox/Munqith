using Munqith.Models;

namespace Munqith.Services
{
    public interface IConversionService
    {
        event Action<ConversionResult>? ConversionCompleted;
        Task PerformConversion();
    }
}
