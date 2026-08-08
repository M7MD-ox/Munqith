using System.Text;

namespace Munqith.Core
{
    public class LayoutConverter : ILayoutConverter
    {
        public static readonly LayoutConverter Instance = new();

        /// <summary>
        /// Detects whether text contains primarily English or Arabic characters.
        /// </summary>
        public bool IsEnglishText(string text)
        {
            if (string.IsNullOrEmpty(text)) return true;

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
        public string Convert(string text, KeyboardLayout layout)
        {
            bool toArabic = IsEnglishText(text);
            return Convert(text, layout, toArabic);
        }

        /// <summary>
        /// Convert text in a specified direction.
        /// </summary>
        public string Convert(string text, KeyboardLayout layout, bool toArabic)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            return toArabic ? ConvertToArabic(text, layout) : ConvertToEnglish(text, layout);
        }

        private static string ConvertToArabic(string text, KeyboardLayout layout)
        {
            var map = KeyboardMaps.GetMap(layout, true);
            var multiMap = KeyboardMaps.EnToArMulti;
            var sb = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                if (multiMap.TryGetValue(c, out var multi))
                {
                    sb.Append(multi);
                }
                else if (map.TryGetValue(c, out char mapped))
                {
                    sb.Append(mapped);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static string ConvertToEnglish(string text, KeyboardLayout layout)
        {
            var map = KeyboardMaps.GetMap(layout, false);
            var multiMap = KeyboardMaps.ArToEnMulti;
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
                        i++;
                        continue;
                    }
                }

                if (map.TryGetValue(text[i], out char mapped))
                {
                    sb.Append(mapped);
                }
                else
                {
                    sb.Append(text[i]);
                }
            }
            return sb.ToString();
        }
    }
}
