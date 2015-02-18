namespace Modules.Acl.Internal.Utils
{
    using System;
    using System.IO;
    using System.Text;

    internal static class PathExtensions
    {
        #region Fields

        public static readonly char AltDirectorySeparatorChar = '\\';
        public static readonly char DirectorySeparatorChar = '/';
        public static readonly char VolumeSeparatorChar = ':';

        #endregion Fields

        #region Methods

        public static String Combine(params String[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }

            int finalSize = 0;
            int firstComponent = 0;

            // We have two passes, the first calcuates how large a buffer to allocate and does some precondition
            // checks on the paths passed in.  The second actually does the combination.

            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i] == null)
                {
                    throw new ArgumentNullException("paths");
                }

                if (paths[i].Length == 0)
                {
                    continue;
                }

                if (Path.IsPathRooted(paths[i]))
                {
                    firstComponent = i;
                    finalSize = paths[i].Length;
                }
                else
                {
                    finalSize += paths[i].Length;
                }

                char ch = paths[i][paths[i].Length - 1];
                if (ch != DirectorySeparatorChar && ch != AltDirectorySeparatorChar && ch != VolumeSeparatorChar)
                    finalSize++;
            }

            StringBuilder finalPath = new StringBuilder(finalSize);

            for (int i = firstComponent; i < paths.Length; i++)
            {
                if (paths[i].Length == 0)
                {
                    continue;
                }

                if (finalPath.Length == 0)
                {
                    finalPath.Append(paths[i]);
                }
                else
                {
                    char ch = finalPath[finalPath.Length - 1];
                    if (ch != DirectorySeparatorChar && ch != AltDirectorySeparatorChar && ch != VolumeSeparatorChar)
                    {
                        finalPath.Append(DirectorySeparatorChar);
                    }

                    finalPath.Append(paths[i]);
                }
            }

            return finalPath.ToString();
        }

        #endregion Methods
    }
}