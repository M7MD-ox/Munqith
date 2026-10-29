namespace Munqith.Core
{
    public interface ILayoutConverter
    {
        bool IsEnglishText(string text);
        string Convert(string text);
        string Convert(string text, bool toArabic);
    }
}
