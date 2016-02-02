namespace Microsoft.Content.Build.DoxygenMigration.ArticleGenerator
{
    using System.Threading.Tasks;
    using System.Xml.Linq;
    
    using Microsoft.Content.Build.DoxygenMigration.Model;
    using Microsoft.Content.Build.DoxygenMigration.Steps;

    public interface IArticleGenerator
    {
        Task<PageModel> GenerateArticleAsync(ArticleContext context, XDocument document);
    }
}
