namespace Microsoft.Content.Build.Code2Yaml.DeclarationGenerator
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using Microsoft.Content.Build.Code2Yaml.Constants;
    using Microsoft.Content.Build.Code2Yaml.Model;
    using Microsoft.Content.Build.Code2Yaml.Steps;
    using Microsoft.Content.Build.Code2Yaml.Utility;

    public class JavaDeclarationGenerator : DeclarationGenerator
    {
        public override string GenerateTypeDeclaration(XElement node)
        {
            StringBuilder sb = new StringBuilder();
            string prot = node.NullableAttribute("prot").NullableValue();
            if (prot != null && !prot.Contains("package"))
            {
                sb.Append($"{prot} ");
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
            var name = YamlUtility.RegularizeName(node.NullableElement("compoundname").Value, Constants.Dot);
            int index = name.LastIndexOf(Constants.Dot);
            string innerName = name.Substring(index < 0 ? 0 : index + Constants.Dot.Length);
            sb.Append(innerName);
            var typeParams = node.XPathSelectElements("templateparamlist/param").ToList();
            if (typeParams.Count > 0)
            {
                var count = new StrongBox<int>(1);
                sb.Append($"<{ParseTypeParameterString(typeParams[0], count)}");
                foreach (var param in typeParams.Skip(1))
                {
                    sb.Append($",{ParseTypeParameterString(param, count)}");
                }
                Debug.Assert(count.Value >= 0);
                sb.Append(new string('>', count.Value));
            }

            return sb.ToString();
        }

        public override string GenerateMemberDeclaration(XElement node)
        {
            StringBuilder sb = new StringBuilder();
            string prot = node.NullableAttribute("prot").NullableValue();
            if (prot != null && !prot.Contains("package"))
            {
                sb.Append($"{prot} ");
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
            string name = YamlUtility.RegularizeName(node.NullableElement("name").NullableValue(), Constants.Dot);
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
            List<string> implements = new List<string>();
            List<string> extends = new List<string>();
            foreach (var ele in yaml.ImplementsOrInherits)
            {
                ArticleItemYaml eleYaml;
                if (articleDict.TryGetValue(ele.Type, out eleYaml))
                {
                    string parent = eleYaml.Parent != null ? articleDict[eleYaml.Parent].FullName : string.Empty;
                    string name = YamlUtility.ParseNameFromFullName(Hierarchy.HierarchyType.Class, parent, ele.SpecializedFullName, Constants.Dot);
                    if (yaml.Type != MemberType.Interface && eleYaml.Type == MemberType.Interface)
                    {
                        implements.Add(name);
                    }
                    else
                    {
                        extends.Add(name);
                    }
                }
            }

            var builder = new StringBuilder();
            if (extends.Count > 0)
            {
                builder.Append($" extends {extends[0]}");
                foreach (var ex in extends.Skip(1))
                {
                    builder.Append($",{ex}");
                }
            }
            if (implements.Count > 0)
            {
                builder.Append($" implements {implements[0]}");
                foreach (var im in implements.Skip(1))
                {
                    builder.Append($",{im}");
                }
            }

            return builder.ToString();
        }

        private string ParseTypeParameterString(XElement paramElement, StrongBox<int> count)
        {
            var typeStr = paramElement.NullableElement("type").NullableValue();
            var typeConstraint = paramElement.NullableElement("typeconstraint");
            if (typeConstraint.IsNull())
            {
                return typeStr;
            }
            var typeConstraintStr = typeConstraint.NullableValue();
            if (typeConstraintStr.Contains("<"))
            {
                count.Value += 1;
            }
            if (typeConstraintStr.Contains(">"))
            {
                count.Value -= 1;
            }
            return $"{typeStr} extends {typeConstraintStr}";
        }
    }
}
