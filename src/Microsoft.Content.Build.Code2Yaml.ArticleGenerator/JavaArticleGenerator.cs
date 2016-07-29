namespace Microsoft.Content.Build.Code2Yaml.ArticleGenerator
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    using Microsoft.Content.Build.Code2Yaml.DataContracts;
    using Microsoft.Content.Build.Code2Yaml.DeclarationGenerator;
    using Microsoft.Content.Build.Code2Yaml.NameGenerator;

    public class JavaArticleGenerator : BasicArticleGenerator
    {
        public JavaArticleGenerator() : base(new JavaNameGenerator(), new JavaDeclarationGenerator())
        {
        }

        public override string Language
        {
            get
            {
                return "java";
            }
        }

        protected override void FillLanguageSpecificMetadata(ArticleItemYaml yaml, ArticleContext context, XElement node)
        {
            HierarchyChange curChange = context.CurrentChange;
            HierarchyChange parentChange = context.ParentChange;
            yaml.PackageName = parentChange?.Uid;
        }

        protected override ReferenceViewModel CreateReferenceWithSpec(string uid, List<SpecViewModel> specs)
        {
            return new ReferenceViewModel
            {
                Uid = uid,
                SpecForJava = specs,
            };
        }

        protected override IEnumerable<string> GetDefaultInheritance(ArticleItemYaml yaml)
        {
            if (yaml.Type == MemberType.Enum)
            {
                yield return $"java.lang.Enum<{yaml.Name}>";
            }

            if (yaml.Type == MemberType.Class || yaml.Type == MemberType.Enum)
            {
                yield return "java.lang.Object";
            }
        }
    }
}
