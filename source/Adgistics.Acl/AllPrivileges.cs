namespace Modules.Acl
{
    /// <summary>
    ///   Used to represent the assigning of rules for/against all the 
    ///   privileges available 
    /// </summary>
    public sealed class AllPrivileges : IPrivilege
    {
        #region Fields

        private const string _description = 
            "This is a special system type that is used to represent all " +
            "privileges available in the system.";
        
        private const string _identifier = "All Privileges";

        #endregion Fields

        #region Properties

        /// <summary>
        ///   Gets the description of the privilege.
        /// </summary>
        public string Description
        {
            get { return _description; }
        }

        /// <summary>
        ///   Gets the friendly title of the privilege.
        /// </summary>
        public string Identifier
        {
            get { return _identifier; }
        }

        #endregion Properties
    }
}