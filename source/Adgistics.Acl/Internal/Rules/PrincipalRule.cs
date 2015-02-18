namespace Modules.Acl.Internal.Rules
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Modules.Acl.Exceptions;

    [DataContract]
    internal sealed class PrincipalRule
    {
        #region Constructors

        public PrincipalRule()
        {
            ByPrivilegeType = new ConcurrentDictionary<Type, PrivilegeRule>();
        }

        #endregion Constructors

        #region Properties

        [DataMember]
        public PrivilegeRule AllPrivileges
        {
            get; set;
        }

        public ConcurrentDictionary<Type, PrivilegeRule> ByPrivilegeType
        {
            get; private set;
        }

        [DataMember]
        private Dictionary<string, PrivilegeRule> ByPrivilegeTypeName
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            ByPrivilegeType = new ConcurrentDictionary<Type, PrivilegeRule>();

            foreach (var pair in ByPrivilegeTypeName)
            {
                Type type;

                try
                {
                    type = Type.GetType(pair.Key);
                }
                catch (Exception ex)
                {
                    // Hmmmm, okay so possibly a type was removed from the
                    // system, or it's namespace/assembly changed.
                    // TODO: Implement a logging system otherwise this type of error will be a pain.

                    throw new AclUnexpectedStateException(
                        string.Format(
                            "Ruleset refers to an IPrivilege type which could " +
                            "not be found in the current application domain: {0}",
                            pair.Key), ex);
                }

                if (type == null)
                {
                    throw new AclUnexpectedStateException(
                        string.Format(
                            "Ruleset refers to an IPrivilege type which could " +
                            "not be found in the current application domain: {0}",
                            pair.Key));
                }

                ByPrivilegeType.TryAdd(type, pair.Value);
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            ByPrivilegeTypeName = new Dictionary<string, PrivilegeRule>();

            foreach (var pair in ByPrivilegeType)
            {
                ByPrivilegeTypeName.Add(
                    pair.Key.AssemblyQualifiedName,
                    pair.Value);
            }
        }

        #endregion Methods
    }
}