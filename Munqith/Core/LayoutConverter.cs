using System.Text;
using System.Text.RegularExpressions;

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
                   (c >= '\uFE70' && c <= '\uFEFF') ||
                   KeyboardMaps.ArabicShiftedSymbols.Contains(c);
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

            if (toArabic)
            {
                text = NormalizeCapsLockText(text);
                return ConvertToArabic(text);
            }

            return ConvertToEnglish(text);
        }

        /// <summary>
        /// Returns true if the given text appears to have been typed with Caps Lock active
        /// (at least 2 uppercase ASCII letters and predominantly uppercase).
        /// Requiring upper >= 2 prevents single shifted Hamza keys like "H[]", "G[]", or "H," from
        /// being misclassified as Caps Lock.
        /// </summary>
        public static bool IsCapsLockText(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            int upper = 0;
            int lower = 0;
            foreach (char c in text)
            {
                if (c >= 'A' && c <= 'Z') upper++;
                else if (c >= 'a' && c <= 'z') lower++;
            }

            return upper >= 2 && (lower == 0 || (double)upper / (upper + lower) >= 0.6);
        }

        /// <summary>
        /// Normalizes text typed while Caps Lock was active.
        /// When typing in English with Caps Lock ON, letters become uppercase A-Z.
        /// On an Arabic Windows keyboard, Caps Lock does not produce shifted keys, ligatures, or diacritics.
        /// </summary>
        public static string NormalizeCapsLockText(string text)
        {
            if (!IsCapsLockText(text)) return text;

            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                sb.Append((c >= 'A' && c <= 'Z') ? char.ToLowerInvariant(c) : c);
            }
            return sb.ToString();
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

        private static bool IsAsciiShiftedArabicSymbol(char c)
        {
            return c is '[' or ']' or '{' or '}' or '/' or '~';
        }

        private static string ConvertToEnglish(string text)
        {
            var sb = new StringBuilder(text.Length);
            int i = 0;

            while (i < text.Length)
            {
                if (char.IsWhiteSpace(text[i]))
                {
                    sb.Append(text[i]);
                    i++;
                    continue;
                }

                int start = i;
                bool tokenHasArabic = false;
                while (i < text.Length && !char.IsWhiteSpace(text[i]))
                {
                    if (IsArabicChar(text[i]))
                    {
                        tokenHasArabic = true;
                    }
                    i++;
                }

                sb.Append(ConvertTokenToEnglish(text, start, i, tokenHasArabic));
            }

            return sb.ToString();
        }

        private static string ConvertTokenToEnglish(string text, int start, int end, bool tokenHasArabic)
        {
            var map = KeyboardMaps.WindowsArabicToEnglish;
            var multiMap = KeyboardMaps.ArToEnMulti;
            var sb = new StringBuilder(end - start);
            bool hadLamAlefB = false;

            for (int i = start; i < end; i++)
            {
                // 1. Check 2-char sequences first (لا, لإ, لأ, لآ) when current char is Lam (\u0644)
                if (text[i] == '\u0644' && i + 1 < end)
                {
                    string twoChars = text.Substring(i, 2);
                    if (multiMap.TryGetValue(twoChars, out char multiResult))
                    {
                        if (multiResult == 'b') hadLamAlefB = true;
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
                        if (presentationResult == 'b') hadLamAlefB = true;
                        sb.Append(presentationResult);
                        continue;
                    }
                }

                // 3. Only treat ASCII bracket/slash/tilde keys as Windows Arabic shifted symbols inside Arabic tokens
                if (IsAsciiShiftedArabicSymbol(text[i]) && !tokenHasArabic)
                {
                    sb.Append(text[i]);
                    continue;
                }

                // 4. Standard single-char mapping
                if (map.TryGetValue(text[i], out char mapped))
                {
                    sb.Append(mapped);
                }
                else
                {
                    sb.Append(text[i]);
                }
            }

            string converted = sb.ToString();
            return hadLamAlefB ? FixLamAlefGhWords(converted) : converted;
        }

        /// <summary>
        /// Disambiguates 'b' produced by Lam-Alef ('لا') when typing English words containing "gh"
        /// on an Arabic keyboard (where 'g' = 'ل' + 'h' = 'ا' merges into 'لا' -> 'b').
        /// </summary>
        private static string FixLamAlefGhWords(string token)
        {
            return Regex.Replace(token, "[A-Za-z]+", match =>
            {
                string word = match.Value;
                if (!word.Contains('b', StringComparison.OrdinalIgnoreCase))
                {
                    return word;
                }

                string lower = word.ToLowerInvariant();
                if (lower is "doubt" or "doubts" or "doubted" or "doubting" or "doubtful" or "doubtless"
                    or "undoubted" or "undoubtedly")
                {
                    return word;
                }

                string result = word;
                // Pattern 1: "ight" (libt -> light, nibt -> night, ribt -> right, bribt -> bright, etc.)
                result = Regex.Replace(result, "(?<=i)b(?=t)", "gh", RegexOptions.IgnoreCase);

                // Pattern 2: "ought" & trailing "ough" (thoubt -> thought, broubt -> brought, enoub -> enough, throub -> through)
                result = Regex.Replace(result, "(?<=ou)b(?=t)", "gh", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(?<=ou)b(?=$|s$|ed$|ing$|ly$|out$|nut$)", "gh", RegexOptions.IgnoreCase);

                // Pattern 3: "augh" (laub -> laugh, caubt -> caught, taubt -> taught, daubter -> daughter)
                result = Regex.Replace(result, "(?<=^lau)b(?=$|s$|ed$|ing$|ter)", "gh", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "(?<=^(c|t|d|n|sl|fr)au)b(?=t|ter|y)", "gh", RegexOptions.IgnoreCase);

                // Pattern 4: standalone/prefix "igh" (hib -> high, sib -> sigh, thib -> thigh, hiber -> higher, hiblight -> highlight)
                result = Regex.Replace(result, "^(h|s|th)ib(?=$|s$|er|est|ly|way|light)", "$1igh", RegexOptions.IgnoreCase);

                // Pattern 5: specific "gh" words where g+h collapsed to 'b' (bost -> ghost, betto -> ghetto, spabetti -> spaghetti, afban -> afghan)
                result = Regex.Replace(result, "^bost(?=$|s$|ly$|ing$|ed$)", "ghost", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "^betto(?=$|es$|s$)", "ghetto", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "^spabetti$", "spaghetti", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "^afb(?=an(?:istan)?$)", "afgh", RegexOptions.IgnoreCase);

                return result;
            });
        }
    }
}
