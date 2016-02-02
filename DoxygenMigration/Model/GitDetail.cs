namespace Microsoft.Content.Build.DoxygenMigration.Model
{
    using System;
    using YamlDotNet.Serialization;

    [Serializable]
    public class GitDetail
    {
        /// <summary>
        /// Relative path of current file to the Git Root Directory
        /// </summary>
        [YamlMember(Alias = "path")]
        public string RelativePath { get; set; }

        [YamlMember(Alias = "branch")]
        public string RemoteBranch { get; set; }

        [YamlMember(Alias = "repo")]
        public string RemoteRepositoryUrl { get; set; }

        [YamlIgnore]
        public string LocalWorkingDirectory { get; set; }

        [YamlMember(Alias = "key")]
        public string Description { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (this.GetType() != obj.GetType()) return false;

            return Equals(this.ToString(), obj.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("branch: {0}, url: {1}, local: {2}, desc: {3}, file: {4}", RemoteBranch, RemoteRepositoryUrl, LocalWorkingDirectory, Description, RelativePath);
        }
    }
}
