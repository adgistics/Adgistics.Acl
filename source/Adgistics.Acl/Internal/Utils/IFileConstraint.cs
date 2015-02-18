namespace Modules.Acl.Internal.Utils
{
    using System.IO;

    /// <summary>
    /// A file constraint to apply for filtering file system objects.
    /// </summary>
    internal interface IFileConstraint
    {
        #region Properties

        string Name
        {
            get;
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
        bool Is(FileSystemInfo fileSystemInfo);

        #endregion Methods
    }
}