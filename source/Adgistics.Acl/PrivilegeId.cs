namespace Modules.Acl
{
    using System;

    /// <summary>
    ///   A system-wide unique identifier for a privilege that within the
    ///   ACL system.
    /// </summary>
    /// 
    /// <remarks>
    ///   <para>
    ///   <see cref="PrivilegeId"/>s are used extensively in hashing lookups.  
    ///   Therefore, you must ensure that implementations provide SYSTEM-WIDE
    ///   unique identifiers.
    ///   </para>
    /// 
    ///   <para>
    ///   Once you have created a unique privilege identifier you should
    ///   not change it as it may break references to it within existing
    ///   ACL rules.
    ///   </para>
    /// </remarks>
    public sealed class PrivilegeId : IEquatable<PrivilegeId>
    {
        #region Fields

        /// <summary>
        ///   The _hash code for this instance.
        /// </summary>
        private readonly int _hashCode;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="PrivilegeId"/> class.
        /// </summary>
        /// 
        /// <param name="type">The type of resource.</param>
        /// <param name="id">The identifier of the resource.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'type' must not be null, whitespace only, or empty.
        ///   </para>
        ///   or
        ///   <para>
        ///   Argument 'id' must not be null, whitespace only, or empty.
        ///   </para>
        /// </exception>
        public PrivilegeId(string type, string id)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException(
                    "Argument 'type' must not be null, whitespace only, or empty.");
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "Argument 'id' must not be null, whitespace only, or empty.");
            }

            Type = type;
            Id = id;

            _hashCode = ((Type != null ? Type.GetHashCode() : 0)*397) ^
                        (Id != null ? Id.GetHashCode() : 0);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///   Gets the identifier of the privilege.
        /// </summary>
        public string Id
        {
            get; private set;
        }

        /// <summary>
        ///   Gets the type of the privilege.
        /// </summary>
        /// 
        /// <example>
        ///   AssetLibrary.View.Protected
        /// </example>
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
        public bool Equals(PrivilegeId other)
        {
            return (other != null) &&
                string.Equals(Type, other.Type) && string.Equals(Id, other.Id);
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

            return Equals(obj as PrivilegeId);
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
                return _hashCode;
            }
        }

        #endregion Methods
    }
}