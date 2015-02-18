using System;
using Modules.Acl.Events;
using NUnit.Framework;

namespace Modules.Acl.Core
{
    [TestFixture]
    public class TestGroups
    {
        private Groups _groups;
        private AccessControl _accessControl;

        [SetUp]
        public void SetUp()
        {
            _accessControl = new AccessControl();

            _groups = _accessControl.Groups;
        }

        [Test]
        public void Create()
        {
            var fooGroup = _groups.Create("foo");

            Assert.IsNotNull(fooGroup, "1.1");
            Assert.AreEqual("foo", fooGroup.Name, "1.2");

            // Name can't be null
            Assert.Catch<ArgumentException>(() =>
            {
                _groups.Create(null);
            }, "1.3");

            // Group already exists
            Assert.Catch<ArgumentException>(() =>
            {
                _groups.Create("foo");
            }, "1.4");

            // Group can't be a subgroup of itself
            var foo2 = _groups.RegisterInheritance(fooGroup, "foo2");
            Assert.Catch<ArgumentException>(() =>
            {
                _groups.RegisterInheritance(foo2, fooGroup);
            });
        }

        [Test]
        public void SetProperties()
        {
            var group = _groups.Create("baz");

            //
            // Not allowed to set a property with name of 'name'.
            Assert.Catch<ArgumentException>(() =>
            {
                group.SetString("name", "bob");
            }, "1.1");
            Assert.Catch<ArgumentException>(() =>
            {
                group.SetInt("name", 1);
            }, "1.2");
            Assert.Catch<ArgumentException>(() =>
            {
                group.SetBool("name", true);
            }, "1.3");
            Assert.Catch<ArgumentException>(() =>
            {
                group.SetString("name", null);
            }, "1.4");


            //
            // Create a valid set of properties
            group = _groups.Create("foo");

            group.SetBool("MyBool", true);
            group.SetInt("MyInt", 123);
            group.SetString("MyString", "abc");

            Assert.AreEqual(true, group.GetBool("MyBool", false));
            Assert.AreEqual(123, group.GetInt("MyInt", 0));
            Assert.AreEqual("abc", group.GetString("MyString", string.Empty));
        }

        [Test]
        public void Delete()
        {
            GroupDeletedEventArgs eventArgsResult = null;

            // Set up an event handler
            _accessControl.Events.GroupDeleted += (sender, args) =>
            {
                eventArgsResult = args;
            };

            // Create, then delete a group.
            var group = _groups.Create("bob");
            _groups.Delete(group);

            Assert.IsFalse(_groups.Exists("bob"), "1.1");
            Assert.IsNotNull(eventArgsResult, "1.2");
            Assert.AreEqual(group, eventArgsResult.Group, "1.3");
        }
    }
}