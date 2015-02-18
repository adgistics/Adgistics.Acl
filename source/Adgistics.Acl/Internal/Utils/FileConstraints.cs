namespace Modules.Acl.Internal.Utils
{
    using System.IO;

    /// <summary>
    /// File filters to be used with <see cref="FilePreconditions"/>.
    /// </summary>
    internal static class FileConstraints
    {
        #region Fields

        /// <summary>
        /// The file system object exists
        /// </summary>
        public static readonly IFileConstraint EXISTING = new Existing("EXISTING", false);

        /// <summary>
        /// The file system object is hidden
        /// </summary>
        public static readonly IFileConstraint HIDDEN = new AttributeCheck("HIDDEN", false, FileAttributes.Hidden);

        /// <summary>
        /// The file system object is a directory
        /// </summary>
        public static readonly IFileConstraint IS_A_DIRECTORY = new AttributeCheck("IS_A_DIRECTORY", false, FileAttributes.Directory);

        /// <summary>
        /// The file system object is a not directory
        /// </summary>
        public static readonly IFileConstraint IS_NOT_A_DIRECTORY = new AttributeCheck("IS_NOT_A_DIRECTORY", true, FileAttributes.Directory);

        /// <summary>
        /// The file system object does not exist
        /// </summary>
        public static readonly IFileConstraint NOT_EXISTING = new Existing("NOT_EXISTING", true);

        /// <summary>
        /// The file system object is not hidden
        /// </summary>
        public static readonly IFileConstraint NOT_HIDDEN = new AttributeCheck("NOT_HIDDEN", true, FileAttributes.Hidden);

        /// <summary>
        /// The file system object is writable
        /// </summary>
        public static readonly IFileConstraint NOT_WRITABLE = new AttributeCheck("NOT_WRITABLE", false, FileAttributes.ReadOnly);

        /// <summary>
        /// The file system object writable
        /// </summary>
        public static readonly IFileConstraint WRITABLE = new AttributeCheck("WRITABLE", true, FileAttributes.ReadOnly);

        #endregion Fields

        #region Nested Types

        /// <summary>
        /// Constraint to check a given attribute on a file system object.
        /// </summary>
        internal class AttributeCheck : BaseFileConstraint
        {
            #region Fields

            /// <summary>
            /// The attribute to check.
            /// </summary>
            private readonly FileAttributes fileAttributes;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="AttributeCheck"/>
            /// class.
            /// </summary>
            /// <param name="name">The name of the constraint.</param>
            /// <param name="invertResult">if set to <c>true</c> the result is
            /// inverted.</param>
            /// <param name="attributes">The attribute to check.</param>
            public AttributeCheck(string name, bool invertResult, FileAttributes attributes)
                : base(name, invertResult)
            {
                this.fileAttributes = attributes;
            }

            #endregion Constructors

            #region Methods

            /// <summary>
            /// The Operation to perform.
            /// <para>
            /// Returns <see langword="true"/> if the operation can be
            /// performed; otherwise <see langword="false"/>
            /// </para>
            /// </summary>
            /// <param name="info">The object to test.</param>
            /// <returns>
            /// <see langword="true"/> if the operation can be performed.
            /// </returns>
            internal override bool Operation(FileSystemInfo info)
            {
                return ((info.Attributes & this.fileAttributes) == this.fileAttributes);
            }

            #endregion Methods
        }

        /// <summary>
        /// Base class for file constraints
        /// </summary>
        internal abstract class BaseFileConstraint : IFileConstraint
        {
            #region Fields

            private readonly bool invertResult;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the
            /// <see cref="BaseFileConstraint"/> class.
            /// </summary>
            /// <example>
            /// Constraints are normally implemented as pairs: e.g.
            /// <c>
            /// Exists();
            /// NotExist();
            /// </c>
            ///
            /// To ease implementation of these pairs the
            /// <paramref name="invertResult"/> argument can be used to flip the
            /// result of a call to Operation so that a single constraint can
            /// serve both <see langword="true"/> and <see langword="false"/>
            /// results.
            ///
            /// <c>
            /// IFileConstraint Exists = new Existing(false);
            /// IFileConstraint NotExist = new Existing(true);
            /// </c>
            /// </example>
            /// <param name="name">The name of the constraint.</param>
            /// <param name="invertResult">if set to <c>true</c> the result is
            /// inverted.</param>
            protected BaseFileConstraint(string name, bool invertResult)
            {
                this.invertResult = invertResult;
                this.Name = name;
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Gets the name of the constraint.
            /// </summary>
            public string Name
            {
                get; private set;
            }

            #endregion Properties

            #region Methods

            /// <summary>
            /// Determines whether the file system object meets a criteria.
            /// </summary>
            /// <param name="fileSystemInfo">The file system info to test.</param>
            /// <returns>
            ///   <c>true</c> if the criteria is met; otherwise, <c>false</c>.
            /// </returns>
            public bool Is(FileSystemInfo fileSystemInfo)
            {
                var outcome = this.Operation(fileSystemInfo);
                if (this.invertResult)
                {
                    return !outcome;
                }
                return outcome;
            }

            public override string ToString()
            {
                return string.Format("Name: {0}", this.Name);
            }

            /// <summary>
            /// The Operation to perform.
            /// <para>
            /// Returns <see langword="true"/> if the operation can be
            /// performed; otherwise <see langword="false"/>
            /// </para>
            /// </summary>
            /// <param name="fileSystemInfo">The file system info.</param>
            /// <returns>true if the operation can be performed </returns>
            internal abstract bool Operation(FileSystemInfo fileSystemInfo);

            #endregion Methods
        }

        /// <summary>
        /// Constraint to check whether a file system object exists
        /// </summary>
        internal class Existing : BaseFileConstraint
        {
            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Existing"/> class.
            /// </summary>
            /// <param name="name">The name of the constraint.</param>
            /// <param name="invertResult">if set to <c>true</c> result is
            /// inverted.</param>
            public Existing(string name, bool invertResult)
                : base(name, invertResult)
            {
            }

            #endregion Constructors

            #region Methods

            /// <summary>
            /// The Operation to perform.
            /// <para>
            /// Returns <see langword="true"/> if the operation can be
            /// performed; otherwise <see langword="false"/>
            /// 	</para>
            /// </summary>
            /// <param name="info">The object to test.</param>
            /// <returns>
            /// true if the operation can be performed
            /// </returns>
            internal override bool Operation(FileSystemInfo info)
            {
                info.Refresh();
                return info.Exists;
            }

            #endregion Methods
        }

        #endregion Nested Types
    }
}