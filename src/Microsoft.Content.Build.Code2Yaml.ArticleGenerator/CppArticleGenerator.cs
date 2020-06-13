namespace Microsoft.Content.Build.Code2Yaml.ArticleGenerator
{
    using System.Collections.Generic;
    using System.Text;
    using System.Xml.Linq;
    using GitSharp;
    using Microsoft.Content.Build.Code2Yaml.Common;
    using Microsoft.Content.Build.Code2Yaml.DataContracts;
    using Microsoft.Content.Build.Code2Yaml.DeclarationGenerator;
    using Microsoft.Content.Build.Code2Yaml.NameGenerator;

    public class CppArticleGenerator : BasicArticleGenerator
    {
        public CppArticleGenerator() : base(new CppNameGenerator(), new CppDeclarationGenerator())
        {
        }

        public override string Language
        {
            get
            {
                return "cplusplus";
            }
        }

        protected override void FillLanguageSpecificMetadata(ArticleItemYaml yaml, ArticleContext context, XElement node)
        {
            HierarchyChange curChange = context.CurrentChange;
            HierarchyChange parentChange = context.ParentChange;
            yaml.NamespaceName = parentChange?.Uid;
            FillHeader(yaml, node);
        }

        protected override ReferenceViewModel CreateReferenceWithSpec(string uid, List<SpecViewModel> specs)
        {
            return new ReferenceViewModel
            {
                Uid = uid,
                SpecForCpp = specs,
            };
        }

        protected override IEnumerable<string> GetDefaultInheritance(ArticleItemYaml yaml)
        {
            if (yaml.Type == MemberType.Class)
            {
                yield return "System::Object";
            }
        }
    }
}
