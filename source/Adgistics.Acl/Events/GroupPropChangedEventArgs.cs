namespace Modules.Acl.Events
{
    using System;

    /// <summary>
    ///   Arguments for the OnGroupChanged event which occurs after a group's
    ///   properties have changed.
    /// </summary>
    public sealed class GroupPropChangedEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the 
        ///   <see cref="GroupDeletedEventArgs"/> class.
        /// </summary>
        /// 
        /// <param name="group">The group that was deleted.</param>
        public GroupPropChangedEventArgs(Group @group)
        {
            Group = @group;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///   The group that was deleted.
        /// </summary>
        public Group Group
        {
            get; private set;
        }

        #endregion Properties
    }
}