using System.Collections.Generic;

namespace Coolishbee.PackageGenerator.PackageDoc
{
    [System.Serializable]
    public sealed class DocfxConfigDto
    {
        public List<Metadata> metadata;
        public Build build;
    }

    [System.Serializable]
    public sealed class Metadata
    {
        public List<SrcItem> src;
        public string dest;
    }

    [System.Serializable]
    public sealed class SrcItem
    {
        public string src;
        public List<string> files;
    }

    [System.Serializable]
    public sealed class Build
    {
        public List<ContentItem> content;
        public List<ResourceItem> resource;
        public string output;
        public List<string> template;
        public Dictionary<string, object> globalMetadata;
        public string markdownEngineName;
        public Dictionary<string, object> markdownEngineProperties;
    }

    [System.Serializable]
    public sealed class ContentItem
    {
        public List<string> files;
        public List<string> exclude;
    }

    [System.Serializable]
    public sealed class ResourceItem
    {
        public List<string> files;
        public List<string> exclude;
    }
}
