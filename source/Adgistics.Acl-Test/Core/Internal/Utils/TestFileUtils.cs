using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Modules.Acl.Internal.Utils;
using NUnit.Framework;

namespace Modules.Acl.Core.Internal.Utils
{
    public class TestFileUtils
    {
        private IList<string> filesToDelete = new List<string>();

        [Test]
        [Sequential]
        public void TestFilenames(
            [Values("foo", "foo.bad", "*", "£", "$%adf%'' ()")] string filename,
            [Values(true, false, false, false, false)] bool expected)
        {
            var result = FileUtils.IsValidFilename(filename);
            Assert.AreEqual(expected, result);
        }

        [Test]
        [Sequential]
        public void TestEscape(
            [Values("foo", "foo@bar", "foo^bar%bar", "[bar]@[foo]_[dog]-(cat)", "%", "me@قققبل.com")] string filename,
            [Values("foo", "foo_at_bar", "foobarbar", "[bar]_at_[foo]_[dog]-(cat)", "", "me_at_قققبلcom")] string
                expected)
        {
            var result = FileUtils.Escape(filename);
            Assert.AreEqual(expected, result);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            foreach (var file in this.filesToDelete)
            {
                var path = Path.Combine(Path.GetTempPath(), file);
                File.Delete(path);
            }
        }

        [Test]
        public void WriteStringToFileASCII()
        {
            // ARRANGE
            var expected = "ABC";
            var filePath = FileUtils.ConcatenateFile(new DirectoryInfo(Path.GetTempPath()), "WriteStringToFile.txt");
            
            // ACT
            IOUtils.WriteStringToFile(filePath, expected, Encoding.ASCII);

            // ASSERT
            var actual = File.ReadAllText(filePath.ToString());

            Assert.AreEqual(expected, actual, "Test writting a single string");


            /** Append Test **/
           
            // ACT
            IOUtils.WriteStringToFile(filePath, expected, Encoding.ASCII, true);

            expected += expected;

            // ASSERT
            actual = File.ReadAllText(filePath.ToString());

            Assert.AreEqual(expected, actual);

            this.filesToDelete.Add("WriteStringToFile.txt");
        }

        [Test]
        public void WriteStringToFileEncoding()
        {
            // ARRANGE
            var expected = "ЀЁЂABC";
            var filePath = FileUtils.ConcatenateFile(new DirectoryInfo(Path.GetTempPath()), "WriteStringToFileEncoding.txt");

            // ACT
            IOUtils.WriteStringToFile(filePath, expected, Encoding.ASCII);

            // ASSERT
            var actual = File.ReadAllText(filePath.ToString());
            expected = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(expected));
            
            Assert.AreEqual(expected, actual, "Failed writing UNICODE as ASCII.");

            /** Write UNICODE Append **/
            
            // ARRANGE
            var append = "ЀЁЂ";

            // ACT
            IOUtils.WriteStringToFile(filePath, append, Encoding.UTF8, true);

            // ASSERT
            actual = File.ReadAllText(filePath.ToString());
            expected = expected + append;

            Assert.AreEqual(expected, actual);

            this.filesToDelete.Add("WriteStringToFileEncoding.txt");
        }

        [Test]
        public void TestEscapeMap()
        {
            // ARRANGE
            var filename = "me@adgistics%^&][hello.com";

            var map = new Dictionary<char, string> { { '%', "pc" }, { '^', "hat" } };

            // ACT
            var result = FileUtils.Escape(filename, map);

            // ASSERT
            Assert.AreEqual("me_at_adgisticspchat][hellocom", result);
        }
    }
}