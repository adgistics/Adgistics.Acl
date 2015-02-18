namespace Modules.Acl.Internal.Groups
{
    using System;
    using System.Collections.Generic;
    using System.Web.Script.Serialization;

    using Collections.Graphs;

    using Modules.Acl.Events;

    // TODO: Merge this into the Groups class??
    /// <summary>
    ///   Manages groups, their relationships between each other, and their 
    ///   persistence.
    /// </summary>
    internal sealed class GroupData
    {
        #region Fields

        private readonly AccessControl _api;

        /// <summary>
        ///   Provides persistence capabilities and tracking of group's 
        ///   relationships.
        /// </summary>
        private readonly DirectedGraph<Group> _groupGraph;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Initializes the <see cref="GroupData"/> class.
        /// </summary>
        /// <param name="api"></param>
        public GroupData(AccessControl api)
        {
            _api = api;

            _groupGraph = api.Repositories.GroupGraphRepository.Load();

            _api.Events.GroupPropChanged += OnGroupChanged;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///   Adds the given child group as a child to the given parent group.
        /// </summary>
        /// 
        /// <param name="parent">The parent.</param>
        /// <param name="child">The child.</param>
        /// 
        /// <remarks>
        ///   If the groups have yet to be persisted within the collection
        ///   then they will be persisted via the <see cref="Create"/>
        ///   method.
        /// </remarks>
        public void AddChild(Group parent, Group child)
        {
            if (false == Exists(parent))
            {
                Create(parent);
            }
            if (false == Exists(child))
            {
                Create(child);
            }

            _groupGraph.AddEdge(parent, child);
        }

        /// <summary>
        ///   Adds the given group.
        /// </summary>
        /// 
        /// <param name="group">The group.</param>
        /// 
        /// <exception cref="System.ArgumentException"></exception>
        public void Create(Group @group)
        {
            if (Exists(@group))
            {
                throw new ArgumentException(
                    string.Format("Group already exists with name: {0}",
                        @group.Name));
            }

            _groupGraph.AddVertex(@group);

            PersistData();
        }

        /// <summary>
        ///   Deletes the specified group.
        /// </summary>
        /// 
        /// <param name="group">The group.</param>
        public void Delete(Group @group)
        {
            // TODO: Need some sort of recursive delete overloaded method
            // so that we can delete child groups recursively.

            _groupGraph.RemoveVertex(@group);

            PersistData();
        }

        /// <summary>
        ///   Deletes all groups.
        /// </summary>
        /// 
        /// <remarks>
        ///   This is a utility method to aid testers.
        /// </remarks>
        public void DeleteAllGroups()
        {
            var allGroups = _groupGraph.GetVertices();

            foreach (var @group in allGroups)
            {
                Delete(@group);
            }

            PersistData();
        }

        /// <summary>
        ///   Determines if a group with the given name exists.
        /// </summary>
        /// 
        /// <param name="name">The name.</param>
        /// 
        /// <returns>
        ///   <c>true</c> if and only if a group with the given name exists.
        /// </returns>
        public bool Exists(string name)
        {
            return Get(name) != null;
        }

        /// <summary>
        ///   Determines if the given group already exists within the 
        ///   persisted group collection.
        /// </summary>
        /// 
        /// <param name="group">The group.</param>
        /// 
        /// <returns>
        ///   <c>true</c> if and only if the group exists, else <c>false</c>.
        /// </returns>
        public bool Exists(Group @group)
        {
            return Exists(@group.Name);
        }

        /// <summary>
        ///   Gets the group with the given name.
        /// </summary>
        /// 
        /// <param name="name">The name.</param>
        /// 
        /// <returns>
        ///   The group with the given name, else <c>null</c>.
        /// </returns>
        public Group Get(string name)
        {
            var targetGroup = new Group(_api, name);

            Group actualGroup;
            if (_groupGraph.HasVertex(targetGroup, out actualGroup))
            {
                return actualGroup;
            }

            return null;
        }

        /// <summary>
        ///   Gets all parents for the given group.
        /// </summary>
        /// 
        /// <param name="group">The group.</param>
        /// 
        /// <returns>
        ///   Gets all the parents for the group.
        /// </returns>
        public IEnumerable<Group> GetAllParentsFor(Group @group)
        {
            return _groupGraph.GetAllParentsFor(@group);
        }

        /// <summary>
        ///   Gets the direct children for the given group.
        /// </summary>
        /// 
        /// <param name="group">The group.</param>
        /// 
        /// <returns>
        ///   The direct children.
        /// </returns>
        public IEnumerable<Group> GetChildrenFor(Group @group)
        {
            return _groupGraph.GetChildrenFor(@group);
        }

        public string GetDotFormat()
        {
            // We will serialise properties of a group to JSON format
            // which will allow for easier peristence.
            Func<Group, string> groupFormatter = @group =>
            {
                return new JavaScriptSerializer().Serialize(@group);
            };

            return _groupGraph.ToDotFormat(groupFormatter);
        }

        /// <summary>
        ///   Gets the direct parents for the given group.
        /// </summary>
        /// 
        /// <param name="group">The group.</param>
        /// 
        /// <returns>
        ///   The direct parents.
        /// </returns>
        public IEnumerable<Group> GetParentsFor(Group @group)
        {
            return _groupGraph.GetParentsFor(@group);
        }

        private void OnGroupChanged(object sender, GroupPropChangedEventArgs e)
        {
            PersistData();
        }

        private void PersistData()
        {
            _api.Repositories.GroupGraphRepository.Save(_groupGraph);
        }

        #endregion Methods
    }
}