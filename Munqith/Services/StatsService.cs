using Munqith.Models;

namespace Munqith.Services
{
    public class StatsService : IStatsService
    {
        private readonly ISettingsService _settingsService;
        private readonly object _statsLock = new();

        public event Action? StatsUpdated;

        public StatsService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public int TotalConversions => _settingsService.Settings.TotalConversions;
        public int TotalCharacters => _settingsService.Settings.TotalCharactersConverted;
        public int TotalWords => _settingsService.Settings.TotalWordsConverted;
        public double TimeSavedSeconds => _settingsService.Settings.TotalTimeSavedSeconds;

        public string GetTimeSavedDisplay()
        {
            var ts = TimeSpan.FromSeconds(TimeSavedSeconds);
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours} hr {ts.Minutes} min";
            if (ts.TotalMinutes >= 1)
                return $"{(int)ts.TotalMinutes} min {ts.Seconds} sec";
            return $"{(int)ts.TotalSeconds} sec";
        }

        public string GetTimeSavedDisplayArabic()
        {
            var ts = TimeSpan.FromSeconds(TimeSavedSeconds);
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours} ساعة {ts.Minutes} دقيقة";
            if (ts.TotalMinutes >= 1)
                return $"{(int)ts.TotalMinutes} دقيقة {ts.Seconds} ثانية";
            return $"{(int)ts.TotalSeconds} ثانية";
        }

        /// <summary>
        /// Record a conversion. Estimates ~3.3 chars/sec average typing speed.
        /// </summary>
        public void RecordConversion(ConversionResult result)
        {
            if (!result.WasConverted) return;

            lock (_statsLock)
            {
                _settingsService.Settings.TotalConversions++;
                _settingsService.Settings.TotalCharactersConverted += result.CharacterCount;
                _settingsService.Settings.TotalWordsConverted += result.WordCount;
                double timeSaved = result.CharacterCount / 3.3;
                _settingsService.Settings.TotalTimeSavedSeconds += timeSaved;
                _settingsService.Save();
            }

            StatsUpdated?.Invoke();
        }
    }
}
