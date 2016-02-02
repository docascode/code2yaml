namespace Microsoft.Content.Build.DoxygenMigration.Model
{
    using System;
    using System.Collections.Generic;
    using YamlDotNet.Serialization;

    [Serializable]
    public class SyntaxDetailViewModel
    {
        [YamlMember(Alias = "content")]
        public string Content { get; set; }

        [YamlMember(Alias = "parameters")]
        public List<ApiParameter> Parameters { get; set; }

        [YamlMember(Alias = "typeParameters")]
        public List<ApiParameter> TypeParameters { get; set; }

        [YamlMember(Alias = "return")]
        public ApiParameter Return { get; set; }
    }
}
