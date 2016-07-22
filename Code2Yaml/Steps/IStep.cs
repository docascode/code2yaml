namespace Microsoft.Content.Build.Code2Yaml.Steps
{
    using System.Threading.Tasks;

    public interface IStep
    {
        string StepName { get; }

        Task RunAsync(BuildContext context);
    }
}
