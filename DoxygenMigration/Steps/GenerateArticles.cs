namespace Microsoft.Content.Build.DoxygenMigration.Steps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Microsoft.Content.Build.DoxygenMigration.ArticleGenerator;
    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Hierarchy;
    using Microsoft.Content.Build.DoxygenMigration.Model;
    using Microsoft.Content.Build.DoxygenMigration.Utility;

    using DocAsCode.YamlSerialization;

    public class GenerateArticles : IStep
    {
        private static readonly ThreadLocal<YamlSerializer> YamlSerializer = new ThreadLocal<YamlSerializer>(() =>
        {
            return new YamlSerializer();
        });

        public string StepName
        {
            get { return "GenerateArticles"; }
        }

        public IArticleGenerator Generator { get; set; }

        public async Task RunAsync(BuildContext context)
        {
            string inputPath = context.GetSharedObject(Constants.InputPath) as string;
            if (inputPath == null)
            {
                throw new ApplicationException(string.Format("Key: {0} doesn't exist in build context", Constants.InputPath));
            }
            string outputPath = context.GetSharedObject(Constants.OutputPath) as string;
            if (outputPath == null)
            {
                throw new ApplicationException(string.Format("Key: {0} doesn't exist in build context", Constants.OutputPath));
            }
            var changesDict = context.GetSharedObject(Constants.Changes) as Dictionary<string, HierarchyChange>;
            if (changesDict == null)
            {
                throw new ApplicationException(string.Format("Key: {0} doesn't exist in build context", Constants.Changes));
            }

            await changesDict.Values.ForEachInParallelAsync(
                async change =>
                {
                    using (var input = File.OpenRead(Path.Combine(inputPath, change.File)))
                    using (var output = File.OpenWrite(Path.Combine(outputPath, YamlUtility.ParseHrefFromChangeFile(change.File))))
                    using (var writer = new StreamWriter(output))
                    {
                        XDocument doc = XDocument.Load(input);
                        var cloned = context.Clone();
                        cloned.SetSharedObject(Constants.CurrentChange, change);
                        HierarchyChange parent = change.Parent != null ? changesDict[change.Parent] : null;
                        cloned.SetSharedObject(Constants.ParentChange, parent);
                        var articleContext = new ArticleContext(cloned);

                        PageModel page = await Generator.GenerateArticleAsync(articleContext, doc);
                        YamlSerializer.Value.Serialize(writer, page);
                    }
                });
        }
    }
}
