namespace Microsoft.Content.Build.Code2Yaml.DataContracts
{
    using System;

    using YamlDotNet.Serialization;

    [Serializable]
    public class SpecViewModel
    {
        [YamlMember(Alias = "uid")]
        public string Uid { get; set; }

        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "fullName")]
        public string FullName { get; set; }

        [YamlMember(Alias = "isExternal")]
        public bool IsExternal { get; set; }

        [YamlMember(Alias = "href")]
        public string Href { get; set; }
    }
}
