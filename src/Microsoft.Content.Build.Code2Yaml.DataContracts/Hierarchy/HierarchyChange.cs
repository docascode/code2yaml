namespace Microsoft.Content.Build.Code2Yaml.DataContracts
{
    using System.Collections.Generic;

    public class HierarchyChange
    {
        public string Uid { get; set; }

        public string Name { get; set; }

        public string File { get; set; }

        public HierarchyType Type { get; set; }

        public string Parent { get; set; }

        public HashSet<string> Children { get; set; }
    }
}
