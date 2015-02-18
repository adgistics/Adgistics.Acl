namespace Modules.Acl
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    ///   Registry which discovers and maintains a reference to all the 
    ///   available <see cref="IPrivilege"/>s within the application
    ///   context.
    /// </summary>
    public sealed class PrivilegeRegistry
    {
        #region Fields

        private readonly Dictionary<Type, IPrivilege> _privileges;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="PrivilegeRegistry"/> 
        ///   class.
        /// </summary>
        internal PrivilegeRegistry()
        {
            _privileges = new Dictionary<Type, IPrivilege>();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///   Gets the privilege instance based on the given type.
        /// </summary>
        /// 
        /// <param name="privilegeType">Type of the privilege.</param>
        /// 
        /// <returns>
        ///   The <see cref="IPrivilege"/> instance if it was found, else
        ///   <c>null</c>.
        /// </returns>
        /// 
        /// <remarks>
        ///    The type should be an implementator of <see cref="IPrivilege"/>.
        /// </remarks>
        public IPrivilege Get(Type privilegeType)
        {
            if (false == _privileges.ContainsKey(privilegeType))
            {
                return null;
            }

            return _privileges[privilegeType];
        }

        /// <summary>
        ///   Gets all the registered privilege instances.
        /// </summary>
        /// 
        /// <returns>
        ///   The registered privilege instances.
        /// </returns>
        public IEnumerable<IPrivilege> GetRegistered()
        {
            return new List<IPrivilege>(_privileges.Values);
        }

        /// <summary>
        ///   Determines whether the specified privilege is registered.
        /// </summary>
        /// 
        /// <param name="privilege">The privilege.</param>
        /// 
        /// <returns>
        ///   <c>true</c> if, and only if, the given privilege type has been
        ///   registered.
        /// </returns>
        public bool IsRegistered(Type privilege)
        {
            return _privileges.ContainsKey(privilege);
        }

        /// <summary>
        ///   Registers the specified privilege for usage.
        /// </summary>
        /// 
        /// <param name="privilege">The privilege.</param>
        public void Register(IPrivilege privilege)
        {
            if (privilege == null  || privilege is AllPrivileges)
            {
                return;
            }

            var type = privilege.GetType();

            // TODO: Thread safety
            if (false == _privileges.ContainsKey(type))
            {
                _privileges.Add(type, privilege);
            }
        }

        /// <summary>
        ///   Discovers the <see cref="IPrivilege"/> implementations available 
        ///   within the current application domain assemblies to be used
        ///   within the ACL system.
        /// </summary>
        /// 
        /// <exception cref="System.TypeLoadException">
        ///   If any failure occurs to initiliaze a discovered 
        ///   <see cref="IPrivilege"/> type.
        /// </exception>
        internal void DiscoverPrivileges()
        {
            var dlls = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var dll in dlls)
            {
                // Start Parsing Types.
                foreach (var type in dll.GetTypes())
                {
                    if (type.IsClass &&
                        type.GetInterfaces().Contains(typeof (IPrivilege)))
                    {
                        try
                        {
                            if (type == typeof (AllPrivileges))
                            {
                                // This is a special system privilege
                                continue;
                            }

                            if (false == _privileges.ContainsKey(type))
                            {
                                var privilege =
                                (IPrivilege)Activator.CreateInstance(type);

                                _privileges.Add(type, privilege);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new TypeLoadException(
                                string.Format(
                                    "ACL system failed to initialize an " +
                                    "IPrivilege implementation.  Does it have a " +
                                    "public constructor?  Recieved type: {0}",
                                    type.FullName), ex);
                        }
                    }
                }
            }
        }

        #endregion Methods
    }
}