namespace Microsoft.Content.Build.DoxygenMigration.NameGenerator
{
    using System;
    using System.Xml.Linq;

    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Utility;

    public abstract class INameGenerator
    {
        public abstract string GenerateTypeFullName(NameGeneratorContext context, XElement node);

        public abstract string GenerateMemberFullName(NameGeneratorContext context, XElement node);

        public abstract string GenerateTypeName(NameGeneratorContext context, XElement node);

        public abstract string GenerateMemberName(NameGeneratorContext context, XElement node);

        public abstract string GenerateId(NameGeneratorContext context, XElement node);

        public abstract string GenerateLabel(NameGeneratorContext context, XElement node);
    }

    public class NameGeneratorFactory
    {
        public static INameGenerator Create(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                throw new ArgumentNullException("language");
            }

            if (language.Equals("cplusplus", StringComparison.OrdinalIgnoreCase))
            {
                return new CppNameGenerator();
            }
            else if (language.Equals("java", StringComparison.OrdinalIgnoreCase))
            {
                return new JavaNameGenerator();
            }
            else
            {
                throw new NotSupportedException($"{language} isn't supported!");
            }
        }
    }
}
