using Munqith.Models;

namespace Munqith.Services
{
    public interface IStatsService
    {
        event Action? StatsUpdated;
        int TotalConversions { get; }
        int TotalCharacters { get; }
        int TotalWords { get; }
        double TimeSavedSeconds { get; }
        string GetTimeSavedDisplay();
        string GetTimeSavedDisplayArabic();
        void RecordConversion(ConversionResult result);
    }
}
