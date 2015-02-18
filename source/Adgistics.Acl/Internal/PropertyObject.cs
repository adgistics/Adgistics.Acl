namespace Modules.Acl.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Collections;

    using Utils;

    /// <summary>
    ///   The default implemenation of <c>IPropertyObject</c>.
    /// </summary>
    ///
    /// <since version="1.0"/>
    [DataContract]
    internal sealed class PropertyObject : IPropertyObject
    {
        #region Fields

        /// <summary>
        ///   The backing store for this properties.
        /// </summary>
        [DataMember]
        private readonly IDictionary<string, object> _properties;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the 
        ///   <see cref="PropertyObject"/> class.
        /// </summary>
        public PropertyObject()
        {
            _properties = new Dictionary<string, object>();
        }

        /// <summary>
        ///   Initializes a new instance of the 
        ///   <see cref="PropertyObject"/> class.
        /// </summary>
        /// 
        /// <param name="properties">
        ///   The properties to initialize this instance with.
        /// </param>
        public PropertyObject(IEnumerable<KeyValuePair<string, object>> properties)
            : this()
        {
            if (properties == null)
            {
                throw new ArgumentException(
                    "Argument 'properties' must not be null.");
            }

            foreach (var pair in properties)
            {
                if (pair.Key == null)
                {
                    throw new ArgumentException("Argument 'properties' contains a null key.");
                }

                if (pair.Value == null)
                {
                    // ignore null as they are considered unknown values which
                    // just return null or a supplied default on 'get'.
                    continue;
                }

                Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        ///   A copy constructor.
        /// </summary>
        /// 
        /// <param name="properties">
        ///   The properties to initialize this properties with.
        /// </param>
        public PropertyObject(IPropertyObject properties)
            : this()
        {
            if (properties == null)
            {
                throw new ArgumentException(
                    "Argument 'properties' must not be null.");
            }

            SetProperties(properties.GetProperties());
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///   Determines if the argument string is light weight string suitable
        ///   for adding as a string property.
        /// </summary>
        /// 
        /// <returns>
        ///   <c>true</c> if is light weight string the specified value;
        ///   otherwise <c>false</c>.
        /// </returns>
        /// 
        /// <param name="value">The Value.</param>
        public static bool IsLightWeightString(string value)
        {
            var result = false;

            if (value != null)
            {
                const int fiveKilobytes = 1024 * 5;
                const int zeroKilobytes = 0;

                // Default char size is 16 bits for UTF-16 (2 bytes) so length
                // of string multiplied by 2 is good enough for our purposes.
                // It could be argued that most serializers output to UTF-8
                // in which case this calculation is a little aggressive and may
                // need to be modified over time.
                var v = value.Length * 2;

                result = NumberUtils.InRange(v, zeroKilobytes, fiveKilobytes);
            }

            return result;
        }

        /// <summary>
        ///  Returns the boolean-valued property with the given name.
        ///  Returns the given default value if the property is undefined
        ///  or is not an boolean value.
        /// </summary>
        /// <see cref="IPropertyObject"/>
        public bool GetBool(string key, bool defaultValue)
        {
            object value = GetProperty(key);
            bool result = defaultValue;

            if (value is Boolean)
            {
                result = (bool)value;
            }

            return result;
        }

        /// <summary>
        ///  Returns the integer-valued property with the given name.
        ///  Returns the given default value if the property is undefined
        ///  or is not an integer value.
        /// </summary>
        /// <see cref="IPropertyObject"/>
        public int GetInt(string key, int defaultValue)
        {
            object value = GetProperty(key);
            int result = defaultValue;

            if (value != null && value is Int32)
            {
                result = (int)value;
            }

            return result;
        }

        /// <summary>
        /// Returns the properties with the given names.
        /// </summary>
        /// <see cref="IPropertyObject"/>
        public object[] GetProperties(params string[] keys)
        {
            if (_properties == null)
            {
                throw new ArgumentException(
                    "Argument 'keys' must not be null.");
            }

            var result = new object[keys.Length];

            for (int i = 0, length = result.Length; i < length; i++)
            {
                result[i] = GetProperty(keys[i]);
            }

            return result;
        }

        /// <summary>
        ///  Returns a dictionary with all the properties for the marker.
        ///  If the marker has no properties then an empty dictionary is 
        ///  returned.
        /// </summary>
        /// <see cref="IPropertyObject"/>
        public IDictionary<string, object> GetProperties()
        {
            // In this implemenation care was taken in the constructor to make
            // sure that the dictionery returned here is modifiable by the
            // internals of this class but not by any external calls on the
            // returned Dictionary
            return ImmutableDictionary<string, object>.Of(_properties);
        }

        /// <summary>
        ///   Get the specified key.
        /// </summary>
        /// <see cref="IPropertyObject"/>
        public object GetProperty(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    "Argument 'key' must not be null, whitespace only, or empty.");
            }

            object result = null;

            if (_properties.ContainsKey(key))
            {
                result = _properties[key];
            }

            return result;
        }

        /// <summary>
        ///  Returns the integer-valued property with the given name.
        ///  Returns the given default value if the property is undefined
        ///  or is not an integer value.
        /// </summary>
        /// <see cref="IPropertyObject"/>
        public string GetString(string key, string defaultValue)
        {
            return ((GetProperty(key)) as  String) ?? defaultValue;
        }

        /// <summary>
        /// Sets the boolean-valued property with the given name.
        /// </summary>
        /// <see cref="IPropertyObject"/>
        public IPropertyObject SetBool(string key, bool? value)
        {
            Add(key, value);

            return this;
        }

        /// <summary>
        /// Sets the boolean-valued property with the given name.
        /// </summary>
        /// <see cref="IPropertyObject"/>
        public IPropertyObject SetInt(string key, int? value)
        {
            Add(key, value);

            return this;
        }

        /// <summary>
        /// Sets the properties with the given keys and values.
        /// </summary>
        /// <see cref="IPropertyObject"/>
        public void SetProperties(IEnumerable<KeyValuePair<string, object>> properties)
        {
            if (properties == null)
            {
                throw new ArgumentException(
                    "Argument 'properties' must not be null.");
            }

            foreach (var pair in properties)
            {
                Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Sets the properties with the given keys and values.
        /// </summary>
        /// <see cref="IPropertyObject"/>
        public IPropertyObject SetProperties(IPropertyObject properties)
        {
            if (properties == null)
            {
                throw new ArgumentException(
                    "Argument 'properties' must not be null.");
            }

            SetProperties(properties.GetProperties());

            return this;
        }

        /// <summary>
        /// Sets the boolean-valued property with the given name.
        /// </summary>
        /// <see cref="IPropertyObject"/>
        public IPropertyObject SetString(string key, string value)
        {
            Add(key, value);

            return this;
        }

        /// <summary>
        ///   Safley add a key and value without throwing an exception in a key
        ///   already exists and making sure any reference held external to this
        ///   class is handled as per the default behavior of the properties
        ///   class and not the dictionery class.
        /// </summary>
        ///
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        private void Add(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(
                    "Argument 'key' must not be null, whitespace only, or empty.");
            }

            // Type check if the value is not null as null is a legal type.
            if (value != null)
            {
                if (false == (value is String
                     || value is Int32
                     || value is Boolean))
                {
                    throw new ArgumentException(
                        string.Format(
                            "Expected 'value' for key:'{0}' to be one of String, Int32 or Boolean was:{1}",
                            key, value.GetType()));
                }
            }

            if (value is String)
            {
                if (false == IsLightWeightString(value as String))
                {
                    throw new ArgumentException(
                        "Argument: 'value' is greater than 5120 Bytes (5 KB).");
                }
            }

            // Only remove the key if the value type has passed its constraints.
            if (_properties.ContainsKey(key))
            {
                _properties.Remove(key);
            }

            // Ignore null as they are considered unknown values which
            // just return null or a supplied default on 'get'.
            if (value != null)
            {
                _properties.Add(key, value);
            }
        }

        #endregion Methods
    }
}