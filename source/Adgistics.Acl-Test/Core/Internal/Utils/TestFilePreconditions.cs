using System;
using System.IO;
using Modules.Acl.Internal.Utils;
using NUnit.Framework;

namespace Modules.Acl.Core.Internal.Utils
{
    [TestFixture]
    public class TestFilePreconditions
    {
        [Test]
        public void CheckFail()
        {
            // ARRANGE
            var fileInfo = new FileInfo(@"c:\file.txt");
            var expected = string.Format(
                "Argument: 'fileInfo' does not meet file constraint. fileInfo:{0}, Constraint:{1}",
                fileInfo.FullName,
                FileConstraints.EXISTING.Name);

            // ACT
            var result =
                Assert.Throws<ArgumentException>(
                    () => FilePreconditions.Check(fileInfo, FileConstraints.EXISTING));

            // ASSERT
            Assert.AreEqual(expected, result.Message);
        }
        [Test]
        public void CheckSucceed()
        {
            // ARRANGE
            var fileInfo = new FileInfo(Path.GetTempFileName());
            
            // ACT
            Assert.DoesNotThrow(() => FilePreconditions.Check(fileInfo, FileConstraints.EXISTING));
        }

        [Test]
        public void IsFalse()
        {
            // ARRANGE
            IFileConstraint failed;

            // ACT
            var result = FilePreconditions.Is(
                new FileInfo(@"c:\file.txt"), new[] { FileConstraints.EXISTING }, out failed);

            // ASSERT
            Assert.IsFalse(result);
            Assert.IsNotNull(failed);
            Assert.AreEqual(failed, FileConstraints.EXISTING);
        }

        [Test]
        public void IsTrue()
        {
            // Arrange
            var tempFile = new FileInfo(Path.GetTempFileName());

            IFileConstraint failed;

            // Act
            var result = FilePreconditions.Is(tempFile, new[] { FileConstraints.EXISTING }, out failed);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNull(failed);
        }
    }
}