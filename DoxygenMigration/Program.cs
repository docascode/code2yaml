namespace Microsoft.Content.Build.DoxygenMigration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Content.Build.DoxygenMigration.ArticleGenerator;
    using Microsoft.Content.Build.DoxygenMigration.Constants;
    using Microsoft.Content.Build.DoxygenMigration.Steps;

    class Program
    {
        private static string _inputPath;
        private static string _outputPath;
        private static string _gitRepo;
        private static string _gitBranch;

        static void Main(string[] args)
        {
            if (!ValidateArgs(args))
            {
                Console.WriteLine("Unrecognized parameters. Usage : DoxygenMigration.exe <input_path:input path> <output_path:output path> <git_repo: git repository> <git_branch: git branch>");
                return;
            }
            var context = new BuildContext();
            context.SetSharedObject(Constants.Constants.InputPath, _inputPath);
            context.SetSharedObject(Constants.Constants.OutputPath, _outputPath);
            context.SetSharedObject(Constants.Constants.GitRepo, _gitRepo);
            context.SetSharedObject(Constants.Constants.GitBranch, _gitBranch);
            var procedure = new StepCollection(
                new PreprocessXml(),
                new ScanHierarchy(),
                new TaskParallel(
                    new List<IStep>
                    {
                        new GenerateToc(),
                        new GenerateArticles { Generator = new BasicArticleGenerator() },
                    }));
            try
            {
                procedure.RunAsync(context).Wait();
            }
            catch
            {
                // do nothing
            }
            Console.WriteLine(string.Concat(context.Logs.Select(l => l.Message)));
        }

        private static bool ValidateArgs(string[] args)
        {
            if (args.Length != 4)
            {
                return false;
            }

            _inputPath = GetArgValueFromCmdLine(args, Constants.Constants.CmdArgInputPath);
            _outputPath = GetArgValueFromCmdLine(args, Constants.Constants.CmdArgOutputPath);
            _gitRepo = GetArgValueFromCmdLine(args, Constants.Constants.CmdArgGitRepo);
            _gitBranch = GetArgValueFromCmdLine(args, Constants.Constants.CmdArgGitBranch);

            return !(string.IsNullOrEmpty(_inputPath) || string.IsNullOrEmpty(_outputPath) || string.IsNullOrEmpty(_gitRepo) || string.IsNullOrEmpty(_gitBranch));
        }

        private static string GetArgValueFromCmdLine(string[] args, string cmdPrefix)
        {
            string value = args.FirstOrDefault(arg => arg.ToLowerInvariant().StartsWith(cmdPrefix));
            if (value != null)
            {
                value = value.Substring(cmdPrefix.Length).Trim();
            }
            return value;
        }
    }
}
