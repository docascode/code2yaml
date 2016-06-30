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
        private static readonly Regex IdRegex = new Regex(@"^(namespace|class|struct|enum)(\S+)$", RegexOptions.Compiled);

        public static string ParseIdFromUid(string uid, string spliter = Constants.NameSpliter)
        {
            if (string.IsNullOrEmpty(spliter))
            {
                throw new ArgumentNullException("spliter");
            }

            int index = uid.LastIndexOf(spliter);
            if (index < 0)
            {
                return uid;
            }
            return uid.Substring(index + spliter.Length);
        }

        public static string ParseHrefFromChangeFile(string changeFile)
        {
            return Path.ChangeExtension(changeFile, Constants.YamlExtension);
        }

        public static string ParseNameFromFullName(HierarchyType htype, string wrapperName, string fullName, string spliter = Constants.NameSpliter)
        {
            if (string.IsNullOrEmpty(spliter))
            {
                throw new ArgumentNullException("spliter");
            }

            switch (htype)
            {
                case HierarchyType.Namespace:
                    return fullName;
                default:
                    if (wrapperName == null)
                    {
                        return fullName;
                    }
                    return fullName.Substring(wrapperName.Length + spliter.Length);
            }
        }

        public static string ParseMemberName(string name, string argstring)
        {
            if (argstring == null)
            {
                return name;
            }
            int index = argstring.LastIndexOf(")");
            return name + argstring.Substring(0, index + 1);
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

        public static string RegularizeName(string name, string nameSpliter)
        {
            if (name == null)
            {
                return name;
            }
            return name.Replace(Constants.NameSpliter, nameSpliter);
        }
    }
}
