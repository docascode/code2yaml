namespace Microsoft.Content.Build.DoxygenMigration.NameGenerator
{
    using Microsoft.Content.Build.DoxygenMigration.Hierarchy;

    public class NameGeneratorContext
    {
        public HierarchyChange CurrentChange { get; set; }

        public HierarchyChange ParentChange { get; set; }
    }
}
