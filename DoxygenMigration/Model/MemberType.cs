namespace Microsoft.Content.Build.DoxygenMigration.Model
{
    using System;

    [Serializable]
    public enum MemberType
    {
        Namespace,
        Class,
        Struct,
        Enum,
        Field,
        Method,//MemberFunction
        //Friend,
        Interface,
        Constructor,
    }

    [Flags]
    public enum AccessLevel
    {
        None = 0,
        Private = 1,
        Public = 2,
        Protected = 4,
        Static = 8,
        Virtual = 16,
        Package = 32,
    }
}
