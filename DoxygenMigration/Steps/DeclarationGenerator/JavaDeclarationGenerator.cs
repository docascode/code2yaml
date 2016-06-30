namespace Microsoft.Content.Build.DoxygenMigration.DeclarationGenerator
{
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Steps;
    using Microsoft.Content.Build.DoxygenMigration.Utility;

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
