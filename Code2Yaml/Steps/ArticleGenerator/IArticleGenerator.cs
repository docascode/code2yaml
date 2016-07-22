namespace Microsoft.Content.Build.Code2Yaml.ArticleGenerator
{
    using System;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    
    using Microsoft.Content.Build.Code2Yaml.Model;
    using Microsoft.Content.Build.Code2Yaml.Steps;

    public interface IArticleGenerator : ICloneable
    {
        Task<PageModel> GenerateArticleAsync(ArticleContext context, XDocument document);

        Task PostGenerateArticleAsync(BuildContext context, PageModel page);
    }
}
