namespace Modules.Acl.Internal.Rules
{
    using System.Runtime.Serialization;

    [DataContract]
    internal sealed class PrivilegeRule
    {
        #region Properties

        [DataMember]
        public RuleType Type
        {
            get; set;
        }

        #endregion Properties
    }
}