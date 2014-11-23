using System.Globalization;

namespace NameIt.Common
{
    public class LanguageRetriever
    {
        public bool IsDutchLanguage()
        {
            string language = CultureInfo.CurrentCulture.Name;
            return (language.IndexOf("nl-", System.StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
