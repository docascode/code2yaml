namespace Microsoft.Content.Build.DoxygenMigration.Model
{
    using System;
    using YamlDotNet.Serialization;

    [Serializable]
    public class CrefInfo
    {
        [YamlMember(Alias = "type")]
        public string Type { get; set; }

        [YamlMember(Alias = "description")]
        public string Description { get; set; }
    }
}
