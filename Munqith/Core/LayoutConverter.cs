using System.Text;

namespace Munqith.Core
{
    public static class LayoutConverter
    {
        // Multi-character mappings EN→AR for Windows Arabic PC
        private static readonly Dictionary<char, string> WinEnToArMulti = new()
        {
            { 'b', "\u0644\u0627" },   // لا
            { 'T', "\u0644\u0625" },   // لإ
            { 'G', "\u0644\u0623" },   // لأ
            { 'B', "\u0644\u0622" },   // لآ
        };

        // Multi-character reverse mappings AR→EN
        private static readonly Dictionary<string, char> WinArToEnMulti = new()
        {
            { "\u0644\u0627", 'b' },   // لا → b
            { "\u0644\u0625", 'T' },   // لإ → T (Shift)
            { "\u0644\u0623", 'G' },   // لأ → G (Shift)
            { "\u0644\u0622", 'B' },   // لآ → B (Shift)
        };

        // Mac Arabic multi-char (same sequences)
        private static readonly Dictionary<char, string> MacEnToArMulti = new()
        {
            { 'b', "\u0644\u0627" },
            { 'T', "\u0644\u0625" },
            { 'G', "\u0644\u0623" },
            { 'B', "\u0644\u0622" },
        };

        private static readonly Dictionary<string, char> MacArToEnMulti = new()
        {
            { "\u0644\u0627", 'b' },
            { "\u0644\u0625", 'T' },
            { "\u0644\u0623", 'G' },
            { "\u0644\u0622", 'B' },
        };

        /// <summary>
        /// Detects whether text contains primarily English or Arabic characters.
        /// </summary>
        public static bool IsEnglishText(string text)
        {
            int english = 0, arabic = 0;
            foreach (char c in text)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) english++;
                else if (c >= '\u0600' && c <= '\u06FF') arabic++;
            }
            return english > arabic;
        }

        /// <summary>
        /// Auto-detect direction and convert text.
        /// </summary>
        public static string Convert(string text, KeyboardLayout layout)
        {
            bool toArabic = IsEnglishText(text);
            return Convert(text, layout, toArabic);
        }

        /// <summary>
        /// Convert text in a specified direction.
        /// </summary>
        public static string Convert(string text, KeyboardLayout layout, bool toArabic)
        {
            if (toArabic)
                return ConvertToArabic(text, layout);
            else
                return ConvertToEnglish(text, layout);
        }

        private static string ConvertToArabic(string text, KeyboardLayout layout)
        {
            var map = KeyboardMaps.GetMap(layout, true);
            var multiMap = layout == KeyboardLayout.MacArabic ? MacEnToArMulti : WinEnToArMulti;
            var sb = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                if (multiMap.TryGetValue(c, out var multi))
                    sb.Append(multi);
                else if (map.TryGetValue(c, out char mapped))
                    sb.Append(mapped);
                else
                    sb.Append(c); // Keep unmapped chars (spaces, numbers, etc.)
            }
            return sb.ToString();
        }

        private static string ConvertToEnglish(string text, KeyboardLayout layout)
        {
            var map = KeyboardMaps.GetMap(layout, false);
            var multiMap = layout == KeyboardLayout.MacArabic ? MacArToEnMulti : WinArToEnMulti;
            var sb = new StringBuilder(text.Length);

            for (int i = 0; i < text.Length; i++)
            {
                // Check 2-char sequences first (لا, لإ, لأ, لآ)
                if (i + 1 < text.Length)
                {
                    string twoChars = text.Substring(i, 2);
                    if (multiMap.TryGetValue(twoChars, out char multiResult))
                    {
                        sb.Append(multiResult);
                        i++; // Skip next char
                        continue;
                    }
                }

                if (map.TryGetValue(text[i], out char mapped))
                    sb.Append(mapped);
                else
                    sb.Append(text[i]);
            }
            return sb.ToString();
        }
    }
}
