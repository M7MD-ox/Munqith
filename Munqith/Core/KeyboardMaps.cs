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
                { '<', ',' },
                { '>', '.' },
                { '?', '\u061F' }, // ؟ Arabic question mark
                { '{', '<' },
                { '}', '>' },
            };

            EnglishToWindowsArabic = new ReadOnlyDictionary<char, char>(enToArWin);

            // Build reverse map (AR -> EN), prioritizing unshifted primary letters
            var winArToEn = new Dictionary<char, char>();
            foreach (var kvp in enToArWin)
            {
                // Only map Arabic characters and shifted symbols that don't collide with ASCII punctuation
                if (!winArToEn.ContainsKey(kvp.Value))
                {
                    winArToEn[kvp.Value] = kvp.Key;
                }
            }
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
