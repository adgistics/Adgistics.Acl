namespace Modules.Acl
{
    /// <summary>
    ///   A resource that supports ACL control.
    /// </summary>
    public interface IResource
    {
        #region Properties

        /// <summary>
        /// Gets the resource identifier.
        /// </summary>
        /// <value>
        /// The resource identifier.
        /// </value>
        ResourceId ResourceId
        {
            get;
        }

        #endregion Properties
    }
}