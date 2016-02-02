namespace Microsoft.Content.Build.DoxygenMigration.Model
{
    using System;
    using System.Collections.Generic;
    using YamlDotNet.Serialization;

    [Serializable]
    public class PageModel
    {
        [YamlMember(Alias = "items")]
        public List<CppYaml> Items { get; set; } = new List<CppYaml>();

        [YamlMember(Alias = "metadata")]
        public Dictionary<string, object> Metadata { get; set; }
    }
}
