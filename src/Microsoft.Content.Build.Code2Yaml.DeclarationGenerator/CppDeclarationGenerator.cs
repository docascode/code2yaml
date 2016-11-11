namespace Microsoft.Content.Build.Code2Yaml.DeclarationGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    using Microsoft.Content.Build.Code2Yaml.Constants;
    using Microsoft.Content.Build.Code2Yaml.DataContracts;
    using Microsoft.Content.Build.Code2Yaml.Utility;

    public class CppDeclarationGenerator : DeclarationGenerator
    {
        public override string GenerateTypeDeclaration(XElement node)
        {
            StringBuilder sb = new StringBuilder();
            string prot = node.NullableAttribute("prot").NullableValue();
            if (prot != null)
            {
                sb.Append($"{prot}: ");
            }
            string virt = node.NullableAttribute("virt").NullableValue();
            if (virt == "virtual" || virt == "pure-virtual")
            {
                sb.Append("virtual ");
            }
            string statics = node.NullableAttribute("static").NullableValue();
            if (statics == "yes")
            {
                sb.Append("static ");
            }
            string kind = node.NullableAttribute("kind").NullableValue();
            if (kind != null)
            {
                sb.Append(string.Format("{0} ", kind));
            }
            var name = node.NullableElement("compoundname").Value;
            int index = name.LastIndexOf(Constants.NameSpliter);
            string innerName = name.Substring(index < 0 ? 0 : index + Constants.NameSpliter.Length);
            sb.Append(kind == "namespace" ? name : innerName);
            return sb.ToString();
        }

        public override string GenerateMemberDeclaration(XElement node)
        {
            StringBuilder sb = new StringBuilder();
            string prot = node.NullableAttribute("prot").NullableValue();
            if (prot != null)
            {
                sb.Append($"{prot}: ");
            }
            string virt = node.NullableAttribute("virt").NullableValue();
            if (virt == "virtual" || virt == "pure-virtual")
            {
                sb.Append("virtual ");
            }
            string statics = node.NullableAttribute("static").NullableValue();
            if (statics == "yes")
            {
                sb.Append("static ");
            }
            string typeStr = node.NullableElement("type").NullableValue();
            if (typeStr != null)
            {
                sb.Append(typeStr + " ");
            }
            string name = node.NullableElement("name").NullableValue();
            if (name != null)
            {
                sb.Append(name);
            }
            var args = node.NullableElement("argsstring").NullableValue();
            if (args != null)
            {
                sb.Append(args);
            }

            var initializer = node.NullableElement("initializer").NullableValue();
            if (initializer != null)
            {
                sb.Append(initializer);
            }

            return sb.ToString();
        }

        public override string GenerateInheritImplementString(IReadOnlyDictionary<string, ArticleItemYaml> articleDict, ArticleItemYaml yaml)
        {
            if (yaml.ImplementsOrInherits == null || yaml.ImplementsOrInherits.Count == 0)
            {
                return string.Empty;
            }
            List<string> extends = new List<string>();
            foreach (var ele in yaml.ImplementsOrInherits)
            {
                ArticleItemYaml eleYaml;
                if (articleDict.TryGetValue(ele.Type, out eleYaml))
                {
                    string parent = eleYaml.Parent != null ? articleDict[eleYaml.Parent].FullName : string.Empty;
                    string name = YamlUtility.ParseNameFromFullName(HierarchyType.Class, parent, ele.SpecializedFullName);
                    extends.Add(name);
                }
            }

            var builder = new StringBuilder();
            if (extends.Count > 0)
            {
                builder.Append($" : public {extends[0]}");
                foreach (var ex in extends.Skip(1))
                {
                    builder.Append($", public {ex}");
                }
            }

            return builder.ToString();
        }
    }
}
