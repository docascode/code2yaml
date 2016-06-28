namespace Microsoft.Content.Build.DoxygenMigration.Steps
{
    using System;
    using System.Collections.Concurrent;
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

            var infoDict = new ConcurrentDictionary<string, ArticleItemYaml>();
            var pages = await changesDict.Values.SelectInParallelAsync(
               async change =>
               {
                   using (var input = File.OpenRead(Path.Combine(inputPath, change.File)))
                   {
                       XDocument doc = XDocument.Load(input);
                       var cloned = context.Clone();
                       cloned.SetSharedObject(Constants.CurrentChange, change);
                       HierarchyChange parent = change.Parent != null ? changesDict[change.Parent] : null;
                       cloned.SetSharedObject(Constants.ParentChange, parent);
                       var articleContext = new ArticleContext(cloned);

                       IArticleGenerator generator = (IArticleGenerator)Generator.Clone();
                       PageModel page = await generator.GenerateArticleAsync(articleContext, doc);
                       foreach (var item in page.Items)
                       {
                           if (!infoDict.TryAdd(item.Uid, item))
                           {
                               context.AddLogEntry(
                                   new LogEntry
                                   {
                                       Level = LogLevel.Warning,
                                       Message = $"Duplicate items {item.Uid} found in {change.File}.",
                                   });
                           }
                       }
                       return page;
                   }
               });

            // update reference and save yaml
            await pages.ForEachInParallelAsync(
                page =>
                {
                    foreach (var reference in page.References)
                    {
                        ArticleItemYaml yaml;
                        if (infoDict.TryGetValue(reference.Uid, out yaml))
                        {
                            reference.Name = yaml.Name;
                            reference.Type = yaml.Type;
                            reference.FullName = yaml.FullName;
                            reference.Href = yaml.Href;
                            reference.Parent = yaml.Parent;
                            reference.Syntax = yaml.Syntax;
                            reference.Summary = yaml.Summary;
                        }
                        else if (reference.SpecForJava != null)
                        {
                            foreach (var spec in reference.SpecForJava)
                            {
                                if (spec.Uid != null)
                                {
                                    var specYaml = infoDict[spec.Uid];
                                    spec.Name = specYaml.Name;
                                    spec.FullName = specYaml.FullName;
                                    spec.Href = specYaml.Href;
                                }
                            }
                        }
                    }
                    using (var writer = new StreamWriter(Path.Combine(outputPath, page.Items[0].Href)))
                    {
                        YamlSerializer.Value.Serialize(writer, page);
                    }
                    return Task.FromResult(1);
                });
        }
    }
}
