using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;

using Coolishbee.PackageGenerator.Utils;

namespace Coolishbee.PackageGenerator.PackageSample
{
    public sealed class PackageSampleGenerator
    {
        private const string SamplesDirName = "Samples~";

        public GeneratePackageSampleOutputDto GeneratePackageSample(in GeneratePackageSampleInputDto input)
        {
            var destDirName = Regex.Replace(input.DisplayName.Trim(), @"\s+", "_");
            if (string.IsNullOrEmpty(destDirName))
            {
                throw new InvalidOperationException("Display Name is required.");
            }

            if (destDirName.Any(ch => char.IsLetterOrDigit(ch) is false && ch != '_'))
            {
                throw new InvalidOperationException("Display Name can contain only letters, numbers, whitespace, and underscores.");
            }

            var packageRootPath = PathUtil.GetPackageRootPath(input.PackageName);
            if (Directory.Exists(packageRootPath) is false)
            {
                throw new InvalidOperationException($"Package '{input.PackageName}' was not found in the current project.");
            }

            var samplePath = $"{SamplesDirName}/{destDirName}";
            var destDirPath = Path.Combine(packageRootPath, SamplesDirName, destDirName);
            if (Directory.Exists(destDirPath))
            {
                throw new InvalidOperationException($"Sample folder '{destDirName}' already exists.");
            }

            AddSampleToPackageJson(packageRootPath, input, samplePath);

            Directory.CreateDirectory(Path.Combine(destDirPath, "Scripts"));
            File.WriteAllText(Path.Combine(destDirPath, $"{destDirName}.asmdef"), GenerateAsmdef(destDirName));
            File.WriteAllText(Path.Combine(destDirPath, "Scripts", "SampleScript.cs"), GenerateSampleScript(BuildSafeNamespace(destDirName)));

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return new GeneratePackageSampleOutputDto { DirPath = destDirPath };
        }

        private static void AddSampleToPackageJson(string packageRootPath, GeneratePackageSampleInputDto input, string samplePath)
        {
            var packageJsonPath = Path.Combine(packageRootPath, "package.json");
            if (File.Exists(packageJsonPath) is false)
            {
                throw new InvalidOperationException($"package.json was not found. Path: {packageJsonPath}");
            }

            var packageJson = JObject.Parse(File.ReadAllText(packageJsonPath));
            var samples = packageJson["samples"] as JArray;
            if (samples == null)
            {
                samples = new JArray();
                packageJson["samples"] = samples;
            }

            var hasDuplicate = samples
                .OfType<JObject>()
                .Any(sample =>
                    sample.Value<string>("displayName") == input.DisplayName ||
                    sample.Value<string>("path") == samplePath);

            if (hasDuplicate)
            {
                throw new InvalidOperationException($"Sample '{input.DisplayName}' is already registered in package.json.");
            }

            samples.Add(new JObject
            {
                ["displayName"] = input.DisplayName,
                ["description"] = input.Description,
                ["path"] = samplePath
            });

            File.WriteAllText(packageJsonPath, packageJson.ToString(Formatting.Indented));
        }

        private static string GenerateAsmdef(string name)
        {
            return $@"{{
    ""name"": ""{name}"",
    ""references"": [],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";
        }

        private static string GenerateSampleScript(string namespaceName)
        {
            return $@"using UnityEngine;

namespace {namespaceName}
{{
    public sealed class SampleScript : MonoBehaviour
    {{
    }}
}}";
        }

        private static string BuildSafeNamespace(string value)
        {
            return char.IsLetter(value[0]) || value[0] == '_' ? value : $"Sample_{value}";
        }
    }
}
