namespace Microsoft.Content.Build.DoxygenMigration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Microsoft.Content.Build.DoxygenMigration.ArticleGenerator;
    using Microsoft.Content.Build.DoxygenMigration.Config;
    using Microsoft.Content.Build.DoxygenMigration.NameGenerator;
    using Microsoft.Content.Build.DoxygenMigration.Steps;
    using Microsoft.Content.Build.DoxygenMigration.Utility;

    using Newtonsoft.Json;

    class Program
    {
        private static ConfigModel _config;
        private static string _gitRepo;
        private static string _gitBranch;

        static void Main(string[] args)
        {
            if (!ValidateConfig(args))
            {
                return;
            }
            var context = new BuildContext();
            context.SetSharedObject(Constants.Constants.Config, _config);
            context.SetSharedObject(Constants.Constants.GitRepo, _gitRepo);
            context.SetSharedObject(Constants.Constants.GitBranch, _gitBranch);
            var procedure = new StepCollection(
                new RunDoxygen(),
                new PreprocessXml(),
                new ScanHierarchy(),
                new TaskParallel(
                    new List<IStep>
                    {
                        new GenerateToc { NameGenerator = NameGeneratorFactory.Create(_config.Language) },
                        new GenerateArticles { Generator = ArticleGeneratorFactory.Create(_config.Language) },
                    }));
            string status = "Failed";
            var watch = Stopwatch.StartNew();
            try
            {
                procedure.RunAsync(context).Wait();
                status = "Succeeded";
            }
            catch
            {
                // do nothing
            }
            finally
            {
                watch.Stop();
            }
            Console.WriteLine($"{status} in {watch.ElapsedMilliseconds} milliseconds.");
        }

        private static bool ValidateConfig(string[] args)
        {
            if (args.Length > 1)
            {
                Console.Error.WriteLine("Unrecognized parameters. Usage : DoxygenMigration.exe [code2yaml.json]");
                return false;
            }
            string configPath = args.Length == 0 ? Constants.Constants.ConfigFileName : args[0];
            if (!File.Exists(configPath))
            {
                Console.Error.WriteLine($"Not find config file: {configPath}");
                return false;
            }
            try
            {
                _config = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(configPath));
                GitUtility.GetGitInfo(_config.InputPath, out _gitRepo, out _gitBranch);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fail to deserialize config file: {configPath}. Exception: {ex}");
                return false;
            }
            Console.WriteLine($"Config file {configPath} found. Start processing...");
            return true;
        }
    }
}
