using System.IO;
using Modules.Acl.Internal.Utils;
using NUnit.Framework;

namespace Modules.Acl.Core.Internal.Utils
{
    public class TestFileConstraints
    {
        [Test]
        public void Exists()
        {
            var tempFile = Path.GetTempFileName();
            var fileInfo = new FileInfo(tempFile);

            Assert.IsTrue(FileConstraints.EXISTING.Is(fileInfo));

            fileInfo.Delete();

            Assert.IsFalse(FileConstraints.EXISTING.Is(fileInfo));
        }

        [Test]
        public void NotExists()
        {
            var tempFile = Path.GetTempFileName();
            var fileInfo = new FileInfo(tempFile);

            Assert.IsFalse(FileConstraints.NOT_EXISTING.Is(fileInfo));

            fileInfo.Delete();

            Assert.IsTrue(FileConstraints.NOT_EXISTING.Is(fileInfo));
        }

        [Test]
        public void Hidden()
        {
            var tempFile = Path.GetTempFileName();
            File.SetAttributes(tempFile, FileAttributes.Hidden);

            var fileInfo = new FileInfo(tempFile);
            Assert.IsTrue(FileConstraints.HIDDEN.Is(fileInfo));

            File.SetAttributes(tempFile, FileAttributes.Normal);
            fileInfo.Refresh();
            Assert.IsFalse(FileConstraints.HIDDEN.Is(fileInfo));
        }

        [Test]
        public void NotHidden()
        {
            var tempFile = Path.GetTempFileName();
            File.SetAttributes(tempFile, FileAttributes.Hidden);

            var fileInfo = new FileInfo(tempFile);
            Assert.IsFalse(FileConstraints.NOT_HIDDEN.Is(fileInfo));

            File.SetAttributes(tempFile, FileAttributes.Normal);
            fileInfo.Refresh();
            Assert.IsTrue(FileConstraints.NOT_HIDDEN.Is(fileInfo));
        }

        [Test]
        public void Writable()
        {
            var tempFile = Path.GetTempFileName();
            File.SetAttributes(tempFile, FileAttributes.ReadOnly);

            var fileInfo = new FileInfo(tempFile);
            Assert.IsFalse(FileConstraints.WRITABLE.Is(fileInfo));

            File.SetAttributes(tempFile, FileAttributes.Normal);
            fileInfo.Refresh();
            Assert.IsTrue(FileConstraints.WRITABLE.Is(fileInfo));
        }

        [Test]
        public void NotWritable()
        {
            var tempFile = Path.GetTempFileName();
            File.SetAttributes(tempFile, FileAttributes.ReadOnly);

            var fileInfo = new FileInfo(tempFile);
            Assert.IsTrue(FileConstraints.NOT_WRITABLE.Is(fileInfo));

            File.SetAttributes(tempFile, FileAttributes.Normal);
            fileInfo.Refresh();
            Assert.IsFalse(FileConstraints.NOT_WRITABLE.Is(fileInfo));
        }

        [Test]
        public void IsDirectory()
        {
            var tempDir = Path.GetTempPath();
            var tempFile = Path.GetTempFileName();

            var dirInfo = new DirectoryInfo(tempDir);
            var fileInfo = new FileInfo(tempFile);

            Assert.IsTrue(FileConstraints.IS_A_DIRECTORY.Is(dirInfo));
            Assert.IsFalse(FileConstraints.IS_A_DIRECTORY.Is(fileInfo));
        }

        [Test]
        public void IsNotDirectory()
        {
            var tempDir = Path.GetTempPath();
            var tempFile = Path.GetTempFileName();

            var dirInfo = new DirectoryInfo(tempDir);
            var fileInfo = new FileInfo(tempFile);

            Assert.IsFalse(FileConstraints.IS_NOT_A_DIRECTORY.Is(dirInfo));
            Assert.IsTrue(FileConstraints.IS_NOT_A_DIRECTORY.Is(fileInfo));
        }
    }
}