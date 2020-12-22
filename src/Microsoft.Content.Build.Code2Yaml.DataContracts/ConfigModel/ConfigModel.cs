namespace Microsoft.Content.Build.Code2Yaml.DataContracts
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    using Microsoft.Content.Build.Code2Yaml.Constants;

    [Serializable]
    public class ConfigModel
    {
        [JsonProperty(Constants.InputPaths, Required = Required.DisallowNull)]
        public List<string> InputPaths { get; set; }

        [JsonProperty(Constants.OutputPath, Required = Required.DisallowNull)]
        public string OutputPath { get; set; }

        [JsonProperty(Constants.Language)]
        public string Language { get; set; } = "java";

        [JsonProperty(Constants.GenerateTocMDFile)]
        public bool GenerateTocMDFile { get; set; } = false;

        [JsonProperty(Constants.GeneratePrivate)]
        public bool GenerateDocForPrivateParts { get; set; } = false;

        [JsonProperty(Constants.ExcludePaths)]
        public List<string> ExcludePaths { get; set; }

        [JsonProperty(Constants.ServiceMapping)]
        public ServiceMappingConfig ServiceMappingConfig { get; set; }

        public int DoxygenTimeout { get; set; }  = 300000; // InMilliseconds 5 minutes
    }
}
