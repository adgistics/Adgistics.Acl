namespace Modules.Acl.Internal.Utils
{
    using System;
    using System.IO;

    ///<summary>
    ///FilePreconditions performs a test on a File and throws an exception if a
    ///condition is not met. This class is primarily designed to reduce the
    ///amount of boiler plate code needed to test a File before use.
    ///</summary>
    ///<example>
    ///Example for testing a directory:
    /// <code>
    ///if (file == null) {
    ///     throw new NullReferenceException("file may not be null");
    /// }
    ///if (((file.Attributes &amp; FileAttributes.Directory) == FileAttributes.Directory) == false) {
    ///     throw new ArgumentException("file must be a directory");
    /// }
    ///if (!file.Exists) {
    ///     throw new ArgumentException("file must exist");
    /// }
    ///if (((file.Attributes &amp; FileAttributes.ReadOnly) == FileAttributes.ReadOnly) == false) {
    ///     throw new ArgumentException("file must be writable");
    /// }
    /// </code>
    ///  With this class:
    /// <code>
    /// FilePreconditions.Check(file, EXISTING, A_DIRECTORY, WRITABLE);
    /// </code>
    ///</example>
    internal static class FilePreconditions
    {
        #region Methods

        /// <summary>
        /// Checks the file constraints apply to the supplied file system object
        /// <p>
        /// NOTE: This implementation assumes that a file that is READ only is not
        /// writable in any way, other implementations may have a better
        /// mechanism for calculating this result.
        /// </p>
        /// </summary>
        /// <param name="fileInfo">The file system object.</param>
        /// <param name="constraints">The file constraints to test.</param>
        /// <returns>true if all the constrains are met otherwise; an
        /// <see cref="ArgumentException"/> is thrown</returns>
        /// <exception cref="ArgumentException">If any constraint is not met
        /// </exception>
        public static bool Check(FileSystemInfo fileInfo, params IFileConstraint[] constraints)
        {
            if (fileInfo == null)
            {
                throw new ArgumentException(
                    "Argument 'fileInfo' may not be null.");
            }

            IFileConstraint failedConstraint;
            var meetsConstraints = Is(fileInfo, constraints, out failedConstraint);

            if (false == meetsConstraints)
            {
                throw new ArgumentException(
                    string.Format(
                        "Argument: 'fileInfo' does not meet file constraint. fileInfo:{0}, Constraint:{1}",
                        fileInfo.FullName,
                        failedConstraint != null ?
                            failedConstraint.Name : string.Empty));
            }

            return true;
        }

        /// <summary>
        /// Checks the file constraints apply to the supplied file system object
        /// <p>
        /// NOTE: This implementation assumes that a file that is READ only is not
        /// writable in any way, other implementations may have a better
        /// mechanism for calculating this result.
        /// </p>
        /// </summary>
        /// <param name="info">The file system object.</param>
        /// <param name="constraints">The file constraints to test.</param>
        /// <returns>true if all the constraints are met otherwise; <see langword="false"/></returns>
        public static bool Is(FileSystemInfo info, params IFileConstraint[] constraints)
        {
            IFileConstraint failed;
            return Is(info, constraints, out failed);
        }

        /// <summary>
        /// Checks the file constraints apply to the supplied file system object, and sets the <paramref name="failed"/>
        /// parameter with constraint that failed should any fail.
        /// <p>
        /// NOTE: This implementation assumes that a file that is READ only is not
        /// writable in any way, other implementations may have a better
        /// mechanism for calculating this result.
        /// </p>
        /// </summary>
        /// <param name="info">The file system object.</param>
        /// <param name="constraints">The file constraints to test.</param>
        /// <param name="failed">The file constraint that was not met, if result is <see langword="false"/></param>
        /// <returns>true if all the constraints are met otherwise; <see langword="false"/></returns>
        /// <remarks>
        ///   If result is <see langword="true"/>, then the <paramref name="failed"/> will be set us null.
        /// </remarks>
        public static bool Is(FileSystemInfo info, IFileConstraint[] constraints, out IFileConstraint failed)
        {
            var result = true;
            failed = null;

            // This value must be re-calculate on entry
            info.Refresh();

            foreach (var constraint in constraints)
            {
                if (constraint.Is(info) == false)
                {
                    result = false;
                    failed = constraint;
                    break;
                }
            }

            return result;
        }

        #endregion Methods
    }
}