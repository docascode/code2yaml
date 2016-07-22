namespace Microsoft.Content.Build.DoxygenMigration.Steps
{
    using Microsoft.Content.Build.DoxygenMigration.Config;
    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Hierarchy;

    public class ArticleContext
    {
        private readonly BuildContext _context;

        public ArticleContext(BuildContext context)
        {
            _context = context;
        }

        public BuildContext Context
        {
            get
            {
                return _context;
            }
        }

        public ConfigModel Config
        {
            get
            {
                return (ConfigModel)Context.GetSharedObject(Constants.Config);
            }
        }

        public HierarchyChange CurrentChange
        {
            get
            {
                return (HierarchyChange)Context.GetSharedObject(Constants.CurrentChange);
            }
        }

        public HierarchyChange ParentChange
        {
            get
            {
                return (HierarchyChange)Context.GetSharedObject(Constants.ParentChange);
            }
        }

        public string GitRepo
        {
            get
            {
                return (string)Context.GetSharedObject(Constants.GitRepo);
            }
        }

        public string GitBranch
        {
            get
            {
                return (string)Context.GetSharedObject(Constants.GitBranch);
            }
        }

        public string BasePath
        {
            get
            {
                return Config.InputPath;
            }
        }
    }
}
