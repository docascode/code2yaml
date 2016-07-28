namespace Microsoft.Content.Build.Code2Yaml.Steps
{
    using System.Threading.Tasks;

    using Microsoft.Content.Build.Code2Yaml.Common;

    public interface IStep
    {
        string StepName { get; }

        Task RunAsync(BuildContext context);
    }
}
