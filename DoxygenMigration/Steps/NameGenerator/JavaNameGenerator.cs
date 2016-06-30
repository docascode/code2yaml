namespace Microsoft.Content.Build.DoxygenMigration.NameGenerator
{
    using System;
    using System.Xml.Linq;

    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Steps;
    using Microsoft.Content.Build.DoxygenMigration.Utility;

    public class JavaNameGenerator : INameGenerator
    {
        public override string GenerateTypeFullName(NameGeneratorContext context, XElement node)
        {
            return YamlUtility.RegularizeName(context.CurrentChange.Name, Constants.Dot);
        }

        public override string GenerateTypeName(NameGeneratorContext context, XElement node)
        {
            return YamlUtility.RegularizeName(YamlUtility.ParseNameFromFullName(context.CurrentChange.Type, context.ParentChange?.Name, context.CurrentChange.Name), Constants.Dot);
        }

        public override string GenerateMemberFullName(NameGeneratorContext context, XElement node)
        {
            return YamlUtility.RegularizeName(YamlUtility.ParseMemberName(node.NullableElement("definition").NullableValue(), node.NullableElement("argsstring").NullableValue()), Constants.Dot);
        }

        public override string GenerateMemberName(NameGeneratorContext context, XElement node)
        {
            return YamlUtility.RegularizeName(YamlUtility.ParseMemberName(node.NullableElement("name").NullableValue(), node.NullableElement("argsstring").NullableValue()), Constants.Dot);
        }

        public override string GenerateId(NameGeneratorContext context, XElement node)
        {
            return YamlUtility.ParseIdFromUid(node.NullableAttribute("id").NullableValue(), Constants.Dot);
        }

        public override string GenerateLabel(NameGeneratorContext context, XElement node)
        {
            return YamlUtility.RegularizeName(node.NullableElement("label").NullableValue(), Constants.Dot);
        }
    }
}
