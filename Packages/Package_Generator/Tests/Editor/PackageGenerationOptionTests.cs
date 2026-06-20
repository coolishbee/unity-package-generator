using NUnit.Framework;

namespace Coolishbee.PackageGenerator
{
    [TestFixture]
    public sealed class PackageGenerationOptionTests
    {
        [Test]
        public void NoneCategory_BuildsPackageNameWithoutCategoryToken()
        {
            var packageName = PackageGenerationOption.BuildPackageName(PackageCategory.None, "My Package");

            Assert.That(packageName, Is.EqualTo("com.author.my-package"));
        }

        [Test]
        public void ScriptingCategory_BuildsPackageNameWithCategoryToken()
        {
            var packageName = PackageGenerationOption.BuildPackageName(PackageCategory.Scripting, "Event Logging");

            Assert.That(packageName, Is.EqualTo("com.author.scripting.event-logging"));
        }

        [Test]
        public void DefaultPackageName_UsesDefaultPrefix()
        {
            var option = new PackageGenerationOption();

            Assert.That(option.PackageName, Is.EqualTo("com.author.my-package"));
        }
    }
}
