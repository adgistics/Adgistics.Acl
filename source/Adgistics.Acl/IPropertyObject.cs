namespace Modules.Acl
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///   Indicates that an implementation class can store and retrieve a
    ///   dynamic set of property values.
    /// </summary>
    ///
    /// <since version="1.0"/>
    public interface IPropertyObject
    {
        #region Methods

        /// <summary>
        ///   Returns the boolean-valued property with the given name.
        ///   Returns the given default value if the property is undefined
        ///   or is not an boolean value.
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
        bool GetBool(string key, bool defaultValue);

        /// <summary>
        ///   Returns the integer-valued property with the given name.
        ///   Returns the given default value if the property is undefined
        ///   or is not an integer value.
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
        int GetInt(string key, int defaultValue);

        /// <summary>
        ///   Returns the properties with the given names.
        /// </summary>
        /// 
        /// <returns>
        ///   The properties with the given names.
        /// </returns>
        /// 
        /// <param name="keys">
        ///   The names of the properties to retrieve.
        /// </param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   If keys is <c>null</c>.
        /// </exception>
        /// 
        /// <remarks>
        ///   <para>
        ///   The result is an an array whose elements correspond to the 
        ///   elements of the given property name array. Each element is 
        ///   <c>null</c> or an instance of one of the following classes: 
        ///   <c>String</c>, <c>Integer</c>, or <c>Boolean</c>. The returned 
        ///   array elements are index as per the order for the <c>keys</c>.
        ///   </para>
        /// </remarks>
        object[] GetProperties(params string[] keys);

        /// <summary>
        ///   Returns a dictionary with all the properties for the marker.
        ///   If the marker has no properties then an empty dictionary is 
        ///   returned.
        /// </summary>
        /// 
        /// <remarks>
        ///   <para>
        ///   The dictionary returned by calls to this method is readonly, 
        ///   however structural changes made to this properties objects are 
        ///   reflected in the returned dictionary. Any attempt to structurally 
        ///   modify the returned dictionary will result in an exception being 
        ///   thrown.
        ///   If you need a dictionary that can be modified then construct a new
        ///   Dictionary from a call to this method 
        ///   <c>new Dictionary&lt;string, object&gt;(properties.Get())</c>, 
        ///   this new dictionary will not reflect structurally changes made to 
        ///   this properties.
        ///   </para>
        /// </remarks>
        /// 
        /// <returns>
        ///   A dictionary with all the properties for the marker.
        /// </returns>
        IDictionary<string, object> GetProperties();

        /// <summary>
        ///   Returns the property with the given name. The result is an 
        ///   instance of one of the following classes: <c>String</c>, 
        ///   <c>Integer</c> or <c>Boolean</c>; <c>null</c> if the property 
        ///   is undefined.
        /// </summary>
        ///
        /// <param name="key">the name of the property.</param>
        /// 
        /// <returns>
        ///   The property with the given name.
        /// </returns>
        object GetProperty(string key);

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
        string GetString(string key, string defaultValue);

        /// <summary>
        ///   Sets the boolean-valued property with the given name.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>
        ///   If a value is <c>null</c>, the new value of the 
        ///   property is considered to be undefined.
        /// </para>
        /// <para>
        ///   This method changes resources; these changes will be reported
        ///   in a subsequent resource change event, including an indication 
        ///   that this marker has been modified.
        /// </para>
        /// </remarks>
        /// 
        /// <param name="key">
        ///   The name of the property, must conform to the Qname naming 
        ///   conventions.
        /// </param>
        /// <param name="value">The value.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   If <c>key</c> is <c>null</c>, empty or whitespace 
        ///   only string.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///   If <c>key</c> does not conform to Qname naming  conventions.
        /// </exception>
        /// <exception cref="ApplicationException">
        ///   If this method fails for any reason.
        /// </exception>
        /// 
        /// <returns>
        ///   The current instance in order to provide a fluent inteface.
        /// </returns>
        IPropertyObject SetBool(string key, bool? value);

        /// <summary>
        ///   Sets the integer-valued property with the given name.
        /// </summary>
        /// 
        /// <param name="key">
        ///   The name of the property, must conform to the Qname naming 
        ///   conventions.
        /// </param>
        /// <param name="value">
        ///   The value.
        /// </param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   If <c>key</c> is <c>null</c>, empty or whitespace 
        ///   only string.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///   If <c>key</c> does not conform to Qname naming  conventions.
        /// </exception>
        /// 
        /// <remarks>
        ///   <para>
        ///   If a value is <c>null</c>, the new value of the 
        ///   property is considered to be undefined.
        ///   </para>
        /// 
        ///   <para>
        ///   This method changes resources; these changes will be reported
        ///   in a subsequent resource change event, including an indication 
        ///   that this marker has been modified.
        ///   </para>
        /// </remarks>
        /// 
        /// <returns>
        ///   The current instance in order to provide a fluent inteface.
        /// </returns>
        IPropertyObject SetInt(string key, int? value);

        /// <summary>
        ///   Sets this properties to those of the argument properties.
        /// </summary>
        /// 
        /// <remarks>
        ///   <para>
        ///   This method changes resources; these changes will be reported
        ///   in a subsequent resource change event, including an indication 
        ///   that this marker has been modified.
        ///   </para>
        /// 
        ///   <para>
        ///   Implemetors mut be carful tahat after calls to this method are 
        ///   complety any changes made to the argumnet <c>properties</c> are 
        ///   not reflected in the internals of this properties.
        ///   </para>
        /// </remarks>
        /// 
        /// <param name="properties">The properties to add.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   If properties is <c>null</c>.
        /// </exception>
        /// 
        /// <returns>
        ///   The current instance in order to provide a fluent inteface.
        /// </returns>
        IPropertyObject SetProperties(IPropertyObject properties);

        /// <summary>
        ///   Sets the object-valued property with the given name.
        /// </summary>
        /// 
        /// <remarks>
        /// <para>
        ///   As properties are intended to be light weight in nature there is 
        ///   an upper limit of 5.0 KB (5120 Bytes) for string values, this 
        ///   upper bound allows for large comments to be added but still 
        ///   enforcing the light weight nature of properties.
        /// </para>
        /// <para>
        ///   If a value is <c>null</c>, the new value of the 
        ///   property is considered to be undefined.
        /// </para>
        /// <para>
        ///   This method changes resources; these changes will be reported
        ///   in a subsequent resource change event, including an indication 
        ///   that this marker has been modified.
        /// </para>
        /// </remarks>
        /// 
        /// <param name="key">
        ///   The name of the property, must conform to the Qname naming 
        ///   conventions.
        /// </param>
        /// <param name="value">The value.</param>
        /// 
        /// <exception cref="System.ArgumentException">
        ///   If <c>key</c> is <c>null</c>, empty or whitespace 
        ///   only string.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///   If <c>value</c> is &gt; 5120 Bytes.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///   If <c>key</c> does not conform to Qname naming  conventions.
        /// </exception>
        /// <exception cref="ApplicationException">
        ///   If this method fails for any reason.
        /// </exception>
        /// 
        /// <returns>
        ///   The current instance in order to provide a fluent inteface.
        /// </returns>
        IPropertyObject SetString(string key, string value);

        #endregion Methods
    }
}