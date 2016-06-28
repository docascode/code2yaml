namespace Microsoft.Content.Build.DoxygenMigration.ArticleGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Hierarchy;
    using Microsoft.Content.Build.DoxygenMigration.Model;
    using Microsoft.Content.Build.DoxygenMigration.Steps;
    using Microsoft.Content.Build.DoxygenMigration.Utility;

    public abstract class BasicArticleGenerator : IArticleGenerator
    {
        public List<ReferenceViewModel> References { get; set; }

        public BasicArticleGenerator()
        {
            References = new List<ReferenceViewModel>();
        }

        public abstract string Language { get; }

        protected abstract void FillLanguageSpecificMetadata(ArticleItemYaml yaml, ArticleContext context, XElement xmlFragment);

        protected abstract ReferenceViewModel CreateReferenceWithSpec(string uid, List<SpecViewModel> specs);

        protected abstract string WriteAccessLabel(string access);

        protected abstract string NameSpliter { get; }

        public Task<PageModel> GenerateArticleAsync(ArticleContext context, XDocument document)
        {
            PageModel page = new PageModel() { Items = new List<ArticleItemYaml>() };
            ArticleItemYaml mainYaml = new ArticleItemYaml();
            page.Items.Add(mainYaml);

            HierarchyChange curChange = context.CurrentChange;
            HierarchyChange parentChange = context.ParentChange;
            mainYaml.Uid = curChange.Uid;
            mainYaml.Id = YamlUtility.ParseIdFromUid(curChange.Uid, NameSpliter);
            mainYaml.SupportedLanguages = new string[] { Language };
            mainYaml.FullName = curChange.Name;
            mainYaml.Name = YamlUtility.ParseNameFromFullName(curChange.Type, parentChange?.Name, mainYaml.FullName, NameSpliter);
            mainYaml.Href = YamlUtility.ParseHrefFromChangeFile(curChange.File);
            mainYaml.Type = YamlUtility.ParseType(curChange.Type.ToString());
            mainYaml.Parent = curChange.Parent;
            mainYaml.Children = curChange.Children != null ? new List<string>(curChange.Children) : new List<string>();
            var main = document.Root.NullableElement("compounddef");
            mainYaml.Summary = main.NullableElement("briefdescription").NullableInnerXml() ?? ParseSummaryFromDetailedDescription(main.NullableElement("detaileddescription"));
            var location = main.NullableElement("location");
            FillSource(mainYaml, location, context.GitRepo, context.GitBranch);
            FillSees(mainYaml, main.NullableElement("detaileddescription"));
            FillInheritance(mainYaml, document);

            mainYaml.Syntax = new SyntaxDetailViewModel();
            FillTypeParameters(mainYaml.Syntax, main);
            FillParameters(mainYaml.Syntax, main);
            FillReturn(mainYaml.Syntax, main);
            FillSignatureForMainYaml(mainYaml, main);

            FillException(mainYaml, main.NullableElement("detaileddescription"));
            FillLanguageSpecificMetadata(mainYaml, context, main);

            var members = new Dictionary<string, ArticleItemYaml>();
            foreach (var section in document.Root.NullableElement("compounddef").Elements("sectiondef"))
            {
                string kind = section.NullableAttribute("kind").NullableValue();
                var tuple = KindMapToType(kind);
                if (tuple.Item1.HasValue && ((tuple.Item2 & AccessLevel.Private) == AccessLevel.None))
                {
                    foreach (var member in section.Elements("memberdef"))
                    {
                        var memberYaml = new ArticleItemYaml();
                        memberYaml.Uid = member.NullableAttribute("id").NullableValue();
                        memberYaml.Id = YamlUtility.ParseIdFromUid(memberYaml.Uid, NameSpliter);
                        memberYaml.SupportedLanguages = new string[] { Language };
                        memberYaml.Name = YamlUtility.ParseMemberName(member.NullableElement("name").NullableValue(), member.NullableElement("argsstring").NullableValue());
                        memberYaml.FullName = YamlUtility.ParseMemberName(member.NullableElement("definition").NullableValue(), member.NullableElement("argsstring").NullableValue());
                        memberYaml.Href = mainYaml.Href;
                        memberYaml.Type = tuple.Item1.Value;
                        memberYaml.Parent = mainYaml.Uid;
                        memberYaml.Summary = member.NullableElement("briefdescription").NullableInnerXml() ?? ParseSummaryFromDetailedDescription(member.NullableElement("detaileddescription"));
                        FillOverridden(memberYaml, member.NullableElement("reimplements"));
                        FillSource(memberYaml, member.NullableElement("location"), context.GitRepo, context.GitBranch);
                        FillSees(memberYaml, member.NullableElement("detaileddescription"));

                        memberYaml.Syntax = new SyntaxDetailViewModel();
                        FillTypeParameters(memberYaml.Syntax, member);
                        FillParameters(memberYaml.Syntax, member);
                        FillReturn(memberYaml.Syntax, member);
                        FillSignature(memberYaml.Syntax, member);

                        FillException(memberYaml, member.NullableElement("detaileddescription"));
                        FillLanguageSpecificMetadata(memberYaml, context, member);

                        if (members.ContainsKey(memberYaml.Uid))
                        {
                            context.Context.AddLogEntry(
                                new LogEntry
                                {
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
            FillInheritedMembers(mainYaml, document);
            FillReferences(page, document, context);

            EmptyToNull(mainYaml);
            return Task.FromResult(page);
        }

        private void EmptyToNull(ArticleItemYaml yaml)
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

        private void FillSignatureForMainYaml(ArticleItemYaml mainYaml, XElement member)
        {
            StringBuilder sb = new StringBuilder();
            string prot = member.NullableAttribute("prot").NullableValue();
            if (prot != null)
            {
                sb.Append(WriteAccessLabel(prot));
            }
            string virt = member.NullableAttribute("virt").NullableValue();
            if (virt == "virtual" || virt == "pure-virtual")
            {
                sb.Append("virtual ");
            }
            string statics = member.NullableAttribute("static").NullableValue();
            if (statics == "yes")
            {
                sb.Append("static ");
            }
            string kind = member.NullableAttribute("kind").NullableValue();
            if (kind != null)
            {
                sb.Append(string.Format("{0} ", kind));
            }
            int index = mainYaml.Name.LastIndexOf(NameSpliter);
            string name = mainYaml.Name.Substring(index < 0 ? 0 : index + NameSpliter.Length);
            sb.Append(name);

            if (!member.NullableElement("templateparamlist").IsNull())
            {
                var typeParams = member.XPathSelectElements("templateparamlist/param").ToList();
                sb.Append($"<{GetTypeParameterString(typeParams[0])}");
                foreach (var param in typeParams.Skip(1))
                {
                    sb.Append($", {GetTypeParameterString(param)}");
                }
                sb.Append(">");
            }

            if (sb.ToString() != string.Empty)
            {
                mainYaml.Syntax.Content = sb.ToString();
            }
        }

        private void FillSignature(SyntaxDetailViewModel syntax, XElement member)
        {
            StringBuilder sb = new StringBuilder();
            string prot = member.NullableAttribute("prot").NullableValue();
            if (prot != null)
            {
                sb.Append(WriteAccessLabel(prot));
            }
            string virt = member.NullableAttribute("virt").NullableValue();
            if (virt == "virtual" || virt == "pure-virtual")
            {
                sb.Append("virtual ");
            }
            string statics = member.NullableAttribute("static").NullableValue();
            if (statics == "yes")
            {
                sb.Append("static ");
            }
            string typeStr = member.NullableElement("type").NullableValue();
            if (typeStr != null)
            {
                sb.Append(typeStr + " ");
            }
            string name = member.NullableElement("name").NullableValue();
            if (name != null)
            {
                sb.Append(name);
            }
            var args = member.NullableElement("argsstring").NullableValue();
            if (args != null)
            {
                sb.Append(args);
            }

            var initializer = member.NullableElement("initializer").NullableValue();
            if (initializer != null)
            {
                sb.Append(initializer);
            }

            if (sb.ToString() != string.Empty)
            {
                syntax.Content = sb.ToString();
            }
        }

        private string GetTypeParameterString(XElement paramElement)
        {
            var typeStr = paramElement.NullableElement("type").NullableValue();
            var typeConstraint = paramElement.NullableElement("typeconstraint");
            if (typeConstraint.IsNull())
            {
                return typeStr;
            }
            var typeConstraintStr = typeConstraint.NullableValue();
            return $"{typeStr} extends {typeConstraintStr}";
        }

        private void FillTypeParameters(SyntaxDetailViewModel syntax, XElement member)
        {
            var templateParamList = member.NullableElement("templateparamlist");
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
                        Type = ParseType(p.NullableElement("type")),
                        Description = ParseParamDescription(member.NullableElement("detaileddescription"), "<" + p.NullableElement("type").NullableValue() + ">")
                    }).ToList();
            }
        }

        private void FillParameters(SyntaxDetailViewModel syntax, XElement member)
        {
            if (member.Elements("param").Any())
            {
                syntax.Parameters = member.Elements("param").Select(
                    p =>
                    new ApiParameter
                    {
                        Name = p.NullableElement("declname").NullableValue(),
                        Type = ParseType(p.NullableElement("type")),
                        Description = ParseParamDescription(member.NullableElement("detaileddescription"), p.NullableElement("declname").NullableValue())
                    }).ToList();
            }
        }

        private void FillReturn(SyntaxDetailViewModel syntax, XElement member)
        {
            string typeStr = member.NullableElement("type").NullableValue();
            if (typeStr != null && (!typeStr.Equals("void", StringComparison.OrdinalIgnoreCase)))
            {
                syntax.Return = new ApiParameter
                {
                    Type = ParseType(member.NullableElement("type")),
                    Description = ParseReturnDescription(member.NullableElement("detaileddescription"))
                };
            }
        }

        private void FillException(ArticleItemYaml yaml, XElement detailedDescription)
        {
            yaml.Exceptions = new List<CrefInfo>();
            var exceptions = detailedDescription.XPathSelectElements("para/parameterlist[@kind='exception']/parameteritem");
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

        private string ParseParamDescription(XElement detailedDescription, string name)
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
            References.Add(CreateReferenceWithSpec(uid, specs));
            return uid;
        }

        private void FillSees(ArticleItemYaml yaml, XElement detailedDescription)
        {
            var sees = detailedDescription.XPathSelectElements("para/simplesect[@kind='see']/para/ref");
            yaml.Sees = (from see in sees
                         select new CrefInfo
                         {
                             Type = see.NullableAttribute("refid").NullableValue(),
                             Description = see.NullableInnerXml()
                         }).ToList();
        }

        private void FillOverridden(ArticleItemYaml yaml, XElement reimplements)
        {
            yaml.Overridden = reimplements.NullableAttribute("refid").NullableValue();
        }

        private void FillSource(ArticleItemYaml yaml, XElement location, string repo, string branch)
        {
            if (!location.IsNull())
            {
                string headerPath = location.NullableAttribute("file").NullableValue();
                string headerStartlineStr = location.NullableAttribute("line").NullableValue();
                string path = location.NullableAttribute("bodyfile").NullableValue();
                string startlineStr = location.NullableAttribute("bodystart").NullableValue();
                int headerStartline = ParseStartline(headerStartlineStr);
                int startline = ParseStartline(startlineStr);
                if (Language == "cplusplus")
                {
                    yaml.Header = new SourceDetail
                    {
                        Remote = new GitDetail { RemoteRepositoryUrl = repo, RemoteBranch = branch, RelativePath = headerPath },
                        Path = headerPath,
                        StartLine = headerStartline,
                    };
                }
                yaml.Source = new SourceDetail
                {
                    Remote = new GitDetail { RemoteRepositoryUrl = repo, RemoteBranch = branch, RelativePath = path ?? headerPath },
                    Path = path ?? headerPath,
                    StartLine = path != null ? startline : headerStartline,
                };
            }
        }

        private void FillInheritance(ArticleItemYaml yaml, XDocument document)
        {
            var nodeIdHash = new Dictionary<string, string>();
            var idHash = new Dictionary<string, List<string>>();
            var inheritanceGraph = document.Root.NullableElement("compounddef").NullableElement("inheritancegraph");
            foreach (var node in inheritanceGraph.Elements("node"))
            {
                string nodeId = node.NullableAttribute("id").NullableValue();
                string id = node.NullableElement("link").NullableAttribute("refid").NullableValue() ?? node.NullableElement("label").NullableValue();
                nodeIdHash.Add(nodeId, id);
                var childNode = node.NullableElement("childnode");
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
            var dict = idHash.GroupBy(pair => nodeIdHash[pair.Key]).ToDictionary(g => g.Key, g => g.SelectMany(p => p.Value).Select(n => nodeIdHash[n]).ToList());
            yaml.Inheritance = new List<string>();
            string start = yaml.Uid;
            while (dict.ContainsKey(start))
            {
                start = dict[start].Single();
                yaml.Inheritance.Add(start);
            }
            yaml.Inheritance.Reverse();

        }

        private void FillInheritedMembers(ArticleItemYaml yaml, XDocument document)
        {
            var allMembers = from m in document.Root.NullableElement("compounddef").NullableElement("listofallmembers").Elements("member")
                             where m.NullableAttribute("prot").NullableValue() != "private"
                             select m.NullableAttribute("refid").NullableValue();
            yaml.InheritedMembers = allMembers.Except(yaml.Children).ToList();
            References.AddRange(yaml.InheritedMembers.Select(i => new ReferenceViewModel { Uid = i }));
        }

        private void FillReferences(PageModel page, XDocument document, ArticleContext context)
        {
            var referenceIds = (from node in document.XPathSelectElements("//node()[@refid and not(parent::listofallmembers) and not(ancestor::inheritancegraph) and not(ancestor::collaborationgraph)]")
                                select node.NullableAttribute("refid").NullableValue()).Where(r => r != null).Distinct();

            // add nested children for namespace api
            var curChange = context.CurrentChange;
            if (curChange.Type == HierarchyType.Namespace)
            {
                referenceIds = referenceIds.Union(curChange.Children);
            }

            References.AddRange(referenceIds.Select(refid => new ReferenceViewModel { Uid = refid }));

            page.References = References.Distinct(new ReferenceEqualComparer()).ToList();
        }

        private static int ParseStartline(string startlineStr)
        {
            return startlineStr != null ? int.Parse(startlineStr) - 1 : 0;
        }

        /// <summary>
        /// <code>
        /// <detailedDescription>
        /// <para>Selects a specific subscription for the APIs to work with. </para>
        /// <para>there is some other text<parameterlist>...</parameterlist><simplesect>...</simplesect></para>
        /// </detailedDescription>
        /// </code>
        /// </summary>
        private static string ParseSummaryFromDetailedDescription(XElement detailedDescription)
        {
            var cloned = new XElement(detailedDescription);
            foreach (var node in cloned.XPathSelectElements("//parameterlist | //simplesect | //computeroutput").ToList())
            {
                node.Remove();
            }
            return cloned.NullableInnerXml();
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

        public object Clone()
        {
            var cloned = (BasicArticleGenerator)this.MemberwiseClone();
            cloned.References = new List<ReferenceViewModel>();
            return cloned;
        }
    }

    internal static class XContainerExtension
    {
        private static XElement _nullElement = new XElement("Null");
        private static XAttribute _nullAttribute = new XAttribute("Null", string.Empty);
        private static readonly TripleSlashCommentTransformer _transformer = new TripleSlashCommentTransformer();

        public static XElement NullableElement(this XContainer element, string name)
        {
            return element.Element(name) ?? _nullElement;
        }

        public static XAttribute NullableAttribute(this XElement element, string name)
        {
            return element.Attribute(name) ?? _nullAttribute;
        }

        public static string NullableValue(this XElement element)
        {
            return element.Value == string.Empty ? null : element.Value;
        }

        public static string NullableInnerXml(this XElement element)
        {
            return element.Value == string.Empty ? null : _transformer.Transform(element).CreateNavigator().InnerXml;
        }

        public static string NullableValue(this XAttribute attribute)
        {
            return attribute.Value == string.Empty ? null : attribute.Value;
        }

        public static bool IsNull(this XElement element)
        {
            return element.Name == _nullElement.Name;
        }

        public static bool IsNull(this XAttribute attribute)
        {
            return attribute.Name == _nullAttribute.Name;
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
