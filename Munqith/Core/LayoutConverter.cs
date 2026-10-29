using System.Text;

namespace Munqith.Core
{
    public class LayoutConverter : ILayoutConverter
    {
        public static readonly LayoutConverter Instance = new();

        /// <summary>
        /// Detects whether text contains primarily English QWERTY keys (including keys like ',' -> 'و', '[' -> 'ج', ']' -> 'د', ';' -> 'ك')
        /// versus Arabic Unicode characters.
        /// </summary>
        public bool IsEnglishText(string text)
        {
            if (string.IsNullOrEmpty(text)) return true;

            int englishLetters = 0;
            int englishArabicKeyPunct = 0;
            int arabicLetters = 0;

            foreach (char c in text)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    englishLetters++;
                }
                else if (IsArabicChar(c))
                {
                    arabicLetters++;
                }
                else if (KeyboardMaps.EnglishPunctuationMappedToArabicLetters.Contains(c))
                {
                    englishArabicKeyPunct++;
                }
            }

            if (arabicLetters == 0 && (englishLetters > 0 || englishArabicKeyPunct > 0))
            {
                return true;
            }

            return englishLetters > arabicLetters;
        }

        public static bool IsArabicChar(char c)
        {
            return (c >= '\u0600' && c <= '\u06FF') ||
                   (c >= '\u0750' && c <= '\u077F') ||
                   (c >= '\uFB50' && c <= '\uFDFF') ||
                   (c >= '\uFE70' && c <= '\uFEFF');
        }

        /// <summary>
        /// Auto-detect direction and convert text.
        /// </summary>
        public string Convert(string text)
        {
            bool toArabic = IsEnglishText(text);
            return Convert(text, toArabic);
        }

        /// <summary>
        /// Convert text in a specified direction.
        /// </summary>
        public string Convert(string text, bool toArabic)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            return toArabic ? ConvertToArabic(text) : ConvertToEnglish(text);
        }

        private static string ConvertToArabic(string text)
        {
            var map = KeyboardMaps.EnglishToWindowsArabic;
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

        private static string ConvertToEnglish(string text)
        {
            var map = KeyboardMaps.WindowsArabicToEnglish;
            var multiMap = KeyboardMaps.ArToEnMulti;
            var sb = new StringBuilder(text.Length);

            for (int i = 0; i < text.Length; i++)
            {
                // 1. Check 2-char sequences first (لا, لإ, لأ, لآ) when current char is Lam (\u0644)
                if (text[i] == '\u0644' && i + 1 < text.Length)
                {
                    string twoChars = text.Substring(i, 2);
                    if (multiMap.TryGetValue(twoChars, out char multiResult))
                    {
                        sb.Append(multiResult);
                        i++;
                        continue;
                    }
                }

                // 2. Check single-char Unicode presentation forms for Lam-Alef (ﻻ, ﻷ, ﻹ, ﻵ)
                if (text[i] >= '\uFEF5' && text[i] <= '\uFEFC')
                {
                    string oneChar = text[i].ToString();
                    if (multiMap.TryGetValue(oneChar, out char presentationResult))
                    {
                        sb.Append(presentationResult);
                        continue;
                    }
                }

                // 3. Standard single-char mapping
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
