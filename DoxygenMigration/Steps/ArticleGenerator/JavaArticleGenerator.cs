﻿namespace Microsoft.Content.Build.DoxygenMigration.ArticleGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.DeclarationGenerator;
    using Microsoft.Content.Build.DoxygenMigration.Hierarchy;
    using Microsoft.Content.Build.DoxygenMigration.Model;
    using Microsoft.Content.Build.DoxygenMigration.NameGenerator;
    using Microsoft.Content.Build.DoxygenMigration.Steps;

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

        protected override bool ShouldWriteHeader
        {
            get
            {
                return false;
            }
        }
    }
}