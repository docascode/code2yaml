namespace Microsoft.Content.Build.DoxygenMigration.ArticleGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;

    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Hierarchy;
    using Microsoft.Content.Build.DoxygenMigration.Model;
    using Microsoft.Content.Build.DoxygenMigration.Steps;
    using Microsoft.Content.Build.DoxygenMigration.Utility;

    public class CppArticleGenerator : BasicArticleGenerator
    {
        public override string Language
        {
            get
            {
                return "cplusplus";
            }
        }

        protected override void FillLanguageSpecificMetadata(ArticleItemYaml yaml, ArticleContext context, XElement xmlFragment)
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

        protected override string WriteAccessLabel(string access)
        {
            return $"{access}: ";
        }

        protected override string NameSpliter
        {
            get
            {
                return Constants.NameSpliter;
            }
        }
    }
}
