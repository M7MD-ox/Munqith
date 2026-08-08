using Munqith.Core;
using Munqith.Models;

namespace Munqith.Services
{
    public class ConversionService
    {
        private readonly SettingsService _settingsService;
        private readonly StatsService _statsService;

        // Anti-reentrancy debounce lock
        private static bool _isConverting = false;
        private static readonly object _lockObj = new();

        public ConversionService(SettingsService settingsService, StatsService statsService)
        {
            _settingsService = settingsService;
            _statsService = statsService;
        }

        public async Task PerformConversion()
        {
            lock (_lockObj)
            {
                if (_isConverting) return;
                _isConverting = true;
            }

            try
            {
                var settings = _settingsService.Settings;

                // 1. Release physical keys
                TextSelectionHelper.ReleaseModifiers();

                // 2. Capture text
                string? text = await TextSelectionHelper.GetTextForConversion();

                if (string.IsNullOrWhiteSpace(text)) return;

                // 3. Convert text bidirectionally (English -> Arabic OR Arabic -> English)
                string converted;
                if (settings.SmartDetectionEnabled)
                {
                    converted = SmartDetector.SmartConvert(text, KeyboardLayout.WindowsArabicPC);
                }
                else
                {
                    converted = LayoutConverter.Convert(text, KeyboardLayout.WindowsArabicPC);
                }

                // If conversion produces no change, ignore
                if (converted == text) return;

                // 4. Place converted text directly on clipboard
                NativeClipboard.SetText(converted);

                // 5. Paste directly into active window in place
                await TextSelectionHelper.SimulatePaste();

                // 6. Record stats
                var result = new ConversionResult
                {
                    OriginalText = text,
                    ConvertedText = converted,
                    WasConverted = true,
                    CharacterCount = converted.Length,
                    WordCount = converted.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                    DetectedAsArabic = !LayoutConverter.IsEnglishText(text)
                };
                _statsService.RecordConversion(result);
            }
            catch { }
            finally
            {
                lock (_lockObj)
                {
                    _isConverting = false;
                }
            }
        }
    }
}
