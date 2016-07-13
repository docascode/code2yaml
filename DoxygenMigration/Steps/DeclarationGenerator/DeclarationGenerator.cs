namespace Microsoft.Content.Build.DoxygenMigration.DeclarationGenerator
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    using Microsoft.Content.Build.DoxygenMigration.Model;

    public abstract class DeclarationGenerator
    {
        public abstract string GenerateTypeDeclaration(XElement node);

        public abstract string GenerateMemberDeclaration(XElement node);

        public abstract string GenerateInheritImplementString(IReadOnlyDictionary<string, ArticleItemYaml> articleDict, ArticleItemYaml yaml);
    }
}
