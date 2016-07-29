namespace Microsoft.Content.Build.Code2Yaml.Common
{
    using System.Collections.Concurrent;
    using System.IO;

    public static class GitUtility
    {
        private static readonly ConcurrentDictionary<string, GitSharp.Core.Repository> RepoCache = new ConcurrentDictionary<string, GitSharp.Core.Repository>();
        private static readonly ConcurrentDictionary<string, GitInfo> Cache = new ConcurrentDictionary<string, GitInfo>();

        public static GitInfo GetGitInfo(string localPath)
        {
            if (string.IsNullOrEmpty(localPath) || !PathUtility.IsPathExisted(localPath))
            {
                return null;
            }
            return Cache.GetOrAdd(Path.GetFullPath(localPath), p => GetGitInfoCore(p));
        }

        private static GitInfo GetGitInfoCore(string localPath)
        {
            var repoPath = GitSharp.Repository.FindRepository(localPath);
            if (string.IsNullOrEmpty(repoPath))
            {
                return null;
            }
            var directory = Path.GetDirectoryName(repoPath);
            var repo = RepoCache.GetOrAdd(directory, p => GitSharp.Core.Repository.Open(directory));
            var info = new GitInfo { LocalWorkingDirectory = repo.WorkingDirectory.FullName };
            if (repo.Head != null)
            {
                info.RemoteRepoUrl = repo.Config.getString("remote", "origin", "url");
                info.RemoteBranch = repo.getBranch();
            }
            return info;
        }
    }

    public class GitInfo
    {
        public string RemoteRepoUrl { get; set; }

        public string RemoteBranch { get; set; }

        public string LocalWorkingDirectory { get; set; }
    }
}
