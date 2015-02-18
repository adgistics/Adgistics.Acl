namespace Modules.Acl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Modules.Acl.Internal;
    using Modules.Acl.Internal.Groups;

    /// <summary>
    ///   Provides actions for creating and querying <see cref="Group"/>s
    ///   and their relationships.
    /// </summary>
    public sealed class Groups
    {
        #region Fields

        private readonly AccessControl _api;
        private readonly GroupData _groupsData;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="Groups" /> class.
        /// </summary>
        /// 
        /// <param name="api">The access control instance api.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'api' must not be null.
        ///   </para>
        /// </exception>
        internal Groups(AccessControl api)
        {
            if (api == null)
            {
                throw new ArgumentException(
                    "Argument 'api' must not be null.");
            }

            _api = api;
            _groupsData = new GroupData(_api);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///    Creates a group with the specified name.
        /// </summary>
        /// 
        /// <param name="name">The name.</param>
        /// 
        /// <returns>
        ///   The created group.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'name' must not be null, whitespace only, or empty.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'name' collides with an existing group name.
        ///   </para>
        /// </exception>
        public Group Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Argument 'name' must not be null, whitespace only, or empty.");
            }
            if (Exists(name))
            {
                throw new ArgumentException(
                    "Argument 'name' collides with an existing group name.");
            }

            var group = new Group(_api, name);

            _groupsData.Create(group);

            return group;
        }

        /// <summary>
        ///   Deletes the given group.
        /// </summary>
        /// 
        /// <param name="group">The group.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   Argument 'group' must not be null.
        /// </exception>
        /// 
        /// <remarks>
        ///   The <see cref="AclEvents.GroupDeleted"/> event will be fired 
        ///   after a group is deleted.
        /// </remarks>
        public void Delete(Group group)
        {
            if (group == null)
            {
                throw new ArgumentException("Argument 'group' must not be null.");
            }

            _groupsData.Delete(group);

            _api.Events.OnGroupDeleted(@group);
        }

        /// <summary>
        ///   Determines if a group with the given name exists.
        /// </summary>
        /// 
        /// <param name="name">The name of the group.</param>
        /// 
        /// <returns>
        ///   <c>true</c> if and only if a group with the given name exists,
        ///   else <c>false</c>.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   Argument 'name' must not be null, whitespace only, or empty.
        /// </exception>
        public bool Exists(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Argument 'name' must not be null, whitespace only, or empty.");
            }

            return _groupsData.Exists(name);
        }

        /// <summary>
        ///   Gets the group with the specified name.
        /// </summary>
        /// 
        /// <param name="name">The name.</param>
        /// 
        /// <returns>
        ///   The group with the given name if it was found, else <c>null</c>.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   Argument 'name' must not be null, whitespace only, or empty.
        /// </exception>
        public Group Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Argument 'name' must not be null, whitespace only, or empty.");
            }

            return _groupsData.Get(name);
        }

        /// <summary>
        ///   Gets all the ancestors for the given group.
        /// </summary>
        /// 
        /// <param name="group">The group.</param>
        /// 
        /// <returns>
        ///   The ancestors for the given group.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'group' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'group' does not exist in ACL system.
        ///   </para>
        /// </exception>
        public IEnumerable<Group> GetAllAncestors(Group group)
        {
            if (group == null)
            {
                throw new ArgumentException(
                    "Argument 'group' must not be null.");
            }
            if (false == _groupsData.Exists(group))
            {
                throw new ArgumentException(
                    "Argument 'group' does not exist in ACL system.");
            }

            return _groupsData.GetAllParentsFor(group);
        }

        /// <summary>
        ///   Gets the direct child groups for the given group.
        /// </summary>
        /// 
        /// <returns>
        ///   The children for the group if it has any, else an empty list.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'group' must not be null, whitespace only, or empty.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'group' does not exist.
        ///   </para>
        /// </exception>
        /// 
        /// <remarks>
        ///   This only returns the "direct" children for the group. 
        ///   i.e. only 1 level deep.
        /// </remarks>
        public IEnumerable<Group> GetChildren(string group)
        {
            if (group == null)
            {
                throw new ArgumentException(
                    "Argument 'group' must not be null.");
            }

            var g = _groupsData.Get(group);
            if (g == null)
            {
                throw new ArgumentException(
                    "Argument 'group' does not exist as a group.");
            }

            return GetChildren(g);
        }

        /// <summary>
        ///   Gets the direct child groups for the given group.
        /// </summary>
        /// 
        /// <returns>
        ///   The children for the group if it has any, else an empty list.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'group' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'group' is not registered in the ACL system.
        ///   </para>
        /// </exception>
        /// 
        /// <remarks>
        ///   This only returns the "direct" children for the group. 
        ///   i.e. only 1 level deep.
        /// </remarks>
        public IEnumerable<Group> GetChildren(Group group)
        {
            if (group == null)
            {
                throw new ArgumentException(
                    "Argument 'group' must not be null.");
            }

            if (false == _groupsData.Exists(group))
            {
                throw new ArgumentException(
                    "Argument 'group' is not registered in the ACL system.");
            }

            return _groupsData.GetChildrenFor(group);
        }

        /// <summary>
        ///   Gets the direct parent groups for the given group.
        /// </summary>
        /// 
        /// <returns>
        ///    The parents for the group if it has any, else an empty list.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'group' must not be null, whitespace only, or empty.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'group' does not exist as a group.
        ///   </para>
        /// </exception>
        /// 
        /// <remarks>
        ///   This only returns the "direct" parents for the group. 
        ///   i.e. only 1 level above.
        /// </remarks>
        public IEnumerable<Group> GetParents(string group)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                throw new ArgumentException(
                    "Argument 'group' must not be null, whitespace only, or empty.");
            }

            var g = _groupsData.Get(group);
            if (g == null)
            {
                throw new ArgumentException(
                    "Argument 'group' does not exist as a group.");
            }

            return GetParents(g);
        }

        /// <summary>
        ///   Gets the direct parent groups for the given group.
        /// </summary>
        /// 
        /// <returns>
        ///    The parents for the group if it has any, else an empty list.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'group' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'group' does not exist in ACL system.
        ///   </para>
        /// </exception>
        /// 
        /// <remarks>
        ///   This only returns the "direct" parents for the group. 
        ///   i.e. only 1 level above.
        /// </remarks>
        public IEnumerable<Group> GetParents(Group group)
        {
            if (group == null)
            {
                throw new ArgumentException(
                    "Argument 'group' must not be null.");
            }
            if (false == _groupsData.Exists(group))
            {
                throw new ArgumentException(
                    "Argument 'group' does not exist in ACL system.");
            }

            return _groupsData.GetParentsFor(group);
        }

        /// <summary>
        ///   Determines if the given <paramref name="group"/> is a child 
        ///   instance of the <paramref name="parent"/> group.
        /// </summary>
        /// 
        /// <param name="group">The group.</param>
        /// <param name="parent">The name of the parent group to test for.</param>
        /// 
        /// <returns>
        ///   <c>true</c> if and only if parent is ancestor for the given
        ///   group, else <c>false</c>.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'group' must not be null, whitespace only, or empty.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'group' does not exist as a group.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'parent' must not be null, whitespace only, or empty.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'parent' does not exist as a group.
        ///   </para>
        /// </exception>
        /// 
        /// <remarks>
        ///   This will check if the <paramref name="parent"/> is ancestor 
        ///   at any level for the group.  i.e. not necessarily a direct 
        ///   parent.  
        /// </remarks>
        public bool IsChildOf(string group, string parent)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                throw new ArgumentException(
                    "Argument 'group' must not be null, whitespace only, or empty.");
            }
            if (string.IsNullOrWhiteSpace(parent))
            {
                throw new ArgumentException(
                    "Argument 'parent' must not be null, whitespace only, or empty.");
            }

            var g = _groupsData.Get(group);
            if (g == null)
            {
                throw new ArgumentException(
                    "Argument 'group' does not exist as a group.");
            }

            var p = _groupsData.Get(parent);
            if (p == null)
            {
                throw new ArgumentException(
                    "Argument 'parent' does not exist as a group.");
            }

            return _groupsData.GetAllParentsFor(g).Contains(p);
        }

        /// <summary>
        ///   Determines if the given <paramref name="group"/> is a child 
        ///   instance of the <paramref name="parent"/> group.
        /// </summary>
        /// 
        /// <param name="group">The group.</param>
        /// <param name="parent">The name of the parent group to test for.</param>
        /// 
        /// <returns>
        ///   <c>true</c> if and only if parent is ancestor for the given
        ///   group, else <c>false</c>.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'group' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'parent' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'group' does not exist in ACL system.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'parent' does not exist in ACL system.
        ///   </para>
        /// </exception>
        /// 
        /// <remarks>
        ///   This will check if the <paramref name="parent"/> is ancestor 
        ///   at any level for the group.  i.e. not necessarily a direct 
        ///   parent.  
        /// </remarks>
        public bool IsChildOf(Group group, Group parent)
        {
            if (group == null)
            {
                throw new ArgumentException(
                    "Argument 'group' must not be null.");
            }
            if (parent == null)
            {
                throw new ArgumentException(
                    "Argument 'parent' must not be null.");
            }
            if (false == _groupsData.Exists(group))
            {
                throw new ArgumentException(
                    "Argument 'group' is not registered in the ACL system.");
            }
            if (false == _groupsData.Exists(parent))
            {
                throw new ArgumentException(
                    "Argument 'parent' is not registered in the ACL system.");
            }

            return _groupsData.GetAllParentsFor(group).Contains(parent);
        }

        /// <summary>
        ///   Adds the given group as a child group to the given group.
        /// </summary>
        /// 
        /// <param name="group">The name of the group.</param>
        /// <param name="child">The name of the child group.</param>
        /// 
        /// <exception cref="ArgumentException">
        ///   <para>
        ///   Argument 'child' must not be null.
        ///   </para>
        ///   OR 
        ///   <para>
        ///   If <paramref name="child"/> already has this instance as a child
        ///   to itself.  This would cause a circular reference.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'group' must not be null, whitespace only, or empty.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'group' is not registered in the ACL system.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'child' must not be null, whitespace only, or empty.
        ///   </para>
        /// </exception>
        /// 
        /// <remarks>
        ///   If a child group with the given name does not yet exist it will be 
        ///   created.
        /// </remarks>
        public Group RegisterInheritance(string group, string child)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                throw new ArgumentException(
                    "Argument 'group' must not be null, whitespace only, or empty.");
            }
            if (string.IsNullOrWhiteSpace(child))
            {
                throw new ArgumentException(
                    "Argument 'child' must not be null, whitespace only, or empty.");
            }

            var g = _groupsData.Get(group);
            if (g == null)
            {
                throw new ArgumentException(
                    "Argument 'group' is not registered in the ACL system.");
            }

            // We will create the child group if it does not exist
            var c = _groupsData.Get(child);
            if (c == null)
            {
                c = Create(child);
            }

            RegisterInheritance(g, c);

            return c;
        }

        /// <summary>
        ///   Adds the given group as a child group to the given group.
        /// </summary>
        /// 
        /// <param name="group">The name of the group.</param>
        /// <param name="child">The name of the child group.</param>
        /// 
        /// <exception cref="ArgumentException">
        ///   <para>
        ///   Argument 'child' must not be null.
        ///   </para>
        ///   OR 
        ///   <para>
        ///   If <paramref name="child"/> already has this instance as a child
        ///   to itself.  This would cause a circular reference.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'group' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'group' is not registered in the ACL system.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'child' must not be null, whitespace only, or empty.
        ///   </para>
        /// </exception>
        /// 
        /// <remarks>
        ///   If a child group with the given name does not yet exist it will be 
        ///   created.
        /// </remarks>
        public Group RegisterInheritance(Group group, string child)
        {
            if (group == null)
            {
                throw new ArgumentException(
                    "Argument 'group' must not be null.");
            }
            if (string.IsNullOrWhiteSpace(child))
            {
                throw new ArgumentException(
                    "Argument 'child' must not be null, whitespace only, or empty.");
            }
            if (false == _groupsData.Exists(group))
            {
                throw new ArgumentException(
                    "Argument 'group' is not registered in the ACL system.");
            }

            // We will create the child group if it does not exist
            var c = _groupsData.Get(child);
            if (c == null)
            {
                c = Create(child);
            }

            RegisterInheritance(group, c);

            return c;
        }

        /// <summary>
        ///   Adds the given group as a child group to the given group.
        /// </summary>
        /// 
        /// <param name="group">The group.</param>
        /// <param name="child">The child.</param>
        /// 
        /// <exception cref="ArgumentException">
        ///   <para>
        ///   Argument 'group' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'child' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'group' does not exist in ACL system.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'child' does not exist in ACL system.
        ///   </para>
        ///   OR 
        ///   <para>
        ///   If <paramref name="child"/> already has this instance as a child
        ///   to itself.  This would cause a circular reference.
        ///   </para>
        /// </exception>
        public void RegisterInheritance(Group group, Group child)
        {
            if (group == null)
            {
                throw new ArgumentException("Argument 'group' must not be null.");
            }
            if (child == null)
            {
                throw new ArgumentException("Argument 'child' must not be null.");
            }
            if (false == _groupsData.Exists(group))
            {
                throw new ArgumentException(
                    "Argument 'group' is not registered in the ACL system.");
            }
            if (false == _groupsData.Exists(child))
            {
                throw new ArgumentException(
                    "Argument 'child' is not registered in the ACL system.");
            }

            _groupsData.AddChild(group, child);
        }

        #endregion Methods
    }
}