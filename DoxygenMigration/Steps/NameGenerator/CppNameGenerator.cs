namespace Microsoft.Content.Build.DoxygenMigration.NameGenerator
{
    using System;
    using System.Xml.Linq;

    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Steps;
    using Microsoft.Content.Build.DoxygenMigration.Utility;

    public class CppNameGenerator : INameGenerator
    {
        public override string GenerateTypeFullName(NameGeneratorContext context, XElement node, bool withTypeParameters)
        {
            return context.CurrentChange.Name;
        }

        public override string GenerateTypeName(NameGeneratorContext context, XElement node, bool withTypeParameters)
        {
            return YamlUtility.ParseNameFromFullName(context.CurrentChange.Type, context.ParentChange?.Name, context.CurrentChange.Name);
        }

        public override string GenerateMemberFullName(NameGeneratorContext context, XElement node)
        {
            return YamlUtility.ParseMemberName(node.NullableElement("definition").NullableValue(), node.NullableElement("argsstring").NullableValue());
        }

        public override string GenerateMemberName(NameGeneratorContext context, XElement node)
        {
            return YamlUtility.ParseMemberName(node.NullableElement("name").NullableValue(), node.NullableElement("argsstring").NullableValue());
        }

        public override string GenerateId(NameGeneratorContext context, XElement node)
        {
            return YamlUtility.ParseIdFromUid(node.NullableAttribute("id").NullableValue(), Constants.NameSpliter);
        }

        public override string GenerateLabel(NameGeneratorContext context, XElement node)
        {
            return node.NullableElement("label").NullableValue();
        }
    }
}
