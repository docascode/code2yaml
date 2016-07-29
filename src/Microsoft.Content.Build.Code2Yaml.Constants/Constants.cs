namespace Microsoft.Content.Build.Code2Yaml.Constants
{
    public static class Constants
    {
        public const string InputPaths = "input_paths";
        public const string OutputPath = "output_path";
        public const string ExcludePaths = "exclude_paths";
        public const string Language = "language";
        public const string GenerateTocMDFile = "generate_toc_md";
        public const string Config = "config";
        public const string IndexFileName = "index.xml";
        public const string TocYamlFileName = "toc.yml";
        public const string TocMDFileName = "index.md";
        public const string ConfigFileName = "code2yaml.json";
        public const string IntermediateFolderName = ".inter";
        public const string XmlFolderName = "xml";
        public const string ProcessedXmlFolderName = "xml_update";
        public const string Changes = "changes";
        public const string CurrentChange = "cur_change";
        public const string ParentChange = "parent_change";
        public const string ArticleItemYamlDict = "article_item_yaml_dict";
        public const string XmlExtension = ".xml";
        public const string YamlExtension = ".yml";
        public const string IdSpliter = "_1_1";
        public const string NameSpliter = "::";
        public const string CmdArgInputPath = "inputpath:";
        public const string CmdArgOutputPath = "outputpath:";
        public const string CmdArgGitRepo = "gitrepo:";
        public const string CmdArgGitBranch = "gitbranch:";
        public const string CmdArgLanguage = "lang:";
        public const string Dot = ".";

        public static class Doxyfile
        {
            public const string INPUT = "INPUT";
            public const string OUTPUT_DIRECTORY = "OUTPUT_DIRECTORY";
            public const string EXCLUDE = "EXCLUDE";
        }
    }
}
