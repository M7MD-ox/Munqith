using System.Collections.ObjectModel;

namespace Munqith.Core
{
    public static class KeyboardMaps
    {
        public static readonly ReadOnlyDictionary<char, char> EnglishToWindowsArabic;
        public static readonly ReadOnlyDictionary<char, char> WindowsArabicToEnglish;

        public static readonly ReadOnlyDictionary<char, string> EnToArMulti;
        public static readonly ReadOnlyDictionary<string, char> ArToEnMulti;

        /// <summary>
        /// Punctuation keys on an English QWERTY keyboard that map directly to primary Arabic letters
        /// on the standard Windows Arabic 101 keyboard.
        /// </summary>
        public static readonly HashSet<char> EnglishPunctuationMappedToArabicLetters = new()
        {
            '`', // ذ
            ';', // ك
            '\'', // ط
            ',', // و
            '.', // ز
            '/', // ظ
            '[', // ج
            ']', // د
            '~', // ّ (Shadda)
            '?'  // ؟
        };

        /// <summary>
        /// Non-Arabic Unicode symbols produced exclusively by Shift+Letter on the Windows Arabic 101 layout.
        /// </summary>
        public static readonly HashSet<char> ArabicShiftedSymbols = new()
        {
            '\u2018', // ‘ (Shift+U)
            '\u2019', // ’ (Shift+M)
            '\u00F7', // ÷ (Shift+I)
            '\u00D7'  // × (Shift+O)
        };

        static KeyboardMaps()
        {
            // === Windows Arabic PC Layout (EN -> AR) ===
            var enToArWin = new Dictionary<char, char>
            {
                // Lowercase letters (QWERTY row)
                { 'q', '\u0636' }, // ض
                { 'w', '\u0635' }, // ص
                { 'e', '\u062B' }, // ث
                { 'r', '\u0642' }, // ق
                { 't', '\u0641' }, // ف
                { 'y', '\u063A' }, // غ
                { 'u', '\u0639' }, // ع
                { 'i', '\u0647' }, // ه
                { 'o', '\u062E' }, // خ
                { 'p', '\u062D' }, // ح
                // Lowercase letters (ASDF row)
                { 'a', '\u0634' }, // ش
                { 's', '\u0633' }, // س
                { 'd', '\u064A' }, // ي
                { 'f', '\u0628' }, // ب
                { 'g', '\u0644' }, // ل
                { 'h', '\u0627' }, // ا
                { 'j', '\u062A' }, // ت
                { 'k', '\u0646' }, // ن
                { 'l', '\u0645' }, // م
                // Lowercase letters (ZXCV row)
                { 'z', '\u0626' }, // ئ
                { 'x', '\u0621' }, // ء
                { 'c', '\u0624' }, // ؤ
                { 'v', '\u0631' }, // ر
                { 'n', '\u0649' }, // ى
                { 'm', '\u0629' }, // ة

                // Special characters (unshifted primary Arabic letters)
                { '`', '\u0630' }, // ذ
                { ';', '\u0643' }, // ك
                { '\'', '\u0637' }, // ط
                { ',', '\u0648' }, // و
                { '.', '\u0632' }, // ز
                { '/', '\u0638' }, // ظ
                { '[', '\u062C' }, // ج
                { ']', '\u062F' }, // د

                // Uppercase (Shift) letters (QWERTY row)
                // In English-to-Arabic layout conversion, map letter keys to primary Arabic letters
                // to prevent auto-capitalization in Word/browsers from generating bogus symbols or diacritics.
                { 'Q', '\u0636' }, // ض
                { 'W', '\u0635' }, // ص
                { 'E', '\u062B' }, // ث
                { 'R', '\u0642' }, // ق
                { 'Y', '\u0625' }, // إ
                { 'U', '\u0639' }, // ع
                { 'I', '\u0647' }, // ه
                { 'O', '\u062E' }, // خ
                { 'P', '\u062D' }, // ح
                // Uppercase (Shift) letters (ASDF row)
                { 'A', '\u0634' }, // ش
                { 'S', '\u0633' }, // س
                { 'D', '\u064A' }, // ي
                { 'F', '\u0628' }, // ب
                { 'H', '\u0623' }, // أ
                { 'J', '\u062A' }, // ت
                { 'K', '\u0646' }, // ن
                { 'L', '\u0645' }, // م
                // Uppercase (Shift) letters (ZXCV row)
                { 'Z', '\u0626' }, // ئ
                { 'X', '\u0621' }, // ء
                { 'C', '\u0624' }, // ؤ
                { 'V', '\u0631' }, // ر
                { 'N', '\u0622' }, // آ
                { 'M', '\u0629' }, // ة

                // Special characters (shifted)
                { '~', '\u0651' }, // ّ shadda
                { '?', '\u061F' }, // ؟ Arabic question mark
            };

            EnglishToWindowsArabic = new ReadOnlyDictionary<char, char>(enToArWin);

            // Explicit reverse map (AR -> EN) preserving unshifted Arabic letters AND shifted Windows Arabic 101 outputs
            var winArToEn = new Dictionary<char, char>
            {
                // Primary Arabic letters (QWERTY row)
                { '\u0636', 'q' }, // ض
                { '\u0635', 'w' }, // ص
                { '\u062B', 'e' }, // ث
                { '\u0642', 'r' }, // ق
                { '\u0641', 't' }, // ف
                { '\u063A', 'y' }, // غ
                { '\u0639', 'u' }, // ع
                { '\u0647', 'i' }, // ه
                { '\u062E', 'o' }, // خ
                { '\u062D', 'p' }, // ح
                { '\u062C', '[' }, // ج
                { '\u062F', ']' }, // د

                // Primary Arabic letters (ASDF row)
                { '\u0634', 'a' }, // ش
                { '\u0633', 's' }, // س
                { '\u064A', 'd' }, // ي
                { '\u0628', 'f' }, // ب
                { '\u0644', 'g' }, // ل
                { '\u0627', 'h' }, // ا
                { '\u062A', 'j' }, // ت
                { '\u0646', 'k' }, // ن
                { '\u0645', 'l' }, // م
                { '\u0643', ';' }, // ك
                { '\u0637', '\'' }, // ط

                // Primary Arabic letters (ZXCV row + backtick)
                { '\u0626', 'z' }, // ئ
                { '\u0621', 'x' }, // ء
                { '\u0624', 'c' }, // ؤ
                { '\u0631', 'v' }, // ر
                { '\u0649', 'n' }, // ى
                { '\u0629', 'm' }, // ة
                { '\u0648', ',' }, // و
                { '\u0632', '.' }, // ز
                { '\u0638', '/' }, // ظ
                { '\u0630', '`' }, // ذ

                // Hamza forms & Arabic punctuation
                { '\u0623', 'H' }, // أ
                { '\u0625', 'Y' }, // إ
                { '\u0622', 'N' }, // آ
                { '\u0651', '~' }, // ّ shadda
                { '\u061F', '?' }, // ؟ Arabic question mark

                // Shifted Windows Arabic 101 diacritics & symbols (for AR -> EN when English was typed with Shift/Caps)
                { '\u064E', 'Q' }, // َ fatha
                { '\u064B', 'W' }, // ً tanween fatha
                { '\u064F', 'E' }, // ُ damma
                { '\u064C', 'R' }, // ٌ tanween damma
                { '\u2018', 'U' }, // ‘
                { '\u00F7', 'I' }, // ÷
                { '\u00D7', 'O' }, // ×
                { '\u061B', 'P' }, // ؛
                { '\u0650', 'A' }, // ِ kasra
                { '\u064D', 'S' }, // ٍ tanween kasra
                { ']', 'D' },
                { '[', 'F' },
                { '\u0640', 'J' }, // ـ tatweel
                { '\u060C', 'K' }, // ،
                { '/', 'L' },
                { '~', 'Z' },
                { '\u0652', 'X' }, // ْ sukun
                { '}', 'C' },
                { '{', 'V' },
                { '\u2019', 'M' }, // ’
            };
            WindowsArabicToEnglish = new ReadOnlyDictionary<char, char>(winArToEn);

            // Multi-character ligatures (لا, لإ, لأ, لآ)
            var multiEnToAr = new Dictionary<char, string>
            {
                { 'b', "\u0644\u0627" },   // لا
                { 'T', "\u0644\u0625" },   // لإ
                { 'G', "\u0644\u0623" },   // لأ
                { 'B', "\u0644\u0622" },   // لآ
            };
            EnToArMulti = new ReadOnlyDictionary<char, string>(multiEnToAr);

            var multiArToEn = new Dictionary<string, char>
            {
                { "\u0644\u0627", 'b' },   // لا → b
                { "\u0644\u0625", 'T' },   // لإ → T
                { "\u0644\u0623", 'G' },   // لأ → G
                { "\u0644\u0622", 'B' },   // لآ → B
                // Single-char Unicode presentation forms for Lam-Alef ligatures
                { "\uFEFB", 'b' },         // ﻻ isolated
                { "\uFEFC", 'b' },         // ﻼ final
                { "\uFEF9", 'T' },         // ﻹ isolated
                { "\uFEFA", 'T' },         // ﻺ final
                { "\uFEF7", 'G' },         // ﻷ isolated
                { "\uFEF8", 'G' },         //  final
                { "\uFEF5", 'B' },         // ﻵ isolated
                { "\uFEF6", 'B' },         // ﻶ final
            };
            ArToEnMulti = new ReadOnlyDictionary<string, char>(multiArToEn);
        }
    }
}
