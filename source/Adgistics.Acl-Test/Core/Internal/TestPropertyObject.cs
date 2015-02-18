using System;
using Modules.Acl.Internal;
using NUnit.Framework;

namespace Modules.Acl.Core.Internal
{
    [TestFixture]
    public class TestPropertyObject
    {
        [Test]
        public void GetSetPropertiesAgent()
        {
            var p = new PropertyObject();
            const string key = "Seven.Agents";
            const string expected = "first-value";

            // Get contract behavior
            Assert.IsNull(p.GetProperty(key), "1.0");
            p.SetString(key,expected);
            Assert.AreEqual(expected, p.GetProperty(key), "1.1");

            // Get null key
            Assert.Catch<ArgumentException>(() =>
            {
                Assert.IsNull(p.GetProperty(null), "2.1");
            });
        }

        [Test]
        public void SetPropertiesAgent()
        {
            var p = new PropertyObject();
            const string key = "Seven.Agents";
            const string expected = "first-value";

            // Set contract behavior
            Assert.IsNull(p.GetProperty(key), "1.0");
            p.SetString(key, expected);
            Assert.AreEqual(expected, p.GetProperty(key), "1.1");

            // Key with null value as per contract
            p.SetString(key, null);
            Assert.IsNull(p.GetProperty(key), "2.0");
            // just check key was not silently added
            var s = p.ToString();
            Assert.False(s.Contains(key), "2.1");

            // Check that duplicate keys are handled correctly
            p.SetString(key, expected);
            p.SetString(key, "second-value");
            Assert.AreEqual("second-value", p.GetProperty(key), "3.0");
        } 
    }
}