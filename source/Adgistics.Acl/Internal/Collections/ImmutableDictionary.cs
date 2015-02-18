namespace Modules.Acl.Internal.Collections
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///   An immutable dictionary. If calls are made to any method that
    ///   would modify the collection, for example <c>Add</c> then a
    ///   <c>NotSupportedException</c> is thrown.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    ///   The dictionary returned by calls to the <c>Of</c> methods are a
    ///   general dictionary, if a custom dictionary is passed into these
    ///   methods then any behavior in the custom dictionary is lost.
    ///   Casting from an <c>ImmutableDictionary</c> back to a custom
    ///   dictionary will also fail.
    /// </para>
    /// </remarks>
    ///
    /// <typeparam name="TK">
    ///   The type of keys in the collection.
    /// </typeparam>
    /// <typeparam name="TV">
    ///   The type of values in the collection.
    /// </typeparam>
    ///
    /// <since version="1.0"/>
    internal sealed class ImmutableDictionary<TK, TV> : ForwardingDictionary<TK, TV>
    {
        #region Fields

        /// <summary>
        ///   The internal backing collection to delegate to.
        /// </summary>
        private IDictionary<TK, TV> _collection;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///   Construct a new <c>ImmutableDictionary</c> empty dictionary.
        /// </summary>
        private ImmutableDictionary()
        {
            // Empty Block
        }

        /// <summary>
        ///   Construct a new <c>ImmutableDictionary</c> from the provide
        ///   dictionary.
        /// </summary>
        ///
        /// <param name="dictionary">
        ///   The dictionary to make immutable.
        /// </param>
        private ImmutableDictionary(IDictionary<TK, TV> dictionary)
        {
            // Copy the elements to a new internal collection
            _collection = new Dictionary<TK, TV>(dictionary);
        }

        /// <summary>
        ///   Construct a new <c>ImmutableDictionary</c> from the provide
        ///   dictionary.
        /// </summary>
        ///
        /// <param name="dictionary">
        ///   The dictionary to make immutable.
        /// </param>
        /// <param name="comparer">
        ///   The comparer to use for the dictionary.
        /// </param>
        private ImmutableDictionary(
            IDictionary<TK, TV> dictionary, IEqualityComparer<TK> comparer)
        {
            // Copy the elements to a new internal collection
            _collection = new Dictionary<TK, TV>(dictionary, comparer);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        ///   Returns <c>true</c> if and only if this collection is read only;
        ///   Otherwise <c>false</c>. As this collection is immutable calls
        ///   to this method always return <c>true</c>.
        /// </summary>
        public override bool IsReadOnly
        {
            get { return true; }
        }

        #endregion Properties

        #region Indexers

        /// <summary>
        ///   Call to this method always throw <c>NotSupportedException</c>
        ///   as this implementation is immutable.
        /// </summary>
        public override TV this[TK key]
        {
            get { return ForwardTo()[key]; }
            set { throw new NotSupportedException("Dictionary is immutable."); }
        }

        #endregion Indexers

        #region Methods

        /// <summary>
        ///   Returns an empty immutable dictionary.
        /// </summary>
        ///
        /// <returns>
        ///   An empty immutable dictionary.
        /// </returns>
        public static ImmutableDictionary<TK, TV> Empty()
        {
            return new ImmutableDictionary<TK, TV>(new Dictionary<TK, TV>());
        }

        /// <summary>
        ///   Create a new immutable view of the collection.
        /// </summary>
        ///
        /// <param name="dictionary">
        ///   The dictionary of elements to make immutable.
        /// </param>
        ///
        /// <returns>
        ///   An immutable dictionary.
        /// </returns>
        ///
        /// <exception cref="System.ArgumentException">
        ///   If dictionary == null
        /// </exception>
        public static ImmutableDictionary<TK, TV> Of(IDictionary<TK, TV> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentException(
                    "Argument 'dictionary' must not be null.");
            }

            return new ImmutableDictionary<TK, TV>(dictionary);
        }

        /// <summary>
        ///   Create a new immutable view of the collection.
        /// </summary>
        ///
        /// <param name="dictionary">
        ///   The dictionary of elements to make immutable.
        /// </param>
        /// <param name="comparer">
        ///   The comparer to use for equality.
        /// </param>
        ///
        /// <returns>
        ///   An immutable dictionary.
        /// </returns>
        ///
        /// <exception cref="System.ArgumentException">
        ///   If dictionary == null
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///   If comparer == null
        /// </exception>
        public static ImmutableDictionary<TK, TV> Of(
            IDictionary<TK, TV> dictionary, IEqualityComparer<TK> comparer)
        {
            if (dictionary == null)
            {
                throw new ArgumentException(
                    "Argument 'dictionary' must not be null.");
            }
            if (comparer == null)
            {
                throw new ArgumentException(
                    "Argument 'comparer' must not be null.");
            }

            return new ImmutableDictionary<TK, TV>(dictionary, comparer);
        }

        /// <summary>
        ///   Call to this method always throw <c>NotSupportedException</c>
        ///   as this implementation is immutable.
        /// </summary>
        public override void Add(TK key, TV value)
        {
            throw new NotSupportedException("Dictionary is immutable.");
        }

        /// <summary>
        ///   Call to this method always throw <c>NotSupportedException</c>
        ///   as this implementation is immutable.
        /// </summary>
        public override void Add(KeyValuePair<TK, TV> item)
        {
            throw new NotSupportedException("Dictionary is immutable.");
        }

        /// <summary>
        ///   Call to this method always throw <c>NotSupportedException</c>
        ///   as this implementation is immutable.
        /// </summary>
        public override void Clear()
        {
            throw new NotSupportedException("Dictionary is immutable.");
        }

        /// <summary>
        ///   Call to this method always throw <c>NotSupportedException</c>
        ///   as this implementation is immutable.
        /// </summary>
        public override bool Remove(TK key)
        {
            throw new NotSupportedException("Dictionary is immutable.");
        }

        /// <summary>
        ///   Call to this method always throw <c>NotSupportedException</c>
        ///   as this implementation is immutable.
        /// </summary>
        public override bool Remove(KeyValuePair<TK, TV> item)
        {
            throw new NotSupportedException("Dictionary is immutable.");
        }

        /// <summary>
        ///   Create a new unmodifiable view of the elements.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   Unmodifiable dictionaries differ from immutable dictionaries in
        ///   that unmodifiable dictionaries are a view of a live dictionary.
        ///   That is externally the dictionary cannot be modified but
        ///   internally the dictionary may be modified. This helps with memory
        ///   management and publishing updates but still have the benefits
        ///   of encapsulation.
        /// </para>
        /// </remarks>
        ///
        /// <param name="dictionary">
        ///   The dictionary to make unmodifiable.
        /// </param>
        ///
        /// <returns>
        ///   An unmodifiable dictionary.
        /// </returns>
        ///
        /// <exception cref="System.ArgumentException">
        ///   If dictionary == null
        /// </exception>
        internal static ImmutableDictionary<TK, TV> Unmodifiable(
            IDictionary<TK, TV> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentException(
                    "Argument 'dictionary' must not be null.");
            }

            var c = new ImmutableDictionary<TK, TV>();
            c._collection = dictionary;

            return c;
        }

        /// <summary>
        ///   Returns the base collection to delegate to.
        /// </summary>
        protected override IDictionary<TK, TV> ForwardTo()
        {
            return _collection;
        }

        #endregion Methods
    }
}