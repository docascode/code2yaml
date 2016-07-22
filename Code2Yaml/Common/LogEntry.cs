namespace Microsoft.Content.Build.Code2Yaml.Common
{
    public class LogEntry
    {
        public string Phase { get; set; }

        public LogLevel Level { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }
    }

    public enum LogLevel
    {
        Error,
        Warning,
        Info,
        Verbose,
    }
}
