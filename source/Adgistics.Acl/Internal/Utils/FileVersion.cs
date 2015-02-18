namespace Modules.Acl.Internal.Utils
{
    using System;
    using System.IO;
    using System.Threading;

    /// <summary>
    ///   <para>
    ///     Provides a simple form of versioning back-up for files.
    ///   </para>
    /// </summary>
    /// 
    /// <remarks>
    ///   <para>
    ///     The version are saved in the following format:
    ///   </para>
    ///   <code>
    ///     __FILEVERSIONS/{filename}.{ext}/{year}/{month}/{day}/{hours}h{minutes}m{seconds}s_{milliseconds}.{ext}
    /// 
    ///     e.g.
    ///     __FILEVERSIONS/MyDocument.doc/2013/12/31/20h51m59s_1232563.doc
    ///   </code>
    ///   
    ///   <para>
    ///     Below is an example folder structure:
    ///   </para>
    ///   <code>
    ///     __FILEVERSIONS
    ///         |
    ///         +---Excel.xls
    ///         |   |
    ///         |   +---2012
    ///         |   |   |
    ///         |   |   +---8
    ///         |   |       |
    ///         |   |       +---1
    ///         |   |       |     08h53m01s_9343346.xls
    ///         |   |       |     16h05m22s_3484563.xls
    ///         |   |       |
    ///         |   |       +---11
    ///         |   |       |     01h11m59s_8235123.xls
    ///         |   |       |
    ///         |   |       +---31
    ///         |   |             11h55m59s_1746643.xls
    ///         |   |
    ///         |   +---2013
    ///         |       |
    ///         |       +---7
    ///         |           |
    ///         |           +---1
    ///         |                 09h12m44s_1837745.xls
    ///         |                 11h55m59s_8348245.xls
    ///         |
    ///         +---MyDocument.Doc
    ///             |
    ///             +---2013
    ///                 |
    ///                 +---10
    ///                     |  
    ///                     +---23
    ///                           12h03m16s_6543135.doc
    ///   </code>
    /// </remarks>
    /// 
    /// <example>
    ///   <para>
    ///     The following code:
    ///   </para>
    /// 
    ///   <code>
    ///     var sourceFile = new FileInfo("C:\bob\test.txt"); // This exists on disk.
    ///     var baseDirectory = new DirectoryInfo("c:\myBackup"); // This also exists on disk.
    ///     
    ///     FileVersion.Create(sourceFile, baseDirectory);
    ///   </code>
    ///    
    ///   <para>
    ///     Will produce a backup file at this location: 
    ///     c:\myBackup\__FILEVERSIONS\test.txt\2013\08\15\16h06m59s_123.txt
    ///   </para>
    /// </example>
    internal static class FileVersion
    {
        #region Fields

        private const string FilenameTimestampFormat = "HH'h'mm'm'ss's_'fffffff";
        private const string VersionDirectoryFormat = "{0}/{1}/{2}/{3}/{4}";
        private const string VersionsDirectoryName = "__FILEVERSIONS";

        private static readonly object TimestampLocker = new object();

        #endregion Fields

        #region Methods

        /// <summary>
        ///   Creates a file version of the given file within a __FILEVERSIONS directory that will live in the 
        ///   same directory as the given file.
        /// </summary>
        /// 
        /// <param name="sourceFile">
        ///   The file to version. This file must exist on disk and be accessible.
        /// </param>
        /// 
        /// <exception cref="ArgumentException">
        ///   <para>If <paramref name="sourceFile"/> is null.</para>
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        ///   If <paramref name="sourceFile"/> does not meet all of the following constraints:
        ///   <para>
        ///       FileConstraints.EXISTING, 
        ///       FileConstraints.NOT_HIDDEN, 
        ///       FileConstraints.IS_NOT_A_DIRECTORY
        ///   </para>
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        ///    If <paramref name="sourceFile"/>'s directory does not meet all of the following constraints:
        ///    <para>
        ///        FileConstraints.EXISTING, 
        ///        FileConstraints.NOT_HIDDEN, 
        ///        FileConstraints.IS_A_DIRECTORY
        ///        FileConstraints.WRITABLE
        ///    </para>
        /// </exception>
        /// 
        /// <exception cref="ApplicationException">
        ///   If any failure occurs while attempting to create the file version.
        /// </exception>
        /// 
        /// <returns>
        ///    true if and only if the operation was successful; otherwise false.
        /// </returns>
        public static bool Create(FileInfo sourceFile)
        {
            return Create(sourceFile, sourceFile.Directory);
        }

        /// <summary>
        ///   Creates a file version of the given file within the __FILEVERSIONS 
        ///   directory at the given directory.
        /// </summary>
        /// 
        /// <param name="sourceFile">
        ///   The file to version. This file must exist on disk and be accessible.
        /// </param>
        /// 
        /// <param name="baseDirectory">
        ///   This is the root directory of where the version files are to be placed. 
        ///   The directory must exist, be accessible and writeable. 
        /// </param>
        /// 
        /// <exception cref="ArgumentException">
        ///   <para>If <paramref name="sourceFile"/> is null.</para>
        ///   OR
        ///   <para>If <paramref name="baseDirectory"/> is null.</para>
        ///   OR
        ///   <para>Argument 'sourceFile' does not exist</para>
        ///   OR
        ///   <para>Argument 'baseDirectory' does not exist</para>
        /// </exception>                
        /// 
        /// <exception cref="ArgumentException">
        ///   If <paramref name="sourceFile"/> does not meet all of the following constraints:
        ///   <para>
        ///       FileConstraints.EXISTING, 
        ///       FileConstraints.NOT_HIDDEN, 
        ///       FileConstraints.IS_NOT_A_DIRECTORY
        ///   </para>
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        ///    If <paramref name="baseDirectory"/> does not meet all of the following constraints:
        ///    <para>
        ///        FileConstraints.EXISTING, 
        ///        FileConstraints.NOT_HIDDEN, 
        ///        FileConstraints.IS_A_DIRECTORY
        ///        FileConstraints.WRITABLE
        ///    </para>
        /// </exception>
        /// 
        /// <exception cref="ApplicationException">
        ///   If any failure occurs while attempting to create the file version.
        /// </exception>
        /// 
        /// <returns>
        ///    true if and only if the operation was successful; otherwise false.
        /// </returns>
        public static bool Create(FileInfo sourceFile, DirectoryInfo baseDirectory)
        {
            if (sourceFile == null)
            {
                throw new ArgumentException(
                    "Argument 'sourceFile' must not be null.");
            }
            if (baseDirectory == null)
            {
                throw new ArgumentException(
                    "Argument 'baseDirectory' must not be null.");
            }

            FilePreconditions.Check(
                sourceFile,
                FileConstraints.EXISTING,
                FileConstraints.NOT_HIDDEN,
                FileConstraints.IS_NOT_A_DIRECTORY);

            FilePreconditions.Check(
                baseDirectory,
                FileConstraints.EXISTING,
                FileConstraints.NOT_HIDDEN,
                FileConstraints.IS_A_DIRECTORY,
                FileConstraints.WRITABLE);

            DateTime datetimeStamp;

            lock (TimestampLocker)
            {
                Thread.Sleep(1);

                datetimeStamp = DateTime.Now;
            }

            var result = CreateFileVersion(sourceFile, baseDirectory, datetimeStamp);

            return result;
        }

        private static bool CreateFileVersion(
            FileInfo sourceFile,
            DirectoryInfo baseDirectory,
            DateTime datetimeStamp)
        {
            var result = true;

            var timestampFilename = datetimeStamp.ToString(FilenameTimestampFormat);

            var versionFilename = string.Concat(timestampFilename, sourceFile.Extension);

            try
            {
                DirectoryInfo baseVersionDirectory = EnsureFileVersionDirectoryExists(
                    sourceFile, baseDirectory, datetimeStamp);

                FileInfo versionFile =
                    FileUtils.ConcatenateFile(baseVersionDirectory,
                        versionFilename);

                if (versionFile.Exists)
                {
                    throw new ApplicationException(
                        string.Format(
                            "Target version file already exits {0}.",
                            versionFile));
                }

                FileUtils.Copy(sourceFile, versionFile);
            }
            catch (ArgumentException exception)
            {
                var message = string.Format("Failed to create file version.");

                throw new ApplicationException(message, exception);
            }
            catch (ApplicationException exception)
            {
                var message = string.Format("Failed to create file version.");

                throw new ApplicationException(message, exception);
            }
            catch (Exception exception)
            {
                var message = string.Format("Failed to create file version.");

                throw new ApplicationException(message, exception);
            }

            return result;
        }

        private static DirectoryInfo EnsureFileVersionDirectoryExists(
            FileSystemInfo sourceFile,
            DirectoryInfo baseDirectory,
            DateTime datetimeStamp)
        {
            var sourceFileName = Path.GetFileName(sourceFile.FullName);

            var versionDirectotyPath = string.Format(
                VersionDirectoryFormat,
                VersionsDirectoryName,
                sourceFileName,
                datetimeStamp.Year,
                datetimeStamp.Month,
                datetimeStamp.Day);

            var pathInfo = FileUtils.Concatenate(baseDirectory, versionDirectotyPath);

            if (FileUtils.Exists(pathInfo) == false)
            {
                Directory.CreateDirectory(pathInfo.ToString());
            }

            return pathInfo;
        }

        #endregion Methods
    }
}