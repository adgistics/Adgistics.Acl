#region Header

// Notes:SM:2015.01.12
// This class was ported from a PHP framework (Zend).  The PHP framework
// had a good logical solution, however it's implementation was very much
// a God like class which did absolutely everything.
// It has been taking some pain but I have been refactoring it out to make
// it a bit more object orientated.
// The next bit of refactoring that should be done is:
// - Move all rule set actions into respective methods within the Rules classes.
// - 
// TODO: Update documentation to reflect concept of IPrincipal rather than Group
// TODO: Change Group identifier to be based on a Guid

#endregion Header

using Modules.Acl.Internal;

namespace Modules.Acl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Exceptions;

    using Internal.Rules;

    using Events;

    /// <summary>
    ///   Access control rules.
    /// </summary>
    /// 
    /// <remarks>
    ///   <para>
    ///   Provides a lightweight and flexible access control list (ACL) 
    ///   implementation for privileges management. In general, an application 
    ///   may utilize such ACL‘s to control access to certain protected objects 
    ///   by other requesting objects.
    ///   </para>
    ///   
    ///   <para>
    ///   For the purposes of this documentation:
    ///   <list type="bullet">
    ///    <item>
    ///       <description>
    ///         a <see cref="IResource"/> is an object to which access is 
    ///         controlled.
    ///       </description>
    ///    </item>
    ///    <item>
    ///       <description>
    ///         a <see cref="Group"/> is an object that may request access to 
    ///         a <see cref="IResource"/>.
    ///       </description>
    ///    </item>
    ///   </list>
    ///   </para>
    /// 
    ///   <para>
    ///   Put simply, <see cref="Group"/>s request access to 
    ///   <see cref="IResource"/>s. For example, if a parking attendant requests 
    ///   access to a car, then the parking attendant is the requesting 
    ///   <see cref="Group"/>, and the car is the <see cref="IResource"/>, since 
    ///   access to the car may not be granted to everyone.
    ///   </para>
    /// 
    ///   <para>
    ///   Through the specification and use of an ACL, an application may 
    ///   control how <see cref="Group"/>s are granted access to 
    ///   <see cref="IResource"/>s.
    ///   </para>
    /// </remarks>
    public sealed class Rules
    {
        #region Fields

        /// <summary>
        ///    A null principal, commonly used to represent 'Global'
        /// </summary>
        private static readonly Group NullGroup;

        /// <summary>
        ///    A null resource, commonly used to represent 'Global'
        /// </summary>
        private static readonly ResourceId NullResourceId;

        private readonly AccessControl _api;

        /// <summary>
        ///   The acl rules for all principals, resources, and privileges.
        /// </summary>
        private readonly Ruleset _ruleset;

        private readonly RulesetRepository _rulesetRepository;

        private readonly RuleAssertCache _ruleAssertCache;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Initializes the <see cref="Rules"/> class.
        /// </summary>
        static Rules()
        {
            NullResourceId = new ResourceId("NULL", "NULL");
            NullGroup = new Group(new AccessControl(), "NULL_GROUP");
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="Rules" />
        ///   class.
        /// </summary>
        /// <param name="api">The API.</param>
        internal Rules(AccessControl api)
        {
            _api = api;
            _rulesetRepository = api.Repositories.RulesetRepository;
            _ruleset = _rulesetRepository.Load();
            _ruleAssertCache = new RuleAssertCache(api);

            // Add event binders
            api.Events.GroupDeleted += OnGroupDeleted;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///   Adds an ACL rule which grants access.
        /// </summary>
        /// 
        /// <typeparam name="TPrivilege">
        ///   The privilege the the rule is applicable to.
        ///   If it is of type <see cref="AllPrivileges"/> then the rule
        ///   will be applicable for all privileges.
        /// </typeparam>
        /// <param name="principal">
        ///   The principal that the rule is applicable to. 
        ///   If null then it's considered to be applicable for all principals.
        /// </param>
        /// <param name="resource">
        ///   The resources that the rule is applicable to.  
        ///   If null then it's considered to be applicable for all resources.
        /// </param>
        /// 
        /// <returns>
        ///   This instance to support a fluent interface.
        /// </returns>
        /// 
        /// <exception cref="PrivilegeNotRegisteredException">
        ///   If the given privilege type is not registered for usage.
        /// </exception>
        /// <exception cref="AclUnexpectedStateException">
        ///   An unkown ACL operation type is executed.
        /// </exception>
        /// 
        /// <remarks>
        ///   <para>
        ///   A rule is added that would allow one or more Groups access to 
        ///   certain Privileges upon the specified Resource(s).
        ///   </para>
        /// 
        ///   <para>
        ///   The $groups and $resources are to indicate the Resources and Groups 
        ///   to which the rule applies. If either $groups or $resources is null, 
        ///   then the rule applies to all Groups or all Resources, respectively.
        ///   Both may be null in order to work with the default rule of the ACL.
        ///   </para>
        /// 
        ///   <para>
        ///   The $privileges parameter may be used to further specify that the 
        ///   rule applies only to certain privileges upon the Resource(s) in 
        ///   question. 
        ///   </para>
        /// 
        ///   <para>
        ///   If $assert is provided, then its Assert(...) method must return 
        ///   <c>true</c> in order for the rule to apply.
        ///   </para> 
        /// 
        ///   <para>
        ///   If $assert is provided with $groups, $resources, and $privileges 
        ///   all equal to null, then the result will imply DENY when the 
        ///   rule's assertion fails. This is because the ACL needs to 
        ///   provide expected behavior when an assertion upon the default ACL 
        ///   rule fails.
        ///   </para>
        /// </remarks>
        public Rules Allow<TPrivilege>(
            IPrincipal principal = null,
            IResource resource = null)
            where TPrivilege : IPrivilege
        {
            return SetRule<TPrivilege>(
                Operation.Add,
                RuleType.Allow,
                principal,
                resource);
        }

        /// <summary>
        ///   Adds an ACL rule which denies access.
        /// </summary>
        /// 
        /// <typeparam name="TPrivilege">
        ///   The privilege the the rule is applicable to.
        ///   If it is of type <see cref="AllPrivileges"/> then the rule
        ///   will be applicable for all privileges.
        /// </typeparam>
        /// <param name="group">
        ///   The principal that the rule is applicable to. 
        ///   If null then it's considered to be applicable for all groups.
        /// </param>
        /// <param name="resource">
        ///   The resource that the rule is applicable to.  
        ///   If null then it's considered to be applicable for all resources.
        /// </param>
        /// 
        /// <returns>
        ///   This instance to support a fluent interface.
        /// </returns>
        /// 
        /// <exception cref="PrivilegeNotRegisteredException">
        ///   If the given privilege type is not registered for usage.
        /// </exception>
        /// <exception cref="AclUnexpectedStateException">
        ///   An unkown ACL operation type is executed.
        /// </exception>
        /// 
        /// <remarks>
        ///   <para>
        ///   A rule is added that would deny one or more Groups access to 
        ///   certain Privileges upon the specified Resource(s).
        ///   </para>
        /// 
        ///   <para>
        ///   The $groups and $resources are to indicate the Resources and Groups 
        ///   to which the rule applies. If either $groups or $resources is null, 
        ///   then the rule applies to all Groups or all Resources, respectively.
        ///   Both may be null in order to work with the default rule of the ACL.
        ///   </para>
        /// 
        ///   <para>
        ///   The $privileges parameter may be used to further specify that the 
        ///   rule applies only to certain privileges upon the Resource(s) in 
        ///   question. 
        ///   </para>
        /// 
        ///   <para>
        ///   If $assert is provided, then its Assert(...) method must return 
        ///   <c>true</c> in order for the rule to apply.
        ///   </para> 
        /// 
        ///   <para>
        ///   If constraint is provided with $groups, $resources, and $privileges 
        ///   all equal to null, then the result will imply ALLOW when the 
        ///   rule's assertion fails. This is because the ACL needs to 
        ///   provide expected behavior when an assertion upon the default ACL 
        ///   rule fails.
        ///   </para>
        /// </remarks>
        public Rules Deny<TPrivilege>(
            Group group = null,
            IResource resource = null)
            where TPrivilege : IPrivilege
        {
            return SetRule<TPrivilege>(
                Operation.Add,
                RuleType.Deny,
                group,
                resource);
        }

        /// <summary>
        ///   Gets the privileges that the given principal has for the given
        ///   resource.
        /// </summary>
        /// 
        /// <param name="group">The principal.</param>
        /// <param name="resource">The resource.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument: 'principal' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument: 'resource' must not be null.
        ///   </para>
        /// </exception>
        /// 
        /// <returns>
        ///   The privileges that the principal has against the given resource.
        /// </returns>
        public IEnumerable<IPrivilege> GetPrivileges(
            Group group,
            IResource resource)
        {
            if (group == null)
            {
                throw new ArgumentException(
                    "Argument: 'principal' must not be null.");
            }

            if (resource == null)
            {
                throw new ArgumentException(
                    "Argument: 'resource' must not be null.");
            }

            IList<IPrivilege> result = new List<IPrivilege>();

            // get the rules for the $resource and $principal
            foreach (var privilege in _api.Privileges.GetRegistered())
            {
                if (IsAllowed(group, resource, privilege.GetType()))
                {
                    result.Add(privilege);
                }
            }

            return result;
        }

        /// <summary>
        ///   Determines whether access should be granted.
        /// </summary>
        /// 
        /// <typeparam name="TPrivilege">
        ///   The type of the privilege to check against.
        ///   If it is of type <see cref="AllPrivileges"/> then the principal
        ///   will need to have access to all privileges for a resource in order
        ///   to be given access.
        /// </typeparam>
        /// <param name="principal">The principal.</param>
        /// <param name="resource">The resource.</param>
        /// 
        /// <returns>
        ///   Returns <c>true</c> if and only if access should be granted, else 
        ///   <c>false</c>.
        /// </returns>
        /// 
        /// <exception cref="PrivilegeNotRegisteredException">
        ///   If the given privilege type is not registered for usage.
        /// </exception>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'principal' has not been registered in the ACL system.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'resource' has not been registered in the ACL system.
        ///   </para>
        /// </exception>
        /// 
        /// <exception cref="AclUnexpectedStateException">
        ///   If the ACL data got into an unexpected state and caused an 
        ///   error that could not be resolved.
        /// </exception>
        /// 
        /// <remarks>
        ///   <para>
        ///   If either $principal or $resource is null, then the query applies to 
        ///   all Groups or all Resources, respectively. 
        ///   </para>
        /// 
        ///   <para>
        ///   If both $principal and $resource are null then the query will check
        ///   whether the ACL has a "blacklist" rule (allow everything to all).
        ///   By default, <see cref="Rules"/> creates a "whitelist" 
        ///   rule (deny everything to all), and this method would return false 
        ///   unless this default has been overridden (i.e. by executing 
        ///   <see cref="Allow{TPrivilege}"/> with no Group or Resource specified).
        ///   </para>
        /// 
        ///   <para>
        ///   If a $permission is not provided then the Role will need to have
        ///   been granted access to all available Privileges against the 
        ///   Resource otherwise <c>false</c> will be returned.
        ///   </para>
        /// </remarks>
        public bool IsAllowed<TPrivilege>(
            IPrincipal principal = null,
            IResource resource = null)
            where TPrivilege : IPrivilege
        {
            if (principal is Group
                && false == _api.Groups.Exists(((Group)principal).Name))
            {
                throw new ArgumentException(
                    "Argument 'principal' refers to a group that has not been " +
                    "registered in the ACL system.");
            }
            if (resource != null && false == _api.Resources.IsRegistered(resource))
            {
                throw new ArgumentException(
                    "Argument 'resource' has not been registered in the ACL system.");
            }

            var privilege = ParsePrivilegeType<TPrivilege>();

            // TODO: Consult result cache first

            if (IsAllowed(principal, resource, privilege))
            {
                return true;
            }

            if (principal is IUser)
            {
                // if this is a User principal, then we can check if any 
                // of their groups grant them access.

                // TODO: This needs to take the Group hierarchy into acccount!
                foreach (var groupName in ((IUser)principal).Groups)
                {
                    var group = _api.Groups.Get(groupName);

                    if (group != null)
                    {
                        if (IsAllowed(group, resource, privilege))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public IEnumerable<ResourceId> GetAllowed<TPrivilege>(
            string type,
            Group @group = null,
            IResource resource = null)
            where TPrivilege : IPrivilege
        {
            var result = new List<ResourceId>();
            
            var resources = _api.Resources.GetIdsByType(type);

            foreach (var resourceId in resources)
            {
                if (IsAllowed<TPrivilege>(group, resource))
                {
                    result.Add(resourceId);
                }
            }

            return result;
        }

        /// <summary>
        ///   Removes "allow" access from the ACL on the following items.
        /// </summary>
        /// 
        /// <typeparam name="TPrivilege">
        ///   The privilege the the rule is applicable to.
        /// </typeparam>
        /// <param name="group">The principal.</param>
        /// <param name="resource">The resource.</param>
        /// 
        /// <exception cref="AclUnexpectedStateException">
        ///   An unkown ACL operation type is executed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   If this instance has been disposed.
        /// </exception>
        /// 
        /// <returns>
        ///   This instance to support a fluent interface.
        /// </returns>
        public Rules RemoveAllow<TPrivilege>(
            Group group = null,
            IResource resource = null)
            where TPrivilege : IPrivilege
        {
            return SetRule<TPrivilege>(
                Operation.Remove,
                RuleType.Allow,
                group,
                resource);
        }

        /// <summary>
        ///   Removes "deny" restrictions from the ACL on the following items.
        /// </summary>
        /// 
        /// <typeparam name="TPrivilege">
        ///   The privilege the the rule is applicable to.
        /// </typeparam>
        /// <param name="group">The principal.</param>
        /// <param name="resource">The resource.</param>
        /// 
        /// <exception cref="PrivilegeNotRegisteredException">
        ///   If the given privilege type is not registered for usage.
        /// </exception>
        /// <exception cref="AclUnexpectedStateException">
        ///   An unkown ACL operation type is executed.
        /// </exception>
        /// 
        /// <returns>
        ///   This instance to support a fluent interface.
        /// </returns>
        public Rules RemoveDeny<TPrivilege>(
            Group group = null,
            IResource resource = null)
            where TPrivilege : IPrivilege
        {
            return SetRule<TPrivilege>(
                Operation.Remove,
                RuleType.Deny,
                group,
                resource);
        }

        /// <summary>
        ///   Clears all the rules relating to the given <see cref="Group"/>.
        /// </summary>
        /// 
        /// <param name="group">The principal.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   Argument 'principal' must not be null.
        /// </exception>
        /// 
        /// <returns>
        ///   This instance to provide a fluent interface.
        /// </returns>
        public Rules RemoveRulesFor(Group @group)
        {
            if (@group == null)
            {
                throw new ArgumentException(
                    "Argument 'principal' must not be null.");
            }

            PrincipalRule foo; // helps with the TryRemout out parameters.

            //
            // Step 1
            var toRemove = new List<string>();

            foreach (var groupNameCurrent in _ruleset.AllResources.ByGroupName.Keys)
            {
                if (Group.NamesEqual(@group.Name, groupNameCurrent))
                {
                    toRemove.Add(groupNameCurrent);
                }
            }

            foreach (var groupName in toRemove)
            {
                _ruleset.AllResources.ByGroupName.TryRemove(groupName, out foo);
            }

            //
            // Step 2
            foreach (var resourceIdCurrent in _ruleset.ByResourceId.Keys)
            {
                ResourceRule resourceRule;
                if (false == _ruleset.ByResourceId.TryGetValue(
                                resourceIdCurrent, out resourceRule))
                {
                    continue;
                }

                toRemove = new List<string>();
                foreach (var groupNameCurrent in resourceRule.ByGroupName.Keys)
                {
                    if (Group.NamesEqual(@group.Name, groupNameCurrent))
                    {
                        toRemove.Add(groupNameCurrent);
                    }
                }

                foreach (var groupName in toRemove)
                {
                    resourceRule.ByGroupName.TryRemove(groupName, out foo);
                }
            }

            return this;
        }

        /// <summary>
        ///   Removes a Resource registration and all of its children.
        /// </summary>
        /// 
        /// <returns>
        ///   This instance to support a fluent interface.
        /// </returns>
        // TODO: Need to make this respond to an event on resources
        public Rules RemoveRulesFor(ResourceId resource)
        {
            if (resource == null)
            {
                throw new ArgumentException("Argument 'resource' cannot be null.");
            }

            ResourceRule rfoo;
            _ruleset.ByResourceId.TryRemove(resource, out rfoo);

            return this;
        }

        private bool IsAllowed(
            IPrincipal principal,
            IResource resource,
            Type privilege)
        {
            ResourceId resourceId = null;
            if (null != resource)
            {
                resourceId = resource.ResourceId;
            }

            if (null == privilege)
            {
                // query on all privileges
                do
                {
                    if (principal is Group)
                    {
                        // depth-first search on Group
                        bool? result = RoleDfsAllPrivileges(
                            (Group)principal, resourceId);

                        if (result != null)
                        {
                            return result.Value;
                        }
                    }
                    else if (principal is IUser)
                    {
                        bool? result = DoesPrincipalHaveAccessTo(
                            principal,
                            resourceId);

                        if (result != null)
                        {
                            return result.Value;
                        }
                    }

                    // look for rule on 'allPrincipals'
                    PrincipalRule rules = _ruleset.ResolvePrincipalRuleFor(resourceId);
                    if (null != rules)
                    {
                        foreach (var priv in rules.ByPrivilegeType.Keys)
                        {
                            RuleType? ruleTypeOnePrivilege = _ruleset.GetRuleResultFor(
                                resourceId,
                                null,
                                priv);

                            if (RuleType.Deny == ruleTypeOnePrivilege)
                            {
                                return false;
                            }
                        }

                        RuleType? ruleTypeAllPrivileges = _ruleset.GetRuleResultFor(
                            resourceId,
                            null,
                            null);

                        if (null != ruleTypeAllPrivileges)
                        {
                            return RuleType.Allow == ruleTypeAllPrivileges;
                        }
                    }

                    if (resourceId == null)
                    {
                        throw new AclUnexpectedStateException(
                            "Unexpected state in ACL.  Possible data corruption.");
                    }

                    // try next Resource
                    resourceId = _api.Resources.GetParentResource(resourceId);
                } while (true); // loop terminates at 'allResources' pseudo-parent
            }
            else
            {
                // query on one privilege
                do
                {
                    if (principal is Group)
                    {
                        // depth-first search on Group
                        bool? result = RoleDfsOnePrivilege(
                            (Group)principal, resourceId, privilege);

                        if (result != null)
                        {
                            return result.Value;
                        }
                    }
                    else if (principal is IUser)
                    {
                        // look for rule for user
                        bool? result = DoesPrincipalHaveAccessTo(
                            principal, resourceId, privilege);

                        if (result != null)
                        {
                            return result.Value;
                        }
                    }

                    // look for rule on 'allPrincipals' pseudo-parent
                    RuleType? ruleType = _ruleset.GetRuleResultFor(
                        resourceId,
                        null,
                        privilege);

                    if (ruleType != null)
                    {
                        return RuleType.Allow == ruleType;
                    }

                    RuleType? ruleTypeAllPrivileges = _ruleset.GetRuleResultFor(
                        resourceId,
                        null,
                        null);

                    if (ruleTypeAllPrivileges != null)
                    {
                        bool result = RuleType.Allow == ruleTypeAllPrivileges;
                        if (result || null == resourceId)
                        {
                            return result;
                        }
                    }

                    // try next Resource
                    resourceId = _api.Resources.GetParentResource(resourceId);
                } while (true); // loop terminates at 'allResources' pseudo-parent
            }
        }

        /// <summary>
        ///   Called when a principal is deleted.
        /// </summary>
        /// 
        /// <param name="sender">The sender.</param>
        /// 
        /// <param name="e">
        ///    The <see cref="GroupDeletedEventArgs"/> instance containing the 
        ///    event data.
        /// </param>
        private void OnGroupDeleted(object sender, GroupDeletedEventArgs e)
        {
            RemoveRulesFor(e.Group);
        }

        private Type ParsePrivilegeType<TPrivilege>()
            where TPrivilege : IPrivilege
        {
            Type privilege = typeof (TPrivilege);

            if (privilege == typeof (AllPrivileges))
            {
                privilege = null;
            }
            else if (false == _api.Privileges.IsRegistered(privilege))
            {
                throw new PrivilegeNotRegisteredException(
                    string.Format(
                        "Argument 'TPrivilege' has not been registered in the ACL system: {0}",
                        privilege.FullName));
            }
            return privilege;
        }

        /**
         * Performs a depth-first search of the Role DAG, starting at $principal,
         * in order to find a rule allowing/denying $principal access to all
         * privileges upon $resource
         *
         * This method returns true if a rule is found and allows access. If a
         * rule exists and denies access, then this method returns false. If no
         * applicable rule is found, then this method returns null.
         *
         * @param  Role\RoleInterface           $principal
         * @param  Resource\ResourceInterface   $resource
         * @return bool|null
         */
        private bool? RoleDfsAllPrivileges(
            Group @group,
            ResourceId resource = null)
        {
            var dfs = new DepthFirstSearchTracker();

            var result = RoleDfsVisitAllPrivileges(@group, resource, dfs);

            if (null != result)
            {
                return result;
            }

            while (dfs.Stack.Count != 0)
            {
                @group = dfs.Stack.Pop();

                if (false == dfs.Visited.Contains(@group))
                {
                    result = RoleDfsVisitAllPrivileges(@group, resource, dfs);

                    if (null != result)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        /**
         * Performs a depth-first search of the Role DAG, starting at $principal,
         * in order to find a rule allowing/denying $principal access to a
         * $privilege upon $resource.
         *
         * This method returns true if a rule is found and allows access. If a
         * rule exists and denies access, then this method returns false. If no
         * applicable rule is found, then this method returns null.
         *
         * @param  Role\RoleInterface           $principal
         * @param  Resource\ResourceInterface   $resource
         * @param  string                       $privilege
         * @return bool|null
         * @throws Exception\RuntimeException
         */
        private bool? RoleDfsOnePrivilege(
            Group @group,
            ResourceId resource = null,
            Type privilege = null)
        {
            if (null == privilege)
            {
                throw new AclUnexpectedStateException(
                    "$privilege based search parameter is null");
            }

            var dfs = new DepthFirstSearchTracker();

            var result = RoleDfsVisitOnePrivilege(
                @group, resource, privilege, dfs);

            if (null != result)
            {
                return result;
            }

            while (dfs.Stack.Count != 0)
            {
                @group = dfs.Stack.Pop();

                if (false == dfs.Visited.Contains(@group))
                {
                    result = RoleDfsVisitOnePrivilege(
                        @group, resource, privilege, dfs);

                    if (null != result)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        /**
         * Visits an $principal in order to look for a rule allowing/denying $principal
         * access to all privileges upon $resource.
         *
         * This method returns true if a rule is found and allows access. If a
         * rule exists and denies access, then this method returns false. If no
         * applicable rule is found, then this method returns null.
         *
         * This method is used by the internal depth-first search algorithm and
         * may modify the DFS data structure.
         *
         * @param  Role\RoleInterface           $principal
         * @param  Resource\ResourceInterface   $resource
         * @param  array                        $dfs
         * @return bool|null
         * @throws Exception\RuntimeException
         */
        private bool? RoleDfsVisitAllPrivileges(
            Group @group,
            ResourceId resource = null,
            DepthFirstSearchTracker dfs = null)
        {
            if (null == dfs)
            {
                throw new AclUnexpectedStateException(
                    "$dfs parameter may not be null");
            }

            bool? hasAccess = DoesPrincipalHaveAccessTo(@group, resource);

            if (hasAccess != null)
            {
                return hasAccess;
            }

            dfs.Visited.Add(@group);

            var parentGroups = _api.Groups.GetParents(@group);
            foreach (var roleParent in parentGroups)
            {
                dfs.Stack.Push(roleParent);
            }

            return null;
        }

        private bool? DoesPrincipalHaveAccessTo(
            IPrincipal principal, ResourceId resource)
        {
            PrincipalRule principalRule =
                _ruleset.ResolvePrincipalRuleFor(resource, principal);

            if (null != principalRule)
            {
                foreach (var privilege in principalRule.ByPrivilegeType.Keys)
                {
                    var ruleTypeOnePrivilege = _ruleset.GetRuleResultFor(
                        resource, principal, privilege);

                    if (RuleType.Deny == ruleTypeOnePrivilege)
                    {
                        return true;
                    }
                }

                var ruleTypeAllPrivileges = _ruleset.GetRuleResultFor(
                    resource, principal, null);

                if (null != ruleTypeAllPrivileges)
                {
                    return true;
                }
            }

            return null;
        }

        private bool? DoesPrincipalHaveAccessTo(
            IPrincipal principal, ResourceId resource, Type privilege)
        {
            var ruleTypeOnePrivilege = _ruleset.GetRuleResultFor(
                resource, principal, privilege);

            if (null != ruleTypeOnePrivilege)
            {
                return RuleType.Allow == ruleTypeOnePrivilege;
            }

            // No rule existed, so use default privilege rule (i.e. All Privileges)
            var ruleTypeAllPrivileges = _ruleset.GetRuleResultFor(
                resource, principal, null);

            if (null != ruleTypeAllPrivileges)
            {
                return RuleType.Allow == ruleTypeAllPrivileges;
            }

            return null;
        }

        /// <summary>
        ///   Visits an $principal in order to look for a rule allowing/denying 
        ///   $principal access to a $privilege upon $resource.
        /// </summary>
        /// 
        /// <param name="group">The principal.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="privilege">The privilege.</param>
        /// <param name="dfs">The DFS.</param>
        /// 
        /// <returns>
        ///   This method returns true if a rule is found and allows access. 
        ///   If a rule exists and denies access, then this method returns false. 
        ///   If no applicable rule is found, then this method returns null.
        /// </returns>
        /// 
        /// <exception cref="AclUnexpectedStateException">
        ///   $privilege parameter may not be null
        ///   or
        ///   $dfs parameter may not be null
        /// </exception>
        /// 
        /// <remarks>
        ///   This method is used by the internal depth-first search algorithm 
        ///   and may modify the DFS data structure.
        /// </remarks>
        private bool? RoleDfsVisitOnePrivilege(
            Group @group,
            ResourceId resource = null,
            Type privilege = null,
            DepthFirstSearchTracker dfs = null)
        {
            if (null == privilege)
            {
                throw new AclUnexpectedStateException(
                    "$privilege parameter may not be null");
            }

            if (null == dfs)
            {
                throw new AclUnexpectedStateException(
                    "$dfs parameter may not be null");
            }

            bool? hasAccess = DoesPrincipalHaveAccessTo(
                @group, 
                resource,
                privilege);

            if (hasAccess != null)
            {
                return hasAccess;
            }

            dfs.Visited.Add(@group);

            var parentRoles = _api.Groups.GetParents(@group);
            foreach (var roleParent in parentRoles)
            {
                dfs.Stack.Push(roleParent);
            }

            return null;
        }

        /// <summary>
        ///   Performs operations on ACL rules.
        /// </summary>
        /// 
        /// <typeparam name="TPrivilege">
        ///   The type of the privilege to set the rule for.
        ///   If it is of type <see cref="AllPrivileges"/> then the rule will
        ///   be for all privileges.
        /// </typeparam>
        /// <param name="operation">
        ///   The operation.
        /// </param>
        /// <param name="type">
        ///   The type of the rule.
        /// </param>
        /// <param name="principal">
        ///   The principal. If null then it's considered to be applicable for all 
        ///   groups.
        /// </param>
        /// <param name="resource">
        ///   The resource.  If null then it's considered to be applicable for
        ///   all resources.
        /// </param>
        /// 
        /// <returns>
        ///   This instance to support a fluent interface.
        /// </returns>
        /// 
        /// <exception cref="PrivilegeNotRegisteredException">
        ///   If the given privilege type is not registered for usage.
        /// </exception>
        /// <exception cref="AclUnexpectedStateException">
        ///   An unkown ACL operation type is executed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///   If this instance has been disposed.
        /// </exception>
        /// 
        /// <remarks>
        ///   <para>
        ///   The $operation parameter may be either OP_ADD or OP_REMOVE, 
        ///   depending on whether the user wants to add or remove a rule, 
        ///   respectively:
        ///   </para>
        /// 
        ///   <para>
        ///   OP_ADD specifics:
        ///   A rule is added that would allow one or more Roles access to 
        ///   [certain $privileges upon] the specified Resource(s).
        ///   </para>
        /// 
        ///   <para>
        ///   OP_REMOVE specifics:
        ///   The rule is removed only in the context of the given Roles, 
        ///   Resources, and privileges. Existing rules to which the remove 
        ///   operation does not apply would remain in the ACL.
        ///   </para>
        /// 
        ///   <para>
        ///   The $type parameter may be either TYPE_ALLOW or TYPE_DENY, 
        ///   depending on whether the rule is intended to allow or deny 
        ///   permission, respectively.
        ///   </para>
        /// 
        ///   <para>
        ///   The $groups and $resources are to indicate the Resources and Groups 
        ///   to which the rule applies. If either $groups or $resources is null, 
        ///   then the rule applies to all Groups or all Resources, respectively.
        ///   Both may be null in order to work with the default rule of the ACL.
        ///   </para>
        /// 
        ///   <para>
        ///   The $privileges parameter may be used to further specify that the 
        ///   rule applies only to certain privileges upon the Resource(s) in 
        ///   question. This may be specified to be a single privilege with a 
        ///   string, and multiple privileges may be specified as an array of 
        ///   strings.
        ///   </para>
        /// 
        ///   <para>
        ///   If $assert is provided, then its assert() method must return true 
        ///   in order for the rule to apply. If $assert is provided with 
        ///   $groups, $resources, and $privileges all equal to null, then a 
        ///   rule having a type of:
        ///   <list type="bullet">
        ///     <item>
        ///       <description>
        ///         TYPE_ALLOW will imply a type of TYPE_DENY, and
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         TYPE_DENY will imply a type of TYPE_ALLOW
        ///       </description>
        ///     </item>
        ///   </list>
        ///   </para>
        /// 
        ///   <para>
        ///   when the rule's assertion fails. This is because the ACL needs to provide expected
        ///   behavior when an assertion upon the default ACL rule fails.
        ///   </para>
        /// </remarks>
        private Rules SetRule<TPrivilege>(
            Operation operation,
            RuleType type,
            IPrincipal principal = null,
            IResource resource = null)
            where TPrivilege : IPrivilege
        {
            var privilege = ParsePrivilegeType<TPrivilege>();

            if (principal == null)
            {
                // No groups were provided, so assuming 'Global' setting.
                principal = NullGroup;
            }

            if ((false == principal is IUser) && (false == principal is Group))
            {
                throw new ArgumentException(
                    "Invalid principal type.  Only IUser and Group instances are allowed.");
            }

            ResourceId resourceId;

            if (resource == null)
            {
                // No resources were provided, so assuming 'Global' setting.
                resourceId = NullResourceId;
            }
            else
            {
                resourceId = resource.ResourceId;
            }

            // We will process all the child resources of this resource, so
            // that they will all get the same rule.
            var resourcesSet = new HashSet<ResourceId>();

            if (resourceId.Equals(NullResourceId))
            {
                resourcesSet.Add(resourceId);
            }
            else
            {
                var childResources = _api.Resources.GetAllChildResources(resourceId);

                foreach (var childResource in childResources)
                {
                    resourcesSet.Add(childResource);
                }

                resourcesSet.Add(resourceId);
            }

            var resources = resourcesSet.ToArray();

            switch (operation) {
                // add to the rules
                case Operation.Add:
                    foreach (var r in resources)
                    {
                        // This is the only place that calls the GetRules
                        // method with the bool flag set to true.
                        PrincipalRule rules = _ruleset.ResolvePrincipalRuleFor(
                            ReferenceEquals(r, NullResourceId) ? null : r,
                            ReferenceEquals(principal, NullGroup) ?
                                null : principal,
                            true);


                        if (privilege == null)
                        {
                            rules.AllPrivileges = new PrivilegeRule
                            {
                                Type = type
                            };
                        }
                        else
                        {
                            var privilegeRule = new PrivilegeRule()
                            {
                                Type = type
                            };

                            rules.ByPrivilegeType.AddOrUpdate(
                                privilege,
                                privilegeRule,
                                (pId, oldRule) =>
                                {
                                    oldRule.Type = type;
                                    return oldRule;
                                });
                        }
                    }

                    break;

                // remove from the rules
                case Operation.Remove:
                    foreach (var r in resources)
                    {
                        PrincipalRule rules = _ruleset.ResolvePrincipalRuleFor(
                            ReferenceEquals(NullResourceId, r) ? null : r,
                            ReferenceEquals(NullGroup, principal) ? 
                                null : principal);

                        if (null == rules)
                        {
                            continue;
                        }

                        if (privilege == null)
                        {
                            if (ReferenceEquals(NullResourceId, r) &&
                                ReferenceEquals(NullGroup, principal))
                            {
                                if (rules.AllPrivileges != null &&
                                    type == rules.AllPrivileges.Type)
                                {
                                    rules.AllPrivileges.Type = RuleType.Deny;
                                    rules.ByPrivilegeType.Clear();
                                }

                                continue;
                            }

                            if (rules.AllPrivileges != null &&
                                type == rules.AllPrivileges.Type)
                            {
                                rules.AllPrivileges = null;
                            }
                        }
                        else
                        {
                            PrivilegeRule privilegeRule;

                            if (rules.ByPrivilegeType.TryGetValue(
                                    privilege, out privilegeRule)
                                && privilegeRule.Type == type)
                            {
                                rules.ByPrivilegeType.TryRemove(
                                    privilege, out privilegeRule);
                            }
                        }
                    }

                    break;

                default:
                    throw new AclUnexpectedStateException(
                        "Unknown ACL operation recieved.");
            }

            _rulesetRepository.Save(_ruleset);

            // TODO: Fire off Assertion Result Cache worker.
            //_ruleAssertCache.Execute()

            return this;
        }

        #endregion Methods

        #region Nested Types

        /// <summary>
        ///   Helper class used for the Depth First Searches on Groups within 
        ///   ACLs.
        /// </summary>
        private sealed class DepthFirstSearchTracker
        {
            #region Constructors

            public DepthFirstSearchTracker()
            {
                Visited = new List<IPrincipal>();
                Stack = new Stack<Group>();
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            ///   Gets or sets the stack of groups to visit.
            /// </summary>
            public Stack<Group> Stack
            {
                get; private set;
            }

            /// <summary>
            ///   Gets or sets the groups that have been visited.
            /// </summary>
            public List<IPrincipal> Visited
            {
                get; private set;
            }

            #endregion Properties
        }

        #endregion Nested Types
    }
}