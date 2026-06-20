using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.Assertions;

using Coolishbee.PackageGenerator.Utils;

namespace Coolishbee.PackageGenerator.PackageDoc
{
    public sealed class DocGenerator
    {
        public void GenerateDocs(in GenerateDocsInputDto input)
        {
            Assert.IsTrue(Directory.Exists(input.DestDirPath), $"'{input.DestDirPath}' must be an existing directory.");

            var docsDirPath = Path.Combine(PathUtil.TemplateDirPath, "Documentation");
            var targetDir = Path.Combine(input.DestDirPath, "Documentation");

            PathUtil.CopyDirectory(docsDirPath, targetDir);
            AppendGitIgnore(targetDir);
            FixDocfxConfig(targetDir, input.DisplayName);
        }

        private static void FixDocfxConfig(string targetPath, string packageDisplayName)
        {
            var docfxJsonFilePath = Path.Combine(targetPath, "docfx", "docfx.json");
            var docfxDirPath = Path.Combine(targetPath, "docfx");
            var relativePath = PathUtil.GetUnityProjectRootRelativePath(docfxDirPath).Replace('\\', '/');

            var json = File.ReadAllText(docfxJsonFilePath);
            var data = JsonConvert.DeserializeObject<DocfxConfigDto>(json);
            data.metadata[0].src[0].src = $"{relativePath}/";
            data.build.globalMetadata["_appName"] = packageDisplayName;
            data.build.globalMetadata["_appTitle"] = packageDisplayName;

            var newJson = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(docfxJsonFilePath, newJson);
        }

        private static void AppendGitIgnore(string destDirPath)
        {
            const string gitIgnoreFileName = ".gitignore";
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("docfx/_site/");
            stringBuilder.AppendLine("docfx/api/");

            var gitIgnorePath = Path.Combine(destDirPath, gitIgnoreFileName);
            if (File.Exists(gitIgnorePath) is false)
            {
                File.WriteAllText(gitIgnorePath, stringBuilder.ToString());
            }
        }
    }
}
