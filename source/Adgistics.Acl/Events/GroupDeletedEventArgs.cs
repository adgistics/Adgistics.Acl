namespace Modules.Acl.Events
{
    using System;

    /// <summary>
    ///   Arguments for the OnGroupDeleted event which occurs after a group
    ///   has been deleted.
    /// </summary>
    public sealed class GroupDeletedEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the 
        ///   <see cref="GroupDeletedEventArgs"/> class.
        /// </summary>
        /// 
        /// <param name="group">The group that was deleted.</param>
        public GroupDeletedEventArgs(Group @group)
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