using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Coolishbee.PackageGenerator.Utils
{
    internal static class PathUtil
    {
        private const string TemplateDirName = "Template~";
        private const string PackageJsonFileName = "package.json";
        private const string Runtime = "Runtime";
        private const string Editor = "Editor";
        private const string Tests = "Tests";
        private const string AsmdefExt = ".asmdef";
        private const string MetaExt = ".meta";

        private static string _currentPackageRootPath;
        public static string CurrentPackageRootPath => _currentPackageRootPath ??= GetPackageRootPath(CurrentPackageName);

        private static string _currentPackageName;
        public static string CurrentPackageName => _currentPackageName ??= GetCurrentPackageName();

        private static string _templateDirPath;
        public static string TemplateDirPath => _templateDirPath ??= Path.Combine(CurrentPackageRootPath, "Editor", TemplateDirName);

        public static void CopyDirectory(string srcPath, string destPath, bool overwrite = false)
        {
            var src = new DirectoryInfo(srcPath);
            var dest = new DirectoryInfo(destPath);
            dest.Create();

            foreach (var subDir in src.EnumerateDirectories())
            {
                CopyDirectory(subDir.FullName, Path.Combine(dest.FullName, subDir.Name), overwrite);
            }

            foreach (var srcFile in src.EnumerateFiles())
            {
                var targetFile = new FileInfo(Path.Combine(dest.FullName, srcFile.Name));
                if (overwrite || targetFile.Exists is false)
                {
                    srcFile.CopyTo(targetFile.FullName, overwrite);
                }
            }
        }

        public static void CopyFile(string srcFilePath, string destDirectory)
        {
            var srcFile = new FileInfo(srcFilePath);
            Directory.CreateDirectory(destDirectory);
            srcFile.CopyTo(Path.Combine(destDirectory, srcFile.Name), true);
        }

        public static string GetProjectRootPath(string packageName)
        {
            var packageRootPath = FileUtil.GetPhysicalPath(Path.Combine("Packages", packageName));
            var packagesFolderPath = Directory.GetParent(packageRootPath)!.FullName;
            return Directory.GetParent(packagesFolderPath)!.FullName;
        }

        public static string GetUnityProjectRootRelativePath(string fromPath)
        {
            var toPath = Path.GetFullPath(Application.dataPath + "/..");
            return Path.GetRelativePath(fromPath, toPath);
        }

        public static string GetPackageRootPath(string packageName)
        {
            return FileUtil.GetPhysicalPath(Path.Combine("Packages", packageName));
        }

        public static string GetRuntimeAsmdefPath(string packageRoot, string assemblyName)
        {
            return Path.Combine(packageRoot, Runtime, $"Coolishbee.{assemblyName}.{Runtime}{AsmdefExt}");
        }

        public static string GetEditorAsmdefPath(string packageRoot, string assemblyName)
        {
            return Path.Combine(packageRoot, Editor, $"Coolishbee.{assemblyName}.{Editor}{AsmdefExt}");
        }

        public static string GetTestRuntimeAsmdefPath(string packageRoot, string assemblyName)
        {
            return Path.Combine(packageRoot, Tests, Runtime, $"Coolishbee.{assemblyName}.{Tests}.{Runtime}{AsmdefExt}");
        }

        public static string GetTestEditorAsmdefPath(string packageRoot, string assemblyName)
        {
            return Path.Combine(packageRoot, Tests, Editor, $"Coolishbee.{assemblyName}.{Tests}.{Editor}{AsmdefExt}");
        }

        public static string GetMetaFilePath(string assetPath)
        {
            return assetPath + MetaExt;
        }

        private static string GetCurrentPackageName()
        {
            var packageJsonPath = GetCurrentPackageJsonPath();
            var packageJson = JObject.Parse(File.ReadAllText(packageJsonPath));
            var packageName = packageJson["name"]?.ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(packageName), $"Package name must be defined in {PackageJsonFileName}");
            return packageName;
        }

        private static string GetCurrentPackageJsonPath()
        {
            var currentDir = Path.GetDirectoryName(GetCurrentFilePath());
            while (string.IsNullOrEmpty(currentDir) is false)
            {
                var packageJsonPath = Path.Combine(currentDir, PackageJsonFileName);
                if (File.Exists(packageJsonPath))
                {
                    return packageJsonPath;
                }

                currentDir = Path.GetDirectoryName(currentDir);
            }

            throw new InvalidOperationException("Could not find package root.");
        }

        private static string GetCurrentFilePath([System.Runtime.CompilerServices.CallerFilePath] string filePath = "") => filePath;
    }
}
