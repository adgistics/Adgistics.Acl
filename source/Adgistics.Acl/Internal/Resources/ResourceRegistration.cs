namespace Modules.Acl.Internal.Resources
{
    using System.Collections.Concurrent;
    using System.Runtime.Serialization;

    /// <summary>
    ///   Represents the parent/child resource relationships for a resource.
    /// </summary>
    /// 
    /// <remarks>
    ///   This could be refactored into a class which utilizes the directed
    ///   graph collection to provide more flexibility.
    /// </remarks>
    [DataContract]
    internal sealed class ResourceRegistration
    {
        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the 
        ///   <see cref="ResourceRegistration"/> class.
        /// </summary>
        /// 
        /// <param name="resource">The resource.</param>
        /// <param name="parent">The parent.</param>
        public ResourceRegistration(IResource resource, IResource parent)
        {
            Instance = resource.ResourceId;

            if (parent != null)
            {
                Parent = parent.ResourceId;
            }

            Children = new ConcurrentDictionary<ResourceId, string>();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///   Gets the child resources for this resource.
        /// </summary>
        [DataMember]
        public ConcurrentDictionary<ResourceId, string> Children
        {
            get; private set;
        }

        /// <summary>
        ///   Gets the resource instanc this registration is for.
        /// </summary>
        [DataMember]
        public ResourceId Instance
        {
            get; private set;
        }

        /// <summary>
        ///  Gets the parent resource for this resource.
        /// </summary>
        /// 
        /// <remarks>
        ///   Will be <c>null</c> if there is no parent.
        /// </remarks>
        [DataMember]
        public ResourceId Parent
        {
            get; private set;
        }

        #endregion Properties
    }
}