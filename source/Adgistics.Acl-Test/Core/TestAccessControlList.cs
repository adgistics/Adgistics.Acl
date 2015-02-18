// *****************************************************************************
// * Copyright (c) Adgistics Limited and others. All rights reserved.
// * The contents of this file are subject to the terms of the
// * Adgistics Development and Distribution License (the "License").
// * You may not use this file except in compliance with the License.
// *
// * http://www.adgistics.com/license.html
// *
// * See the License for the specific language governing permissions
// * and limitations under the License.
// *****************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Acl.Example;
using Modules.Acl.Exceptions;
using NUnit.Framework;

namespace Modules.Acl.Core
{
    [TestFixture]
    public class TestAccessControlList
    {
        [Test]
        public void InitAndGet()
        {
            var accessControl =
                AccessControl.Initialize(
                    new AclConfigBuilder().SetIdentifier("foo").Build());


            var ac = AccessControl.Get("foo");

            Assert.IsNotNull(ac, "1.1");
        }

        [Test]
        public void BasicUsage()
        {
            var accessControl = new AccessControl();
            var rules = accessControl.Rules;
            var groups = accessControl.Groups;
            var resources = accessControl.Resources;

            //
            // Create groups
            var groupGuest = groups.Create("guest");

            //
            // Register resources
            var resourceNews = new Resource("news");
            resources.RegisterResource(resourceNews);


            //
            // Check allows when no permissions have been assigned
            Assert.IsFalse(rules.IsAllowed<AllPrivileges>(groupGuest, resourceNews), "1.1");
            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceNews), "1.2");


            //
            // Assign 'view' privilege to 'guest' for 'news'
            rules.Allow<ViewPrivilege>(groupGuest, resourceNews);
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceNews), "2.1");


            //
            // Get the privilege's assigned to 'guest' for 'news'.
            var privileges = rules.GetPrivileges(groupGuest, resourceNews);
            CollectionAssert.AreEquivalent(
                ToTypeArray(new [] { new ViewPrivilege() }),
                ToTypeArray(privileges), 
                "3.1");


            //
            // Remove the allow on 'view' privileged
            rules.RemoveAllow<ViewPrivilege>(groupGuest, resourceNews);

            privileges = rules.GetPrivileges(groupGuest, resourceNews);
            CollectionAssert.AreEqual(new IPrivilege[0], privileges, "4.1");


            //
            // Deny the 'view' privilege from 'guest' for 'news'
            // This isn't particularly useful in this case as the default 
            // rules will deny anyway.  It the case of inherited resources/
            // group rules then the Deny rules become more useful.
            rules.Deny<ViewPrivilege>(groupGuest, resourceNews);
            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceNews), "5.1");

            privileges = rules.GetPrivileges(groupGuest, resourceNews);
            CollectionAssert.AreEqual(
                new Type[0],
                ToTypeArray(privileges), 
                "4.2");

            //
            // Remove the deny on 'view' privilege
            // There will now be no rules set on the resource, so the default
            // rules should take place - i.e. deny all.
            rules.RemoveDeny<ViewPrivilege>(groupGuest, resourceNews);
            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceNews), "6.1");
            
            privileges = rules.GetPrivileges(groupGuest, resourceNews);
            CollectionAssert.AreEqual(
                new Type[0],
                ToTypeArray(privileges), 
                "5.2");
        }

        [Test]
        public void UserPrincipal()
        {
            var accessControl = new AccessControl();
            var rules = accessControl.Rules;
            var groups = accessControl.Groups;
            var resources = accessControl.Resources;

            //
            // Create groups
            var groupGuest = groups.Create("guest");

            //
            // Create users
            var userBob = new User("bob");

            //
            // Register resources
            var resourceNews = new Resource("news");
            resources.RegisterResource(resourceNews);

            var resourceSports = new Resource("sports news");
            resources.RegisterResource(resourceSports, resourceNews);

            //
            // Check allows when no permissions have been assigned
            Assert.IsFalse(rules.IsAllowed<AllPrivileges>(groupGuest, resourceNews), "1.1");
            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceNews), "1.2");
            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceSports), "1.3");
            Assert.IsFalse(rules.IsAllowed<AllPrivileges>(userBob, resourceNews), "1.4");
            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(userBob, resourceNews), "1.5");
            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(userBob, resourceSports), "1.6");

            //
            // Assign user to group
            userBob.AssignToGroup(groupGuest);

            //
            // Allow permission to group
            rules.Allow<ViewPrivilege>(groupGuest, resourceSports);

            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceNews), "2.1");
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceSports), "2.2");
            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(userBob, resourceNews), "2.3");
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(userBob, resourceSports), "2.4");

            //
            // Allow permission to user
            rules.Allow<ViewPrivilege>(userBob, resourceNews);

            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceNews), "3.1");
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceSports), "3.2");
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(userBob, resourceNews), "3.3");
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(userBob, resourceSports), "3.4");
        }

        private Type[] ToTypeArray(IEnumerable<IPrivilege> privileges)
        {
            return privileges.Select(x => x.GetType()).ToArray();
        }

        [Test]
        public void ResourceInheritence()
        {
            var accessControl = new AccessControl();

            var rules = accessControl.Rules;
            var groups = accessControl.Groups;
            var resources = accessControl.Resources;

            // 
            // Create groups
            var groupGuest = groups.Create("guest");

            //
            // Register resources
            var resourceNews = new Resource("news");
            resources.RegisterResource(resourceNews);

            var resourceAnnouncement = new Resource("announcement");
            resources.RegisterResource(resourceAnnouncement, resourceNews);


            //
            // Check allows when no permissions have been assigned
            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceNews), "1.1");
            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceAnnouncement), "1.2");


            //
            // Assign 'view' privilege to 'guest' for 'news'
            rules.Allow<ViewPrivilege>(groupGuest, resourceNews);
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceNews), "2.1");
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceAnnouncement), "2.2");


            //
            // Register another resource
            var resourceNewsletter = new Resource("newsletter");
            resources.RegisterResource(resourceNewsletter, resourceAnnouncement);
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceAnnouncement), "3.1");
        }

        [Test]
        public void GroupInheritence()
        {
            var accessControl = new AccessControl();

            var rules = accessControl.Rules;
            var groups = accessControl.Groups;
            var resources = accessControl.Resources;

            //
            // Create groups
            var groupGuest = groups.Create("guest");
            var groupStaff = groups.Create("staff");

            // 
            // Set up group inheritance
            groups.RegisterInheritance(groupGuest, groupStaff);


            // 
            // Create and register resources
            var resourceNews = new Resource("news");
            resources.RegisterResource(resourceNews);


            //
            // Check allows when no permissions have been assigned
            Assert.IsFalse(rules.IsAllowed<AllPrivileges>(groupGuest, resourceNews), "1.1");
            Assert.IsFalse(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceNews), "1.2");


            //
            // Assign 'view' privilege to 'guest' for 'news'
            rules.Allow<ViewPrivilege>(groupGuest, resourceNews);
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(groupGuest, resourceNews), "2.1");
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(groupStaff, resourceNews), "2.2");


            //
            // Add another child group
            var groupAdmin = groups.Create("administrator");
            groups.RegisterInheritance(groupStaff, groupAdmin);
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(groupAdmin, resourceNews), "3.1");
        }

        [Test]
        public void SpecificPrivileges()
        {
            var privileges = new IPrivilege[]
            {
                new ViewPrivilege(),
                new PublishPrivilege()
            };

            var config = new AclConfigBuilder()
                .SetPrivileges(privileges)
                .Build();

            var accessControl = new AccessControl(config);

            var rules = accessControl.Rules;
            var groups = accessControl.Groups;
            var resources = accessControl.Resources;


            //
            // Create groups
            var groupGuest = groups.Create("guest");


            // 
            // Create and register resources
            var resourceNews = new Resource("news");
            resources.RegisterResource(resourceNews);


            //
            // Check that a rule can be created for each privilege.
            Assert.DoesNotThrow(() =>
            {
                rules.Allow<ViewPrivilege>(groupGuest, resourceNews);
            }, "1.1");
            Assert.DoesNotThrow(() =>
            {
                rules.Allow<PublishPrivilege>(groupGuest, resourceNews);
            }, "1.2");


            // 
            // This privilege was not registered, so should throw error.
            Assert.Throws<PrivilegeNotRegisteredException>(() =>
            {
                rules.Allow<AdminPrivilege>(groupGuest, resourceNews);
            }, "1.3");
        }

        [Test]
        public void PrivilegeDiscovery()
        {
            var accessControl = new AccessControl();

            var rules = accessControl.Rules;
            var groups = accessControl.Groups;
            var resources = accessControl.Resources;

            //
            // Create groups
            var groupGuest = groups.Create("guest");

            //
            // Register resources
            var resourceNews = new Resource("news");
            resources.RegisterResource(resourceNews);

            //
            // Check some of the expected privileges can be used in actions.
            Assert.DoesNotThrow(() =>
            {
                rules.Allow<ViewPrivilege>(groupGuest, resourceNews);
            }, "1.1");
            Assert.DoesNotThrow(() =>
            {
                rules.Allow<PublishPrivilege>(groupGuest, resourceNews);
            }, "1.2");
            Assert.DoesNotThrow(() =>
            {
                rules.Allow<AdminPrivilege>(groupGuest, resourceNews);
            }, "1.3");
        }

        /// <summary>
        ///   These are the test for the example code contained within the 
        ///   Zend Framework ACL implementation documentation.  Although our
        ///   port of the code has been refactored a fair deal, the documented
        ///   examples should still work with only minor syntax changes.
        /// </summary>
        [Test]
        public void ZendDocumentation()
        {
            var accessControl = new AccessControl();

            var rules = accessControl.Rules;
            var groups = accessControl.Groups;
            var resources = accessControl.Resources;

            var groupGuest = groups.Create("guest");
            var groupStaff = groups.Create("staff");
            var groupEditor = groups.Create("editor");
            var groupAdmin = groups.Create("administrator");
            var groupMarketing = groups.Create("marketing");

            var resourceNewsletter = new Resource("newsletter");
            var resourceNews = new Resource("news");
            var resourceLatest = new Resource("latest");
            var resourceAnnouncement = new Resource("announcement");

            //------------------------------------------------------------------
            // 1. INTRODUCTION
            // http://framework.zend.com/manual/2.2/en/modules/zend.permissions.acl.intro.html#defining-access-controls

            groups.RegisterInheritance(groupGuest, groupStaff);
            groups.RegisterInheritance(groupStaff, groupEditor);

            // Guest may only view content
            rules.Allow<ViewPrivilege>(groupGuest);

            // Staff inherits view privilege from guest, but also needs 
            // additional privileges.
            rules.Allow<EditPrivilege>(groupStaff);
            rules.Allow<SubmitPrivilege>(groupStaff);
            rules.Allow<RevisePrivilege>(groupStaff);

            // Editor inherits view, edit, submit, and revise privileges from
            // staff, but also needs additional privileges.
            rules.Allow<PublishPrivilege>(groupEditor);
            rules.Allow<ArchivePrivilege>(groupEditor);
            rules.Allow<DeletePrivilege>(groupEditor);

            // Administrator inherits nothing, but is allowed all privileges.
            rules.Allow<AllPrivileges>(groupAdmin);


            // allowed
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(groupGuest));

            // denied
            Assert.IsFalse(rules.IsAllowed<PublishPrivilege>(groupStaff));

            // allowed
            Assert.IsTrue(rules.IsAllowed<RevisePrivilege>(groupStaff));

            // allowed because of inheritance from guest
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(groupEditor));

            // denied because no allow rule for "update"
            Assert.IsFalse(rules.IsAllowed<UpdatePrivilege>(groupEditor));

            // allowed because administrator is allowed all privileges
            Assert.IsTrue(rules.IsAllowed<ViewPrivilege>(groupAdmin));

            // allowed because administrator is allowed all privileges
            Assert.IsTrue(rules.IsAllowed<AllPrivileges>(groupAdmin));

            // allowed because administrator is allowed all privileges
            Assert.IsTrue(rules.IsAllowed<UpdatePrivilege>(groupAdmin));

            //------------------------------------------------------------------
            // 2. REFINING ACL CONFIGURATION
            // http://framework.zend.com/manual/2.2/en/modules/zend.permissions.acl.refining.html

            // The new marketing group inherits permissions from staff
            groups.RegisterInheritance(groupStaff, groupMarketing);

            // Register resources within the ACL.

            // newsletter
            resources.RegisterResource(resourceNewsletter);

            // news
            resources.RegisterResource(resourceNews);

            // latest news, as a sub resource of news
            resources.RegisterResource(resourceLatest, resourceNews);

            // announcement news, as a sub resource of news
            resources.RegisterResource(resourceAnnouncement, resourceNews);

            // Marketing must be able to publish and archive newsletters and the
            // latest news
            rules.Allow<PublishPrivilege>(
                groupMarketing,
                resourceNewsletter);
            rules.Allow<PublishPrivilege>(
                groupMarketing,
                resourceLatest);
            rules.Allow<ArchivePrivilege>(
                groupMarketing,
                resourceNewsletter);
            rules.Allow<ArchivePrivilege>(
                groupMarketing,
                resourceLatest);

            // Staff (and marketing, by inheritance), are denied permission to
            // revise the latest news
            rules.Deny<RevisePrivilege>(
                groupStaff,
                resourceLatest);

            // Everyone (including administrators) are denied permission to
            // archive news announcements
            rules.Deny<ArchivePrivilege>(resource: resourceAnnouncement);


            // denied
            Assert.IsFalse(
                rules.IsAllowed<PublishPrivilege>(
                    groupStaff,
                    resourceNewsletter));

            // allowed
            Assert.IsTrue(
                rules.IsAllowed<PublishPrivilege>(
                    groupMarketing,
                    resourceNewsletter));

            // denied
            Assert.IsFalse(
                rules.IsAllowed<PublishPrivilege>(groupStaff, resourceLatest));

            // allowed
            Assert.IsTrue(
                rules.IsAllowed<PublishPrivilege>(
                    groupMarketing,
                    resourceLatest));

            // allowed
            Assert.IsTrue(
                rules.IsAllowed<ArchivePrivilege>(
                    groupMarketing,
                    resourceLatest));

            // denied
            Assert.IsFalse(
                rules.IsAllowed<RevisePrivilege>(
                    groupMarketing,
                    resourceLatest));

            // denied
            Assert.IsFalse(
                rules.IsAllowed<ArchivePrivilege>(
                    groupEditor,
                    resourceAnnouncement));

            // denied
            Assert.IsFalse(
                rules.IsAllowed<ArchivePrivilege>(
                    groupAdmin,
                    resourceAnnouncement));


            // NOTE: The Constraint functionality was removed as it was felt
            // that it made the ACL rulesets assertions difficult to cache and/or
            // optimize.
        }
    }
}