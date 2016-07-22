namespace Microsoft.Content.Build.DoxygenMigration.Config
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    using Microsoft.Content.Build.DoxygenMigration.Constants;

    [Serializable]
    public class ConfigModel
    {
        [JsonProperty(Constants.InputPath, Required = Required.DisallowNull)]
        public string InputPath { get; set; }

        [JsonProperty(Constants.OutputPath, Required = Required.DisallowNull)]
        public string OutputPath { get; set; }

        [JsonProperty(Constants.Language)]
        public string Language { get; set; } = "cplusplus";

        [JsonProperty(Constants.GenerateTocMDFile)]
        public bool GenerateTocMDFile { get; set; } = false;

        [JsonProperty(Constants.ExcludePaths)]
        public List<string> ExcludePaths { get; set; }
    }
}
