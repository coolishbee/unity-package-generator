using NUnit.Framework;

namespace Coolishbee.PackageGenerator
{
    [TestFixture]
    public sealed class PackageGenerationOptionValidatorTests
    {
        [Test]
        public void DefaultOption_IsValid()
        {
            var option = new PackageGenerationOption
            {
                DisplayName = "Validator Test Package",
                PackageName = "com.author.validator-test-package"
            };

            Assert.That(PackageGenerationOptionValidator.Validate(option, out var error), Is.True, error);
        }

        [Test]
        public void InvalidPackageName_IsRejected()
        {
            var option = new PackageGenerationOption
            {
                PackageName = "Invalid Package Name"
            };

            Assert.That(PackageGenerationOptionValidator.Validate(option, out _), Is.False);
        }
    }
}
