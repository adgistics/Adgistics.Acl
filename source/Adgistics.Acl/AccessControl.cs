namespace Modules.Acl
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;

    using Internal;

    /// <summary>
    ///   An access control configuration.
    /// </summary>
    /// 
    /// <remarks>
    ///   This is the main API wrapper for ACLs.
    /// </remarks>
    public sealed class AccessControl
    {
        #region Fields

        private static readonly object InstanceLocker;
        private static readonly ConcurrentDictionary<string, AccessControl> Instances;

        /// <summary>
        /// The _configuration
        /// </summary>
        private readonly AclConfig _config;

        /// <summary>
        /// The _groups api
        /// </summary>
        private readonly Groups _groups;

        /// <summary>
        /// The _resources api
        /// </summary>
        private readonly Resources _resources;

        /// <summary>
        /// The _rules api
        /// </summary>
        private readonly Rules _rules;

        /// <summary>
        /// The _events api
        /// </summary>
        private AclEvents _events;

        /// <summary>
        ///   Holds a reference to the privileges which have been registered
        ///   to be used by the <see cref="Rules"/>.
        /// </summary>
        private PrivilegeRegistry _privileges;

        /// <summary>
        ///   The _repositories
        /// </summary>
        private Repositories _repositories;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Initializes the <see cref="AccessControl"/> class.
        /// </summary>
        static AccessControl()
        {
            Instances = new ConcurrentDictionary<string, AccessControl>();
            InstanceLocker = new object();
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="AccessControl" />
        ///   class using the default configuration options.
        /// </summary>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'identifier' must not be blank, whitespace only, or empty.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'identifier' must contain alphanumeric characters only. 
        ///   No spaces, hyphens or other special characters are allowed.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'config' must not be null.
        ///   </para>
        /// </exception>
        internal AccessControl()
            : this(new AclConfig(new AclConfigBuilder()))
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="AccessControl" />
        ///   class with the given configuration.
        /// </summary>
        /// 
        /// <param name="config">The configuration.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'identifier' must not be blank, whitespace only, or empty.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'identifier' must contain alphanumeric characters only. 
        ///   No spaces, hyphens or other special characters are allowed.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'config' must not be null.
        ///   </para>
        /// </exception>
        internal AccessControl(AclConfig config)
        {
            if (config == null)
            {
                throw new ArgumentException(
                    "Argument 'config' must not be null.");
            }

            _config = config;

            // Initialize storage directory
            if (false == _config.RootDirectory.Exists)
            {
                _config.RootDirectory.Create();
            }
            else
            {
                Console.WriteLine("Exists: {0}", _config.RootDirectory.FullName);
            }

            _rules = new Rules(this);

            _groups = new Groups(this);

            _resources = new Resources(this);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///   Gets the configuration for the ACL system.
        /// </summary>
        public AclConfig Config
        {
            get { return _config; }
        }

        /// <summary>
        ///   Gets the events API.
        /// </summary>
        public AclEvents Events
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref _events, () =>
                {
                    return new AclEvents();
                });

                return _events;
            }
        }

        /// <summary>
        ///   Gets the groups API.
        /// </summary>
        public Groups Groups
        {
            get { return _groups; }
        }

        /// <summary>
        ///   Gets the privileges API.
        /// </summary>
        public PrivilegeRegistry Privileges
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref _privileges, () =>
                {
                    var result = new PrivilegeRegistry();

                    if (Config.Privileges != null && Config.Privileges.Any())
                    {
                        foreach (var privilege in Config.Privileges)
                        {
                            result.Register(privilege);
                        }
                    }

                    if (false == result.GetRegistered().Any())
                    {
                        result.DiscoverPrivileges();
                    }

                    return result;
                });

                return _privileges;
            }
        }

        /// <summary>
        ///   Gets the resources API.
        /// </summary>
        public Resources Resources
        {
            get { return _resources; }
        }

        /// <summary>
        ///   Gets the rules API.
        /// </summary>
        public Rules Rules
        {
            get { return _rules; }
        }

        // TODO: Not sure if this should be seperated away from the actual domains
        internal Repositories Repositories
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref _repositories, () =>
                {
                    var result = new Repositories(this);

                    return result;
                });

                return _repositories;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        ///   Gets the access control instance for the given identifier.
        /// </summary>
        /// 
        /// <param name="identifier">The identifier.</param>
        /// 
        /// <returns>
        ///   The <see cref="AccessControl"/> for the given identifier if it
        ///   exists, else <c>null</c>.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   Argument 'identifier' must not be null, whitespace only, or empty.
        /// </exception>
        public static AccessControl Get(string identifier)
        {
            identifier = AccessControlIdentifier.Clean(identifier);

            AccessControl result;

            Instances.TryGetValue(identifier, out result);

            return result;
        }

        /// <summary>
        ///   Initializes the specified <see cref="AccessControl"/> 
        ///   configuration.
        /// </summary>
        /// 
        /// <param name="config">The configuration.</param>
        /// 
        /// <returns>
        ///   The intialized <see cref="AccessControl"/> instance.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'config' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   An access control instance has already been intialize with the 
        ///   given identifier.
        ///   </para>
        /// </exception>
        /// 
        /// <remarks>
        ///   This should only be called once, typically on application start
        ///   up for each Access Control configuration that you wish to
        ///   have available for the application.
        /// </remarks>
        public static AccessControl Initialize(AclConfig config)
        {
            if (config == null)
            {
                throw new ArgumentException("Argument 'config' must not be null.");
            }

            AccessControl result;

            lock (InstanceLocker)
            {
                if (Instances.ContainsKey(config.Identifier))
                {
                    throw new ArgumentException(
                        string.Format(
                            "An access control instance has already been " +
                            "initialized with the given identifier: {0}",
                            config.Identifier));
                }

                result = new AccessControl(config);

                Instances.TryAdd(config.Identifier, result);
            }

            return result;
        }

        /// <summary>
        ///   Initializes the specified <see cref="AccessControl"/> 
        ///   using the default configuration.
        /// </summary>
        /// 
        /// <returns>
        ///   The intialized <see cref="AccessControl"/> instance.
        /// </returns>
        /// 
        /// <remarks>
        ///   <para>
        ///   This should only be called once, typically on application start
        ///   up for each Access Control configuration that you wish to
        ///   have available for the application.
        ///   </para>
        /// 
        ///   <para>
        ///   This method will create a configuration with a random identifier.
        ///   So if you wish to persist across sessions you should use the 
        ///   overloaded method to provide your own identifier.
        ///   </para>
        /// </remarks>
        public static AccessControl Initialize()
        {
            return Initialize(new AclConfigBuilder().Build());
        }

        #endregion Methods
    }
}