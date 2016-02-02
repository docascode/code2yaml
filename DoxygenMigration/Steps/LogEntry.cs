namespace Microsoft.Content.Build.DoxygenMigration.Steps
{
    public class LogEntry
    {
        public LogLevel Level { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }
    }

    public enum LogLevel
    {
        Error,
        Warning,
        Info,
    }
}
