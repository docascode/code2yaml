namespace Microsoft.Content.Build.DoxygenMigration.ArticleGenerator
{
    using System;

    public class ArticleGeneratorFactory
    {
        public static  IArticleGenerator Create(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                throw new ArgumentNullException("language");
            }

            if (language.Equals("cplusplus", StringComparison.OrdinalIgnoreCase))
            {
                return new CppArticleGenerator();
            }
            else if (language.Equals("java", StringComparison.OrdinalIgnoreCase))
            {
                return new JavaArticleGenerator();
            }
            else
            {
                throw new NotSupportedException($"{language} isn't supported!");
            }
        }
    }
}
