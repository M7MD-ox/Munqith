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

        // In-Place Toggle state
        private string? _lastPastedText;
        private string? _lastAlternateText;
        private DateTime _lastConversionTime = DateTime.MinValue;

        public ConversionService(SettingsService settingsService, StatsService statsService)
        {
            _settingsService = settingsService;
            _statsService = statsService;
        }

        public async Task PerformConversion()
        {
            if (!_settingsService.Settings.IsEnabled) return;

            lock (_lockObj)
            {
                if (_isConverting) return;
                _isConverting = true;
            }

            try
            {
                var settings = _settingsService.Settings;

                // 1. IN-PLACE TOGGLE CHECK (Back & Forth flip in same position)
                if (!string.IsNullOrEmpty(_lastPastedText) && 
                    !string.IsNullOrEmpty(_lastAlternateText) &&
                    (DateTime.UtcNow - _lastConversionTime).TotalSeconds < 8)
                {
                    // Select the characters of the last pasted text backward from cursor
                    await TextSelectionHelper.SimulateSelectCharsBackward(_lastPastedText.Length);
                    string? currentText = NativeClipboard.GetText(maxRetries: 4, delayMs: 6);

                    // Swap the text in place
                    string nextText = _lastAlternateText;
                    string prevText = _lastPastedText;

                    _lastPastedText = nextText;
                    _lastAlternateText = prevText;
                    _lastConversionTime = DateTime.UtcNow;

                    NativeClipboard.SetText(nextText);
                    await TextSelectionHelper.SimulatePaste();
                    return;
                }

                // 2. FRESH CONVERSION
                TextSelectionHelper.ReleaseModifiers();

                // Get text based on mode
                string? text = await TextSelectionHelper.GetTextForConversion(settings.ConversionMode);

                // Smart Fallback: If no text was highlighted, fallback to LastWord automatically
                if (string.IsNullOrWhiteSpace(text) && settings.ConversionMode == ConversionMode.SelectedText)
                {
                    text = await TextSelectionHelper.GetTextForConversion(ConversionMode.LastWord);
                }

                if (string.IsNullOrWhiteSpace(text)) return;

                // Convert text
                string converted;
                if (settings.SmartDetectionEnabled)
                {
                    converted = SmartDetector.SmartConvert(text, settings.SelectedLayout);
                }
                else
                {
                    converted = LayoutConverter.Convert(text, settings.SelectedLayout);
                }

                if (converted == text) return;

                // Store state for subsequent in-place toggles
                _lastAlternateText = text;
                _lastPastedText = converted;
                _lastConversionTime = DateTime.UtcNow;

                // Set clipboard and paste in place
                NativeClipboard.SetText(converted);
                await TextSelectionHelper.SimulatePaste();

                // Record stats
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
