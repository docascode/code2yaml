namespace Microsoft.Content.Build.Code2Yaml.Utility
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    using Microsoft.Content.Build.Code2Yaml.Constants;
    using Microsoft.Content.Build.Code2Yaml.DataContracts;

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

                    string res = "";
                    if (fullName.Contains(spliter))
                    {
                        res = fullName.Substring(wrapperName.Length + spliter.Length);
                    }
                    else if(fullName.Contains(Constants.Dot))
                    {
                        spliter = Constants.Dot;
                        res = fullName.Substring(wrapperName.Length + spliter.Length);
                    }

                    return res;
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

        public static bool IsFiltered(string prot)
        {
            if (prot == null)
            {
                return false;
            }
            return prot.Contains("private") || prot.Contains("package");
        }
    }
}
