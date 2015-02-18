namespace Modules.Acl.Events
{
    using System;

    /// <summary>
    ///   Arguments for the OnGroupDeleted event which occurs after a group
    ///   has been deleted.
    /// </summary>
    public sealed class ResourceUnregisteredEventArgs : EventArgs
    {
        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the 
        ///   <see cref="ResourceUnregisteredEventArgs"/> class.
        /// </summary>
        /// 
        /// <param name="resourceId">
        ///   The identifier of the resource that was unregistered.
        /// </param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   Argument 'resourceId' must not be null.
        /// </exception>
        public ResourceUnregisteredEventArgs(ResourceId resourceId)
        {
            if (resourceId == null)
            {
                throw new ArgumentException(
                    "Argument 'resourceId' must not be null.");
            }

            ResourceId = resourceId;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///   The identifier of the resource that was unregistered.
        /// </summary>
        public ResourceId ResourceId
        {
            get; private set;
        }

        #endregion Properties
    }
}