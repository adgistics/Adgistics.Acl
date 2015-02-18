using System;

namespace Modules.Acl.Internal
{
    internal sealed class RuleAssertResult
    {
        public Guid PrincipalId { get; private set; }

        public Guid ResourceId { get; private set; }

        public string PrivilegeId { get; private set; }

        public bool IsAllowed { get; private set; }
    }
}