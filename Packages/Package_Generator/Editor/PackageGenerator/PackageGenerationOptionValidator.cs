using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Coolishbee.PackageGenerator
{
    public static class PackageGenerationOptionValidator
    {
        private static readonly Regex PackageNameRegex = new Regex(@"^[a-z0-9][a-z0-9._-]*[a-z0-9]$", RegexOptions.Compiled);

        public static bool Validate(PackageGenerationOption option, out string error)
        {
            if (string.IsNullOrWhiteSpace(option.Location))
            {
                error = "[ Location ] is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(option.DisplayName))
            {
                error = "[ Display Name ] is required.";
                return false;
            }

            if (option.DisplayName.Any(ch => Path.GetInvalidFileNameChars().Contains(ch)))
            {
                error = "[ Display Name ] contains characters that cannot be used in a folder name.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(option.PackageName) || PackageNameRegex.IsMatch(option.PackageName) is false)
            {
                error = "[ Package Name ] can contain only lowercase letters, numbers, hyphens, underscores, and dots.";
                return false;
            }

            if (Directory.Exists(option.PackageRoot))
            {
                error = $"[ {option.PackageRoot} ] already exists.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(option.Description))
            {
                error = "[ Description ] is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(option.Author))
            {
                error = "[ Author ] is required.";
                return false;
            }

            if (option.IncludeRuntime is false && option.IncludeEditor is false)
            {
                error = "At least one Runtime or Editor assembly must be included.";
                return false;
            }

            if (option.IncludeTests && option.IncludeRuntimeTests is false && option.IncludeEditorTests is false)
            {
                error = "When tests are included, at least one Runtime Tests or Editor Tests assembly must be included.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
