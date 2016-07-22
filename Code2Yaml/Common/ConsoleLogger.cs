namespace Microsoft.Content.Build.Code2Yaml.Common
{
    using System;
    using System.Text;

    public static class ConsoleLogger
    {
        private static object _lock = new object();

        public static void WriteLine(LogEntry entry)
        {
            var output = new StringBuilder($"{entry.Level}: ");
            if (!string.IsNullOrEmpty(entry.Phase))
            {
                output.Append($"[{entry.Phase}]");
            }
            output.Append(entry.Message);
            if (entry.Data != null)
            {
                output.Append($"Data: {entry.Data}");
            }

            var foregroundColor = Console.ForegroundColor;
            lock (_lock)
            {
                try
                {
                    ChangeConsoleColor(entry.Level);
                    Console.WriteLine(output.ToString());
                }
                finally
                {
                    Console.ForegroundColor = foregroundColor;
                }
            }
        }

        private static void ChangeConsoleColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Verbose:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    throw new NotSupportedException(level.ToString());
            }
        }
    }
}
