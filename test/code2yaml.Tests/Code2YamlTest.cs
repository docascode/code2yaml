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
            var outputPath = Path.Combine(outputFolder, "com.mycompany.app.App(class).yml");
            Assert.True(File.Exists(outputPath));
            var model = YamlUtility.Deserialize<PageModel>(outputPath);
            Assert.Equal(10, model.Items.Count);

            var item = model.Items.Find(i => i.Name == "App<T>");
            Assert.NotNull(item);
            Assert.Equal(MemberType.Class, item.Type);
            Assert.Equal("com.mycompany.app.App", item.Uid);
            Assert.Equal("com.mycompany.app.App(class).yml", item.Href);
            Assert.Equal("com.mycompany.app.App<T>", item.FullName);
            Assert.Equal("T", item.Syntax.TypeParameters[0].Name);
            Assert.Equal("<p>App's summary</p>\r\n<p>\r\n  <ul>\r\n    <li>\r\n      <p>Test ScalarStyle for Summary of reference view model. </p>\r\n    </li>\r\n  </ul>\r\n</p>".Replace("\r\n", "\n"), item.Summary.Replace("\r\n", "\n"));

            item = model.Items.Find(i => i.Name == "main(String[] args)");
            Assert.NotNull(item);
            Assert.Equal(MemberType.Method, item.Type);
            Assert.Equal("com.mycompany.app.App<T>.main(String[] args)", item.FullName);
            Assert.Equal("<p>Main's summary, continued from the line above </p>\n<p>It needs a `</p>\n<p>` to start another line </p>", item.Summary.Replace("\r\n", "\n"));

            item = model.Items.Find(i => i.Name == "testCommentsWithList()");
            Assert.NotNull(item);
            Assert.Equal(MemberType.Method, item.Type);
            Assert.Equal("<p>Test a list:<ul><li><p>first item</p></li><li><p>second item </p></li></ul></p>", item.Summary.Replace("\r\n", "\n"));

            item = model.Items.Find(i => i.Name == "testCommentsWithHtmlTag()");
            Assert.NotNull(item);
            Assert.Equal("<p>Write the list with HTML tag in Summary:</p>\n<p>\n  <ul>\n    <li>\n      <p>first item </p>\n    </li>\n    <li>\n      <p>second item </p>\n    </li>\n  </ul>\n</p>", item.Summary.Replace("\r\n", "\n"));

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
            Assert.Equal("<p>This is first line. <br />\n This is second line. </p>", item.Summary.Replace("\r\n", "\n"));

            item = model.Items.Find(i => i.Name == "testCommentsWithExternalLink()");
            Assert.NotNull(item);
            Assert.Equal(MemberType.Method, item.Type);
            Assert.Equal("<p>Test external link. See: <a href=\"https://dotnet.github.io/docfx/\">DocFX</a></p>", item.Summary);

            item = model.Items.Find(i => i.Name == "testIndentationWithPre()");
            Assert.NotNull(item);
            Assert.Equal(MemberType.Method, item.Type);
            Assert.Equal(@"Use pre help keep the indentation in code snippet whithin apiNote 
```Java
// No indentation for line 1 and 2
public void checkIndentation() {
    // 4 spaces indentation
        // 8 spaces indentation
}
```".Replace("\r\n", "\n"), item.Remarks.Replace("\r\n", "\n"));

            item = model.Items.Find(i => i.Name == "testNOTEFormat()");
            Assert.NotNull(item);
            Assert.Equal(MemberType.Method, item.Type);
            Assert.Equal(@">[!NOTE] Here is a Note with a list below:<ul><li><p>item 1</p></li><li><p>item 2 </p></li></ul>", item.Remarks);

            item = model.Items.Find(i => i.Name == "testEncode()");
            Assert.NotNull(item);
            Assert.Equal(MemberType.Method, item.Type);
            Assert.Equal("<p>Not decoded for summary: `&lt;`, `&gt;` </p>", item.Summary);
            Assert.Equal("Decoded in remarks: `<`, `>`\n```Java\n//Decoded in code snippet of remarks: `<`, `>`\n```", item.Remarks.Replace("\r\n", "\n"));

            var checkNameUidFormatPath = Path.Combine(outputFolder, "com.mycompany.app.App.testIfCode2YamlIsCorrectlyConvertFileNameAndIdToRegularizedCompoundNameForLongFileNamesThatWillBeConvertedToHashByDoxygen.yml");
            Assert.True(File.Exists(checkNameUidFormatPath));
            model = YamlUtility.Deserialize<PageModel>(checkNameUidFormatPath);
            item = model.Items.Find(i => i.Uid == "com.mycompany.app.App.testIfCode2YamlIsCorrectlyConvertFileNameAndIdToRegularizedCompoundNameForLongFileNamesThatWillBeConvertedToHashByDoxygen");
            Assert.NotNull(item);

            var checkExtendedTypePath = Path.Combine(outputFolder, "com.mycompany.app.app(namespace).yml");
            Assert.True(File.Exists(checkExtendedTypePath));

            var checkReferenceViewModelPath = Path.Combine(outputFolder, "com.mycompany.app.yml");
            Assert.True(File.Exists(checkReferenceViewModelPath));
            model = YamlUtility.Deserialize<PageModel>(checkReferenceViewModelPath);
            var referenceItem = model.References.Find(i => i.Uid == "com.mycompany.app.App");
            Assert.NotNull(referenceItem);
            Assert.Equal("<p>App's summary</p>\r\n<p>\r\n  <ul>\r\n    <li>\r\n      <p>Test ScalarStyle for Summary of reference view model. </p>\r\n    </li>\r\n  </ul>\r\n</p>".Replace("\r\n", "\n"), referenceItem.Summary.Replace("\r\n", "\n"));
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
