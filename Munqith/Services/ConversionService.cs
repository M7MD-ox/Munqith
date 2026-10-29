using Munqith.Core;
using Munqith.Interop;
using Munqith.Models;

namespace Munqith.Services
{
    public class ConversionService : IConversionService
    {
        private readonly ISettingsService _settingsService;
        private readonly IStatsService _statsService;
        private readonly ILayoutConverter _layoutConverter;
        private readonly ISmartDetector _smartDetector;
        private readonly ITextSelectionService _textSelectionService;
        private readonly INativeClipboard _clipboard;

        // Anti-reentrancy debounce lock
        private static bool _isConverting = false;
        private static readonly object _lockObj = new();

        public ConversionService(
            ISettingsService settingsService,
            IStatsService statsService,
            ILayoutConverter? layoutConverter = null,
            ISmartDetector? smartDetector = null,
            ITextSelectionService? textSelectionService = null,
            INativeClipboard? clipboard = null)
        {
            _settingsService = settingsService;
            _statsService = statsService;
            _layoutConverter = layoutConverter ?? LayoutConverter.Instance;
            _smartDetector = smartDetector ?? SmartDetector.Instance;
            _textSelectionService = textSelectionService ?? new TextSelectionService();
            _clipboard = clipboard ?? new NativeClipboard();
        }

        public async Task PerformConversion()
        {
            lock (_lockObj)
            {
                if (_isConverting) return;
                _isConverting = true;
            }

            string? previousClipboardText = null;
            bool pastedConvertedText = false;

            try
            {
                var settings = _settingsService.Settings;

                // 1. Backup existing text on clipboard so user doesn't lose copied data if nothing is selected
                previousClipboardText = _clipboard.GetText(maxRetries: 2, delayMs: 4);

                // 2. Capture highlighted text
                string? text = await _textSelectionService.GetTextForConversion();

                if (string.IsNullOrWhiteSpace(text)) return;

                // 3. Convert text bidirectionally (English -> Arabic OR Arabic -> English)
                string converted = settings.SmartDetectionEnabled
                    ? _smartDetector.SmartConvert(text)
                    : _layoutConverter.Convert(text);

                // If conversion produces no change, ignore
                if (converted == text) return;

                // 4. Place converted text directly on clipboard
                _clipboard.SetText(converted);

                // 5. Paste directly into active window in place
                await _textSelectionService.SimulatePaste();
                pastedConvertedText = true;

                // 6. Record stats
                var result = new ConversionResult
                {
                    OriginalText = text,
                    ConvertedText = converted,
                    WasConverted = true,
                    CharacterCount = converted.Length,
                    WordCount = converted.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                    DetectedAsArabic = !_layoutConverter.IsEnglishText(text)
                };
                _statsService.RecordConversion(result);
            }
            catch { }
            finally
            {
                try
                {
                    // Restore user's original clipboard text if no conversion occurred,
                    // or after waiting briefly for the target app to finish pasting.
                    if (!string.IsNullOrEmpty(previousClipboardText))
                    {
                        if (pastedConvertedText)
                        {
                            await Task.Delay(50);
                        }
                        _clipboard.SetText(previousClipboardText, maxRetries: 3, delayMs: 5);
                    }
                }
                catch { }

                lock (_lockObj)
                {
                    _isConverting = false;
                }
            }
        }
    }
}
