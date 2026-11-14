namespace Munqith.Core
{
    public interface ISmartDetector
    {
        string SmartConvert(string text, IReadOnlyCollection<string>? customPreservedWords = null);
        bool IsLikelyEnglishWord(string word);
    }
}
