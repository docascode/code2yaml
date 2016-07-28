namespace Microsoft.Content.Build.Code2Yaml.Utility
{
    using System.IO;
    using Microsoft.Content.Build.Code2Yaml.Constants;

    public static class StepUtility
    {
        public static string GetIntermediateOutputPath(string outputPath)
        {
            return Path.Combine(outputPath, Constants.IntermediateFolderName);
        }

        public static string GetDoxygenXmlOutputPath(string outputPath)
        {
            return Path.Combine(GetIntermediateOutputPath(outputPath), Constants.XmlFolderName);
        }

        public static string GetProcessedXmlOutputPath(string outputPath)
        {
            return Path.Combine(GetIntermediateOutputPath(outputPath), Constants.ProcessedXmlFolderName);
        }
    }
}
