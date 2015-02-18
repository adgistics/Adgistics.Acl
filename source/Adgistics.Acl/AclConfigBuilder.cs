using Modules.Acl.Internal;

namespace Modules.Acl
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Modules.Acl.Internal.Utils;

    /// <summary>
    ///   Builder for ACL system configurations (<see cref="AclConfig"/>).
    /// </summary>
    public sealed class AclConfigBuilder
    {
        #region Fields

        private string _identifier;
        private IEnumerable<IPrivilege> _privileges;
        private DirectoryInfo _storageDirectory;

        #endregion Fields

        #region Constructors

        static AclConfigBuilder()
        {
            
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///   Builds a configuration for the ACL system.
        /// </summary>
        /// 
        /// <returns>
        ///   Configuration for the ACL system.
        /// </returns>
        public AclConfig Build()
        {
            return new AclConfig(this);
        }

        /// <summary>
        ///   Sets the desired identifier for the access control instance.
        /// </summary>
        /// 
        /// <param name="value">The value.</param>
        /// 
        /// <returns>
        ///   This instance to provide a fluent interface.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'value' must not be blank, whitespace only, or empty.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'identifier' must contain alphanumeric
        ///   characters only. No spaces, hyphens or other special 
        ///   characters are allowed.
        ///   </para>
        /// </exception>
        public AclConfigBuilder SetIdentifier(string value)
        {
            _identifier = AccessControlIdentifier.Clean(value);

            return this;
        }

        

        /// <summary>
        ///   Sets the privileges that the ACL system should use.
        /// </summary>
        /// 
        /// <param name="value">The privileges.</param>
        /// 
        /// <returns>
        ///   This <see cref="AclConfig"/> instance in order to 
        ///   provide a fluent interface.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'value' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'value' contains no privileges.
        ///   </para>
        /// </exception>
        public AclConfigBuilder SetPrivileges(IEnumerable<IPrivilege> value)
        {
            if (null == value)
            {
                throw new ArgumentException("Argument 'value' must not be null.");
            }

            // remove any null items from the list
            value = value.Where(x => x != null).ToList();

            if (false == value.Any())
            {
                throw new ArgumentException(
                    "Argument 'value' contains no privileges.");
            }

            _privileges = value;

            return this;
        }

        /// <summary>
        ///   Sets the storage directory.
        /// </summary>
        /// 
        /// <param name="value">The value.</param>
        /// 
        /// <returns>
        ///   This instance to provide a fluent interface.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'value' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'value' refers to a directory that does not exist, or
        ///   is not a directory, or is not writable.
        ///   </para>
        /// </exception>
        /// 
        /// <remarks>
        ///   The storage directory is a directory in which things like any 
        ///   desired configuration, caching, persistence can be stored for
        ///   an <see cref="AccessControl"/> instance.
        /// </remarks>
        public AclConfigBuilder SetStorageDirectory(DirectoryInfo value)
        {
            if (null == value)
            {
                throw new ArgumentException("Argument 'value' must not be null.");
            }

            FilePreconditions.Check(
                value,
                FileConstraints.EXISTING,
                FileConstraints.IS_A_DIRECTORY,
                FileConstraints.WRITABLE);

            _storageDirectory = value;

            return this;
        }

        /// <summary>
        ///   Gets the identifier of this <see cref="AccessControl"/>
        ///   instance.
        /// </summary>
        internal string GetIdentifier()
        {
            if (string.IsNullOrWhiteSpace(_identifier))
            {
                return
                    Guid.NewGuid()
                        .ToString()
                        .Replace("-", "")
                        .ToLowerInvariant();
            }

            return _identifier;
        }

        internal IEnumerable<IPrivilege> GetPrivileges()
        {
            return _privileges;
        }

        internal DirectoryInfo GetStorageDirectory()
        {
            if (_storageDirectory == null)
            {
                return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            }

            return _storageDirectory;
        }

        #endregion Methods
    }
}