namespace Microsoft.Content.Build.Code2Yaml.ArticleGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using Microsoft.Content.Build.Code2Yaml.Common;
    using Microsoft.Content.Build.Code2Yaml.Constants;
    using Microsoft.Content.Build.Code2Yaml.DataContracts;
    using Microsoft.Content.Build.Code2Yaml.DeclarationGenerator;
    using Microsoft.Content.Build.Code2Yaml.NameGenerator;
    using Microsoft.Content.Build.Code2Yaml.Utility;

    public abstract class BasicArticleGenerator : IArticleGenerator
    {
        private List<ReferenceViewModel> _references = new List<ReferenceViewModel>();
        private INameGenerator _nameGenerator;
        private DeclarationGenerator _declarationGenerator;
        private static readonly Regex GenericMethodPostFix = new Regex(@"``\d+$", RegexOptions.Compiled);

        public BasicArticleGenerator(INameGenerator nameGenerator, DeclarationGenerator declarationGenerator)
        {
            _nameGenerator = nameGenerator;
            _declarationGenerator = declarationGenerator;
        }

        #region Abstract/Virtual Members
        public abstract string Language { get; }

        protected abstract void FillLanguageSpecificMetadata(ArticleItemYaml yaml, ArticleContext context, XElement node);

        protected abstract ReferenceViewModel CreateReferenceWithSpec(string uid, List<SpecViewModel> specs);

        protected abstract IEnumerable<string> GetDefaultInheritance(ArticleItemYaml yaml);
        #endregion

        public Task<PageModel> GenerateArticleAsync(BuildContext context, XDocument document)
        {
            PageModel page = new PageModel() { Items = new List<ArticleItemYaml>() };
            ArticleItemYaml mainYaml = new ArticleItemYaml();
            page.Items.Add(mainYaml);

            var articleContext = new ArticleContext(context);
            HierarchyChange curChange = articleContext.CurrentChange;
            HierarchyChange parentChange = articleContext.ParentChange;
            var nameContext = new NameGeneratorContext { CurrentChange = curChange, ParentChange = parentChange };
            var main = document.Root.NullableElement("compounddef");
            mainYaml.Uid = curChange.Uid;
            mainYaml.Id = _nameGenerator.GenerateId(nameContext, main);
            mainYaml.SupportedLanguages = new string[] { Language };
            mainYaml.FullName = _nameGenerator.GenerateTypeFullName(nameContext, main, true);
            mainYaml.Name = _nameGenerator.GenerateTypeName(nameContext, main, true);
            mainYaml.NameWithType = mainYaml.Name;
            mainYaml.FullNameWithoutTypeParameter = _nameGenerator.GenerateTypeFullName(nameContext, main, false);
            mainYaml.NameWithoutTypeParameter = _nameGenerator.GenerateTypeName(nameContext, main, false);
            mainYaml.Href = YamlUtility.ParseHrefFromChangeFile(curChange.File);
            mainYaml.Type = YamlUtility.ParseType(curChange.Type.ToString());
            mainYaml.Parent = curChange.Parent;
            mainYaml.Children = curChange.Children != null ? new List<string>(curChange.Children.OrderBy(c => c)) : new List<string>();
            FillSummary(mainYaml, main);
            FillRemarks(mainYaml, main);
            FillSource(mainYaml, main);
            FillSees(mainYaml, main);
            FillException(mainYaml, main);
            FillInheritance(nameContext, mainYaml, main);
            FillSyntax(mainYaml, main, isMain: true);
            FillImplementsOrInherits(mainYaml, main);
            FillLanguageSpecificMetadata(mainYaml, articleContext, main);

            var members = new Dictionary<string, ArticleItemYaml>();
            foreach (var section in main.Elements("sectiondef"))
            {
                string kind = section.NullableAttribute("kind").NullableValue();
                var tuple = KindMapToType(kind);
                ConfigModel config = (ConfigModel) context.GetSharedObject(Constants.Config);
                bool generate = config.GenerateDocForPrivateParts ||
                         ((tuple.Item2 & AccessLevel.NotAccessible) == AccessLevel.None);

                if (tuple.Item1.HasValue && generate)
                {
                    foreach (var member in section.Elements("memberdef"))
                    {
                        var memberYaml = new ArticleItemYaml();
                        memberYaml.Uid = member.NullableAttribute("id").NullableValue();
                        memberYaml.Id = _nameGenerator.GenerateId(nameContext, member);
                        memberYaml.SupportedLanguages = new string[] { Language };
                        memberYaml.FullName = _nameGenerator.GenerateMemberFullName(nameContext, member);
                        memberYaml.Name = _nameGenerator.GenerateMemberName(nameContext, member);
                        memberYaml.NameWithType = _nameGenerator.GenerateMemberNameWithType(memberYaml.Name, mainYaml.Name);
                        memberYaml.Href = mainYaml.Href;
                        memberYaml.Type = string.IsNullOrEmpty(member.NullableElement("type").NullableValue()) && tuple.Item1.Value == MemberType.Method ? MemberType.Constructor : tuple.Item1.Value;
                        memberYaml.Parent = mainYaml.Uid;
                        FillSummary(memberYaml, member);
                        FillRemarks(memberYaml, member);
                        FillSource(memberYaml, member);
                        FillSees(memberYaml, member);
                        FillException(memberYaml, member);
                        FillOverridden(memberYaml, member);
                        FillSyntax(memberYaml, member, isMain: false);
                        FillLanguageSpecificMetadata(memberYaml, articleContext, member);
                        FillOverload(memberYaml, member);

                        if (members.ContainsKey(memberYaml.Uid))
                        {
                            ConsoleLogger.WriteLine(
                                new LogEntry
                                {
                                    Phase = "GenerateArticles",
                                    Level = LogLevel.Warning,
                                    Message = $"Duplicate items {memberYaml.Uid} found in {curChange.File}.",
                                });
                            continue;
                        }
                        members[memberYaml.Uid] = memberYaml;
                        EmptyToNull(memberYaml);
                    }
                }
            }

            mainYaml.Children.AddRange(from p in members
                                       orderby p.Value.Name.ToLower()
                                       select p.Key);
            page.Items.AddRange(from i in members.Values
                                orderby i.Name.ToLower()
                                select i);

            // after children are filled, fill inherited members
            FillInheritedMembers(mainYaml, main);
            FillReferences(page, document, articleContext);

            EmptyToNull(mainYaml);
            return Task.FromResult(page);
        }

        public Task PostGenerateArticleAsync(BuildContext context, PageModel page)
        {
            var mainItem = page.Items[0];
            var infoDict = (IReadOnlyDictionary<string, ArticleItemYaml>)context.GetSharedObject(Constants.ArticleItemYamlDict);
            if (mainItem.ImplementsOrInherits != null && mainItem.ImplementsOrInherits.Count > 0)
            {
                mainItem.Syntax.Content += _declarationGenerator.GenerateInheritImplementString(infoDict, mainItem);
            }
            return Task.FromResult(1);
        }

        protected void FillOverload(ArticleItemYaml yaml, XElement node)
        {
            var kind = node.NullableAttribute("kind").NullableValue();
            var type = KindMapToType(kind).Item1;
            switch (type)
            {
                case MemberType.Method:
                case MemberType.Constructor:
                    yaml.Overload = AddOverloadReference(yaml);
                    break;
            }
        }

        private string AddOverloadReference(ArticleItemYaml yaml)
        {
            string id = yaml.Uid;
            var uidBody = RemoveArgs(id);
            uidBody = GenericMethodPostFix.Replace(uidBody, string.Empty);
            string uid = uidBody + "*";
            ReferenceViewModel reference = new ReferenceViewModel()
            {
                Uid = uid,
                Name = RemoveArgs(yaml.Name),
                FullName = RemoveArgs(yaml.FullName),
                NameWithType = RemoveArgs(yaml.NameWithType),
                PackageName = yaml.PackageName,
            };
            _references.Add(reference);
            return uid;
        }

        private static string RemoveArgs(string original)
        {
            var index = original.IndexOf('(');
            if (index != -1)
            {
                return original.Remove(index);
            }
            return original;
        }

        protected void FillSummary(ArticleItemYaml yaml, XElement node)
        {
            yaml.Summary = node.NullableElement("briefdescription").NullableInnerXml() + ParseSummaryFromDetailedDescription(node.NullableElement("detaileddescription"));
            if (yaml.Summary == string.Empty)
            {
                yaml.Summary = null;
            }
        }

        protected void FillRemarks(ArticleItemYaml yaml, XElement node)
        {
            var par = node.XPathSelectElement("detaileddescription/para/simplesect[@kind='par']");
            if (par?.NullableInnerXmlRemoveBr()?.StartsWith("<title>API Note:</title>") != true)
            {
                return;
            }
            yaml.Remarks = WebUtility.HtmlDecode(node.XPathSelectElement("detaileddescription/para/simplesect[@kind='par']/para").NullableInnerXmlRemoveBr());
            if (yaml.Remarks == string.Empty)
            {
                yaml.Remarks = null;
            }
        }

        protected void FillSyntax(ArticleItemYaml yaml, XElement node, bool isMain)
        {
            yaml.Syntax = new SyntaxDetailViewModel();
            FillTypeParameters(yaml.Syntax, node);
            FillParameters(yaml.Syntax, node);
            FillReturn(yaml.Syntax, node);
            FillDeclaration(yaml.Syntax, node, isMain);
        }

        protected void FillTypeParameters(SyntaxDetailViewModel syntax, XElement node)
        {
            var templateParamList = node.NullableElement("templateparamlist");
            if (templateParamList.IsNull())
            {
                return;
            }
            if (templateParamList.Elements("param").Any())
            {
                syntax.TypeParameters = templateParamList.Elements("param").Select(
                    p =>
                    new ApiParameter
                    {
                        Name = p.NullableElement("type").NullableValue(),
                        Type = ParseType(p.NullableElement("type")),
                        Description = ParseParameterDescription(node.NullableElement("detaileddescription"), "<" + p.NullableElement("type").NullableValue() + ">")
                    }).ToList();
            }
        }

        protected void FillParameters(SyntaxDetailViewModel syntax, XElement node)
        {
            if (node.Elements("param").Any())
            {
                syntax.Parameters = node.Elements("param").Select(
                    p =>
                    new ApiParameter
                    {
                        Name = p.NullableElement("declname").NullableValue(),
                        Type = ParseType(p.NullableElement("type")),
                        Description = ParseParameterDescription(node.NullableElement("detaileddescription"), p.NullableElement("declname").NullableValue())
                    }).ToList();
            }
        }

        protected void FillReturn(SyntaxDetailViewModel syntax, XElement node)
        {
            string typeStr = node.NullableElement("type").NullableValue();
            if (!string.IsNullOrEmpty(typeStr) && (!typeStr.Equals("void", StringComparison.OrdinalIgnoreCase)))
            {
                syntax.Return = new ApiParameter
                {
                    Type = ParseType(node.NullableElement("type")),
                    Description = ParseReturnDescription(node.NullableElement("detaileddescription"))
                };
            }
        }

        protected void FillDeclaration(SyntaxDetailViewModel syntax, XElement node, bool isMain)
        {
            string declaration = isMain ? _declarationGenerator.GenerateTypeDeclaration(node) : _declarationGenerator.GenerateMemberDeclaration(node);
            if (!string.IsNullOrEmpty(declaration))
            {
                syntax.Content = declaration;
            }
        }

        protected void FillException(ArticleItemYaml yaml, XElement node)
        {
            yaml.Exceptions = new List<CrefInfo>();
            var exceptions = node.XPathSelectElements("detaileddescription/para/parameterlist[@kind='exception']/parameteritem");
            foreach (var ex in exceptions)
            {
                yaml.Exceptions.Add(
                    new CrefInfo()
                    {
                        Type = ParseType(ex.XPathSelectElement("parameternamelist/parametername")),
                        Description = ex.NullableElement("parameterdescription").NullableInnerXml(),
                    });
            }
        }

        protected void FillSees(ArticleItemYaml yaml, XElement node)
        {
            var sees = node.XPathSelectElements("detaileddescription/para/simplesect[@kind='see']/para/ref");
            yaml.Sees = (from see in sees
                         select new CrefInfo
                         {
                             Type = see.NullableAttribute("refid").NullableValue(),
                             Description = see.NullableInnerXml()
                         }).ToList();
        }

        protected void FillOverridden(ArticleItemYaml yaml, XElement node)
        {
            yaml.Overridden = node.NullableElement("reimplements").NullableAttribute("refid").NullableValue();
        }

        protected void FillSource(ArticleItemYaml yaml, XElement node)
        {
            var location = node.NullableElement("location");
            if (!location.IsNull())
            {
                string headerPath = location.NullableAttribute("file").NullableValue();
                string headerStartlineStr = location.NullableAttribute("line").NullableValue();
                int headerStartline = ParseStartline(headerStartlineStr);
                string bodyPath = location.NullableAttribute("bodyfile").NullableValue();
                string bodyStartlineStr = location.NullableAttribute("bodystart").NullableValue();
                int bodyStartline = ParseStartline(bodyStartlineStr);
                string path = bodyPath ?? headerPath;
                int startLine = path == bodyPath ? bodyStartline : headerStartline;
                var info = GitUtility.GetGitInfo(path);
                string relativePath = GetRelativePath(path, info?.LocalWorkingDirectory);
                yaml.Source = new SourceDetail
                {
                    Remote = new GitDetail { RemoteRepositoryUrl = info?.RemoteRepoUrl, RemoteBranch = info?.RemoteBranch, RelativePath = relativePath },
                    Path = relativePath,
                    StartLine = startLine,
                };
            }
        }

        protected void FillHeader(ArticleItemYaml yaml, XElement node)
        {
            var location = node.NullableElement("location");
            if (!location.IsNull())
            {
                string headerPath = location.NullableAttribute("file").NullableValue();
                string headerStartlineStr = location.NullableAttribute("line").NullableValue();
                int headerStartline = ParseStartline(headerStartlineStr);
                var info = GitUtility.GetGitInfo(headerPath);
                string relativePath = GetRelativePath(headerPath, info?.LocalWorkingDirectory);
                yaml.Header = new SourceDetail
                {
                    Remote = new GitDetail { RemoteRepositoryUrl = info?.RemoteRepoUrl, RemoteBranch = info?.RemoteBranch, RelativePath = relativePath },
                    Path = relativePath,
                    StartLine = headerStartline,
                };
            }
        }

        protected void FillInheritance(NameGeneratorContext context, ArticleItemYaml yaml, XElement node)
        {
            if (yaml.Type == MemberType.Interface)
            {
                return;
            }

            var nodeIdHash = new Dictionary<string, string>();
            var idHash = new Dictionary<string, List<string>>();
            var inheritanceGraph = node.NullableElement("inheritancegraph");
            foreach (var n in inheritanceGraph.Elements("node"))
            {
                string nodeId = n.NullableAttribute("id").NullableValue();
                string id = n.NullableElement("link").NullableAttribute("refid").NullableValue() ?? _nameGenerator.GenerateLabel(context, n);
                nodeIdHash.Add(nodeId, id);
                var childNode = n.NullableElement("childnode");
                if (!childNode.IsNull())
                {
                    if (!idHash.ContainsKey(nodeId))
                    {
                        idHash[nodeId] = new List<string>();
                    }
                    idHash[nodeId].Add(childNode.NullableAttribute("refid").NullableValue());
                }
            }

            //yaml.Inheritance = idHash.ToDictionary(pair => nodeIdHash[pair.Key], pair => pair.Value.Select(n => nodeIdHash[n]).ToList());
            // var dict = idHash.ToDictionary(pair => nodeIdHash[pair.Key], pair => pair.Value.Select(n => nodeIdHash[n]).ToList());
            var dict = idHash.GroupBy(pair => Regex.Replace(nodeIdHash[pair.Key], "<.*?>", string.Empty)).ToDictionary(g => g.Key, g => g.SelectMany(p => p.Value).Select(n => nodeIdHash[n]).ToList());
            yaml.Inheritance = new List<string>();
            string start = yaml.Uid;
            while (dict.ContainsKey(start))
            {
                start = dict[start].Single();
                yaml.Inheritance.Add(start);
            }
            var defaultInheritance = GetDefaultInheritance(yaml);
            yaml.Inheritance.AddRange(defaultInheritance);
            yaml.Inheritance.Reverse();
        }

        protected void FillInheritedMembers(ArticleItemYaml yaml, XElement node)
        {
            var allMembers = from m in node.NullableElement("listofallmembers").Elements("member")
                             where !YamlUtility.IsFiltered(m.NullableAttribute("prot").NullableValue())
                             select m.NullableAttribute("refid").NullableValue();
            yaml.InheritedMembers = allMembers.Except(yaml.Children).ToList();
            _references.AddRange(yaml.InheritedMembers.Select(i => new ReferenceViewModel { Uid = i }));
        }

        protected void FillReferences(PageModel page, XDocument document, ArticleContext context)
        {
            var referenceIds = (from node in document.XPathSelectElements("//node()[@refid and not(parent::listofallmembers) and not(ancestor::inheritancegraph) and not(ancestor::collaborationgraph) and not(self::innerclass)]")
                                select node.NullableAttribute("refid").NullableValue()).Where(r => r != null).Distinct();

            // add nested children for namespace api
            var curChange = context.CurrentChange;
            if (curChange.Type == HierarchyType.Namespace)
            {
                referenceIds = referenceIds.Union(curChange.Children);
            }

            _references.AddRange(referenceIds.Select(refid => new ReferenceViewModel { Uid = refid }));

            page.References = _references.Distinct(new ReferenceEqualComparer()).ToList();
        }

        protected void FillImplementsOrInherits(ArticleItemYaml yaml, XElement node)
        {
            foreach (var basenode in node.Elements("basecompoundref"))
            {
                string refId = basenode.NullableAttribute("refid").NullableValue();
                string specializedFullName = YamlUtility.RegularizeName(basenode.NullableValue(), Constants.Dot);
                if (refId != null)
                {
                    yaml.ImplementsOrInherits.Add(new SpecializedType { Type = refId, SpecializedFullName = specializedFullName });
                }
            }
        }

        private string ParseParameterDescription(XElement detailedDescription, string name)
        {
            var param = detailedDescription.XPathSelectElement(string.Format("para/parameterlist[@kind='param']/parameteritem[parameternamelist/parametername[text() = '{0}']]/parameterdescription", name));
            if (param == null)
            {
                return null;
            }
            return param.NullableInnerXml();
        }

        private string ParseReturnDescription(XElement detailedDescription)
        {
            var returnValue = detailedDescription.XPathSelectElement("para/simplesect[@kind='return']");
            if (returnValue == null)
            {
                return null;
            }
            return returnValue.NullableInnerXml();
        }

        /// <summary>
        /// <code>
        /// <detailedDescription>
        /// <para>Selects a specific subscription for the APIs to work with. </para>
        /// <para>there is some other text<parameterlist>...</parameterlist><simplesect>...</simplesect></para>
        /// </detailedDescription>
        /// </code>
        /// </summary>
        private string ParseSummaryFromDetailedDescription(XElement detailedDescription)
        {
            var cloned = new XElement(detailedDescription);
            foreach (var node in cloned.XPathSelectElements("//parameterlist | //simplesect | //computeroutput").ToList())
            {
                node.Remove();
            }
            return cloned.NullableInnerXml();
        }

        private string ParseType(XElement type)
        {
            //if (type.NullableElement("ref").IsNull())
            //{
            //    return type.NullableValue();
            //}
            if (type.IsNull())
            {
                return type.NullableValue();
            }

            List<SpecViewModel> specs = (from node in type.CreateNavigator().Select("node()").Cast<XPathNavigator>()
                                         select node.Name == "ref" ? new SpecViewModel { Uid = node.GetAttribute("refid", string.Empty), IsExternal = false, } : new SpecViewModel { Name = node.Value, FullName = node.Value }).ToList();

            if (specs.Count == 1 && specs[0].Uid != null)
            {
                return specs[0].Uid;
            }
            string uid = string.Concat(specs.Select(spec => spec.Uid ?? StringUtility.ComputeHash(spec.Name)));
            _references.Add(CreateReferenceWithSpec(uid, specs));
            return uid;
        }

        private static int ParseStartline(string startlineStr)
        {
            return startlineStr != null ? int.Parse(startlineStr) - 1 : 0;
        }

        private static Tuple<MemberType?, AccessLevel> KindMapToType(string kind)
        {
            MemberType? type = null;
            AccessLevel level = AccessLevel.None;

            if (kind.Contains("func"))
            {
                type = MemberType.Method;
            }
            else if (kind.Contains("attrib"))
            {
                type = MemberType.Field;
            }
            //else if (kind.Contains("friend"))
            //{
            //    type = MemberType.Friend;
            //}

            if (kind.Contains("public"))
            {
                level |= AccessLevel.Public;
            }
            if (kind.Contains("protected"))
            {
                level |= AccessLevel.Protected;
            }
            if (kind.Contains("private"))
            {
                level |= AccessLevel.Private;
            }
            if (kind.Contains("static"))
            {
                level |= AccessLevel.Static;
            }
            if (kind.Contains("package"))
            {
                level |= AccessLevel.Package;
            }

            return Tuple.Create(type, level);
        }

        private static string GetRelativePath(string original, string basePath)
        {
            if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(basePath))
            {
                return original;
            }

            return PathUtility.MakeRelativePath(Path.GetFullPath(basePath), Path.GetFullPath(original));
        }

        private static void EmptyToNull(ArticleItemYaml yaml)
        {
            if (yaml.Children != null && yaml.Children.Count == 0)
            {
                yaml.Children = null;
            }
            if (yaml.Inheritance != null && yaml.Inheritance.Count == 0)
            {
                yaml.Inheritance = null;
            }
            if (yaml.InheritedMembers != null && yaml.InheritedMembers.Count == 0)
            {
                yaml.InheritedMembers = null;
            }
            if (yaml.Sees != null && yaml.Sees.Count == 0)
            {
                yaml.Sees = null;
            }
            if (yaml.Syntax != null && yaml.Syntax.Parameters != null && yaml.Syntax.Parameters.Count == 0)
            {
                yaml.Syntax.Parameters = null;
            }
            if (yaml.Syntax != null && yaml.Syntax.TypeParameters != null && yaml.Syntax.TypeParameters.Count == 0)
            {
                yaml.Syntax.TypeParameters = null;
            }
            if (yaml.Exceptions != null && yaml.Exceptions.Count == 0)
            {
                yaml.Exceptions = null;
            }
        }

        public object Clone()
        {
            var cloned = (BasicArticleGenerator)this.MemberwiseClone();
            cloned._references = new List<ReferenceViewModel>();
            return cloned;
        }
    }

    internal class ReferenceEqualComparer : IEqualityComparer<ReferenceViewModel>
    {
        public bool Equals(ReferenceViewModel x, ReferenceViewModel y)
        {
            return x.Uid == y.Uid;
        }

        public int GetHashCode(ReferenceViewModel obj)
        {
            return obj.Uid.GetHashCode();
        }
    }
}
