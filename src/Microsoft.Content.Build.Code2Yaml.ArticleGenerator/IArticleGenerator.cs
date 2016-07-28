namespace Microsoft.Content.Build.Code2Yaml.ArticleGenerator
{
    using System;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using Microsoft.Content.Build.Code2Yaml.Common;
    using Microsoft.Content.Build.Code2Yaml.DataContracts;

    public interface IArticleGenerator : ICloneable
    {
        Task<PageModel> GenerateArticleAsync(BuildContext context, XDocument document);

        Task PostGenerateArticleAsync(BuildContext context, PageModel page);
    }
}
