namespace Modules.Acl
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Threading;

    using Internal;

    /// <summary>
    ///   A Group.
    /// </summary>
    /// 
    /// <remarks>
    ///   <para>
    ///   Used to represent any organisational structure / user on which an
    ///   arbitrary set of properties can be attached, and for which a set
    ///   of access permissions can be defined.
    ///   </para>
    /// 
    ///   <para>
    ///   Groups follow a graphing structure, allowing a group to have 
    ///   multiple parents and multiple children.  
    ///   </para>
    /// 
    ///   <para>
    ///   It is recommended that you create extended types of group for which
    ///   the properties get set using the <see cref="IPropertyObject"/>
    ///   setters and getters.
    ///   </para>
    /// </remarks>
    [DataContract]
    public sealed class Group : IPrincipal, IPropertyObject, IEquatable<Group>
    {
        #region Fields

        /// <summary>
        ///   The name of the <see cref="Name"/> property.
        /// </summary>
        /// 
        /// <remarks>
        ///   This is needed for guards on the setter methods.
        /// </remarks>
        private const string NameProperty = "name";

        /// <summary>
        ///   The _properties object for this instance.  Allows dynamic 
        ///   volume of properties to be set on the object.
        /// </summary>
        [DataMember]
        private readonly PropertyObject _properties;

        private AccessControl _api;

        /// <summary>
        ///   The _hashcode for this instance.
        /// </summary>
        private int? _hashcode;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        /// 
        /// <param name="api">The AccessControl api</param>
        /// <param name="name">The name of the group.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///    Argument 'name' must not be null, whitespace only, or empty.
        /// </exception>
        /// 
        /// <remarks>
        ///   The name must be unique.
        /// </remarks>
        internal Group(AccessControl api, string name)
        {
            _api = api;

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Argument 'name' must not be null, whitespace only, or empty.");
            }

            Name = name;

            // Holds the dynamic properties of group.
            _properties = new PropertyObject();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///   Gets the API to the <see cref="AccessControl"/> instance that
        ///   this group belongs to.
        /// </summary>
        public AccessControl Api
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref _api, () =>
                {
                    return AccessControl.Get(ApiIdentifier);
                });

                return _api;
            }
        }

        /// <summary>
        ///   Gets the identifier of the <see cref="AccessControl"/> API 
        ///   instance to which this belongs.
        /// </summary>
        [DataMember]
        public string ApiIdentifier
        {
            get;
            private set;
        }

        /// <summary>
        ///   Gets the name of the group.
        /// </summary>
        [DataMember]
        public string Name
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        ///   Determines if the given two group names are considered equal.
        /// </summary>
        /// 
        /// <param name="groupName">Name of the group.</param>
        /// <param name="otherGroupName">Name of the other group.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   <para>
        ///   Argument 'groupName' must not be empty whitespace only or empty.
        ///   </para>
        ///   OR
        ///   <para>
        ///   Argument 'otherGroupName' must not be empty whitespace only or empty.
        ///   </para>
        /// </exception>
        /// 
        /// <returns>
        ///   <c>true</c> if and only if both the group names are considered
        ///   equal, else <c>false</c>.
        /// </returns>
        public static bool NamesEqual(string groupName, string otherGroupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new ArgumentException(
                    "Argument 'groupName' must not be empty whitespace only or empty.");
            }
            if (string.IsNullOrWhiteSpace(otherGroupName))
            {
                throw new ArgumentException(
                    "Argument 'otherGroupName' must not be empty whitespace only or empty.");
            }

            return groupName.Equals(otherGroupName,
                StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        ///   Determines whether the specified <see cref="Group" />, is 
        ///   equal to this instance.
        /// </summary>
        /// 
        /// <param name="other">
        ///   The <see cref="Group" /> to compare with this instance.
        /// </param>
        /// 
        /// <returns>
        ///   <c>true</c> if the specified <see cref="Group" /> is equal 
        ///   to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Group other)
        {
            return other != null &&
                   NamesEqual(Name, other.Name);
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

            return Equals(obj as Group);
        }

        /// <summary>
        ///   Returns the boolean-valued property with the given name.
        ///   Returns the given default value if the property is undefined
        ///   or is not an boolean value.
        /// </summary>
        /// 
        /// <param name="key">The name of the property.</param>
        /// 
        /// <param name="defaultValue">
        ///   The value to use if no value is found.
        /// </param>
        /// 
        /// <returns>
        ///   The value or the default value if no value was found.
        /// </returns>
        public bool GetBool(string key, bool defaultValue)
        {
            return _properties.GetBool(key, defaultValue);
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
            // Hashcode is purely on name of group.
            if (_hashcode == null)
            {
                _hashcode = Name.GetHashCode();
            }

            return _hashcode.Value;
        }

        /// <summary>
        ///   Returns the integer-valued property with the given name.
        ///   Returns the given default value if the property is undefined
        ///   or is not an integer value.
        /// </summary>
        /// 
        /// <param name="key">
        ///   The name of the property.
        /// </param>
        /// <param name="defaultValue">
        ///   The value to use if no value is found.
        /// </param>
        /// 
        /// <returns>
        ///   The value or the default value if no value was found.
        /// </returns>
        public int GetInt(string key, int defaultValue)
        {
            return _properties.GetInt(key, defaultValue);
        }

        /// <summary>
        ///   Returns the properties with the given names.
        /// </summary>
        /// 
        /// <param name="keys">The names of the properties to retrieve.</param>
        /// 
        /// <returns>
        ///   The properties with the given names.
        /// </returns>
        /// 
        /// <remarks>
        ///   The result is an an array whose elements correspond to the
        ///   elements of the given property name array. Each element is
        ///   <c>null</c> or an instance of one of the following classes:
        ///   <c>String</c>, <c>Integer</c>, or <c>Boolean</c>. The returned
        ///   array elements are index as per the order for the <c>keys</c>.
        /// </remarks>
        public object[] GetProperties(params string[] keys)
        {
            return _properties.GetProperties(keys);
        }

        /// <summary>
        ///   Returns a dictionary with all the properties for this group.
        ///   If the group has no properties then an empty dictionary is
        ///   returned.
        /// </summary>
        /// 
        /// <returns>
        ///   A dictionary with all the properties for the group.
        /// </returns>
        /// 
        /// <remarks>
        ///   <para>
        ///   The dictionary returned by calls to this method is readonly,
        ///   however structural changes made to this properties objects are
        ///   reflected in the returned dictionary. Any attempt to structurally
        ///   modify the returned dictionary will result in an exception being
        ///   thrown.
        ///   </para>
        /// 
        ///   <para>
        ///   If you need a dictionary that can be modified then construct a new
        ///   Dictionary from a call to this method
        ///   <c>new Dictionary&lt;string, object&gt;(properties.Get())</c>,
        ///   this new dictionary will not reflect structurally changes made to
        ///   this properties.
        ///   </para>
        /// </remarks>
        public IDictionary<string, object> GetProperties()
        {
            return _properties.GetProperties();
        }

        /// <summary>
        ///   Returns the property with the given name. The result is an
        ///   instance of one of the following classes: <c>String</c>,
        ///   <c>Integer</c> or <c>Boolean</c>; <c>null</c> if the property
        ///   is undefined.
        /// </summary>
        /// 
        /// <param name="key">
        ///   The name of the property.
        /// </param>
        /// 
        /// <returns>
        ///   The property with the given name.
        /// </returns>
        public object GetProperty(string key)
        {
            return _properties.GetProperty(key);
        }

        /// <summary>
        ///   Returns the string-valued property with the given name.
        ///   Returns the given default value if the property is undefined
        ///   or is not an string value.
        /// </summary>
        /// 
        /// <param name="key">The name of the property.</param>
        /// <param name="defaultValue">
        ///   The value to use if no value is found.
        /// </param>
        /// 
        /// <returns>
        ///   The value or the default value if no value was found.
        /// </returns>
        public string GetString(string key, string defaultValue)
        {
            return _properties.GetString(key, defaultValue);
        }

        /// <summary>
        ///   Sets the boolean-valued property with the given name.
        /// </summary>
        /// 
        /// <param name="key">The name of the property.</param>
        /// <param name="value">The value.</param>
        /// 
        /// <returns>
        /// The current instance in order to provide a fluent inteface.
        /// </returns>
        /// 
        /// <remarks>
        ///   <para>
        ///   If a value is <c>null</c>, the new value of the
        ///   property is considered to be undefined.
        ///   </para>
        ///   <para>
        ///   This method changes resources; these changes will be reported
        ///   in a subsequent resource change event, including an indication
        ///   that this marker has been modified.
        ///   </para>
        /// </remarks>
        public IPropertyObject SetBool(string key, bool? value)
        {
            CheckKeySetPreconditions(key);

            _properties.SetBool(key, value);

            OnPropertiesChanged();

            return this;
        }

        /// <summary>
        ///   Sets the integer-valued property with the given name.
        /// </summary>
        /// 
        /// <param name="key">The name of the property.</param>
        /// <param name="value">The value.</param>
        /// 
        /// <returns>
        ///   The current instance in order to provide a fluent inteface.
        /// </returns>
        /// 
        /// <remarks>
        ///   <para>
        ///   If a value is <c>null</c>, the new value of the
        ///   property is considered to be undefined.
        ///   </para>
        /// </remarks>
        public IPropertyObject SetInt(string key, int? value)
        {
            CheckKeySetPreconditions(key);

            _properties.SetInt(key, value);

            OnPropertiesChanged();

            return this;
        }

        /// <summary>
        ///   Sets this properties to those of the argument properties.
        /// </summary>
        /// 
        /// <param name="properties">The properties to add.</param>
        /// 
        /// <returns>
        ///   The current instance in order to provide a fluent inteface.
        /// </returns>
        public IPropertyObject SetProperties(IPropertyObject properties)
        {
            _properties.SetProperties(properties);

            OnPropertiesChanged();

            return this;
        }

        /// <summary>
        ///   Sets the string-valued property with the given name.
        /// </summary>
        /// 
        /// <param name="key">The name of the property.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The current instance in order to provide a fluent inteface.
        /// </returns>
        /// <remarks>
        /// <para>
        /// As properties are intended to be light weight in nature there is
        /// an upper limit of 5.0 KB (5120 Bytes) for string values, this
        /// upper bound allows for large comments to be added but still
        /// enforcing the light weight nature of properties.
        /// </para>
        /// <para>
        /// If a value is <c>null</c>, the new value of the
        /// property is considered to be undefined.
        /// </para>
        /// <para>
        /// This method changes resources; these changes will be reported
        /// in a subsequent resource change event, including an indication
        /// that this marker has been modified.
        /// </para>
        /// </remarks>
        public IPropertyObject SetString(string key, string value)
        {
            CheckKeySetPreconditions(key);

            _properties.SetString(key, value);

            OnPropertiesChanged();

            return this;
        }

        /// <summary>
        ///   Checks the key "property"  to ensure it meets the preconditions.
        /// </summary>
        /// 
        /// <param name="key">The key.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   Argument 'key' is invalid.  'Name' property cannot be changed.
        /// </exception>
        private void CheckKeySetPreconditions(string key)
        {
            if (NameProperty.Equals(key,
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException(
                    "Argument 'key' is invalid.  'Name' property cannot be changed.");
            }
        }

        private void OnPropertiesChanged()
        {
            _api.Events.OnGroupPropChanged(this);
        }

        #endregion Methods
    }
}