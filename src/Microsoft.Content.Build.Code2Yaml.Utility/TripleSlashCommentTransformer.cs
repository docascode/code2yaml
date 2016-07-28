namespace Microsoft.Content.Build.Code2Yaml.Utility
{
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    using Microsoft.Content.Build.Code2Yaml.Common;

    public class TripleSlashCommentTransformer
    {
        private readonly XslCompiledTransform _transform;

        public TripleSlashCommentTransformer()
        {
            var type = this.GetType();
            var assembly = type.Assembly;
            var xsltFilePath = $"{type.Namespace}.TripleSlashCommentTransform.xsl";
            using (var stream = assembly.GetManifestResourceStream(xsltFilePath))
            using (var reader = XmlReader.Create(stream))
            {
                var xsltSettings = new XsltSettings(true, true);
                _transform = new XslCompiledTransform();
                _transform.Load(reader, xsltSettings, new XmlUrlResolver());
            }
        }

        public XElement Transform(XElement element)
        {
            using (var ms = new MemoryStream())
            using (var writer = new XHtmlWriter(new StreamWriter(ms)))
            {
                _transform.Transform(element.CreateNavigator(), writer);
                ms.Seek(0, SeekOrigin.Begin);
                return XElement.Load(ms, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }
        }
    }
}
