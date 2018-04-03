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
            Assert.Equal(5, model.Items.Count);

            var item = model.Items.Find(i => i.Name == "App");
            Assert.NotNull(item);
            Assert.Equal(MemberType.Class, item.Type);
            Assert.Equal("com.mycompany.app.App", item.FullName);
            Assert.Equal("<p>\n\n  <xref uid=\"com.mycompany.app._app\" data-throw-if-not-resolved=\"false\">App</xref>'s summary </p>", item.Summary.Replace("\r\n", "\n"));

            item = model.Items.Find(i => i.Name == "main(String[] args)");
            Assert.NotNull(item);
            Assert.Equal(MemberType.Method, item.Type);
            Assert.Equal("<p>Main's summary, continued from the line above </p>\n<p>It needs a `</p>\n<p>` to start another line </p>", item.Summary.Replace("\r\n", "\n"));

            item = model.Items.Find(i => i.Name == "testCommentsWithList()");
            Assert.NotNull(item);
            Assert.Equal(MemberType.Method, item.Type);
            Assert.Equal("<p>Test a list:<ul><li><p>first item</p></li><li><p>second item </p></li></ul></p>", item.Summary.Replace("\r\n", "\n"));

            item = model.Items.Find(i => i.Name == "testCommentsWithApiNote()");
            Assert.NotNull(item);
            Assert.Equal(MemberType.Method, item.Type);
            Assert.Equal(@"## examples
Here is a sample code:
[!code-java[Sample_Code](~/_sample/APITests.java?name={Sample_code1} ""Sample for ContainerURL.create"")] ".Replace("\r\n", "\n"),
item.Remarks.Replace("\r\n", "\n"));

            item = model.Items.Find(i => i.Name == "testCommentsWithBr()");
            Assert.NotNull(item);
            Assert.Equal(MemberType.Method, item.Type);
            Assert.Equal("<p>This is first line. <br />\n\n This is second line. </p>", item.Summary.Replace("\r\n", "\n"));
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
