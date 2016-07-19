namespace Microsoft.Content.Build.DoxygenMigration.Steps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Utility;

    public class PreprocessXml : IStep
    {
        private static readonly Regex IdRegex = new Regex(@"^(namespace|class|struct|enum|interface)(\S+)$", RegexOptions.Compiled);

        public string StepName { get { return "Preprocess"; } }

        public async Task RunAsync(BuildContext context)
        {
            string inputPath = context.GetSharedObject(Constants.InputPath) as string;
            if (inputPath == null)
            {
                throw new ApplicationException(string.Format("Key: {0} doesn't exist in build context", Constants.InputPath));
            }

            string dirName = Path.GetDirectoryName(inputPath);
            string updatedFolderName = Path.GetFileName(inputPath) + "_update";
            var updatedFolderPath = Path.Combine(dirName, updatedFolderName);
            if (Directory.Exists(updatedFolderPath))
            {
                Directory.Delete(updatedFolderPath, recursive: true);
            }
            var dirInfo = Directory.CreateDirectory(updatedFolderPath);
            string updatedPath = dirInfo.FullName;

            // workaround for Doxygen Bug: it generated extra namespace for code `public string namespace(){ return ""; }`.
            // so if we find namespace which has same name with class, remove it from index file and also remove its file.
            string indexFile = Path.Combine(inputPath, Constants.IndexFileName);
            XDocument indexDoc = XDocument.Load(indexFile);
            var duplicateItems = (from ele in indexDoc.Root.Elements("compound")
                                  let uid = (string)ele.Attribute("refid")
                                  group ele by RegularizeUid(uid) into g
                                  let duplicate = g.FirstOrDefault(e => (string)e.Attribute("kind") == "namespace")
                                  where g.Count() > 1 && duplicate != null
                                  select (string)duplicate.Attribute("refid")).ToList();

            await Directory.EnumerateFiles(inputPath, "*.xml").ForEachInParallelAsync(
            p =>
            {
                XDocument doc = XDocument.Load(p);
                if (Path.GetFileName(p) == Constants.IndexFileName)
                {
                    var toBeRemoved = (from item in duplicateItems
                                       select doc.XPathSelectElement($"//compound[@refid='{item}']")).ToList();
                    foreach (var element in toBeRemoved)
                    {
                        element.Remove();
                    }
                }
                else if (duplicateItems.Contains(Path.GetFileNameWithoutExtension(p)))
                {
                    return Task.FromResult(1);
                }
                foreach (var node in doc.XPathSelectElements("//node()[@refid]"))
                {
                    node.Attribute("refid").Value = RegularizeUid(node.Attribute("refid").Value);
                }
                foreach (var node in doc.XPathSelectElements("//node()[@id]"))
                {
                    node.Attribute("id").Value = RegularizeUid(node.Attribute("id").Value);
                }
                doc.Save(Path.Combine(updatedPath, RegularizeUid(Path.GetFileNameWithoutExtension(p)) + Path.GetExtension(p)));
                return Task.FromResult(1);
            });
            context.SetSharedObject(Constants.InputPath, updatedPath);
        }

        private static string RegularizeUid(string uid)
        {
            if (uid == null)
            {
                return uid;
            }
            var m = IdRegex.Match(uid);
            if (m.Success)
            {
                uid = m.Groups[2].Value;
            }
            return uid.Replace(Constants.IdSpliter, Constants.Dot);
        }
    }
}
