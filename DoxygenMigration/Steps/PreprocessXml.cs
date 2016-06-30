namespace Microsoft.Content.Build.DoxygenMigration.Steps
{
    using System;
    using System.IO;
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

            await Directory.EnumerateFiles(inputPath, "*.xml").ForEachInParallelAsync(
                p =>
                {
                    XDocument doc = XDocument.Load(p);
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
            var m = IdRegex.Match(uid);
            if (m.Success)
            {
                uid = m.Groups[2].Value;
            }
            return uid.Replace(Constants.IdSpliter, Constants.Dot);
        }
    }
}
