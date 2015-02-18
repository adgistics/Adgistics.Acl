namespace Modules.Acl
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Exceptions;

    using Internal.Resources;

    /// <summary>
    ///   Resources manager.
    /// </summary>
    /// 
    /// <remarks>
    ///   <para>
    ///   Provides the functionality to register resources to be used by the
    ///   ACL system.
    ///   </para>
    /// </remarks>
    public sealed class Resources
    {
        #region Fields

        private readonly AccessControl _api;

        /// <summary>
        ///   Registered resources as well as their relationship to each other.
        /// </summary>
        private readonly ConcurrentDictionary<ResourceId, ResourceRegistration> _resources;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="Resources"/> class.
        /// </summary>
        /// 
        /// <param name="api">The Access Control API.</param>
        internal Resources(
            AccessControl api)
        {
            _api = api;
            _resources = _api.Repositories.ResourceRepository.Load();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///   Gets all child resources for the given resource.
        /// </summary>
        /// 
        /// <param name="resourceId">The resource identifier.</param>
        /// 
        /// <exception cref="ArgumentException">
        ///   Argument 'resourceId' must not be null.
        /// </exception>
        /// 
        /// <returns>
        ///   All the child resources, else an empty array.
        /// </returns>
        public ResourceId[] GetAllChildResources(ResourceId resourceId)
        {
            if (resourceId == null)
            {
                throw new ArgumentException(
                    "Argument 'resourceId' must not be null.");
            }

            List<ResourceId> results = new List<ResourceId>();

            ResourceRegistration resourceReg;
            if (_resources.TryGetValue(resourceId, out resourceReg))
            {
                foreach (var child in resourceReg.Children.Keys)
                {
                    var childReturn = GetAllChildResources(child);
                    results.AddRange(childReturn);
                }
                results.AddRange(resourceReg.Children.Keys.ToArray());
            }

            return results.ToArray();
        }

        /// <summary>
        ///   Gets the identifiers of all registered resources.
        /// </summary>
        /// 
        /// <returns>
        ///   Array of registered resource identifiers.
        /// </returns>
        public ResourceId[] GetAllRegistered()
        {
            return _resources.Keys.ToArray();
        }

        /// <summary>
        ///   Gets the identifier of parent resource for the given resource.
        /// </summary>
        /// 
        /// <param name="resource">The resource.</param>
        /// 
        /// <returns>
        ///   The parent resource identifier, else <c>null</c>.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'resource' is not yet registered in the ACL system.
        ///   </para>
        /// </exception>
        /// 
        /// <exception cref="AclUnexpectedStateException">
        ///   <para>
        ///   A resource registration for the parent of the given resource
        ///   was not found.  This may mean that the ACL resource management
        ///   system has corrupted.
        ///   </para>
        /// </exception>
        public ResourceId GetParentResource(ResourceId resource)
        {
            ResourceId result;

            ResourceRegistration resourceReg;
            if (_resources.TryGetValue(resource, out resourceReg))
            {
                if (resourceReg.Parent != null)
                {
                    var parentId = resourceReg.Parent;
                    if (_resources.TryGetValue(parentId, out resourceReg))
                    {
                        result = resourceReg.Instance;
                    }
                    else
                    {
                        throw new AclUnexpectedStateException(
                            string.Format(
                                "A resource registration for the parent of the given resource " +
                                "was not found.  This may mean that the ACL resource management " +
                                "system has corrupted. parentId: {0}",
                                parentId));
                    }
                }
                else
                {
                    result = null; // i.e. All Resources
                }
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        "Argument 'resource' is not yet registered in the ACL system: {0}",
                        resource));
            }

            return result;
        }

        /// <summary>
        ///   Determines if <paramref name="resource"/> is registered as a child
        ///   of <paramref name="parent"/>.
        /// </summary>
        /// 
        /// <param name="resource">The resource.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="onlyDirectParent">
        ///   If set to <c>true</c> then $resource must parent directly from
        ///   $parent in order to return true.
        /// </param>
        /// 
        /// <returns>
        /// 
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'resource' cannot be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'parent' cannot be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Either of the resources are not registered in the ACL system.
        ///   </para>
        /// </exception>
        /// 
        /// <remarks>
        ///   By default, this method looks through the entire inheritance tree 
        ///   to determine whether $resource inherits from $parent through its 
        ///   ancestor Resources.
        /// </remarks>
        public bool Inherits(
            IResource resource,
            IResource parent,
            bool onlyDirectParent = false)
        {
            if (resource == null)
            {
                throw new ArgumentException("Argument 'resource' cannot be null.");
            }
            if (parent == null)
            {
                throw new ArgumentException("Argument 'parent' cannot be null.");
            }

            ResourceRegistration resourceReg;
            ResourceRegistration inheritReg;
            // TODO: Throw individual exceptions.
            if (false == _resources.TryGetValue(resource.ResourceId, out resourceReg)
                && false == _resources.TryGetValue(parent.ResourceId, out inheritReg))
            {
                throw new ArgumentException(
                    string.Format(
                        "Resources are not registered in ACL: '{0}', '{1}'",
                        resource,
                        parent));
            }

            if (null != resourceReg.Parent)
            {
                if (resourceReg.Parent.Equals(parent))
                {
                    return true;
                }
                if (onlyDirectParent)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            while (true)
            {
                ResourceRegistration parentReg;
                if (resourceReg.Parent == null ||
                    false == _resources.TryGetValue(resourceReg.Parent, out parentReg))
                {
                    break;
                }

                resourceReg = parentReg;

                if (parentReg.Instance.Equals(parent))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///   Determines whether the specified resource is registered.
        /// </summary>
        /// 
        /// <param name="resource">The resource.</param>
        /// 
        /// <returns>
        ///   Returns true if and only if the Resource is registered within the 
        ///   ACL.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   Argument resource must not be null.
        /// </exception>
        public bool IsRegistered(IResource resource)
        {
            if (resource == null)
            {
                throw new ArgumentException(
                    "Argument resource must not be null.");
            }

            ResourceRegistration resourceReg;

            return _resources.TryGetValue(
                resource.ResourceId,
                out resourceReg);
        }

        /// <summary>
        ///   Registers the given resource for usage in the ACL system.
        /// </summary>
        /// 
        /// <param name="resource">The resource.</param>
        /// <param name="parent">The parent.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'resource' must not be null.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Resource has already been registered within the ACL.
        ///   </para>
        ///   or
        ///   <para>
        ///   Parent resource is not registered.
        ///   </para>
        /// </exception>
        public void RegisterResource(IResource resource, IResource parent = null)
        {
            if (resource == null)
            {
                throw new ArgumentException(
                    "Argument 'resource' must not be null.");
            }

            if (IsRegistered(resource))
            {
                throw new ArgumentException(
                    "Resource has already been registered within the ACL.");
            }

            if (null != parent)
            {
                if (false == IsRegistered(parent))
                {
                    throw new ArgumentException(
                        string.Format(
                            "Parent Resource is not registered within ACL: {0}",
                            parent));
                }

                ResourceRegistration parentReg;
                if (_resources.TryGetValue(parent.ResourceId,
                    out parentReg))
                {
                    parentReg.Children.TryAdd(resource.ResourceId, string.Empty);
                }
            }

            var resourceReg = new ResourceRegistration(resource, parent);
            _resources.TryAdd(resource.ResourceId, resourceReg);

            Persist();
        }

        /// <summary>
        ///   Unregisters the given resource from the ACL system.
        /// </summary>
        /// 
        /// <param name="resource">The resource.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   Argument 'resource' must not be null.
        /// </exception>
        public void Unregister(IResource resource)
        {
            if (resource == null)
            {
                throw new ArgumentException(
                    "Argument 'resource' must not be null.");
            }

            Unregister(resource.ResourceId);
        }

        /// <summary>
        ///   Unregisters the given resource from the ACL system.
        /// </summary>
        /// 
        /// <param name="resource">The resource.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   Argument 'resource' must not be null.
        /// </exception>
        public void Unregister(ResourceId resource)
        {
            if (resource == null)
            {
                throw new ArgumentException(
                    "Argument 'resource' must not be null.");
            }

            string sfoo;

            var resourceReg = GetResourceRegistration(resource);

            if (resourceReg != null)
            {
                if (resourceReg.Parent != null)
                {
                    var parentReg = GetResourceRegistration(resource);

                    if (parentReg != null)
                    {
                        parentReg.Children.TryRemove(resource, out sfoo);
                    }
                }

                foreach (var child in resourceReg.Children.Keys)
                {
                    var childReg = GetResourceRegistration(child);

                    if (childReg != null)
                    {
                        Unregister(childReg.Instance);
                    }
                }
            }

            _resources.TryRemove(resource, out resourceReg);

            Persist();

            _api.Events.OnResourceUnregistered(resource);
        }

        internal ResourceRegistration GetResourceRegistration(ResourceId resource)
        {
            ResourceRegistration resourceReg;
            if (_resources.TryGetValue(resource,
                out resourceReg))
            {
                return resourceReg;
            }

            return null;
        }

        private void Persist()
        {
            _api.Repositories.ResourceRepository.Save(_resources);
        }

        #endregion Methods

        public IEnumerable<ResourceId> GetIdsByType(string type)
        {
            return _resources
                .Keys
                .Where(k => 
                    k.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
        }
    }
}