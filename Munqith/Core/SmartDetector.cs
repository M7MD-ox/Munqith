using System.Text;
using System.Text.RegularExpressions;

namespace Munqith.Core
{
    public class SmartDetector : ISmartDetector
    {
        public static readonly SmartDetector Instance = new();

        private readonly ILayoutConverter _layoutConverter;

        /// <summary>
        /// Unambiguous English words (4+ chars and specific tech acronyms) that do not collide
        /// with common Arabic words when typed on an English QWERTY layout.
        /// </summary>
        private static readonly HashSet<string> CommonEnglishWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Tech & Brands (3+ chars, unambiguous)
            "app", "web", "api", "url", "usb", "pdf", "ram", "cpu", "gpu", "sql", "css", "xml", "git",
            "html", "json", "http", "https", "wifi", "code", "data", "user", "file", "menu", "icon",
            "copy", "paste", "click", "login", "logout", "email", "phone", "video", "audio", "image",
            "pixel", "mouse", "screen", "window", "button", "folder", "memory", "system", "server",
            "client", "device", "mobile", "tablet", "laptop", "camera", "switch", "option", "update",
            "delete", "upload", "search", "browser", "website", "network", "program", "process",
            "account", "profile", "setting", "feature", "version", "release", "desktop", "monitor",
            "display", "battery", "install", "computer", "software", "hardware", "internet", "database",
            "password", "download", "keyboard", "bluetooth", "google", "youtube", "facebook", "twitter",
            "windows", "linux", "android", "apple", "microsoft", "chrome", "firefox", "github", "vscode",

            // Common unambiguous 4+ letter English words
            "that", "with", "have", "this", "will", "your", "from", "they", "know", "want", "been",
            "good", "much", "some", "time", "very", "when", "come", "here", "just", "like", "long",
            "make", "many", "more", "only", "over", "such", "take", "than", "them", "well", "were",
            "what", "about", "after", "again", "below", "could", "every", "first", "found", "great",
            "house", "large", "learn", "never", "other", "place", "plant", "point", "right", "small",
            "sound", "spell", "still", "study", "their", "there", "these", "thing", "think", "three",
            "water", "where", "which", "world", "would", "write", "years", "people", "because",
            "before", "change", "number", "mother", "father", "family", "school", "answer", "always",
            "hello", "please", "thanks", "welcome", "testing", "working", "loading", "running"
        };

        public SmartDetector(ILayoutConverter? layoutConverter = null)
        {
            _layoutConverter = layoutConverter ?? LayoutConverter.Instance;
        }

        public string SmartConvert(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            // Determine overall conversion direction at the sentence level so standalone tokens
            // like ',' ('و') or '[]' ('جد') always convert consistently with the sentence.
            bool toArabic = _layoutConverter.IsEnglishText(text);

            var parts = Regex.Split(text, @"(\s+)");
            var result = new StringBuilder(text.Length);

            int nonWhitespaceTokens = 0;
            int preservedEnglishTokens = 0;

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part) || part.All(char.IsWhiteSpace))
                {
                    result.Append(part);
                    continue;
                }

                nonWhitespaceTokens++;

                if (IsUrlOrEmail(part))
                {
                    preservedEnglishTokens++;
                    result.Append(part);
                    continue;
                }

                // Only strip non-Arabic-mapped outer symbols like parentheses or quotes when checking English words.
                // NEVER strip ',', '.', ';', '\'', '[', ']', '`', '/' because those keys ARE Arabic letters (و, ز, ك, ط, ج, د, ذ, ظ)!
                string cleaned = part.Trim('!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+');

                if (toArabic && IsLikelyEnglishWord(cleaned))
                {
                    preservedEnglishTokens++;
                    result.Append(part);
                }
                else
                {
                    result.Append(_layoutConverter.Convert(part, toArabic));
                }
            }

            // If EVERY token in the selection was preserved as English (e.g., user explicitly highlighted
            // a single word like "hello" and pressed Alt+Q to convert it), perform the direct conversion.
            if (nonWhitespaceTokens > 0 && preservedEnglishTokens == nonWhitespaceTokens)
            {
                return _layoutConverter.Convert(text, toArabic);
            }

            return result.ToString();
        }

        public bool IsLikelyEnglishWord(string word)
        {
            if (string.IsNullOrEmpty(word) || word.Length < 3) return false;
            if (!word.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))) return false;
            return CommonEnglishWords.Contains(word);
        }

        private static bool IsUrlOrEmail(string token)
        {
            if (token.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                token.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                token.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Simple email check (contains @ with ASCII letters/digits around it)
            int atIndex = token.IndexOf('@');
            if (atIndex > 0 && atIndex < token.Length - 3 && token.IndexOf('.', atIndex) > atIndex + 1)
            {
                return true;
            }

            return false;
        }
    }
}
