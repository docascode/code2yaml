namespace Microsoft.Content.Build.DoxygenMigration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Content.Build.DoxygenMigration.ArticleGenerator;
    using Microsoft.Content.Build.DoxygenMigration.Config;
    using Microsoft.Content.Build.DoxygenMigration.NameGenerator;
    using Microsoft.Content.Build.DoxygenMigration.Steps;

    using Newtonsoft.Json;

    class Program
    {
        private static ConfigModel _config;

        static void Main(string[] args)
        {
            if (!ValidateConfig(args))
            {
                return;
            }
            var context = new BuildContext();
            context.SetSharedObject(Constants.Constants.Config, _config);
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
            try
            {
                procedure.RunAsync(context).Wait();
            }
            catch
            {
                // do nothing
            }
            Console.WriteLine(string.Join(Environment.NewLine, context.Logs.Select(l => GetFormattedLog(l))));
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
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fail to deserialize config file: {configPath}. Exception: {ex}");
                return false;
            }
            return true;
        }

        private static string GetFormattedLog(LogEntry log)
        {
            return string.Join("\t", log.Level, log.Message, log.Data);
        }
    }
}
