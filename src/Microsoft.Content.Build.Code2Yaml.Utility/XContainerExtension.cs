namespace Microsoft.Content.Build.Code2Yaml.Utility
{
    using System.Xml.Linq;
    using System.Xml.XPath;

    public static class XContainerExtension
    {
        private static XElement _nullElement = new XElement("Null");
        private static XAttribute _nullAttribute = new XAttribute("Null", string.Empty);
        private static readonly TripleSlashCommentTransformer _transformer = new TripleSlashCommentTransformer();

        public static XElement NullableElement(this XContainer element, string name)
        {
            return element.Element(name) ?? _nullElement;
        }

        public static XAttribute NullableAttribute(this XElement element, string name)
        {
            return element.Attribute(name) ?? _nullAttribute;
        }

        public static string NullableValue(this XElement element)
        {
            return element.Value == string.Empty ? null : element.Value;
        }

        public static string NullableInnerXml(this XElement element)
        {
            return element.Value == string.Empty ? null : _transformer.Transform(element).CreateNavigator().InnerXml;
        }

        public static string NullableValue(this XAttribute attribute)
        {
            return attribute.Value == string.Empty ? null : attribute.Value;
        }

        public static bool IsNull(this XElement element)
        {
            return element.Name == _nullElement.Name;
        }

        public static bool IsNull(this XAttribute attribute)
        {
            return attribute.Name == _nullAttribute.Name;
        }
    }
}
