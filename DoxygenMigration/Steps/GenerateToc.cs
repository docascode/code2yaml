namespace Microsoft.Content.Build.DoxygenMigration.Steps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Content.Build.DoxygenMigration.Config;
    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Hierarchy;
    using Microsoft.Content.Build.DoxygenMigration.Model;
    using Microsoft.Content.Build.DoxygenMigration.NameGenerator;
    using Microsoft.Content.Build.DoxygenMigration.Utility;

    using DocAsCode.YamlSerialization;

    public class GenerateToc : IStep
    {
        public INameGenerator NameGenerator { get; set; }

        public string StepName
        {
            get { return "GenerateToc"; }
        }

        public Task RunAsync(BuildContext context)
        {
            var config = context.GetSharedObject(Constants.Config) as ConfigModel;
            if (config == null)
            {
                throw new ApplicationException(string.Format("Key: {0} doesn't exist in build context", Constants.Config));
            }
            string outputPath = config.OutputPath;
            var changesDict = context.GetSharedObject(Constants.Changes) as Dictionary<string, HierarchyChange>;
            if (changesDict == null)
            {
                throw new ApplicationException(string.Format("Key: {0} doesn't exist in build context", Constants.Changes));
            }

            TocYaml tocYaml = new TocYaml(
                from change in changesDict.Values
                where change.Parent == null
                let toc = FromHierarchyChange(changesDict, change, context)
                orderby toc.Name.ToLower()
                select toc);

            string tocFile = Path.Combine(outputPath, Constants.TocYamlFileName);
            using (var writer = new StreamWriter(tocFile))
            {
                new YamlSerializer().Serialize(writer, tocYaml);
            }

            string tocMDFile = Path.Combine(outputPath, Constants.TocMDFileName);
            using (var writer = new StreamWriter(tocMDFile))
            {
                bool generateTocMDFile = config.GenerateTocMDFile;
                if (generateTocMDFile)
                {
                    foreach (var item in tocYaml)
                    {
                        WriteTocItemMD(writer, item, 1);
                    }
                }
            }

            return Task.FromResult(1);
        }

        private TocItemYaml FromHierarchyChange(IReadOnlyDictionary<string, HierarchyChange> changeDict, HierarchyChange change, BuildContext context)
        {
            var parentChange = change.Parent != null ? changeDict[change.Parent] : null;
            return new TocItemYaml
            {
                Uid = change.Uid,
                Name = NameGenerator.GenerateTypeName(new NameGeneratorContext { CurrentChange = change, ParentChange = parentChange }, null, true),
                Href = YamlUtility.ParseHrefFromChangeFile(change.File),
                Items = change.Children.Any() ? new TocYaml(
                from child in change.Children
                let toc = FromHierarchyChange(changeDict, changeDict[child], context)
                orderby toc.Name.ToLower()
                select toc) : null,
            };
        }

        private void WriteTocItemMD(StreamWriter writer, TocItemYaml item, int depth)
        {
            writer.WriteLine($"{new string('#', depth)} [{item.Name}]({item.Href})");
            if (item.Items != null)
            {
                foreach (var c in item.Items)
                {
                    WriteTocItemMD(writer, c, depth + 1);
                }
            }
        }
    }
}
