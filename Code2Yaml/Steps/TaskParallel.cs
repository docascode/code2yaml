namespace Microsoft.Content.Build.Code2Yaml.Steps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class TaskParallel : IStep
    {
        private List<IStep> _children;

        public TaskParallel(List<IStep> children)
        {
            if (children == null)
            {
                throw new ArgumentNullException("children");
            }
            _children = children;
        }

        public string StepName
        {
            get { return "TaskParallel"; }
        }

        public List<IStep> Children
        {
            get
            {
                return _children;
            }
        }

        public async Task RunAsync(BuildContext context)
        {
            if (Children != null && Children.Count != 0)
            {
                await Task.WhenAll(from child in Children select child.RunAsync(context.Clone()));
            }
        }
    }
}
