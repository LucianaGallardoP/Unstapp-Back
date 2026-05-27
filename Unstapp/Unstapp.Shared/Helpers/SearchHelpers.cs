using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unstapp.Shared.Helpers
{
    public static class SearchHelpers
    {
        public static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);

            var chars = normalized
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();

            return new string(chars).Normalize(NormalizationForm.FormC);
        }
    }
}
