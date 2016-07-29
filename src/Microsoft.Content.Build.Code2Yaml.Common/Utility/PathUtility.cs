namespace Microsoft.Content.Build.Code2Yaml.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class PathUtility
    {
        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="basePath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="absolutePath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string MakeRelativePath(string basePath, string absolutePath)
        {
            if (string.IsNullOrEmpty(basePath)) return absolutePath;
            if (string.IsNullOrEmpty(absolutePath)) return null;
            if (FilePathComparer.OSPlatformSensitiveComparer.Equals(basePath, absolutePath)) return string.Empty;

            // Append / to the directory
            if (basePath[basePath.Length - 1] != '/')
            {
                basePath = basePath + "/";
            }

            Uri fromUri = new Uri(Path.GetFullPath(basePath));
            Uri toUri = new Uri(Path.GetFullPath(absolutePath));

            if (fromUri.Scheme != toUri.Scheme) { return absolutePath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath.BackSlashToForwardSlash();
        }

        public static string GetAbsolutePath(string basePath, string relativePath)
        {
            if (basePath == null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }

            if (relativePath == null)
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            Uri resultUri = new Uri(new Uri(basePath), new Uri(relativePath, UriKind.Relative));
            return resultUri.LocalPath;
        }

        public static bool IsPathExisted(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }
    }

    public class FilePathComparer
        : IEqualityComparer<string>
    {
        private readonly static StringComparer _stringComparer = GetStringComparer();

        public static readonly FilePathComparer OSPlatformSensitiveComparer = new FilePathComparer();
        public static readonly StringComparer OSPlatformSensitiveStringComparer = GetStringComparer();

        public bool Equals(string x, string y)
        {
            return _stringComparer.Equals(x.ToNormalizedFullPath(), y.ToNormalizedFullPath());
        }

        public int GetHashCode(string obj)
        {
            string path = obj.ToNormalizedFullPath();

            return _stringComparer.GetHashCode(obj);
        }

        private static StringComparer GetStringComparer()
        {
            if (Environment.OSVersion.Platform < PlatformID.Unix)
            {
                return StringComparer.OrdinalIgnoreCase;
            }
            else
            {
                return StringComparer.Ordinal;
            }
        }
    }
}
