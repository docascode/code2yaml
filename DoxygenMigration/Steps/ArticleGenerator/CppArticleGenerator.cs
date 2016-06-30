namespace Microsoft.Content.Build.DoxygenMigration.ArticleGenerator
{
    using System.Collections.Generic;
    using System.Text;
    using System.Xml.Linq;

    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.DeclarationGenerator;
    using Microsoft.Content.Build.DoxygenMigration.Hierarchy;
    using Microsoft.Content.Build.DoxygenMigration.Model;
    using Microsoft.Content.Build.DoxygenMigration.NameGenerator;
    using Microsoft.Content.Build.DoxygenMigration.Steps;

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
        }

        protected override ReferenceViewModel CreateReferenceWithSpec(string uid, List<SpecViewModel> specs)
        {
            return new ReferenceViewModel
            {
                Uid = uid,
                SpecForCpp = specs,
            };
        }

        protected override bool ShouldWriteHeader
        {
            get
            {
                return true;
            }
        }
    }
}
