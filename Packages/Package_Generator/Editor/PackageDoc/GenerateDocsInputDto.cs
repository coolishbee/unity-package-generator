namespace Coolishbee.PackageGenerator.PackageDoc
{
    public readonly struct GenerateDocsInputDto
    {
        public string DestDirPath { get; init; }
        public string DisplayName { get; init; }
        public string Description { get; init; }
        public bool IncludeChangelog { get; init; }
    }
}
