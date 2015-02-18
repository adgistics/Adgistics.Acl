namespace Modules.Acl.Internal.Utils
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    /// <summary>
    /// General IO stream manipulation utilities.
    /// </summary>
    internal static class IOUtils
    {
        #region Fields

        /// <summary>
        /// The default buffer size to use.
        /// </summary>
        private const int BUFFERSIZE = 4 * 1024; // 4k

        #endregion Fields

        #region Methods

        /// <summary>
        /// Unconditionally close a stream.
        /// <para>
        /// Equivalent to Stream.Close(), except any exceptions will be ignored.
        /// This is typically used in finally blocks.
        /// </para>
        /// </summary>
        /// <param name="stream">the Stream to close, may be
        /// <see langword="null"/> or already closed</param>
        public static void CloseQuietly(Stream stream)
        {
            try
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                // ignore
            }
        }

        /// <summary>
        /// Copy bytes from a large (over 2GB) readable Stream to an writable
        /// Stream.
        /// <p>
        /// This method buffers the input internally, so there is no need to use
        /// a <see cref="BufferedStream"/>.
        /// </p>
        /// </summary>
        /// <param name="input">the Stream to read from. </param>
        /// <param name="output">the Stream to write to. </param>
        /// <returns>the number of bytes copied.</returns>
        /// <exception cref="NullReferenceException">if the input or output is
        /// <see langword="null"/>.</exception>
        /// <exception cref="IOException">if an I/O error occurs.</exception>
        public static long Copy(Stream input, Stream output)
        {
            var buffer = new byte[BUFFERSIZE];
            long count = 0;
            int n;

            while ((n = input.Read(buffer, 0, BUFFERSIZE)) != 0)
            {
                output.Write(buffer, 0, n);
                count += n;
                output.Flush();
            }

            output.Flush();
            return count;
        }

        /// <summary>
        /// Copy bytes from a uri to an writable Stream.
        /// <p>
        /// This method buffers the input internally, so there is no need to use
        /// a <see cref="BufferedStream"/>.
        /// </p>
        /// </summary>
        /// <param name="input">the uri endpoint to create a readable stream from.</param>
        /// <param name="output">the Stream to write to.</param>
        /// <returns>the number of bytes copied.</returns>
        /// <exception cref="NullReferenceException">if the input or output is
        /// <see langword="null"/>.</exception>
        /// <exception cref="IOException">if an I/O error occurs.</exception>
        public static long Copy(Uri input, Stream output)
        {
            if (input == null)
            {
                throw new ArgumentException("Argument 'input' must not be null");
            }
            if (output == null)
            {
                throw new ArgumentException("Argument 'output' must not be null");
            }

            var request = WebRequest.CreateDefault(input);

            using (var response = request.GetResponse())
            {
                Stream inputStream;
                try
                {
                    inputStream = response.GetResponseStream();
                }
                catch (NotSupportedException ex)
                {
                    throw new IOException(ex.Message, ex);
                }
                catch (NullReferenceException ex)
                {
                    throw new IOException(ex.Message, ex);
                }
                return Copy(inputStream, output);
            }
        }

        /// <summary>
        /// Permanently delete the file from the file system. 
        /// </summary>
        /// 
        /// <param name="fileInfo">
        /// The file to delete.
        /// </param>
        /// 
        /// <exception cref="ArgumentException">
        ///  <paramref name="fileInfo"/> may not be null.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// <paramref name="fileInfo"/> must be a file.
        /// </exception>
        /// 
        /// <exception cref="ArgumentException">
        /// <paramref name="fileInfo"/> must exist.
        /// </exception>
        public static void DeleteFile(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentException("Argument 'fileInfo' must not be null.");
            }

            FilePreconditions.Check(
                fileInfo,
                FileConstraints.IS_NOT_A_DIRECTORY,
                FileConstraints.EXISTING);

            fileInfo.Delete();
        }

        /// <summary>
        /// Reads the file to a string.
        /// </summary>
        /// <param name="fileInfo">The file to read from.</param>
        /// <returns>Contents of the file as a string.</returns>
        /// <exception cref="ArgumentException">If the fileInfo is invalid.</exception>
        /// <exception cref="IOException">If any IO error occurs.</exception>
        public static string ReadFileToString(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentException("Argument 'fileInfo' must not be null.");
            }
            if (false == FileUtils.Exists(fileInfo))
            {
                throw new ArgumentException(
                    "Argument 'fileInfo' does not exist.");
            }

            try
            {
                return File.ReadAllText(fileInfo.FullName);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new IOException("Error reading file to string.", ex);
            }
        }

        /// <summary>
        /// Writes the string to a file using UTF-8 Encoding.
        /// </summary>
        /// <param name="fileInfo">The file to write to.</param>
        /// <param name="text">The string text.</param>
        public static void WriteStringToFile(FileInfo fileInfo, string text)
        {
            WriteStringToFile(fileInfo, text, Encoding.UTF8);
        }

        /// <summary>
        /// Writes the string to a file.
        /// </summary>
        /// <param name="fileInfo">The file to write to.</param>
        /// <param name="text">The string text.</param>
        /// <param name="encoding">The encoding.</param>
        public static void WriteStringToFile(FileInfo fileInfo, string text, Encoding encoding)
        {
            WriteStringToFile(fileInfo, text, encoding, false);
        }

        /// <summary>
        /// Writes the string to a file.
        /// </summary>
        /// <param name="fileInfo">The file to write to.</param>
        /// <param name="text">The string text.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="append">Appends the text to the end of the file.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="fileInfo" /> does not exist on disk.
        /// </exception>
        public static void WriteStringToFile(FileInfo fileInfo, string text, Encoding encoding, bool append)
        {
            if (append)
            {
                // Sometime we have to refresh to make sure.
                fileInfo.Refresh();

                if (false == FileUtils.Exists(fileInfo))
                {
                    throw new ArgumentException(
                        "Argument 'fileInfo' does not exist.");
                }
            }

            var bytes = encoding.GetBytes(text);
            var mode = append ? FileMode.Append : FileMode.Create;

            using (var writer = fileInfo.Open(mode, FileAccess.Write))
            {
                writer.Write(bytes, 0, bytes.Length);
            }
        }

        #endregion Methods
    }
}