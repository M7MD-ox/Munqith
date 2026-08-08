namespace Munqith.Core
{
    public interface ILayoutConverter
    {
        bool IsEnglishText(string text);
        string Convert(string text, KeyboardLayout layout);
        string Convert(string text, KeyboardLayout layout, bool toArabic);
    }
}
