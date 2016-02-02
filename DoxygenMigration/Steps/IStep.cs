namespace Microsoft.Content.Build.DoxygenMigration.Steps
{
    using System.Threading.Tasks;

    public interface IStep
    {
        string StepName { get; }

        Task RunAsync(BuildContext context);
    }
}
