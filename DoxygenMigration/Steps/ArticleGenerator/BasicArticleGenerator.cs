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

    public class BasicArticleGenerator : IArticleGenerator
    {
        private List<ReferenceViewModel> _references = new List<ReferenceViewModel>();

        public Task<PageModel> GenerateArticleAsync(ArticleContext context, XDocument document)
        {
            PageModel page = new PageModel() { Items = new List<CppYaml>() };
            CppYaml mainYaml = new CppYaml();
            page.Items.Add(mainYaml);

            HierarchyChange curChange = context.CurrentChange;
            HierarchyChange parentChange = context.ParentChange;
            mainYaml.Uid = curChange.Uid;
            mainYaml.Id = YamlUtility.ParseIdFromUid(curChange.Uid);
            mainYaml.NamespaceName = parentChange != null ? parentChange.Name : null;
            mainYaml.Name = YamlUtility.ParseNameFromFullName(curChange.Type, mainYaml.NamespaceName, curChange.Name);
            mainYaml.FullName = curChange.Name;
            mainYaml.Href = YamlUtility.ParseHrefFromChangeFile(curChange.File);
            mainYaml.Type = YamlUtility.ParseType(curChange.Type.ToString());
            mainYaml.Parent = curChange.Parent;
            mainYaml.Children = curChange.Children != null ? new List<string>(curChange.Children) : new List<string>();
            mainYaml.Summary = document.Root.NullableElement("compounddef").NullableElement("briefdescription").NullableValue();
            var location = document.Root.NullableElement("compounddef").NullableElement("location");
            FillSource(mainYaml, location, context.GitRepo, context.GitBranch);
            FillSees(mainYaml, document.Root.NullableElement("compounddef").NullableElement("detaileddescription"));
            FillInheritance(mainYaml, document);
            FillSignatureForMainYaml(mainYaml, document.Root.NullableElement("compounddef"));

            foreach (var section in document.Root.NullableElement("compounddef").Elements("sectiondef"))
            {
                string kind = section.NullableAttribute("kind").NullableValue();
                var tuple = KindMapToType(kind);
                if (tuple.Item1.HasValue && ((tuple.Item2 & AccessLevel.Private) == AccessLevel.None))
                {
                    foreach (var member in section.Elements("memberdef"))
                    {
                        var memberYaml = new CppYaml();
                        memberYaml.Uid = member.NullableAttribute("id").NullableValue();
                        memberYaml.Id = YamlUtility.ParseIdFromUid(memberYaml.Uid);
                        memberYaml.Name = YamlUtility.ParseMemberName(member.NullableElement("name").NullableValue(), member.NullableElement("argsstring").NullableValue());
                        memberYaml.FullName = member.NullableElement("definition").NullableValue();
                        memberYaml.Href = mainYaml.Href;
                        memberYaml.Type = tuple.Item1.Value;
                        memberYaml.Parent = mainYaml.Uid;
                        memberYaml.NamespaceName = mainYaml.NamespaceName;
                        memberYaml.Summary = member.NullableElement("briefdescription").NullableValue();
                        FillOverridden(memberYaml, member.NullableElement("reimplements"));
                        FillSource(memberYaml, member.NullableElement("location"), context.GitRepo, context.GitBranch);
                        FillSees(memberYaml, member.NullableElement("detaileddescription"));

                        memberYaml.Syntax = new SyntaxDetailViewModel();
                        FillSignature(memberYaml.Syntax, member);
                        FillParameters(memberYaml.Syntax, member);
                        FillReturn(memberYaml.Syntax, member);

                        mainYaml.Children.Add(memberYaml.Uid);
                        EmptyToNull(memberYaml);
                        page.Items.Add(memberYaml);
                    }
                }
            }

            // after children are filled, fill inherited members
            FillInheritedMembers(mainYaml, document);

            EmptyToNull(mainYaml);
            FillReferences(page, document, context);
            return Task.FromResult(page);
        }

        private void EmptyToNull(CppYaml yaml)
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

        }

        private void FillSignatureForMainYaml(CppYaml mainYaml, XElement member)
        {
            mainYaml.Syntax = new SyntaxDetailViewModel();
            StringBuilder sb = new StringBuilder();
            string prot = member.NullableAttribute("prot").NullableValue();
            if (prot != null)
            {
                sb.Append(string.Format("{0}: ", prot));
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
            int index = mainYaml.Name.LastIndexOf(Constants.CppSpliter);
            string name = mainYaml.Name.Substring(index < 0 ? 0 : index + Constants.CppSpliter.Length);
            sb.Append(name);

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
                sb.Append(string.Format("{0}: ", prot));
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

        private string ParseParamDescription(XElement detailedDescription, string name)
        {
            var param = detailedDescription.XPathSelectElement(string.Format("para/parameterlist[@kind='param']/parameteritem[parameternamelist/parametername[text() = '{0}']]/parameterdescription", name));
            if (param == null)
            {
                return null;
            }
            return param.NullableValue();
        }

        private string ParseReturnDescription(XElement detailedDescription)
        {
            var returnValue = detailedDescription.XPathSelectElement("para/simplesect[@kind='return']");
            if (returnValue == null)
            {
                return null;
            }
            return returnValue.NullableValue();
        }

        private string ParseType(XElement type)
        {
            if (type.NullableElement("ref").IsNull())
            {
                return type.NullableValue();
            }

            List<SpecViewModel> specs = (from node in type.CreateNavigator().Select("node()").Cast<XPathNavigator>()
                                         select node.Name == "ref" ? new SpecViewModel { Uid = node.GetAttribute("refid", string.Empty), IsExternal = false, } : new SpecViewModel { Name = node.Value, FullName = node.Value}).ToList();
            
            if (specs.Count == 1 && specs[0].Uid != null)
            {
                return specs[0].Uid;
            }
            string uid = string.Concat(specs.Select(spec => spec.Uid ?? spec.Name));
            _references.Add(new ReferenceViewModel
            {
                Uid = uid,
                SpecForCpp = specs,
            });
            return uid;
        }

        private void FillSees(CppYaml yaml, XElement detailedDescription)
        {
            var sees = detailedDescription.XPathSelectElements("para/simplesect[@kind='see']/para/ref");
            yaml.Sees = (from see in sees
                         select new CrefInfo
                         {
                             Type = see.NullableAttribute("refid").NullableValue(),
                             Description = see.NullableValue()
                         }).ToList();
        }

        private void FillOverridden(CppYaml yaml, XElement reimplements)
        {
            yaml.Overridden = reimplements.NullableAttribute("refid").NullableValue();
        }

        private void FillSource(CppYaml yaml, XElement location, string repo, string branch)
        {
            if (!location.IsNull())
            {
                string headerPath = location.NullableAttribute("file").NullableValue();
                string headerStartline = location.NullableAttribute("line").NullableValue();
                string path = location.NullableAttribute("bodyfile").NullableValue();
                string startline = location.NullableAttribute("bodystart").NullableValue();
                yaml.Source = new SourceDetail
                {
                    Remote = new GitDetail { RemoteRepositoryUrl = repo, RemoteBranch = branch, HeaderRelativePath = headerPath, RelativePath = path ?? headerPath },
                    HeaderPath = headerPath,
                    HeaderStartLine = headerStartline != null ? int.Parse(headerStartline) - 1 : 0,
                    Path = path ?? headerPath,
                    StartLine = path != null ? (startline != null ? int.Parse(startline) - 1 : 0) : (headerStartline != null ? int.Parse(headerStartline) - 1 : 0),
                };
            }
        }

        private void FillInheritance(CppYaml yaml, XDocument document)
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
            var dict = idHash.ToDictionary(pair => nodeIdHash[pair.Key], pair => pair.Value.Select(n => nodeIdHash[n]).ToList());
            yaml.Inheritance = new List<string>();
            string start = yaml.Uid;
            while (dict.ContainsKey(start))
            {
                start = dict[start].Single();
                yaml.Inheritance.Add(start);
            }
            yaml.Inheritance.Reverse();
        }

        private void FillInheritedMembers(CppYaml yaml, XDocument document)
        {
            var allMembers = from m in document.Root.NullableElement("compounddef").NullableElement("listofallmembers").Elements("member")
                             where m.NullableAttribute("prot").NullableValue() != "private"
                             select m.NullableAttribute("refid").NullableValue();
            yaml.InheritedMembers = allMembers.Except(yaml.Children).ToList();
        }

        private void FillReferences(PageModel page, XDocument document, ArticleContext context)
        {
            var changesDict = context.Context.GetSharedObject(Constants.Changes) as Dictionary<string, HierarchyChange>;
            if (changesDict == null)
            {
                throw new ApplicationException(string.Format("Key: {0} doesn't exist in build context", Constants.Changes));
            }

            var referenceIds = (from node in document.XPathSelectElements("//node()[@refid and not(parent::listofallmembers) and not(ancestor::inheritancegraph) and not(ancestor::collaborationgraph)]")
                                select node.NullableAttribute("refid").NullableValue()).Distinct();

            // add nested children for namespace api
            var curChange = context.CurrentChange;
            if (curChange.Type == HierarchyType.Namespace)
            {
                referenceIds = referenceIds.Union(curChange.Children);
            }

            foreach (var refid in referenceIds)
            {
                var item = page.Items.SingleOrDefault(i => i.Uid == refid);
                HierarchyChange change;
                changesDict.TryGetValue(refid, out change);
                if (item == null && change == null)
                {
                    continue;
                }
                HierarchyChange parentChange = (change != null && change.Parent != null) ? changesDict[change.Parent] : null;
                string namespaceName = parentChange != null ? parentChange.Name : null;
                _references.Add(new ReferenceViewModel
                {
                    Uid = refid,
                    Type = item != null ? item.Type : YamlUtility.ParseType(change.Type.ToString()),
                    FullName = item != null ? item.FullName : change.Name,
                    Name = item != null ? item.Name : YamlUtility.ParseNameFromFullName(change.Type, namespaceName, change.Name),
                    Href = item != null ? item.Href : YamlUtility.ParseHrefFromChangeFile(change.File),
                    IsExternal = false,
                    Summary = item != null ? item.Summary : null,
                });
            }

            page.References = _references.Distinct(new ReferenceEqualComparer()).ToList();
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

            return Tuple.Create(type, level);
        }
    }

    internal static class XContainerExtension
    {
        private static XElement _nullElement = new XElement("Null");
        private static XAttribute _nullAttribute = new XAttribute("Null", string.Empty);

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
