namespace Modules.Acl
{
    using System.Collections.Generic;
    using System.IO;

    using Modules.Acl.Internal.Utils;

    /// <summary>
    ///   Configuration for the ACL system.
    /// </summary>
    public sealed class AclConfig
    {
        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="AclConfig"/> 
        ///   class.
        /// </summary>
        internal AclConfig(
            AclConfigBuilder builder)
        {
            Privileges = builder.GetPrivileges();

            Identifier = builder.GetIdentifier();

            RootDirectory = FileUtils.Concatenate(
                builder.GetStorageDirectory(), Identifier);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///   Gets the identifier.
        /// </summary>
        public string Identifier
        {
            get; private set;
        }

        /// <summary>
        ///   Gets the privileges that the ACL system manages.
        /// </summary>
        public IEnumerable<IPrivilege> Privileges
        {
            get; private set;
        }

        /// <summary>
        ///   Gets the root directory location for this <see cref="AccessControl"/>
        ///   configuration.  
        /// </summary>
        /// 
        /// <remarks>
        ///   This is where things like caching and persistence for this 
        ///   <see cref="AccessControl"/> instance can be stored.
        /// </remarks>
        public DirectoryInfo RootDirectory
        {
            get; private set;
        }

        #endregion Properties
    }
}