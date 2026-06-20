using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Assertions;

using Coolishbee.PackageGenerator.Utils;

namespace Coolishbee.PackageGenerator
{
    public static class PackageGeneratorCore
    {
        public const string ProgressOptionJsonPath = "Temp/__coolishbee_package_generation_option__.json";

        public static void Generate(PackageGenerationOption option)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Please wait", "Generating package...", 1f);
                var packageRoot = option.PackageRoot;
                var assemblyName = option.BuildAssemblyName();
                var rootNamespace = $"Coolishbee.{assemblyName}";

                Directory.CreateDirectory(packageRoot);
                GeneratePackageJson(packageRoot, option);

                if (option.IncludeRuntime)
                {
                    GenerateAssemblyDefinition(PathUtil.GetRuntimeAsmdefPath(packageRoot, assemblyName), rootNamespace, false, false);
                }

                if (option.IncludeEditor)
                {
                    GenerateAssemblyDefinition(PathUtil.GetEditorAsmdefPath(packageRoot, assemblyName), rootNamespace, true, false);
                }

                if (option.IncludeTests)
                {
                    Directory.CreateDirectory(Path.Combine(packageRoot, "Tests"));
                    if (option.IncludeRuntimeTests)
                    {
                        GenerateAssemblyDefinition(PathUtil.GetTestRuntimeAsmdefPath(packageRoot, assemblyName), rootNamespace, false, true);
                    }

                    if (option.IncludeEditorTests)
                    {
                        GenerateAssemblyDefinition(PathUtil.GetTestEditorAsmdefPath(packageRoot, assemblyName), rootNamespace, true, true);
                    }
                }

                File.WriteAllText(ProgressOptionJsonPath, JsonUtility.ToJson(option));
                AddPackageIfExternalLocation(option);
                EditorApplication.delayCall += Client.Resolve;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void GeneratePackageJson(string packageRoot, PackageGenerationOption option)
        {
            var packageJson = new JObject
            {
                ["name"] = option.PackageName.Trim(),
                ["displayName"] = option.DisplayName.Trim(),
                ["version"] = "0.1.0",
                ["description"] = option.Description.Trim(),
                ["unity"] = option.UnityVersion.Trim(),
                ["author"] = new JObject
                {
                    ["name"] = option.Author.Trim()
                }
            };

            if (option.IncludeTests)
            {
                packageJson["dependencies"] = new JObject
                {
                    ["com.unity.test-framework"] = "1.4.5"
                };
            }

            File.WriteAllText(Path.Combine(packageRoot, "package.json"), packageJson.ToString(Formatting.Indented));
        }

        private static void GenerateAssemblyDefinition(string asmdefPath, string rootNamespace, bool isEditor, bool isTest)
        {
            var dirPath = Path.GetDirectoryName(asmdefPath);
            Assert.IsNotNull(dirPath, "Assembly definition directory not found.");
            Directory.CreateDirectory(dirPath);

            var assemblyName = Path.GetFileNameWithoutExtension(asmdefPath);
            var asmdef = new JObject
            {
                ["name"] = assemblyName,
                ["rootNamespace"] = rootNamespace,
                ["references"] = new JArray(),
                ["includePlatforms"] = isEditor ? new JArray("Editor") : new JArray(),
                ["excludePlatforms"] = new JArray(),
                ["allowUnsafeCode"] = false,
                ["overrideReferences"] = isTest,
                ["precompiledReferences"] = isTest ? new JArray("nunit.framework.dll") : new JArray(),
                ["autoReferenced"] = true,
                ["defineConstraints"] = isTest ? new JArray("UNITY_INCLUDE_TESTS") : new JArray(),
                ["versionDefines"] = new JArray(),
                ["noEngineReferences"] = false
            };

            File.WriteAllText(asmdefPath, asmdef.ToString(Formatting.Indented));
        }

        private static void AddPackageIfExternalLocation(PackageGenerationOption option)
        {
            if (option.Location == PackageGenerationOption.LocalLocation)
            {
                return;
            }

            var manifest = ManifestHelper.GetManifestJObject();
            var dependencies = ManifestHelper.AcquireDependencies(manifest);
            dependencies[option.PackageName] = $"file:../{option.PackageRoot}";

            if (option.IncludeTests)
            {
                var testables = ManifestHelper.AcquireTestables(manifest);
                if (testables.All(token => token.Value<string>() != option.PackageName))
                {
                    testables.Add(option.PackageName);
                }
            }

            ManifestHelper.Save(manifest);
        }
    }
}
