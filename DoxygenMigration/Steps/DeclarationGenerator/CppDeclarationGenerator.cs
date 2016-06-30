namespace Microsoft.Content.Build.DoxygenMigration.DeclarationGenerator
{
    using System.Text;
    using System.Xml.Linq;

    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Steps;

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
            sb.Append(innerName);
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

    }
}
