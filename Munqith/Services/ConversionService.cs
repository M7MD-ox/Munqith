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

            try
            {
                var settings = _settingsService.Settings;

                // 1. Release physical keys
                _textSelectionService.ReleaseModifiers();

                // 2. Capture text
                string? text = await _textSelectionService.GetTextForConversion();

                if (string.IsNullOrWhiteSpace(text)) return;

                // 3. Convert text bidirectionally (English -> Arabic OR Arabic -> English)
                string converted;
                if (settings.SmartDetectionEnabled)
                {
                    converted = _smartDetector.SmartConvert(text, KeyboardLayout.WindowsArabicPC);
                }
                else
                {
                    converted = _layoutConverter.Convert(text, KeyboardLayout.WindowsArabicPC);
                }

                // If conversion produces no change, ignore
                if (converted == text) return;

                // 4. Place converted text directly on clipboard
                _clipboard.SetText(converted);

                // 5. Paste directly into active window in place
                await _textSelectionService.SimulatePaste();

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
                lock (_lockObj)
                {
                    _isConverting = false;
                }
            }
        }
    }
}
