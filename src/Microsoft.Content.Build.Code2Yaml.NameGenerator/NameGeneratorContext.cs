namespace Microsoft.Content.Build.Code2Yaml.NameGenerator
{
    using Microsoft.Content.Build.Code2Yaml.DataContracts;

    public class NameGeneratorContext
    {
        public HierarchyChange CurrentChange { get; set; }

        public HierarchyChange ParentChange { get; set; }
    }
}
