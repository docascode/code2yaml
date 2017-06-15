namespace Microsoft.Content.Build.Code2Yaml.Steps
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Content.Build.Code2Yaml.Common;
    using Microsoft.Content.Build.Code2Yaml.Constants;
    using Microsoft.Content.Build.Code2Yaml.DataContracts;
    using DocAsCode.YamlSerialization;

    public class GenerateServiceMappingFile : IStep
    {
        private const string LandingPageTypeService = "Service";

        public string StepName
        {
            get
            {
                return "GenerateServiceMappingFile";
            }
        }

        public Task RunAsync(BuildContext context)
        {
            var config = context.GetSharedObject(Constants.Config) as ConfigModel;
            if (config == null)
            {
                throw new ApplicationException(string.Format("Key: {0} doesn't exist in build context", Constants.Config));
            }
            var mappingConfig = config.ServiceMappingConfig;
            if (mappingConfig == null)
            {
                return Task.FromResult(0);
            }
            string outputPath = mappingConfig.OutputPath;
            var articlesDict = context.GetSharedObject(Constants.ArticleItemYamlDict) as ConcurrentDictionary<string, ArticleItemYaml>;

            var newservices = (from article in articlesDict.Values
                               where article.Type == MemberType.Namespace
                               let serviceCategory = Find(mappingConfig.Mappings, FormatPath(article.Source.Path))
                               where serviceCategory != null
                               group article.Uid by serviceCategory into g
                               select g
                              ).ToDictionary(g => g.Key, g => g.ToList());

            List<ServiceMappingItem> others = new List<ServiceMappingItem>();
            Dictionary<string, string> serviceHrefMapping = new Dictionary<string, string>();
            if (File.Exists(outputPath))
            {
                using (var reader = new StreamReader(outputPath))
                {
                    var oldMapping = new YamlDeserializer().Deserialize<ServiceMapping>(reader);
                    var oldservices = (from m in oldMapping[0].items
                                       from sm in m.items ?? Enumerable.Empty<ServiceMappingItem>()
                                       select new { SM = sm, Name = m.name }
                                       ).ToDictionary(i => new ServiceCategory { Service = i.Name, Category = i.SM.name }, i => i.SM.children?.ToList() ?? new List<string>());
                    Merge(newservices, oldservices);
                    var other = oldMapping[0].items.SingleOrDefault(i => i.name == "Other");
                    if (other != null)
                    {
                        others.Add(other);
                    }
                    serviceHrefMapping = oldMapping[0].items.ToDictionary(i => i.name, i => i.href);
                }

            }
            var services = (from item in newservices
                            group item by item.Key.Service into g
                            select new
                            {
                                Service = g.Key,
                                Items = (from v in g
                                         group v.Value by v.Key.Category into g0
                                         select new
                                         {
                                             Category = g0.Key,
                                             Uids = g0.SelectMany(i => i).OrderBy(i => i).Distinct().ToList()
                                         }).ToList(),
                            }).ToDictionary(p => p.Service, p => p.Items);

            var mapping = new ServiceMapping()
            {
                new ServiceMappingItem()
                {
                    uid = "landingpage.reference",
                    name = "Reference",
                    landingPageType = "Root",
                    items = new ServiceMapping((from pair in services
                                           let service = pair.Key
                                           let hrefAndType = GetServiceHrefAndType(serviceHrefMapping, service)
                                           select new ServiceMappingItem()
                                           {
                                               name = service,
                                               href = hrefAndType.Item1,
                                               landingPageType = hrefAndType.Item2,
                                               uid = "landingpage.services." + service,
                                               items = new ServiceMapping(from item in pair.Value
                                                                          let category = item.Category
                                                                          select new ServiceMappingItem()
                                                                          {
                                                                              name = item.Category,
                                                                              uid = "landingpage.services." + service + "." + category,
                                                                              landingPageType = LandingPageTypeService,
                                                                              children = item.Uids.ToList()
                                                                          })
                                           }).OrderBy(s => s.name))
                }
            };
            mapping[0].items.AddRange(others);
            using (var writer = new StreamWriter(outputPath))
            {
                new YamlSerializer().Serialize(writer, mapping);
            }
            return Task.FromResult(0);
        }

        private ServiceCategory Find(Dictionary<string, ServiceCategory> mappings, string path)
        {
            foreach (var mapping in mappings)
            {
                if (path.StartsWith(GetPathFromSrcRepository(mapping.Key)))
                {
                    return mapping.Value;
                }
            }
            return null;
        }

        private static string GetPathFromSrcRepository(string path)
        {
            var formatted = FormatPath(path);
            var parts = formatted.Split('/');
            return string.Join("/", parts.Skip(2));
        }

        private static string FormatPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return path.Replace('\\', '/');
        }

        private static void Merge(Dictionary<ServiceCategory, List<string>> a, Dictionary<ServiceCategory, List<string>> b)
        {
            foreach (var pair in b)
            {
                List<string> value;
                if (!a.TryGetValue(pair.Key, out value))
                {
                    a[pair.Key] = new List<string>();
                }
                // to-do: when product repo's mapping file is ready, should change the behavior to overwrite.
                a[pair.Key].AddRange(pair.Value);
            }
        }

        private static Tuple<string, string> GetServiceHrefAndType(Dictionary<string, string> mapping, string service)
        {
            string href;
            if (mapping.TryGetValue(service, out href))
            {
                return Tuple.Create<string, string>(href, null);
            }
            else
            {
                return Tuple.Create<string, string>(null, LandingPageTypeService);
            }
        }
    }
}
