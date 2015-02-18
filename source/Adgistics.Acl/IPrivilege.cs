namespace Modules.Acl
{
    /// <summary>
    ///   An ACL privilege (aka permission).
    /// </summary>
    public interface IPrivilege
    {
        #region Properties

        /// <summary>
        ///   Gets the description of the privilege.
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        ///   Gets the identifier of the privilege.
        /// </summary>
        string Identifier
        {
            get;
        }

        #endregion Properties
    }
}