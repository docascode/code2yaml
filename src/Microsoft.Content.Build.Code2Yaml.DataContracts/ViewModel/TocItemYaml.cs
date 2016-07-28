namespace Microsoft.Content.Build.Code2Yaml.DataContracts
{
    using System;

    using YamlDotNet.Serialization;

    [Serializable]
    public class TocItemYaml
    {
        [YamlMember(Alias = "uid")]
        public string Uid { get; set; }

        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "href")]
        public string Href { get; set; }

        [YamlMember(Alias = "originalHref")]
        public string OriginalHref { get; set; }

        [YamlMember(Alias = "homepage")]
        public string Homepage { get; set; }

        [YamlMember(Alias = "items")]
        public TocYaml Items { get; set; }
    }
}
