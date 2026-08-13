using Microsoft.Extensions.Logging;

public static class LogWriter
{
    public static LogLevel MaxLogLevel { get; set; } = LogLevel.Information;
    public static async Task WriteAsync(LogLevel logLevel, string message)
    {
        if (logLevel <= MaxLogLevel)
        {
            Console.Error.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {message}");
        }
    }
    public static async Task WriteInfoAsync(string message) => await WriteAsync(LogLevel.Information, message);
    public static async Task WriteDebugAsync(string message) => await WriteAsync(LogLevel.Debug, message);
    public static async Task WriteTraceAsync(string message) => await WriteAsync(LogLevel.Trace, message);
    public static async Task WriteWarningAsync(string message) => await WriteAsync(LogLevel.Warning, message);
    public static async Task WriteErrorAsync(string message) => await WriteAsync(LogLevel.Error, message);
    public static async Task WriteCriticalAsync(string message) => await WriteAsync(LogLevel.Critical, message);
}