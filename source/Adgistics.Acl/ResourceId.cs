namespace Modules.Acl
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading;

    /// <summary>
    ///   A system-wide unique identifier for a resource that supports
    ///   ACL control.
    /// </summary>
    /// 
    /// <remarks>
    ///   <para>
    ///   <see cref="ResourceId"/>s are used extensively in hashing lookups.  
    ///   Therefore, you must ensure that implementations override the hashcode 
    ///   and equality methods and produce a SYSTEM-WIDE unique hashcode that 
    ///   represents the specific resource.
    ///   </para>
    /// </remarks>
    [DataContract]
    public sealed class ResourceId : IEquatable<ResourceId>
    {
        #region Fields

        /// <summary>
        ///   The _hash code for this instance.
        /// </summary>
        private int? _hashCode;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="ResourceId"/> class.
        /// </summary>
        /// 
        /// <param name="type">The type of resource.</param>
        /// <param name="name">The name of the resource.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'type' must not be null, whitespace only, or empty.
        ///   </para>
        ///   or
        ///   <para>
        ///   Argument 'name' must not be null, whitespace only, or empty.
        ///   </para>
        /// </exception>
        public ResourceId(string type, string name)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException(
                    "Argument 'type' must not be null, whitespace only, or empty.");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Argument 'name' must not be null, whitespace only, or empty.");
            }

            Type = type;
            Name = name;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///   Gets the name of the resource.
        /// </summary>
        [DataMember]
        public string Name
        {
            get; private set;
        }

        /// <summary>
        ///   Gets the type of the resource.
        /// </summary>
        [DataMember]
        public string Type
        {
            get; private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        ///   Indicates whether the current object is equal to another object of 
        ///   the same type.
        /// </summary>
        /// 
        /// <param name="other">An object to compare with this object.</param>
        /// 
        /// <returns>
        ///   true if the current object is equal to the 
        ///   <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(ResourceId other)
        {
            return (other != null) &&
                string.Equals(Type, other.Type) && string.Equals(Name, other.Name);
        }

        /// <summary>
        ///   Determines whether the specified <see cref="System.Object" />, is 
        ///   equal to this instance.
        /// </summary>
        /// 
        /// <param name="obj">
        ///   The <see cref="System.Object" /> to compare with this instance.
        /// </param>
        /// 
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal 
        ///   to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return Equals(obj as ResourceId);
        }

        /// <summary>
        ///   Returns a hash code for this instance.
        /// </summary>
        /// 
        /// <returns>
        ///   A hash code for this instance, suitable for use in hashing 
        ///   algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                if (_hashCode == null)
                {
                    _hashCode =  (Type.GetHashCode()*397) ^
                           (Name.GetHashCode());
                }

                return _hashCode.Value;
            }
        }

        #endregion Methods
    }
}