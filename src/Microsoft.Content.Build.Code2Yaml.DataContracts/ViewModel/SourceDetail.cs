namespace Microsoft.Content.Build.Code2Yaml.DataContracts
{
    using System;

    using YamlDotNet.Serialization;

    [Serializable]
    public class SourceDetail
    {
        [YamlMember(Alias = "remote")]
        public GitDetail Remote { get; set; }

        [YamlMember(Alias = "base")]
        public string BasePath { get; set; }

        [YamlMember(Alias = "id")]
        public string Name { get; set; }

        /// <summary>
        /// The url path for current source, should be resolved at some late stage
        /// </summary>
        [YamlMember(Alias = "href")]
        public string Href { get; set; }

        [YamlMember(Alias = "path")]
        public string Path { get; set; }

        [YamlMember(Alias = "startLine")]
        public int StartLine { get; set; }

        [YamlMember(Alias = "endLine")]
        public int EndLine { get; set; }

        [YamlMember(Alias = "content")]
        public string Content { get; set; }

        /// <summary>
        /// The external path for current source if it is not locally available
        /// </summary>
        [YamlMember(Alias = "isExternal")]
        public bool IsExternalPath { get; set; }
    }
}
