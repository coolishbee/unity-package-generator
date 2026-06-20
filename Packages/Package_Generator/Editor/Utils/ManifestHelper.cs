using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Coolishbee.PackageGenerator.Utils
{
    internal static class ManifestHelper
    {
        public const string ManifestJsonPath = "Packages/manifest.json";
        public const string DependenciesPropertyName = "dependencies";
        public const string TestablesPropertyName = "testables";

        public static JObject GetManifestJObject()
        {
            if (File.Exists(ManifestJsonPath) is false)
            {
                return new JObject();
            }

            var manifestJson = File.ReadAllText(ManifestJsonPath);
            return string.IsNullOrWhiteSpace(manifestJson) ? new JObject() : JObject.Parse(manifestJson);
        }

        public static JObject AcquireDependencies(JObject manifest)
        {
            var dependencies = manifest[DependenciesPropertyName] as JObject;
            if (dependencies == null)
            {
                dependencies = new JObject();
                manifest[DependenciesPropertyName] = dependencies;
            }

            return dependencies;
        }

        public static JArray AcquireTestables(JObject manifest)
        {
            var testables = manifest[TestablesPropertyName] as JArray;
            if (testables == null)
            {
                testables = new JArray();
                manifest[TestablesPropertyName] = testables;
            }

            return testables;
        }

        public static void Save(JObject manifest)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ManifestJsonPath));
            File.WriteAllText(ManifestJsonPath, manifest.ToString(Formatting.Indented));
        }

    }
}
