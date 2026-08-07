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

        /// <summary>
        /// Main conversion method — called when hotkey is pressed.
        /// </summary>
        public async Task PerformConversion()
        {
            if (!_settingsService.Settings.IsEnabled) return;

            var settings = _settingsService.Settings;

            try
            {
                // Small delay to let hotkey keys release
                await Task.Delay(100);

                // Get text based on conversion mode
                string? text = await TextSelectionHelper.GetTextForConversion(settings.ConversionMode);

                if (string.IsNullOrEmpty(text)) return;

                // Check for Undo: if text matches last conversion, restore original
                if (_lastConvertedText != null && text.Trim() == _lastConvertedText.Trim())
                {
                    ClipboardHelper.SetText(_lastOriginalText!);
                    await TextSelectionHelper.SimulatePaste();
                    _lastOriginalText = null;
                    _lastConvertedText = null;
                    return;
                }

                // Convert the text
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

                // Paste the converted text
                ClipboardHelper.SetText(converted);
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
