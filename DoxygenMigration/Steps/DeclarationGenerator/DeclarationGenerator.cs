namespace Microsoft.Content.Build.DoxygenMigration.DeclarationGenerator
{
    using System.Xml.Linq;

    public abstract class DeclarationGenerator
    {
        public abstract string GenerateTypeDeclaration(XElement node);

        public abstract string GenerateMemberDeclaration(XElement node);
    }
}
