using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Clays.Application.Managers
{
    public class MachineManager
    {
        private static readonly string[] WheelLoaderKeywords =
        [
            "wheel loader",
            "wheeled loader",
            "front loader",
            "loader",
            "pyorakuormaaja",
            "pyöräkuormaaja",
            "rengaskuormaaja",
            "kuormaaja"
        ];

        private static readonly string[] ExcavatorKeywords =
        [
            "excavator",
            "crawler excavator",
            "wheeled excavator",
            "mini excavator",
            "digger",
            "kaivinkone",
            "kaivuri",
            "telakaivinkone",
            "pyorakaivinkone",
            "pyöräkaivinkone",
            "minikaivuri"
        ];

        private static readonly string[] WheelLoaderBrands = ["volvo", "caterpillar", "cat"];

        private static readonly string[] ExcavatorBrands = ["volvo", "caterpillar", "cat", "doosan", "develon", "hitachi", "hyundai"];

        public static bool IsWantedMachine(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return false;
            }

            var normalizedTitle = Normalize(title);

            var isWheelLoader = ContainsAny(normalizedTitle, WheelLoaderKeywords) && ContainsAllowedBrand(normalizedTitle, WheelLoaderBrands);
            var isExcavator = ContainsAny(normalizedTitle, ExcavatorKeywords) && ContainsAllowedBrand(normalizedTitle, ExcavatorBrands);

            return isWheelLoader || isExcavator;
        }

        private static bool ContainsAny(string value, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (value.Contains(Normalize(keyword), StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsAllowedBrand(string normalizedTitle, string[] brands)
        {
            foreach (var brand in brands)
            {
                var normalizedBrand = Normalize(brand);

                if (normalizedBrand == "cat")
                {
                    if (Regex.IsMatch(normalizedTitle, @"\bcat\b", RegexOptions.CultureInvariant))
                    {
                        return true;
                    }

                    continue;
                }

                if (normalizedTitle.Contains(normalizedBrand, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string Normalize(string value)
        {
            var decomposed = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
