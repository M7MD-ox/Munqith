using Munqith.Services;

namespace Munqith.Core
{
    public static class TextSelectionHelper
    {
        private static readonly ITextSelectionService _service = new TextSelectionService();

        public static void ReleaseModifiers() => _service.ReleaseModifiers();
        public static Task<string?> TryCopyCurrentSelection() => _service.TryCopyCurrentSelection();
        public static Task SimulatePaste() => _service.SimulatePaste();
        public static Task<string?> TrySelectAndCopyLine() => _service.TrySelectAndCopyLine();
        public static Task<string?> TrySelectAndCopyLastWord() => _service.TrySelectAndCopyLastWord();
        public static Task<string?> GetTextForConversion() => _service.GetTextForConversion();
    }
}
