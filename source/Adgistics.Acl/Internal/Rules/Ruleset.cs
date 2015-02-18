using System;

namespace Modules.Acl.Internal.Rules
{
    using System.Collections.Concurrent;
    using System.Runtime.Serialization;

    [DataContract]
    internal sealed class Ruleset
    {
        #region Constructors

        public Ruleset()
        {
            ByResourceId = new ConcurrentDictionary<ResourceId, ResourceRule>();
        }

        #endregion Constructors

        #region Properties

        [DataMember]
        public ResourceRule AllResources
        {
            get; set;
        }

        [DataMember]
        public ConcurrentDictionary<ResourceId, ResourceRule> ByResourceId
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        ///   Gets the default rule set.
        /// </summary>
        public static Ruleset GetDefaultRuleSet()
        {
            // This is equivalent to DENY EVERYTHING
            return new Ruleset()
            {
                AllResources = new ResourceRule()
                {
                    AllPrincipals = new PrincipalRule()
                    {
                        AllPrivileges = new PrivilegeRule()
                        {
                            Type = RuleType.Deny
                        }
                    }
                }
            };
        }

        /**
         * Returns the rule type associated with the specified Resource, 
         * Principal and Privilege combination.
         *
         * If a rule does not exist or its attached assertion fails, which means that
         * the rule is not applicable, then this method returns null. Otherwise, the
         * rule type applies and is returned as either TYPE_ALLOW or TYPE_DENY.
         *
         * If $resource or $principal is null, then this means that the rule must apply to
         * all Resources or Roles, respectively.
         *
         * If $privilege is null, then the rule must apply to all privileges.
         *
         * If all three parameters are null, then the default ACL rule type is returned,
         * based on whether its assertion method passes.
         *
         * @param  null|Resource\ResourceInterface  $resource
         * @param  null|Role\RoleInterface          $principal
         * @param  null|string                      $privilege
         * @return string|null
         */
        public RuleType? GetRuleResultFor(
            ResourceId resource,
            IPrincipal principal,
            Type privilege)
        {
            // get the rules for the $resource and $principal
            var rules = ResolvePrincipalRuleFor(resource, principal);

            if (null == rules)
            {
                return null;
            }

            // follow $privilege
            PrivilegeRule rule;

            if (null == privilege)
            {
                if (null != rules.AllPrivileges)
                {
                    rule = rules.AllPrivileges;
                }
                else
                {
                    return null;
                }
            }
            else if (false == rules.ByPrivilegeType.TryGetValue(privilege, out rule))
            {
                return null;
            }

            return rule.Type;
        }

        /// <summary>
        ///   Returns the rules associated with a Resource and a Principal, or null 
        ///   if no such rules exist.
        /// </summary>
        /// 
        /// <param name="resource">The resource identifier.</param>
        /// <param name="principal">The principal name.</param>
        /// <param name="create">
        ///   if set to <c>true</c> create the associated rules.
        /// </param>
        /// 
        /// <returns>
        ///   The principal rule.
        /// </returns>
        /// 
        /// <remarks>
        ///   <para>
        ///   If either $resource or $principal is null, this means that the rules 
        ///   returned are for all Resources or all Groups, respectively. 
        ///   </para>
        /// 
        ///   <para>
        ///   Both can be null to return the default rule set for all
        ///   Resources and all Groups.
        ///   </para>
        /// 
        ///   <para>
        ///   If the $create parameter is true, then a rule set is first created 
        ///   and then returned to the caller.   
        ///   </para>
        /// </remarks>
        public PrincipalRule ResolvePrincipalRuleFor(
            ResourceId resource = null,
            IPrincipal principal = null,
            bool create = false)
        {
            ResourceRule visitor;

            if (null == resource)
            {
                // use the 'All Resources' rule.
                visitor = AllResources;
            }
            else
            {
                // follow $resource
                ResourceRule resourceRules;
                if (false == ByResourceId.TryGetValue(resource,
                        out resourceRules))
                {
                    if (false == create)
                    {
                        return null;
                    }
                    resourceRules = new ResourceRule();
                    
                    ByResourceId.TryAdd(resource, resourceRules);
                }

                visitor = resourceRules;
            }

            // follow $principal
            if (null == principal)
            {
                if (null == visitor.AllPrincipals)
                {
                    if (false == create)
                    {
                        return null;
                    }
                    visitor.AllPrincipals = new PrincipalRule();
                }

                return visitor.AllPrincipals;
            }

            PrincipalRule principalRule;

            var group = principal as Group;

            if (group != null)
            {
                if (false ==
                    visitor.ByGroupName.TryGetValue(group.Name,
                        out principalRule))
                {
                    if (false == create)
                    {
                        return null;
                    }
                    principalRule = new PrincipalRule();
                    visitor.ByGroupName.TryAdd(group.Name, principalRule);
                }
            }
            else
            {
                // User
                var user = (IUser) principal;

                if (false ==
                    visitor.ByUserId.TryGetValue(user.Identifier,
                        out principalRule))
                {
                    if (false == create)
                    {
                        return null;
                    }

                    principalRule = new PrincipalRule();
                    visitor.ByUserId.TryAdd(user.Identifier, principalRule);
                }
            }
            
            return principalRule;
        }

        #endregion Methods
    }
}