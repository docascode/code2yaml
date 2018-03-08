namespace Microsoft.Content.Build.Code2Yaml.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Microsoft.Content.Build.Code2Yaml.ArticleGenerator;
    using Microsoft.Content.Build.Code2Yaml.Common;
    using Microsoft.Content.Build.Code2Yaml.DataContracts;
    using Microsoft.Content.Build.Code2Yaml.NameGenerator;
    using Microsoft.Content.Build.Code2Yaml.Steps;
    using Microsoft.DocAsCode.Common;
    using Xunit;

    public class Code2YamlTest : IDisposable
    {
        private string _workingFolder;

        public Code2YamlTest()
        {
            var folder = Path.GetRandomFileName();
            if (Directory.Exists(folder))
            {
                folder += DateTime.Now.ToString("HHmmssffff");
                if (Directory.Exists(folder))
                {
                    throw new InvalidOperationException($"Random folder name collides {folder}");
                }
            }
            Directory.CreateDirectory(folder);
            _workingFolder = folder;
        }

        [Fact]
        public void TestMetadataFromJavaProject()
        {
            // arrange
            var outputFolder = Path.Combine(_workingFolder, "output");
            var config = new ConfigModel
            {
                InputPaths = new List<string> { "TestData" },
                Language = "java",
                OutputPath = outputFolder,
            };
            var context = new BuildContext();
            context.SetSharedObject(Constants.Constants.Config, config);
            var procedure = new StepCollection(
                new RunDoxygen(),
                new PreprocessXml(),
                new ScanHierarchy(),
                new TaskParallel(
                    new List<IStep>
                    {
                        new GenerateToc { NameGenerator = NameGeneratorFactory.Create(config.Language) },
                        new GenerateArticles { Generator = ArticleGeneratorFactory.Create(config.Language) },
                    }));

            // act
            procedure.RunAsync(context).Wait();

            // assert
            var outputPath = Path.Combine(outputFolder, "com.mycompany.app._app.yml");
            Assert.True(File.Exists(outputPath));
            var model = YamlUtility.Deserialize<PageModel>(outputPath);
            Assert.Equal(2, model.Items.Count);

            Assert.Equal(MemberType.Class, model.Items[0].Type);
            Assert.Equal("com.mycompany.app.App", model.Items[0].FullName);
            Assert.Equal("App", model.Items[0].Name);
            Assert.Equal("<p>\n\n  <xref uid=\"com.mycompany.app._app\" data-throw-if-not-resolved=\"false\">App</xref>'s summary </p>", model.Items[0].Summary);

            Assert.Equal(MemberType.Method, model.Items[1].Type);
            Assert.Equal("main(String[] args)", model.Items[1].Name);
            Assert.Equal("<p>Main's summary </p>", model.Items[1].Summary);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_workingFolder))
                {
                    Directory.Delete(_workingFolder, true);
                }
            }
            catch
            {

            }
        }
    }
}
