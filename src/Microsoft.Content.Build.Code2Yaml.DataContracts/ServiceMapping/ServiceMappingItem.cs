namespace Microsoft.Content.Build.Code2Yaml.DataContracts
{
    using System.Collections.Generic;

    public class ServiceMappingItem
    {
        public string name { get; set; }
        public string uid { get; set; }
        public string href { get; set; }
        public string landingPageType { get; set; }
        public List<string> children { get; set; }
        public ServiceMapping items { get; set; }
    }
}
