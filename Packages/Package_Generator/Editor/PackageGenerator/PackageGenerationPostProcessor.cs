using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

using Coolishbee.PackageGenerator.Utils;

namespace Coolishbee.PackageGenerator
{
    [InitializeOnLoad]
    public static class PackageGenerationPostProcessor
    {
        private static readonly Regex MetaGuidRegex = new Regex(@"^guid: (.+)$", RegexOptions.Compiled | RegexOptions.Multiline);

        static PackageGenerationPostProcessor()
        {
            if (File.Exists(PackageGeneratorCore.ProgressOptionJsonPath))
            {
                EditorApplication.delayCall += PostGenerate;
            }
        }

        public static void PostGenerate()
        {
            var json = File.ReadAllText(PackageGeneratorCore.ProgressOptionJsonPath);
            File.Delete(PackageGeneratorCore.ProgressOptionJsonPath);

            var option = JsonUtility.FromJson<PackageGenerationOption>(json);
            var packageRoot = option.PackageRoot;
            var assemblyName = option.BuildAssemblyName();

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ResolveDependencies(packageRoot, assemblyName, option);

            PackageGeneratorWindow.ForceClose();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(Path.Combine(packageRoot, "package.json"));
            Client.Resolve();
        }

        private static void ResolveDependencies(string packageRoot, string assemblyName, PackageGenerationOption option)
        {
            var runtimeGuid = option.IncludeRuntime ? ParseGuid(PathUtil.GetMetaFilePath(PathUtil.GetRuntimeAsmdefPath(packageRoot, assemblyName))) : string.Empty;
            var editorGuid = option.IncludeEditor ? ParseGuid(PathUtil.GetMetaFilePath(PathUtil.GetEditorAsmdefPath(packageRoot, assemblyName))) : string.Empty;
            var testRuntimeGuid = option.IncludeRuntimeTests ? ParseGuid(PathUtil.GetMetaFilePath(PathUtil.GetTestRuntimeAsmdefPath(packageRoot, assemblyName))) : string.Empty;

            if (option.IncludeEditor && option.IncludeRuntime)
            {
                InjectReferences(PathUtil.GetEditorAsmdefPath(packageRoot, assemblyName), false, runtimeGuid);
            }

            if (option.IncludeTests)
            {
                if (option.IncludeRuntimeTests)
                {
                    InjectReferences(PathUtil.GetTestRuntimeAsmdefPath(packageRoot, assemblyName), true, runtimeGuid);
                }

                if (option.IncludeEditorTests)
                {
                    InjectReferences(PathUtil.GetTestEditorAsmdefPath(packageRoot, assemblyName), true, runtimeGuid, editorGuid, testRuntimeGuid);
                }
            }
        }

        private static string ParseGuid(string metaPath)
        {
            return File.Exists(metaPath) ? MetaGuidRegex.Match(File.ReadAllText(metaPath)).Groups[1].Value : string.Empty;
        }

        private static void InjectReferences(string targetPath, bool isTest, params string[] guids)
        {
            if (File.Exists(targetPath) is false)
            {
                return;
            }

            var json = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(targetPath));
            var references = guids
                .Where(guid => string.IsNullOrEmpty(guid) is false)
                .Select(guid => $"GUID:{guid}");
            if (isTest)
            {
                references = new[] { "UnityEngine.TestRunner", "UnityEditor.TestRunner" }.Concat(references);
            }

            json["references"] = new Newtonsoft.Json.Linq.JArray(references);
            File.WriteAllText(targetPath, json.ToString(Newtonsoft.Json.Formatting.Indented));
        }
    }
}
