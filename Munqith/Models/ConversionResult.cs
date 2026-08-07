namespace Munqith.Models
{
    public class ConversionResult
    {
        public string OriginalText { get; set; } = "";
        public string ConvertedText { get; set; } = "";
        public bool WasConverted { get; set; }
        public int CharacterCount { get; set; }
        public int WordCount { get; set; }
        public bool DetectedAsArabic { get; set; }
    }
}
