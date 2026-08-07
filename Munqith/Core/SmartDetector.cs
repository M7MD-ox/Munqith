using System.Text;
using System.Text.RegularExpressions;

namespace Munqith.Core
{
    /// <summary>
    /// Smart detection that preserves correctly-typed English words
    /// and only converts garbled text.
    /// </summary>
    public static class SmartDetector
    {
        private static readonly HashSet<string> CommonEnglishWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "the", "be", "to", "of", "and", "in", "that", "have", "it", "for",
            "not", "on", "with", "he", "as", "you", "do", "at", "this", "but",
            "his", "by", "from", "they", "we", "say", "her", "she", "or", "an",
            "will", "my", "one", "all", "would", "there", "their", "what", "so", "up",
            "out", "if", "about", "who", "get", "which", "go", "me", "when", "make",
            "can", "like", "time", "no", "just", "him", "know", "take", "people", "into",
            "year", "your", "good", "some", "could", "them", "see", "other", "than", "then",
            "now", "look", "only", "come", "its", "over", "think", "also", "back", "after",
            "use", "two", "how", "our", "work", "first", "well", "way", "even", "new",
            "want", "because", "any", "these", "give", "day", "most", "us", "great", "big",
            "long", "much", "world", "old", "right", "still", "find", "here", "thing", "many",
            "live", "high", "last", "own", "house", "help", "need", "start", "keep", "name",
            "read", "small", "run", "end", "does", "set", "try", "ask", "went", "men",
            "put", "play", "home", "call", "hand", "move", "been", "where", "down", "should",
            "may", "part", "place", "every", "head", "school", "again", "never", "change", "letter",
            "city", "next", "mean", "before", "number", "open", "group", "write", "under", "story",
            "car", "food", "area", "left", "water", "white", "room", "mother", "light", "door",
            "point", "book", "study", "word", "life", "paper", "money", "game", "side", "line",
            "close", "body", "girl", "power", "music", "woman", "boy", "sure", "table", "face",
            "horse", "watch", "color", "south", "night", "north", "center", "drive", "field", "town",
            "stand", "land", "turn", "voice", "morning", "class", "dog", "fish", "love", "road",
            "heart", "hold", "picture", "bring", "stop", "hard", "late", "father", "question", "family",
            "real", "best", "rock", "talk", "hear", "air", "example", "fire", "eye", "children",
            "plan", "eat", "tree", "second", "beautiful", "cut", "ground", "green", "hundred", "above",
            "test", "island", "state", "west", "ever", "east", "red", "reach", "street", "sleep",
            "snow", "bed", "mark", "dream", "mile", "half", "fly", "build", "true", "full",
            "farm", "rest", "boat", "river", "fall", "ship", "front", "song", "age", "dark",
            "warm", "top", "cold", "hot", "fast", "walk", "six", "cross", "four", "sit",
            "ride", "able", "draw", "animal", "step", "bit", "bird", "began", "few", "round",
            "act", "month", "sent", "ball", "led", "king", "carry", "sun", "sat", "main",
            "ran", "ear", "fill", "push", "lot", "wide", "told", "wind", "join", "sign",
            "gold", "cost", "deep", "egg", "born", "hit", "check", "oil", "ring", "queen",
            "box", "miss", "foot", "sell", "seem", "meet", "sea", "wait", "past", "size",
            "low", "hour", "war", "lay", "add", "wife", "hat", "pay", "rain", "pick",
            "lift", "mix", "let", "tie", "iron", "tool", "press", "leg", "brown", "trip",
            "save", "store", "pair", "fresh", "total", "type", "spring", "flat", "shall", "speed",
            "deal", "stone", "sort", "band", "key", "rose", "bar", "match", "bank", "mine",
            "art", "bread", "camp", "fat", "rich", "sand", "ten", "nine", "lead", "thin",
            "string", "block", "fine", "coat", "shape", "lake", "inch", "page", "feed", "felt",
            "wave", "beat", "wonder", "list", "touch", "huge", "spot", "wild", "board", "shop",
            "army", "track", "lost", "heat", "break", "jump", "skill", "cry", "soft", "unit",
            "steel", "suit", "milk", "hole", "drop", "dust", "race", "dress", "cloud", "quiet",
            "mouth", "wash", "roll", "print", "dear", "stock", "sharp", "fix", "sweet", "crowd",
            "charge", "simple", "master", "valley", "double", "count", "length", "finger", "market", "basic",
            "silver", "root", "event", "neck", "truck", "sail", "nor", "spread", "glad", "thick",
            "drink", "tube", "seed", "tone", "view", "winter", "firm", "pull", "chance", "bell",
            "win", "rush", "pour", "grew", "final", "shout", "pain", "nose", "cool", "straight",
            "fair", "pitch", "tiny", "poem", "wish", "flow", "climb", "anger", "fear", "gift",
            "row", "wheel", "skin", "plain", "chart", "share", "brain", "edge",
            "computer", "program", "system", "software", "hardware", "internet", "network", "database",
            "server", "client", "file", "code", "data", "user", "password", "email", "website",
            "browser", "search", "click", "download", "upload", "screen", "window", "button", "menu",
            "icon", "folder", "memory", "process", "error", "debug", "update", "install", "delete",
            "copy", "paste", "input", "output", "device", "mobile", "phone", "tablet", "laptop",
            "desktop", "keyboard", "mouse", "monitor", "display", "pixel", "image", "video", "audio",
            "camera", "bluetooth", "wifi", "cable", "port", "battery", "switch", "reset", "boot",
            "login", "logout", "profile", "account", "setting", "option", "feature", "version", "release",
            "patch", "bug", "crash", "free", "paid", "premium", "pro", "lite", "empty", "short",
            "narrow", "shallow", "heavy", "bright", "loud", "smooth", "rough", "clean", "dirty",
            "wet", "dry", "fresh", "stale", "sweet", "bitter", "sour", "salty", "spicy", "strong",
            "weak", "cheap", "expensive", "ugly", "hello", "world", "please", "thank", "thanks",
            "sorry", "yes", "done", "okay", "sure", "maybe", "always", "often", "sometimes",
            "today", "tomorrow", "yesterday", "evening", "afternoon", "tonight", "week",
            "minute", "testing", "working", "loading", "saving", "running", "starting",
            "stopping", "opening", "closing", "google", "youtube", "facebook", "twitter",
            "windows", "linux", "android", "apple", "microsoft", "chrome", "firefox"
        };

        /// <summary>
        /// Converts text smartly — preserving real English words and only converting garbled text.
        /// </summary>
        public static string SmartConvert(string text, KeyboardLayout layout)
        {
            // Split text preserving spaces and delimiters
            var parts = Regex.Split(text, @"(\s+)");
            var result = new StringBuilder();

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part) || part.All(char.IsWhiteSpace))
                {
                    result.Append(part);
                    continue;
                }

                // Strip punctuation for dictionary check
                string cleaned = part.Trim(',', '.', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}');

                if (IsLikelyEnglishWord(cleaned))
                {
                    result.Append(part); // Keep as-is
                }
                else
                {
                    result.Append(LayoutConverter.Convert(part, layout));
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Checks if a word is likely a real English word.
        /// </summary>
        private static bool IsLikelyEnglishWord(string word)
        {
            if (string.IsNullOrEmpty(word) || word.Length < 2) return false;
            // Must be all ASCII letters
            if (!word.All(c => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))) return false;
            return CommonEnglishWords.Contains(word);
        }
    }
}
