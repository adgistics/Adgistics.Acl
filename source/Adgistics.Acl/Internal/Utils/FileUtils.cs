namespace Modules.Acl.Internal.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Text;

    /// <summary>
    /// General file manipulation utilities.
    /// </summary>
    internal static class FileUtils
    {
        #region Fields

        public const int MaxPathLength = 248;

        private const int MaxFilenameLength = 260;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Checks the length of the directory is valid.
        /// </summary>
        /// <param name="directory">The directory to test.</param>
        /// <returns>true if the filename is valid; otherwise false</returns>
        public static bool CheckDirectoryLength(string directory)
        {
            return CheckLength(directory, MaxPathLength);
        }

        /// <summary>
        /// Checks the length of the filename is valid.
        /// </summary>
        /// <param name="file">The file to test.</param>
        /// <returns>true if the filename is valid; otherwise false</returns>
        public static bool CheckFilenameLength(string file)
        {
            return CheckLength(file, MaxFilenameLength);
        }

        /// <summary>
        /// Concatenates the specified base directory with a relative path.
        /// </summary>
        /// <param name="baseDirectory">The base directory.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <exception cref="ArgumentException">
        /// If baseDirectory == null
        /// If relativePath is null, only whitespace or an empty string
        /// </exception>
        /// <returns>A directory info object with the new path value</returns>
        public static DirectoryInfo Concatenate(DirectoryInfo baseDirectory, params string[] relativePath)
        {
            if (baseDirectory == null)
            {
                throw new ArgumentException(
                    "Argument 'baseDirectory' may not be null.");
            }

            var trimmedPath = CheckPathsAreValid(baseDirectory.FullName, relativePath);

            var path = PathExtensions.Combine(trimmedPath);

            if (CheckDirectoryLength(path) == false)
            {
                throw new ArgumentException(
                    string.Format("Path was greater than 248 characters actual: {0}", path.Length));
            }

            return new DirectoryInfo(path);
        }

        /// <summary>
        /// Concatenates the specified base directory with a relative path.
        /// </summary>
        /// <param name="baseDirectory">The base directory.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <exception cref="ArgumentException">
        /// If baseDirectory == null
        /// If relativePath is null, only whitespace or an empty string
        /// </exception>
        /// <returns>A file info object with the new path value</returns>
        public static FileInfo ConcatenateFile(DirectoryInfo baseDirectory, params string[] relativePath)
        {
            if (baseDirectory == null)
            {
                throw new ArgumentException(
                    "Argument 'baseDirectory' may not be null.");
            }

            var trimmedPath = CheckPathsAreValid(baseDirectory.FullName, relativePath);

            var fileName = PathExtensions.Combine(trimmedPath);

            if (CheckFilenameLength(fileName) == false)
            {
                throw new ArgumentException(
                    string.Format("Filename was greater than 260 characters actual: {0}", fileName.Length));
            }

            return new FileInfo(fileName);
        }

        /// <summary>
        /// This method copies the contents of the specified source file to the
        /// specified destination file.
        /// <para>
        /// This method exists primarly as a memory efficient copy rather than
        /// <see cref="FileInfo"/>.CopyTo()
        /// </para>
        /// </summary>
        /// <param name="src">an existing file to copy, must not be
        /// <code>null</code></param>
        /// <param name="dest">the destination to copy to, must not be
        /// <code>null</code></param>
        /// <exception cref="ArgumentException">
        /// If the src is null or does not exist If the dest is null or already
        /// exists or the destinations parent directory does not exist, is not
        /// writable or is hidden.
        /// </exception>
        /// <exception cref="IOException">
        /// if an IO error occurs during the copy
        /// </exception>
        public static void Copy(FileInfo src, FileInfo dest)
        {
            FilePreconditions.Check(src, FileConstraints.EXISTING);
            FilePreconditions.Check(dest, FileConstraints.NOT_EXISTING);
            FilePreconditions.Check(dest.Directory, FileConstraints.EXISTING, FileConstraints.WRITABLE, FileConstraints.NOT_HIDDEN);

            Stream input = src.OpenRead();
            Stream output = dest.OpenWrite();

            try
            {
                IOUtils.Copy(input, output);
            }
            finally
            {
                IOUtils.CloseQuietly(input);
                IOUtils.CloseQuietly(output);
            }
        }

        /// <summary>
        /// Deletes a directory, never throwing an exception.
        /// <p>
        /// The difference between DirectoryInfo.Delete() and this method are:
        ///     <ul>
        /// 		<li>No exceptions are thrown when a file or directory cannot be deleted.</li>
        ///         <li>All subdirectories are deleted underneath the current directory</li>
        /// 	</ul>
        /// </p>
        /// </summary>
        /// <param name="directory">directory to delete, can be <code>null</code></param>
        /// <returns>
        ///   <code>true</code> if the directory was deleted, otherwise
        ///   <code>false</code>
        /// </returns>
        public static bool Delete(DirectoryInfo directory)
        {
            bool result = false;

            if (directory != null && Exists(directory))
            {
                try
                {
                    directory.Delete(true);
                    result = true;
                }
                catch (IOException)
                {
                    // Nothing we can do return false.
                }
                catch (SecurityException)
                {
                    // Nothing we can do return false.
                }
                catch (UnauthorizedAccessException)
                {
                    // Nothing we can do return false.
                }
            }

            return result;
        }

        /// <summary>
        /// Deletes a file, never throwing an exception.
        /// <p>
        /// The difference between File.Delete() and this method are:
        ///     <ul>
        /// 		<li>No exceptions are thrown when a file or directory cannot be deleted.</li>
        /// 	</ul>
        /// </p>
        /// </summary>
        /// <param name="file">file to delete, can be <code>null</code></param>
        /// <returns>
        ///   <code>true</code> if the file was deleted, otherwise
        ///   <code>false</code>
        /// </returns>
        public static bool Delete(FileInfo file)
        {
            bool result = false;

            if (file != null && Exists(file))
            {
                try
                {
                    file.Delete();
                    result = true;
                }
                catch (IOException)
                {
                    // Nothing we can do return false.
                }
                catch (SecurityException)
                {
                    // Nothing we can do return false.
                }
                catch (UnauthorizedAccessException)
                {
                    // Nothing we can do return false.
                }
            }
            return result;
        }

        /// <summary>
        /// Escapes a supplied <paramref name="value"/> to contain only valid
        /// characters as determined by a call to
        /// <seealso cref="IsValidFilename" />.
        /// </summary>
        ///
        /// <param name="value">
        /// The value to escape.
        /// </param>
        ///
        /// <returns>
        /// Returns the escaped string.
        /// </returns>
        ///
        /// <exception cref="ArgumentException">
        /// If the supplied <paramref name="value"/> is null, whitespace only, or empty.
        /// </exception>
        public static string Escape(string value)
        {
            var map = new Dictionary<char, string>();
            return Escape(value, map);
        }

        /// <summary>
        /// Escapes a supplied <paramref name="value"/> to contain only valid
        /// characters as determined by a call to
        /// <seealso cref="IsValidFilename" />.
        ///
        /// A dictionary can be supplied to replace the invalid characters determined
        /// by the <seealso cref="CheckChar"/> with an alternative.
        /// </summary>
        ///
        /// <param name="value">
        ///  The value to escape.
        /// </param>
        ///
        /// <param name="map">
        /// A map that replaces the invalid characters determined
        /// by the <seealso cref="CheckChar"/> with an alternative if they are
        /// present within the map.
        /// </param>
        ///
        /// <returns>
        /// Returns the escaped string.
        /// </returns>
        ///
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> is <see langword="null"/>, whitespace
        /// only, or empty.
        /// </exception>
        ///
        /// <exception cref="ArgumentException">
        /// If <paramref name="map"/> is <see langword="null"/>.
        /// </exception>
        public static string Escape(string value, IDictionary<char, string> map)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Argument: 'value' must not be null, whitespace only, or empty.");
            }
            if (map == null)
            {
                throw new ArgumentException(
                    "Argument 'map' may not be null.");
            }

            var stringBuilder = new StringBuilder();
            map.Add('@', "_at_");

            foreach (var chr in value)
            {
                if (CheckChar(chr))
                {
                    stringBuilder.Append(chr);
                }
                else if (map.Keys.Contains(chr))
                {
                    var keyValue = map[chr];
                    stringBuilder.Append(keyValue);
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Determines whether the file or directory info passed in exists
        /// <p>
        /// This method is the same as calling:
        /// <pre>
        ///   fileInfo.Refresh();
        ///   fileInfo.Exists;
        /// </pre>
        /// And only exists for convenience
        /// </p>
        /// </summary>
        /// <param name="fileInfo">The file or directory info.</param>
        /// <returns>true if the file or directory exits; otherwise false</returns>
        /// <exception cref="NullReferenceException">If (fileInfo == <see langword="null"/>)</exception>
        public static bool Exists(FileSystemInfo fileInfo)
        {
            fileInfo.Refresh();
            return fileInfo.Exists;
        }

        /// <summary>
        /// Asserts that a string only contains the following characters (and no
        /// extension): is a letter, digit, underscore, hyphen, left square
        /// bracket, right square bracket, left parenthesis or right
        /// parenthesis.
        /// </summary>
        ///
        /// <param name="filename">
        /// The file name to check.
        /// </param>
        ///
        /// <returns>
        /// <see langword="true"/>  if valid, <see langword="false"/> if not.
        /// </returns>
        public static bool IsValidFilename(string filename)
        {
            var result = true;

            if (false == string.IsNullOrWhiteSpace(filename))
            {
                if (filename.Any(chr => CheckChar(chr) == false))
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Makes a directory, including any necessary but nonexistent
        /// parent directories. If there already exists a file with the
        /// specified name or the directory cannot be created then an
        /// exception is thrown.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Returns <code>true</code> if the directory was created</returns>
        /// <exception cref="ArgumentException">if the directory cannot be created</exception>
        public static bool Mkdir(DirectoryInfo path)
        {
            var result = Exists(path);
            if (result == false)
            {
                try
                {
                    path.Create();
                    result = Exists(path);

                    if (result == false)
                    {
                        throw new ArgumentException("Unable to create directory '{0}'", path.FullName);
                    }
                }
                catch (IOException)
                {
                    throw new ArgumentException("Unable to create directory '{0}'", path.FullName);
                }
            }

            return result;
        }

        /// <summary>
        /// Makes a directory, including any necessary but nonexistent
        /// parent directories. If there already exists a file with the
        /// specified name or the directory cannot be created then an
        /// exception is thrown.
        /// </summary>
        /// 
        /// <param name="pathfragments">
        ///  The path fragments that get concatenated as if by a call to Path.Combine(). 
        /// </param>
        /// 
        /// <returns><para>Returns <c>true</c> if the directory was created.</para></returns>
        /// 
        /// <exception cref="ArgumentException">If the directory cannot be created.</exception>
        /// 
        /// <exception cref="ArgumentException">
        /// if pathfragments == null or pathfragments.Length == 0
        /// </exception>
        public static bool Mkdir(params string[] pathfragments)
        {
            if (pathfragments == null)
            {
                throw new ArgumentException(
                    "Argument: 'pathfragments' may not be null");
            }

            if (false == pathfragments.Any())
            {
                throw new ArgumentException(
                    "Argument: 'pathfragments' must contain at least one fragment.");
            }

            if (pathfragments.Length == 0)
            {
                throw new ArgumentException(
                    "Argument: 'pathfragments' must contain at least one fragment.");
            }

            var builder = new StringBuilder();
            foreach (var fragment in pathfragments)
            {
                builder.Append(Path.Combine(builder.ToString(), fragment));
            }

            var dirInfo = new DirectoryInfo(builder.ToString());
            return Mkdir(dirInfo);
        }

        /// <summary>
        /// Makes a directory, including any necessary but nonexistent
        /// parent directories. If there already exists a file with the
        /// specified name or the directory cannot be created then an
        /// exception is thrown.
        /// </summary>
        /// 
        /// <param name="path">
        /// The root path to start the construction from.
        /// </param>
        /// 
        /// <param name="pathfragments">
        ///  The path fragments that get concatenated as if by a call to Path.Combine(). 
        /// </param>
        /// 
        /// <returns><para>Returns <c>true</c> if the directory was created.</para></returns>
        /// 
        /// <exception cref="ArgumentException">If path is a hidden, does not exist, a directory or is not writable .</exception>
        /// 
        /// <exception cref="ArgumentException">If the directory cannot be created.</exception>
        /// 
        /// <exception cref="ArgumentException">
        /// if pathfragments == null or pathfragments.Length == 0
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// if pathfragments == null or pathfragments.Length == 0
        /// </exception>
        public static bool Mkdir(DirectoryInfo path, params string[] pathfragments)
        {
            if (path == null)
            {
                throw new ArgumentException("Argument: 'path' may not be null.");
            }

            FilePreconditions.Check(
                path, FileConstraints.EXISTING,
                FileConstraints.NOT_HIDDEN,
                FileConstraints.IS_A_DIRECTORY,
                FileConstraints.WRITABLE);

            if (pathfragments == null)
            {
                throw new ArgumentException(
                    "Argument: 'pathfragments' may not be null");
            }

            if (false == pathfragments.Any())
            {
                throw new ArgumentException(
                    "Argument: 'pathfragments' must contain at least one fragment.");
            }

            if (pathfragments.Length == 0)
            {
                throw new ArgumentException(
                    "Argument: 'pathfragments' must contain at least one fragment.");
            }

            int length = pathfragments.Length + 1;

            var args = new string[length];

            args[0] = path.FullName;
            Array.Copy(pathfragments, 0, args, 1, length);
            return Mkdir(args);
        }

        /// <summary>
        /// This method moves the specified source file to the specified
        /// destination file location.
        /// </summary>
        /// <param name="src">an existing file to move, must not be
        /// <code>null</code></param>
        /// <param name="dest">the destination to move to, must not be
        /// <code>null</code></param>
        /// <exception cref="ArgumentException">
        /// If the src is null or does not exist If the dest is null or already
        /// exists or the destinations parent directory does not exist, is not
        /// writable or is hidden.
        /// </exception>
        /// <exception cref="IOException">
        /// if an IO error occurs during the move
        /// </exception>
        public static void Move(FileInfo src, FileInfo dest)
        {
            FilePreconditions.Check(src, FileConstraints.EXISTING);
            FilePreconditions.Check(dest, FileConstraints.NOT_EXISTING);
            FilePreconditions.Check(dest.Directory,
                FileConstraints.EXISTING,
                FileConstraints.WRITABLE,
                FileConstraints.NOT_HIDDEN);

            try
            {
                src.MoveTo(dest.FullName);
            }
            catch (SecurityException e)
            {
                throw new IOException("Security exception", e);
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if and only if
        /// <paramref name="chr"/> is a letter, digit, underscore or hyphen. This
        /// implementation uses UNICODE  safe determination of letters and
        /// digits; in other words calls to this method are language safe.
        /// </summary>
        ///
        /// <param name="chr">
        /// The <see cref="char" /> to test.
        /// </param>
        ///
        /// <returns>
        /// <see langword="true"/> if and only if
        /// <paramref name="chr"/> is a letter, digit, underscore or hyphen.
        /// </returns>
        private static bool CheckChar(char chr)
        {
            var result = char.IsLetterOrDigit(chr)
                          || ('_' == chr)
                          || ('-' == chr)
                          || ('[' == chr)
                          || (']' == chr)
                          || ('(' == chr)
                          || (')' == chr)
                          || (' ' == chr);

            return result;
        }

        private static bool CheckLength(string input, int maximum)
        {
            return input.Length <= maximum;
        }

        private static string[] CheckPathsAreValid(string baseDirectory, params string[] relativePath)
        {
            if (relativePath.Length == 0)
            {
                throw new ArgumentException(
                    "At least one argument must be supplied as a parameter.");
            }

            //pre streching array to avoid re-allocation.
            var output = new List<string>((relativePath.Length / 3) + relativePath.Length);

            output.Add(baseDirectory);

            foreach (var path in relativePath)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentException(
                        "An Argument 'relativePath' contains a string which is " +
                        "null, only whitespace or an empty string.");
                }

                output.Add(path.Trim());
            }

            return output.ToArray();
        }

        #endregion Methods
    }
}