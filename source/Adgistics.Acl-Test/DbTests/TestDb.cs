using NUnit.Framework;

namespace EasyACL.DbTests
{
    [TestFixture]
    public class TestDb
    {
        [Test]
        public void RunDbTests()
        {
            TestHelper.RunALL(true);
        }
    }
}