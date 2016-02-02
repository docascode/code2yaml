namespace Microsoft.Content.Build.DoxygenMigration.Steps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Hierarchy;
    using Microsoft.Content.Build.DoxygenMigration.Model;
    using Microsoft.Content.Build.DoxygenMigration.Utility;

    using DocAsCode.YamlSerialization;

    public class GenerateToc : IStep
    {
        public string StepName
        {
            get { return "GenerateToc"; }
        }

        public Task RunAsync(BuildContext context)
        {
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

            TocYaml tocYaml = new TocYaml(
                from change in changesDict.Values
                where change.Parent == null
                orderby change.Name.ToLower()
                select FromHierarchyChange(changesDict, change));

            string tocFile = Path.Combine(outputPath, Constants.TocYamlFileName);
            using (var stream = File.OpenWrite(tocFile))
            using (var writer = new StreamWriter(stream))
            {
                new YamlSerializer().Serialize(writer, tocYaml);
            }

            return Task.FromResult(1);
        }

        private TocItemYaml FromHierarchyChange(IReadOnlyDictionary<string, HierarchyChange> changeDict, HierarchyChange change)
        {
            string namespaceName = change.Parent != null ? changeDict[change.Parent].Name : null;
            return new TocItemYaml
            {
                Uid = change.Uid,
                Name = YamlUtility.ParseNameFromFullName(change.Type, namespaceName, change.Name),
                Href = YamlUtility.ParseHrefFromChangeFile(change.File),
                Items = change.Children.Any() ? new TocYaml(
                    from child in change.Children
                    orderby changeDict[child].Name.ToLower()
                    select FromHierarchyChange(changeDict, changeDict[child])) : null,
            };
        }
    }
}
