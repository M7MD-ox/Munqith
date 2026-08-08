namespace Munqith.Core
{
    public interface ISmartDetector
    {
        string SmartConvert(string text, KeyboardLayout layout);
        bool IsLikelyEnglishWord(string word);
    }
}
