namespace Microsoft.Content.Build.DoxygenMigration.Steps
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Hierarchy;

    public class ScanHierarchy : IStep
    {
        private Dictionary<string, HierarchyChange> _changeDict;

        public string StepName
        {
            get { return "ScanHierarchy"; }
        }

        public Task RunAsync(BuildContext context)
        {
            string inputPath = context.GetSharedObject(Constants.InputPath) as string;
            if (inputPath == null)
            {
                throw new ApplicationException(string.Format("Key: {0} doesn't exist in build context", Constants.InputPath));
            }

            // Parse Index file
            string indexFile = Path.Combine(inputPath, Constants.IndexFileName);
            using (var stream = File.OpenRead(indexFile))
            {
                XDocument doc = XDocument.Load(stream);
                _changeDict = (from ele in doc.Root.Elements("compound")
                               let uid = (string)ele.Attribute("refid")
                               let type = ParseType((string)ele.Attribute("kind"))
                               where uid != null && type.HasValue
                               select new HierarchyChange
                               {
                                   Uid = uid,
                                   Name = (string)ele.Element("name"),
                                   File = uid + Constants.XmlExtension,
                                   Type = type.Value,
                               }).ToDictionary(c => c.Uid);
            }

            // Parse File to get parent/children info and package-private items
            // assume that couldn't define public class inside package-private class
            var itemsPackagePrivate = new List<string>();
            foreach (var pair in _changeDict)
            {
                using (var stream = File.OpenRead(Path.Combine(inputPath, pair.Value.File)))
                {
                    HashSet<string> children = new HashSet<string>();
                    string parent = pair.Key;
                    XDocument doc = XDocument.Load(stream);
                    var def = doc.Root.Element("compounddef");
                    if (def == null)
                    {
                        throw new ApplicationException(string.Format("there is no compounddef section for {0}", parent));
                    }

                    // filter out package-private item
                    var prot = (string)def.Attribute("prot");
                    if ("package".Equals(prot, StringComparison.OrdinalIgnoreCase))
                    {
                        itemsPackagePrivate.Add(pair.Key);
                        continue;
                    }

                    var innerClasses = def.Elements("innerclass").Where(e => !"package".Equals((string)e.Attribute("prot"), StringComparison.OrdinalIgnoreCase));
                    foreach (var inner in innerClasses)
                    {
                        string innerId = (string)inner.Attribute("refid");
                        HierarchyChange change;
                        if (innerId == null || !_changeDict.TryGetValue(innerId, out change))
                        {
                            throw new ApplicationException(string.Format("Inner {0} isn't in change dict.", innerId));
                        }
                        change.Parent = parent;
                        children.Add(innerId);
                    }
                    pair.Value.Children = children;
                }
            }
            foreach (var key in itemsPackagePrivate)
            {
                _changeDict.Remove(key);
            }

            // remove namespace that is empty and update its parent
            var dict = new Dictionary<string, HierarchyChange>(_changeDict);
            foreach (var change in from c in _changeDict.Values
                                   where c.Type == HierarchyType.Namespace
                                   orderby c.Children.Count
                                   select c)
            {
                if (change.Children.Count() == 0)
                {
                    if (!dict.Remove(change.Uid))
                    {
                        throw new ApplicationException(string.Format("fail to remove empty namespace change: {0}", change.Uid));
                    }

                    if (change.Parent != null)
                    {
                        dict[change.Parent].Children.Remove(change.Uid);
                    }
                }
            }
            _changeDict = dict;

            // update innerclass's parent to its outerclass's parent recursively until namespace
            foreach (var pair in _changeDict)
            {
                string parent = pair.Value.Parent;
                string originalParent = parent;
                while (parent != null)
                {
                    var parentChange = _changeDict[parent];
                    if (parentChange.Type == HierarchyType.Namespace)
                    {
                        pair.Value.Parent = parent;
                        if (originalParent != parent)
                        {
                            _changeDict[parent].Children.Add(pair.Key);
                            _changeDict[originalParent].Children.Remove(pair.Key);
                        }
                        break;
                    }
                    parent = parentChange.Parent;
                }
            }

            context.SetSharedObject(Constants.Changes, _changeDict);
            return Task.FromResult(1);
        }

        private static HierarchyType? ParseType(string typeStr)
        {
            HierarchyType htype;
            if (typeStr == null || !Enum.TryParse<HierarchyType>(typeStr, true, out htype))
            {
                return null;
            }
            return htype;
        }
    }
}
