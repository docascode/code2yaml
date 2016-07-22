namespace Microsoft.Content.Build.Code2Yaml.Steps
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Content.Build.Code2Yaml.Common;
    using Microsoft.Content.Build.Code2Yaml.Config;
    using Microsoft.Content.Build.Code2Yaml.Constants;
    using Microsoft.Content.Build.Code2Yaml.Doxyfile;
    using Microsoft.Content.Build.Code2Yaml.Utility;

    public class RunDoxygen : IStep
    {
        private static readonly string DoxygenLocation = "Tools/doxygen.exe";
        private static readonly string DoxyFileTemplate = $"{typeof(RunDoxygen).Assembly.GetName().Name}.Tools.DoxyfileTemplate";
        private const int TimeoutInMilliseconds = 300000; // 5 minutes

        public string StepName
        {
            get
            {
                return "RunDoxygen";
            }
        }

        public Task RunAsync(BuildContext context)
        {
            return Task.Run(() =>
            {
                string doxyFile = GenerateDoxyfile(context);
                using (var doxygenProcess = new Process())
                {
                    doxygenProcess.StartInfo.FileName = DoxygenLocation;
                    doxygenProcess.StartInfo.UseShellExecute = false;
                    doxygenProcess.StartInfo.CreateNoWindow = true;
                    doxygenProcess.StartInfo.Arguments = doxyFile;
                    doxygenProcess.StartInfo.RedirectStandardError = true;

                    var errorBuilder = new StringBuilder();
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        doxygenProcess.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                errorWaitHandle.Set();
                            }
                            else
                            {
                                errorBuilder.AppendLine(e.Data);
                            }
                        };
                        doxygenProcess.Start();
                        doxygenProcess.BeginErrorReadLine();
                        if (doxygenProcess.WaitForExit(TimeoutInMilliseconds) && errorWaitHandle.WaitOne(TimeoutInMilliseconds))
                        {
                            if (doxygenProcess.ExitCode != 0)
                            {
                                ConsoleLogger.WriteLine(
                                    new LogEntry
                                    {
                                        Phase = StepName,
                                        Level = LogLevel.Error,
                                        Message = $"Run Doxygen failed with exit code {doxygenProcess.ExitCode}. Error message: {errorBuilder.ToString()}.",
                                        Data = $"DoxyFile: {File.ReadAllText(doxyFile)}",
                                    });
                                throw new ApplicationException("RunDoxygen step failed");
                            }
                        }
                        else
                        {
                            doxygenProcess.Kill();
                            ConsoleLogger.WriteLine(
                                new LogEntry
                                {
                                    Phase = StepName,
                                    Level = LogLevel.Error,
                                    Message = $"Run Doxygen timeout in {TimeoutInMilliseconds} milliseconds.",
                                    Data = $"DoxyFile: {File.ReadAllText(doxyFile)}",
                                });
                            throw new TimeoutException("RunDoxygen timed out");
                        }
                    }
                }
            });
        }

        /// <summary>
        /// generate Doxyfile and write to file
        /// </summary>
        /// <returns>doxyfile path</returns>
        private string GenerateDoxyfile(BuildContext context)
        {
            ConfigModel config = context.GetSharedObject(Constants.Config) as ConfigModel;
            string intermediateFolder = StepUtility.GetIntermediateOutputPath(config.OutputPath);
            if (Directory.Exists(intermediateFolder))
            {
                Directory.Delete(intermediateFolder, recursive: true);
            }
            Directory.CreateDirectory(intermediateFolder);
            string doxyfile = Path.Combine(intermediateFolder, "Doxyfile");
            using (var sw = new StreamWriter(doxyfile))
            using (var writer = new DoxyfileWriter(sw))
            {
                var content = DoxyfileParser.ParseDoxyfile(typeof(RunDoxygen).Assembly.GetManifestResourceStream(DoxyFileTemplate));

                // update with config
                content[Constants.Doxyfile.INPUT] = PathUtility.MakeRelativePath(Environment.CurrentDirectory, Path.GetFullPath(config.InputPath));
                content[Constants.Doxyfile.OUTPUT_DIRECTORY] = PathUtility.MakeRelativePath(Environment.CurrentDirectory, Path.GetFullPath(intermediateFolder));
                if (config.ExcludePaths != null)
                {
                    content[Constants.Doxyfile.EXCLUDE] = (from e in config.ExcludePaths
                                                           select PathUtility.MakeRelativePath(Environment.CurrentDirectory, Path.GetFullPath(e))).ToList();
                }

                writer.Write(content);
            }
            return doxyfile;
        }
    }
}
