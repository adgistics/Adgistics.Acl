using System;

namespace Modules.Acl.Internal.Rules
{
    using System.Collections.Concurrent;
    using System.Runtime.Serialization;

    [DataContract]
    internal sealed class ResourceRule
    {
        #region Constructors

        public ResourceRule()
        {
            ByGroupName = new ConcurrentDictionary<string, PrincipalRule>();
            ByUserId = new ConcurrentDictionary<Guid, PrincipalRule>();
        }

        #endregion Constructors

        #region Properties

        [DataMember]
        public PrincipalRule AllPrincipals
        {
            get; set;
        }

        [DataMember]
        public ConcurrentDictionary<string, PrincipalRule> ByGroupName
        {
            get; private set;
        }

        [DataMember]
        public ConcurrentDictionary<Guid, PrincipalRule> ByUserId
        {
            get; private set;
        } 

        #endregion Properties
    }
}