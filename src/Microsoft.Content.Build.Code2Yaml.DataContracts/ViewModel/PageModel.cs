namespace Microsoft.Content.Build.Code2Yaml.DataContracts
{
    using System;
    using System.Collections.Generic;
    using YamlDotNet.Serialization;

    [Serializable]
    public class PageModel
    {
        [YamlMember(Alias = "items")]
        public List<ArticleItemYaml> Items { get; set; } = new List<ArticleItemYaml>();

        [YamlMember(Alias = "metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        [YamlMember(Alias = "references")]
        public List<ReferenceViewModel> References { get; set; } = new List<ReferenceViewModel>();
    }
}
