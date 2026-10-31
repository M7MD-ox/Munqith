using Munqith.Core;
using Munqith.Models;
using Munqith.Services;
using Xunit;

namespace Munqith.Tests
{
    public class ConversionBehaviorTests
    {
        private readonly LayoutConverter _converter = new();
        private readonly SmartDetector _smartDetector;

        public ConversionBehaviorTests()
        {
            _smartDetector = new SmartDetector(_converter);
        }

        // Regression test (2026-10-29): User reported ',' (comma) failing to convert to 'و' (Waw)
        // in standalone, attached prefix/suffix, and non-alpha Arabic word contexts.
        [Theory]
        [InlineData("lpl] , hpl]", "محمد و احمد")]
        [InlineData("hgpl]ggi , hga;v ggi", "الحمدلله و الشكر لله")]
        [InlineData(",hggi", "والله")]
        [InlineData("d,l", "يوم")]
        [InlineData("ulv,", "عمرو")]
        [InlineData(",", "و")]
        [InlineData(",]", "ود")]
        [InlineData(",.", "وز")]
        [InlineData(",in", "وهى")]
        [InlineData(",to", "وفخ")]
        [InlineData(",he", "واث")]
        [InlineData("he]", "اثد")]
        [InlineData("[]", "جد")]
        [InlineData(";hk", "كان")]
        public void test_regression_comma_and_punctuation_mapped_arabic_keys_convert_to_arabic(string input, string expected)
        {
            Assert.Equal(expected, _smartDetector.SmartConvert(input));
            Assert.Equal(expected, _converter.Convert(input));
        }

        [Theory]
        [InlineData("محمد و احمد", "lpl] , hpl]")]
        [InlineData("الحمدلله رب العالمين", "hgpl]ggi vf hguhgldk")]
        [InlineData("و", ",")]
        [InlineData("لا إله إلا الله", "b Ygi Yb hggi")]
        [InlineData("لأجل", "G[g")]
        [InlineData("الآن", "hBk")]
        [InlineData("\uFEFB", "b")] // Isolated presentation form ﻻ -> b
        public void test_arabic_text_converts_back_to_english_qwerty_keys(string arabicInput, string expectedEnglish)
        {
            Assert.Equal(expectedEnglish, _smartDetector.SmartConvert(arabicInput));
            Assert.Equal(expectedEnglish, _converter.Convert(arabicInput));
        }

        [Theory]
        [InlineData("HGFVKHL[ LFRHA AYHG ;,DS , HKH LA UHVT HJUHLG UGDI", "البرنامج مبقاش شغال كويس و انا مش عارف اتعامل عليه")]
        [InlineData("HGFVKHL[", "البرنامج")]
        [InlineData("LFRHA", "مبقاش")]
        [InlineData("AYHG", "شغال")]
        [InlineData("Lfrha ayhg", "مبقاش شغال")]
        [InlineData("Hpl]", "أحمد")]
        [InlineData("G[g", "لأجل")]
        public void test_caps_lock_and_sentence_case_convert_to_natural_arabic(string input, string expected)
        {
            Assert.Equal(expected, _smartDetector.SmartConvert(input));
            Assert.Equal(expected, _converter.Convert(input));
        }

        [Theory]
        [InlineData("hkh fajyg ugn windows , github ;g d,l", "انا بشتغل على windows و github كل يوم")]
        [InlineData("hgsghl ugd;l hello", "السلام عليكم hello")]
        [InlineData("hgsghl https://github.com/M7MD-ox/Munqith ugd;l", "السلام https://github.com/M7MD-ox/Munqith عليكم")]
        [InlineData("hgsghl user@example.com ugd;l", "السلام user@example.com عليكم")]
        public void test_smart_detector_preserves_embedded_english_words_urls_and_emails_in_arabic_sentence(string input, string expected)
        {
            Assert.Equal(expected, _smartDetector.SmartConvert(input));
        }

        [Theory]
        [InlineData("sun", "سعى")]
        [InlineData("leg", "مثل")]
        [InlineData("hot", "اخف")]
        [InlineData("dog", "يخل")]
        [InlineData("in", "هى")]
        [InlineData("to", "فخ")]
        [InlineData("hello", "اثممخ")]
        public void test_single_highlighted_word_always_converts_when_explicitly_selected(string input, string expected)
        {
            Assert.Equal(expected, _smartDetector.SmartConvert(input));
        }

        [Fact]
        public void test_concurrent_settings_saves_and_stats_recording_preserve_all_increments_without_corruption()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "MunqithTest_" + Guid.NewGuid().ToString("N"));
            try
            {
                var settingsService = new SettingsService(tempDir);
                var statsService = new StatsService(settingsService);

                Parallel.For(0, 25, _ =>
                {
                    statsService.RecordConversion(new ConversionResult
                    {
                        WasConverted = true,
                        CharacterCount = 10,
                        WordCount = 2
                    });
                });

                // Reload from disk to verify atomic file persistence
                var reloadedService = new SettingsService(tempDir);
                Assert.Equal(25, reloadedService.Settings.TotalConversions);
                Assert.Equal(250, reloadedService.Settings.TotalCharactersConverted);
                Assert.Equal(50, reloadedService.Settings.TotalWordsConverted);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }
    }
}
