namespace Microsoft.Content.Build.Code2Yaml.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    [Serializable]
    public class ArticleItemYaml
    {
        [YamlMember(Alias = "uid")]
        public string Uid { get; set; }

        [YamlMember(Alias = "id")]
        public string Id { get; set; }

        [YamlMember(Alias = "parent")]
        public string Parent { get; set; }

        [YamlMember(Alias = "children")]
        public List<string> Children { get; set; }

        [YamlMember(Alias = "href")]
        public string Href { get; set; }

        [YamlMember(Alias = "langs")]
        public string[] SupportedLanguages { get; set; }

        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "nameWithType")]
        public string NameWithType { get; set; }

        [YamlMember(Alias = "nameWithoutTypeParameter")]
        [YamlIgnore]
        public string NameWithoutTypeParameter { get; set; }

        [YamlMember(Alias = "fullName")]
        public string FullName { get; set; }

        [YamlMember(Alias = "fullNameWithoutTypeParameter")]
        [YamlIgnore]
        public string FullNameWithoutTypeParameter { get; set; }

        [YamlMember(Alias = "overload")]
        public string Overload { get; set; }

        [YamlMember(Alias = "type")]
        public MemberType? Type { get; set; }

        [YamlMember(Alias = "source")]
        public SourceDetail Source { get; set; }

        [YamlMember(Alias = "documentation")]
        public SourceDetail Documentation { get; set; }

        [YamlMember(Alias = "header")]
        public SourceDetail Header { get; set; }

        [YamlMember(Alias = "assemblies")]
        public List<string> AssemblyNameList { get; set; }

        [YamlMember(Alias = "namespace")]
        public string NamespaceName { get; set; }

        [YamlMember(Alias = "package")]
        public string PackageName { get; set; }

        [YamlMember(Alias = "summary", ScalarStyle = ScalarStyle.Literal)]
        public string Summary { get; set; }

        [YamlMember(Alias = "remarks")]
        public string Remarks { get; set; }

        [YamlMember(Alias = "example")]
        public List<string> Examples { get; set; }

        [YamlMember(Alias = "syntax")]
        public SyntaxDetailViewModel Syntax { get; set; }

        [YamlMember(Alias = "overridden")]
        public string Overridden { get; set; }

        [YamlMember(Alias = "exceptions")]
        public List<CrefInfo> Exceptions { get; set; }

        [YamlMember(Alias = "seealso")]
        public List<CrefInfo> SeeAlsos { get; set; }

        [YamlMember(Alias = "see")]
        public List<CrefInfo> Sees { get; set; }

        [YamlMember(Alias = "inheritance")]
        //public Dictionary<string, List<string>> Inheritance { get; set; }
        public List<string> Inheritance { get; set; }

        [YamlMember(Alias = "inheritedMembers")]
        public List<string> InheritedMembers { get; set; }

        [YamlMember(Alias = "conceptual")]
        public string Conceptual { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [YamlIgnore]
        public List<SpecializedType> ImplementsOrInherits { get; set; } = new List<SpecializedType>();
    }
}
