namespace Munqith.Core
{
    public enum KeyboardLayout
    {
        WindowsArabicPC,
        MacArabic
    }

    public static class KeyboardMaps
    {
        public static readonly Dictionary<char, char> EnglishToWindowsArabic;
        public static readonly Dictionary<char, char> WindowsArabicToEnglish;
        public static readonly Dictionary<char, char> EnglishToMacArabic;
        public static readonly Dictionary<char, char> MacArabicToEnglish;

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
                // 'b' → لا (2 chars, handled in LayoutConverter)
                { 'n', '\u0649' }, // ى
                { 'm', '\u0629' }, // ة

                // Uppercase (Shift) letters (QWERTY row)
                { 'Q', '\u064E' }, // َ fatha
                { 'W', '\u064B' }, // ً tanween fatha
                { 'E', '\u064F' }, // ُ damma
                { 'R', '\u064C' }, // ٌ tanween damma
                // 'T' → لإ (2 chars, handled in LayoutConverter)
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
                // 'G' → لأ (2 chars, handled in LayoutConverter)
                { 'H', '\u0623' }, // أ
                { 'J', '\u0640' }, // ـ tatweel
                { 'K', '\u060C' }, // ، Arabic comma
                { 'L', '/' },
                // Uppercase (Shift) letters (ZXCV row)
                { 'Z', '~' },
                { 'X', '\u0652' }, // ْ sukun
                { 'C', '}' },
                { 'V', '{' },
                // 'B' → لآ (2 chars, handled in LayoutConverter)
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

            EnglishToWindowsArabic = enToArWin;

            // Build reverse map (Arabic → English)
            WindowsArabicToEnglish = new Dictionary<char, char>();
            foreach (var kvp in enToArWin)
            {
                if (!WindowsArabicToEnglish.ContainsKey(kvp.Value))
                    WindowsArabicToEnglish[kvp.Value] = kvp.Key;
            }

            // === Mac Arabic Layout ===
            // Mac Arabic is mostly the same for letter keys
            // Key differences are in some special characters
            var enToArMac = new Dictionary<char, char>
            {
                // Letters are mostly the same as Windows Arabic PC
                { 'q', '\u0636' }, { 'w', '\u0635' }, { 'e', '\u062B' }, { 'r', '\u0642' },
                { 't', '\u0641' }, { 'y', '\u063A' }, { 'u', '\u0639' }, { 'i', '\u0647' },
                { 'o', '\u062E' }, { 'p', '\u062D' },
                { 'a', '\u0634' }, { 's', '\u0633' }, { 'd', '\u064A' }, { 'f', '\u0628' },
                { 'g', '\u0644' }, { 'h', '\u0627' }, { 'j', '\u062A' }, { 'k', '\u0646' },
                { 'l', '\u0645' },
                { 'z', '\u0626' }, { 'x', '\u0621' }, { 'c', '\u0624' }, { 'v', '\u0631' },
                { 'n', '\u0649' }, { 'm', '\u0629' },

                // Shift letters
                { 'Q', '\u064E' }, { 'W', '\u064B' }, { 'E', '\u064F' }, { 'R', '\u064C' },
                { 'Y', '\u0625' },
                { 'U', '\u2018' }, { 'I', '\u00F7' }, { 'O', '\u00D7' }, { 'P', '\u061B' },
                { 'A', '\u0650' }, { 'S', '\u064D' },
                { 'D', ']' }, { 'F', '[' },
                { 'H', '\u0623' }, { 'J', '\u0640' }, { 'K', '\u060C' }, { 'L', '/' },
                { 'Z', '~' }, { 'X', '\u0652' }, { 'C', '}' }, { 'V', '{' },
                { 'N', '\u0622' }, { 'M', '\u2019' },

                // Special characters
                { '`', '\u0630' }, { ';', '\u0643' }, { '\'', '\u0637' },
                { ',', '\u0648' }, { '.', '\u0632' }, { '/', '\u0638' },
                { '[', '\u062C' }, { ']', '\u062F' },
                { '~', '\u0651' }, { '<', ',' }, { '>', '.' },
                { '?', '\u061F' }, { '{', '<' }, { '}', '>' },
            };

            EnglishToMacArabic = enToArMac;

            MacArabicToEnglish = new Dictionary<char, char>();
            foreach (var kvp in enToArMac)
            {
                if (!MacArabicToEnglish.ContainsKey(kvp.Value))
                    MacArabicToEnglish[kvp.Value] = kvp.Key;
            }
        }

        public static Dictionary<char, char> GetMap(KeyboardLayout layout, bool toArabic)
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
