using System.Collections.ObjectModel;

namespace Munqith.Core
{
    public enum KeyboardLayout
    {
        WindowsArabicPC,
        MacArabic
    }

    public static class KeyboardMaps
    {
        public static readonly ReadOnlyDictionary<char, char> EnglishToWindowsArabic;
        public static readonly ReadOnlyDictionary<char, char> WindowsArabicToEnglish;
        public static readonly ReadOnlyDictionary<char, char> EnglishToMacArabic;
        public static readonly ReadOnlyDictionary<char, char> MacArabicToEnglish;

        public static readonly ReadOnlyDictionary<char, string> EnToArMulti;
        public static readonly ReadOnlyDictionary<string, char> ArToEnMulti;

        static KeyboardMaps()
        {
            // === Windows Arabic PC Layout ===
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

                // Uppercase (Shift) letters (QWERTY row)
                { 'Q', '\u064E' }, // َ fatha
                { 'W', '\u064B' }, // ً tanween fatha
                { 'E', '\u064F' }, // ُ damma
                { 'R', '\u064C' }, // ٌ tanween damma
                { 'Y', '\u0625' }, // إ
                { 'U', '\u2018' }, // ' left single quote
                { 'I', '\u00F7' }, // ÷
                { 'O', '\u00D7' }, // ×
                { 'P', '\u061B' }, // ؛ Arabic semicolon
                // Uppercase (Shift) letters (ASDF row)
                { 'A', '\u0650' }, // ِ kasra
                { 'S', '\u064D' }, // ٍ tanween kasra
                { 'D', ']' },
                { 'F', '[' },
                { 'H', '\u0623' }, // أ
                { 'J', '\u0640' }, // ـ tatweel
                { 'K', '\u060C' }, // ، Arabic comma
                { 'L', '/' },
                // Uppercase (Shift) letters (ZXCV row)
                { 'Z', '~' },
                { 'X', '\u0652' }, // ْ sukun
                { 'C', '}' },
                { 'V', '{' },
                { 'N', '\u0622' }, // آ
                { 'M', '\u2019' }, // ' right single quote

                // Special characters (unshifted)
                { '`', '\u0630' }, // ذ
                { ';', '\u0643' }, // ك
                { '\'', '\u0637' }, // ط
                { ',', '\u0648' }, // و
                { '.', '\u0632' }, // ز
                { '/', '\u0638' }, // ظ
                { '[', '\u062C' }, // ج
                { ']', '\u062F' }, // د

                // Special characters (shifted)
                { '~', '\u0651' }, // ّ shadda
                { '<', ',' },
                { '>', '.' },
                { '?', '\u061F' }, // ؟ Arabic question mark
                { '{', '<' },
                { '}', '>' },
            };

            EnglishToWindowsArabic = new ReadOnlyDictionary<char, char>(enToArWin);

            var winArToEn = new Dictionary<char, char>();
            foreach (var kvp in enToArWin)
            {
                if (!winArToEn.ContainsKey(kvp.Value))
                    winArToEn[kvp.Value] = kvp.Key;
            }
            WindowsArabicToEnglish = new ReadOnlyDictionary<char, char>(winArToEn);

            // Mac layout mappings
            var enToArMac = new Dictionary<char, char>(enToArWin);
            EnglishToMacArabic = new ReadOnlyDictionary<char, char>(enToArMac);

            var macArToEn = new Dictionary<char, char>(winArToEn);
            MacArabicToEnglish = new ReadOnlyDictionary<char, char>(macArToEn);

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
            };
            ArToEnMulti = new ReadOnlyDictionary<string, char>(multiArToEn);
        }

        public static IReadOnlyDictionary<char, char> GetMap(KeyboardLayout layout, bool toArabic)
        {
            return (layout, toArabic) switch
            {
                (KeyboardLayout.WindowsArabicPC, true) => EnglishToWindowsArabic,
                (KeyboardLayout.WindowsArabicPC, false) => WindowsArabicToEnglish,
                (KeyboardLayout.MacArabic, true) => EnglishToMacArabic,
                (KeyboardLayout.MacArabic, false) => MacArabicToEnglish,
                _ => EnglishToWindowsArabic
            };
        }
    }
}
