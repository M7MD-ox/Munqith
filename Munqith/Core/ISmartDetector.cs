namespace Munqith.Core
{
    public interface ISmartDetector
    {
        string SmartConvert(string text);
        bool IsLikelyEnglishWord(string word);
    }
}
