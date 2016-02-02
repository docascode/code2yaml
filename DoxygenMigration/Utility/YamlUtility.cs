namespace Microsoft.Content.Build.DoxygenMigration.Utility
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Hierarchy;
    using Microsoft.Content.Build.DoxygenMigration.Model;

    public static class YamlUtility
    {
        private static readonly Regex IdRegex = new Regex(@"^(namespace | class | struct | enum)(\S+)$", RegexOptions.Compiled);

        public static string ParseIdFromUid(string uid)
        {
            int index = uid.LastIndexOf(Constants.CppIdSpliter);
            if (index < 0)
            {
                return IdRegex.Match(uid).Groups[2].Value;
            }
            return uid.Substring(index + Constants.CppIdSpliter.Length);
        }

        public static string ParseHrefFromChangeFile(string changeFile)
        {
            return Path.ChangeExtension(changeFile, Constants.YamlExtension);
        }

        public static string ParseNameFromFullName(HierarchyType htype, string namespaceName, string fullName)
        {
            switch (htype)
            {
                case HierarchyType.Namespace:
                    return fullName;
                default:
                    if (namespaceName == null)
                    {
                        return fullName;
                    }
                    return fullName.Substring(namespaceName.Length + Constants.CppSpliter.Length);
            }
        }

        public static MemberType? ParseType(string typeStr)
        {
            MemberType type;
            if (Enum.TryParse<MemberType>(typeStr, true, out type))
            {
                return type;
            }
            return null;
        }
    }
}
