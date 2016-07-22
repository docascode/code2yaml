namespace Microsoft.Content.Build.DoxygenMigration.Utility
{
    using System.IO;
    using Microsoft.Content.Build.DoxygenMigration.Constants;

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
