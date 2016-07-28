namespace Microsoft.Content.Build.Code2Yaml.Common
{
    using System.IO;

    public static class GitUtility
    {
        public static bool GetGitInfo(string localPath, out string remoteRepoUrl, out string remoteBranch)
        {
            var repoPath = GitSharp.Repository.FindRepository(localPath);
            if (string.IsNullOrEmpty(repoPath))
            {
                remoteRepoUrl = remoteBranch = null;
                return false;
            }
            var directory = Path.GetDirectoryName(repoPath);
            var repo = GitSharp.Core.Repository.Open(directory);
            remoteRepoUrl = repo.Config.getString("remote", "origin", "url");
            remoteBranch = repo.getBranch();
            return true;
        }
    }
}
