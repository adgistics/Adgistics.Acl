using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Modules.Acl.Internal.Utils;
using NUnit.Framework;

namespace Modules.Acl.Core.Internal.Utils
{
    [TestFixture]
    public class TestFileVersion
    {
        private FileInfo testFile;

        private DirectoryInfo baseDirectory;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {

        }

        [SetUp]
        public void SetUp()
        {
            testFile = TestingFileResources.Get("test.jpeg");

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            baseDirectory = new DirectoryInfo(tempDir);
            baseDirectory.Create();

            Console.WriteLine("Versions Dir: {0}", baseDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            baseDirectory.Delete(true);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {

        }

        [Test]
        public void Create()
        {
            // ACT
            var result = FileVersion.Create(testFile, baseDirectory);

            // ASSERT
            Assert.IsTrue(result, "The file version create should have returned 'true'.");
            
            var versionsRootDir = baseDirectory.GetDirectories().First();
            Assert.IsNotNull(versionsRootDir, "No 'versions root' directory was found.");
            Assert.AreEqual("__FILEVERSIONS", versionsRootDir.Name, "The versions root directory does not have the expected name.");

            var filenameDir = versionsRootDir.GetDirectories().First();
            Assert.IsNotNull(filenameDir, "No 'file' directory was found.");
            Assert.AreEqual(testFile.Name, filenameDir.Name, "The filename directory should have same name as source file.");

            var yearDir = filenameDir.GetDirectories().First();
            Assert.IsNotNull(yearDir, "No 'year' directory was found.");
            Assert.IsTrue(Regex.IsMatch(yearDir.Name, @"^\d{4}$"), "The year directory is not of expected format");
            Assert.AreEqual(DateTime.Now.Year.ToString(), yearDir.Name);

            var monthDir = yearDir.GetDirectories().First();
            Assert.IsNotNull(monthDir, "No 'month' directory was found.");
            Assert.IsTrue(Regex.IsMatch(monthDir.Name, @"^\d{1,2}$"), "The month directory is not of expected format");
            Assert.AreEqual(DateTime.Now.Month.ToString(), monthDir.Name);

            var dayDir = monthDir.GetDirectories().First();
            Assert.IsNotNull(dayDir, "No 'day' directory was found.");
            Assert.IsTrue(Regex.IsMatch(dayDir.Name, @"^\d{1,2}$"), "The day directory is not of expected format");
            Assert.AreEqual(DateTime.Now.Day.ToString(), dayDir.Name);

            var fileVersion = dayDir.GetFiles().First();
            Assert.IsNotNull(fileVersion, "No 'file version' file was found.");
            Assert.IsTrue(
                Regex.IsMatch(fileVersion.Name, @"^\d{2}h\d{2}m\d{2}s_\d{7}\.jpeg$"),
                "The file version name is not of expected format");
            FileAssert.AreEqual(testFile, fileVersion, "The created file version does not match the source file.");
        }

        [Test]
        public void CreateMultiple()
        {
            // ACT
            FileVersion.Create(testFile, this.baseDirectory);
            FileVersion.Create(testFile, this.baseDirectory);
            FileVersion.Create(testFile, this.baseDirectory);
            FileVersion.Create(testFile, this.baseDirectory);
            FileVersion.Create(testFile, this.baseDirectory);

            // ASSERT
            var versionsRootDir = baseDirectory.GetDirectories().First();
            Assert.IsNotNull(versionsRootDir, "No 'versions root' directory was found.");
            Assert.AreEqual("__FILEVERSIONS", versionsRootDir.Name, "The versions root directory does not have the expected name.");

            var filenameDir = versionsRootDir.GetDirectories().First();
            Assert.IsNotNull(filenameDir, "No 'file' directory was found.");
            Assert.AreEqual(testFile.Name, filenameDir.Name, "The filename directory should have same name as source file.");

            var yearDir = filenameDir.GetDirectories().First();
            Assert.IsNotNull(yearDir, "No 'year' directory was found.");
            Assert.IsTrue(Regex.IsMatch(yearDir.Name, @"^\d{4}$"), "The year directory is not of expected format");
            Assert.AreEqual(DateTime.Now.Year.ToString(), yearDir.Name);

            var monthDir = yearDir.GetDirectories().First();
            Assert.IsNotNull(monthDir, "No 'month' directory was found.");
            Assert.IsTrue(Regex.IsMatch(monthDir.Name, @"^\d{1,2}$"), "The month directory is not of expected format");
            Assert.AreEqual(DateTime.Now.Month.ToString(), monthDir.Name);

            var dayDir = monthDir.GetDirectories().First();
            Assert.IsNotNull(dayDir, "No 'day' directory was found.");
            Assert.IsTrue(Regex.IsMatch(dayDir.Name, @"^\d{1,2}$"), "The day directory is not of expected format");
            Assert.AreEqual(DateTime.Now.Day.ToString(), dayDir.Name);

            Assert.AreEqual(5, dayDir.GetFiles().Count(), "There should be 5 version files.");

            foreach (var fileVersion in dayDir.GetFiles())
            {
                Assert.IsTrue(
                    Regex.IsMatch(fileVersion.Name, @"^\d{2}h\d{2}m\d{2}s_\d{7}\.jpeg$"),
                    "The file version name is not of expected format");
                FileAssert.AreEqual(testFile, fileVersion, "The created file version does not match the source file.");
            }
        }
    }
}