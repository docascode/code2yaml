namespace Microsoft.Content.Build.Code2Yaml.DataContracts
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Content.Build.Code2Yaml.Constants;

    using Newtonsoft.Json;

    public class ServiceMappingConfig
    {
        [JsonProperty(Constants.ServiceMappingConfig.Mappings)]
        public Dictionary<string, ServiceCategory> Mappings { get; set; }

        [JsonProperty(Constants.ServiceMappingConfig.OutputPath)]
        public string OutputPath { get; set; }
    }

    public class ServiceCategory : IEquatable<ServiceCategory>
    {
        [JsonProperty(Constants.ServiceMappingConfig.Service)]
        public string Service { get; set; }

        [JsonProperty(Constants.ServiceMappingConfig.Category)]
        public string Category { get; set; }

        public bool Equals(ServiceCategory other)
        {
            if (other == null)
            {
                return false;
            }
            return Service == other.Service &&
                Category == other.Category;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ServiceCategory);
        }

        public override int GetHashCode()
        {
            return Service.GetHashCode() ^ Category.GetHashCode();
        }
    }
}
