namespace Microsoft.Content.Build.DoxygenMigration.Utility
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class StringUtility
    {
        /// <summary>
        /// Hashes a non GUID assetId string into a GUID format
        /// </summary>
        /// <param name="id">AssetId to hash</param>
        /// <param name="length">max length</param>
        /// <param name="encoding"></param>
        /// <returns>new AssetId in GUID format</returns>
        public static string ComputeHash(string id, int length = 8, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            var inputBuf = encoding.GetBytes(id);
            var csp = new MD5CryptoServiceProvider();
            var outputBuf = csp.ComputeHash(inputBuf);

            var output = new Guid(outputBuf).ToString();
            int hashTokenLength = output.Length > 8 ? 8 : output.Length;
            return output.Substring(0, hashTokenLength);
        }

        public static string BackSlashToForwardSlash(this string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            return input.Replace('\\', '/');
        }

        public static string ToNormalizedFullPath(this string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return Path.GetFullPath(path).BackSlashToForwardSlash().TrimEnd('/');
        }
    }
}
