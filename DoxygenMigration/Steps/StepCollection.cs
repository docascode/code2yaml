namespace Microsoft.Content.Build.DoxygenMigration.Steps
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class StepCollection : IStep
    {
        public string StepName
        {
            get { return "StepCollection"; }
        }

        private IEnumerable<IStep> _steps;

        public StepCollection(IEnumerable<IStep> steps)
        {
            if (steps == null)
            {
                throw new ArgumentNullException("steps");
            }
            _steps = steps;
        }

        public StepCollection(params IStep[] steps)
        {
            _steps = steps;
        }

        public async Task RunAsync(BuildContext context)
        {
            foreach (IStep step in _steps)
            {
                try
                {
                    await step.RunAsync(context);
                }
                catch (Exception ex)
                {
                    context.AddLogEntry(
                        new LogEntry
                        {
                            Level = LogLevel.Error,
                            Message = string.Format("Step {0} failed. Error message: {1}.", step.StepName, ex.Message),
                            Data = ex,
                        });
                    // rethrow exception
                    throw;
                }
            }
        }
    }
}
