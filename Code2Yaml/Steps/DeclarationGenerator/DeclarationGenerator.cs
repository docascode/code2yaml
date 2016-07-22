namespace Microsoft.Content.Build.Code2Yaml.DeclarationGenerator
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    using Microsoft.Content.Build.Code2Yaml.Model;

    public abstract class DeclarationGenerator
    {
        public abstract string GenerateTypeDeclaration(XElement node);

        public abstract string GenerateMemberDeclaration(XElement node);

        public abstract string GenerateInheritImplementString(IReadOnlyDictionary<string, ArticleItemYaml> articleDict, ArticleItemYaml yaml);
    }
}
