namespace Modules.Acl.Internal.Collections
{
    using System.Collections.Generic;

    /// <summary>
    ///   Represents a dictionary which forwards all its method calls to
    ///   another dictionary.
    ///   Subclasses should override one or more methods to modify the behavior
    ///   of the backing dictionary as per the decorator pattern.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    ///   Warning: The methods of <c>ForwardingDictionary</c> forward
    ///   indiscriminately to the methods of the delegate. For example,
    ///   overriding <c>dictionary[key] = value</c> alone will not change the
    ///   behavior of <c>Add(key, value)</c>, which can lead to unexpected
    ///   behavior.
    /// </para>
    /// <para>
    ///   The methods and any collection and views they return are not
    ///   guaranteed to be thread-safe, even when all of the methods that they
    ///   depend on are thread-safe.
    /// </para>
    /// </remarks>
    ///
    /// <typeparam name="TK">
    ///   The type of keys in the dictionary.
    /// </typeparam>
    /// <typeparam name="TV">
    ///   The type of value in the dictionary.
    /// </typeparam>
    ///
    /// <since version="1.0"/>
    internal abstract class ForwardingDictionary<TK, TV> : ForwardingObject<IDictionary<TK, TV>>, IDictionary<TK, TV>
    {
        #region Constructors

        /// <summary>
        ///   Constructor for sub-classes.
        /// </summary>
        protected ForwardingDictionary()
        {
            // Empty Block
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///   Gets the number of elements contained in the dictionary.
        /// </summary>
        ///
        /// <value>
        ///   The number of elements in the dictionary.
        /// </value>
        public virtual int Count
        {
            get { return ForwardTo().Count; }
        }

        /// <summary>
        ///   Gets a value indicating whether the dictionary is read-only.
        /// </summary>
        ///
        /// <value>
        ///   <c>true</c> if the dictionary is read-only;
        ///   otherwise <c>false</c>.
        /// </value>
        public virtual bool IsReadOnly
        {
            get { return ForwardTo().IsReadOnly; }
        }

        /// <summary>
        ///   Gets a collection containing the keys of the dictionary.
        /// </summary>
        public virtual ICollection<TK> Keys
        {
            get { return ForwardTo().Keys; }
        }

        /// <summary>
        ///   Gets an collection containing the values in the dictionary.
        /// </summary>
        public virtual ICollection<TV> Values
        {
            get { return ForwardTo().Values; }
        }

        #endregion Properties

        #region Indexers

        /// <summary>
        ///   Gets or sets the element with the specified key.
        /// </summary>
        ///
        /// <param name="key">
        ///   The key of the element to get or set.
        /// </param>
        ///
        /// <exception cref="System.ArgumentException">
        ///   If a forwarding implementation places additional constrains on
        ///   calls to this method.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///   If key == <c>null</c>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        ///   The property is retrieved and <c>key</c> is not found.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        ///   If the collection is read-only.
        /// </exception>
        public virtual TV this[TK key]
        {
            get { return ForwardTo()[key]; }
            set { ForwardTo()[key] = value; }
        }

        #endregion Indexers

        #region Methods

        /// <summary>
        ///   Adds an element with the provided key and value to the
        ///   dictionary.
        /// </summary>
        ///
        /// <param name="key">
        ///   The object to use as the key of the element to add.
        /// </param>
        /// <param name="value">
        ///   The object to use as the value of the element to add.
        /// </param>
        ///
        /// <exception cref="System.ArgumentException">
        ///   If a forwarding implementation places additional constrains on
        ///   calls to this method.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///   if key == <c>null</c>
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///   An element with the same key already exists in the dictionary.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        ///   If the collection is read-only.
        /// </exception>
        public virtual void Add(TK key, TV value)
        {
            ForwardTo().Add(key, value);
        }

        /// <summary>
        ///   Adds an key value pair to the dictionary.
        /// </summary>
        ///
        /// <param name="item">
        ///   The key value pair to add.
        /// </param>
        ///
        /// <exception cref="System.ArgumentException">
        ///   If a forwarding implementation places additional constrains on
        ///   calls to this method.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///   If item == <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///   An element with the same key already exists in the dictionary.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        ///   If the collection is read-only.
        /// </exception>
        public virtual void Add(KeyValuePair<TK, TV> item)
        {
            ForwardTo().Add(item);
        }

        /// <summary>
        ///   Removes all items from the collection.
        /// </summary>
        ///
        /// <exception cref="System.NotSupportedException">
        ///   If the collection is read-only.
        /// </exception>
        public virtual void Clear()
        {
            ForwardTo().Clear();
        }

        /// <summary>
        ///   Determines whether the dictionary contains a specific value.
        /// </summary>
        ///
        /// <param name="item">
        ///   The object to locate in the collection.
        /// </param>
        ///
        /// <returns>
        ///   <c>true</c> if item is found in the dictionary;
        ///   otherwise <c>false</c>.
        /// </returns>
        public virtual bool Contains(KeyValuePair<TK, TV> item)
        {
            return ForwardTo().Contains(item);
        }

        /// <summary>
        ///   Determines whether the dictionary contains an element with the
        ///   specified key.
        /// </summary>
        ///
        /// <param name="key">
        ///   The key to locate in the dictionary.
        /// </param>
        ///
        /// <returns>
        ///   <c>true</c> if the dictionary contains an element with the key;
        ///   otherwise <c>false</c>.
        /// </returns>
        ///
        /// <exception cref="System.ArgumentException">
        ///   If a forwarding implementation places additional constrains on
        ///   calls to this method.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///   if key == <c>null</c>
        /// </exception>
        public virtual bool ContainsKey(TK key)
        {
            return ForwardTo().ContainsKey(key);
        }

        /// <summary>
        ///   Copies the elements of the dictionary to an Array,
        ///   starting at the specified index.
        /// </summary>
        ///
        /// <param name="array">
        ///   The one-dimensional array that is the destination of the
        ///   elements copied. The array must have zero-based indexing.
        /// </param>
        /// <param name="index">
        ///   The zero-based index in the array at which copying begins.
        /// </param>
        ///
        /// <exception cref="System.ArgumentNullException">
        ///   If array == <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///   If index &lt; 0.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///   The number of elements in the dictionary is greater than the
        ///   available space from <c>index</c> to the end of the
        ///   destination array.
        /// </exception>
        public virtual void CopyTo(KeyValuePair<TK, TV>[] array, int index)
        {
            ForwardTo().CopyTo(array, index);
        }

        /// <summary>
        ///   Returns an enumerator that iterates through the collection.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   NOTE: If this method is overridden then an implicit override of
        ///   <c>System.Collections.IEnumerable.GetEnumerator()</c> is also
        ///   required for no generic calling in the sub-class.
        /// </para>
        /// </remarks>
        ///
        /// <returns>
        ///   An enumerator that iterates through the collection.
        /// </returns>
        public virtual IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            return ForwardTo().GetEnumerator();
        }

        /// <summary>
        ///   Removes the element with the specified key from the dictionary.
        /// </summary>
        ///
        /// <param name="key">
        ///   The key of the element to remove.
        /// </param>
        ///
        /// <returns>
        ///   <c>true</c> if the element is successfully removed;
        ///   otherwise <c>false</c>.
        /// </returns>
        ///
        /// <exception cref="System.ArgumentException">
        ///   If a forwarding implementation places additional constrains on
        ///   calls to this method.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///   if key == <c>null</c>
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        ///   If the collection is read-only.
        /// </exception>
        public virtual bool Remove(TK key)
        {
            return ForwardTo().Remove(key);
        }

        /// <summary>
        ///   Removes the element matching the key value pair from the dictionary.
        /// </summary>
        ///
        /// <param name="item">
        ///   The key value pair of the element to remove.
        /// </param>
        ///
        /// <returns>
        ///   <c>true</c> if item was successfully removed from the dictionary;
        ///   otherwise <c>false</c>.
        /// </returns>
        public virtual bool Remove(KeyValuePair<TK, TV> item)
        {
            return ForwardTo().Remove(item);
        }

        /// <summary>
        ///   Returns an enumerator that iterates through the collection.
        /// </summary>
        ///
        /// <returns>
        ///   An enumerator that iterates through the collection.
        /// </returns>
        /// 
        /// <remarks>
        ///   It is the responsibility of the caller to dispose of the 
        ///   returned enumerator.
        /// </remarks>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ForwardTo().GetEnumerator();
        }

        /// <summary>
        ///   Gets the value associated with the specified key.
        /// </summary>
        ///
        /// <param name="key">
        ///   The key whose value to get.
        /// </param>
        /// <param name="value">
        ///   When this method returns, the <c>value</c> associated with the
        ///   specified <c>key</c>, if the <c>key</c> is found;
        ///   otherwise the default <c>value</c> for the type of the
        ///   <c>value</c> parameter. This parameter is passed uninitialized.
        /// </param>
        ///
        /// <returns>
        ///   <c>true</c> if the object that implements dictionary contains
        ///   an element with the specified key; otherwise <c>false</c>.
        /// </returns>
        ///
        /// <exception cref="System.ArgumentException">
        ///   If a forwarding implementation places additional constrains on
        ///   calls to this method.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///   If key == <c>null</c>.
        /// </exception>
        public virtual bool TryGetValue(TK key, out TV value)
        {
            return ForwardTo().TryGetValue(key, out value);
        }

        #endregion Methods
    }
}