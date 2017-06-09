namespace Microsoft.Content.Build.Code2Yaml
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Microsoft.Content.Build.Code2Yaml.ArticleGenerator;
    using Microsoft.Content.Build.Code2Yaml.Common;
    using Microsoft.Content.Build.Code2Yaml.DataContracts;
    using Microsoft.Content.Build.Code2Yaml.NameGenerator;
    using Microsoft.Content.Build.Code2Yaml.Steps;

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
                        new StepCollection(
                            new GenerateArticles { Generator = ArticleGeneratorFactory.Create(_config.Language) },
                            new GenerateServiceMappingFile()),
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
                Console.Error.WriteLine("Unrecognized parameters. Usage : Code2Yaml.exe [code2yaml.json]");
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
                _config = LoadConfig(Path.GetFullPath(configPath));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Fail to deserialize config file: {configPath}. Exception: {ex}");
                return false;
            }
            Console.WriteLine($"Config file {configPath} found. Start processing...");
            return true;
        }

        private static ConfigModel LoadConfig(string configPath)
        {
            var config = JsonConvert.DeserializeObject<ConfigModel>(File.ReadAllText(configPath));
            config.InputPaths = (from p in config.InputPaths
                                 select TransformPath(configPath, p)).ToList();
            config.OutputPath = TransformPath(configPath, config.OutputPath);
            if (config.ExcludePaths != null)
            {
                config.ExcludePaths = (from p in config.ExcludePaths
                                       select TransformPath(configPath, p)).ToList();
            }
            return config;
        }

        private static string TransformPath(string configPath, string path)
        {
            return PathUtility.IsRelativePath(path) ? PathUtility.GetAbsolutePath(configPath, path) : path;
        }
    }
}
