using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Coolishbee.PackageGenerator
{
    public enum PackageCategory
    {
        None,
        Tools,
        Utilities,
        ThirdParty,
        UI,
        Game,
        Platform,
        Scripting
    }

    [Serializable]
    public sealed class PackageGenerationOption
    {
        public const string DefaultLocation = "../../Packages";
        public const string LocalLocation = "Packages";
        public const string DefaultPackageNamePrefix = "com.author.";

        public string Location = DefaultLocation;
        public string DisplayName = "My Package";
        public PackageCategory Category = PackageCategory.None;
        public string PackageName = "com.author.my-package";
        public string Description = "This is my package";
        public string Author = "author";
        public string UnityVersion = "6000.2";
        public bool IncludeRuntime = true;
        public bool IncludeEditor = true;
        public bool IncludeTests = true;
        public bool IncludeRuntimeTests;
        public bool IncludeEditorTests = true;

        public string PackageRoot => Path.Combine(Location, SanitizeFolderName(DisplayName)).Replace('\\', '/');

        public string BuildAssemblyName()
        {
            var id = PackageName.Split('.').LastOrDefault() ?? PackageName;
            return string.Concat(id
                .Split(new[] { '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(token => char.ToUpperInvariant(token[0]) + token.Substring(1)));
        }

        public static string BuildPackageName(PackageCategory category, string displayName)
        {
            var categoryToken = GetPackageCategoryToken(category);
            var displayNameToken = NormalizeToken(displayName);
            var name = string.Join(".", new[] { categoryToken, displayNameToken }
                .Where(token => string.IsNullOrEmpty(token) is false));
            return $"{DefaultPackageNamePrefix}{name}";
        }

        public static string SanitizeFolderName(string displayName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var chars = displayName.Trim()
                .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
                .ToArray();
            return new string(chars).Replace(' ', '_');
        }

        private static string GetPackageCategoryToken(PackageCategory category)
        {
            return category switch
            {
                PackageCategory.None => string.Empty,
                PackageCategory.Tools => "tools",
                PackageCategory.Utilities => "utilities",
                PackageCategory.ThirdParty => "thirdparty",
                PackageCategory.UI => "ui",
                PackageCategory.Game => "game",
                PackageCategory.Platform => "platform",
                PackageCategory.Scripting => "scripting",
                _ => throw new NotSupportedException()
            };
        }

        private static string NormalizeToken(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }

            const char hyphen = '-';
            var normalized = new StringBuilder();
            foreach (var ch in str.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(ch))
                {
                    normalized.Append(ch);
                    continue;
                }

                if (normalized.Length > 0 && normalized[^1] != hyphen)
                {
                    normalized.Append(hyphen);
                }
            }

            return normalized.ToString().Trim(hyphen);
        }
    }
}
