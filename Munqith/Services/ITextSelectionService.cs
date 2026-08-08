namespace Munqith.Services
{
    public interface ITextSelectionService
    {
        void ReleaseModifiers();
        Task<string?> TryCopyCurrentSelection();
        Task SimulatePaste();
        Task<string?> TrySelectAndCopyLine();
        Task<string?> TrySelectAndCopyLastWord();
        Task<string?> GetTextForConversion();
    }
}
