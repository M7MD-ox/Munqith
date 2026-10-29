namespace Munqith.Services
{
    public interface ITextSelectionService
    {
        void ReleaseModifiers();
        Task<string?> TryCopyCurrentSelection();
        Task SimulatePaste();
        Task<string?> GetTextForConversion();
    }
}
