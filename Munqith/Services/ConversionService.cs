using Munqith.Core;
using Munqith.Models;

namespace Munqith.Services
{
    public class ConversionService
    {
        private readonly SettingsService _settingsService;
        private readonly StatsService _statsService;
        private string? _lastOriginalText;
        private string? _lastConvertedText;

        public ConversionService(SettingsService settingsService, StatsService statsService)
        {
            _settingsService = settingsService;
            _statsService = statsService;
        }

        public async Task PerformConversion()
        {
            if (!_settingsService.Settings.IsEnabled) return;

            var settings = _settingsService.Settings;

            try
            {
                // Wait briefly for physical modifier keys to settle
                TextSelectionHelper.ReleaseModifiers();
                await Task.Delay(50);

                // Try configured conversion mode first
                string? text = await TextSelectionHelper.GetTextForConversion(settings.ConversionMode);

                // Smart Fallback: If no text was selected, fallback to LastWord mode automatically
                if (string.IsNullOrWhiteSpace(text) && settings.ConversionMode == ConversionMode.SelectedText)
                {
                    text = await TextSelectionHelper.GetTextForConversion(ConversionMode.LastWord);
                }

                if (string.IsNullOrWhiteSpace(text)) return;

                // Check for Undo: if text matches last converted text, restore original
                if (_lastConvertedText != null && text.Trim() == _lastConvertedText.Trim())
                {
                    NativeClipboard.SetText(_lastOriginalText!);
                    await TextSelectionHelper.SimulatePaste();
                    _lastOriginalText = null;
                    _lastConvertedText = null;
                    return;
                }

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

                // If no change, return
                if (converted == text) return;

                // Save for Undo
                _lastOriginalText = text;
                _lastConvertedText = converted;

                // Set text directly into Windows clipboard with Native Win32 API
                NativeClipboard.SetText(converted);
                await Task.Delay(30);

                // Paste converted text into active window
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
        }
    }
}
