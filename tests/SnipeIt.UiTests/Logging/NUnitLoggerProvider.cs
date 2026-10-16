using System.Globalization;
using Microsoft.Extensions.Logging;

namespace SnipeIt.UiTests.Logging;

/// <summary>Writes one plain line per log entry to the test's output: console logger, TRX and Allure all show it.</summary>
internal sealed class NUnitLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new NUnitLogger(categoryName);

    public void Dispose()
    {
    }

    private sealed class NUnitLogger(string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            var line = string.Create(CultureInfo.InvariantCulture,
                $"{DateTime.UtcNow:HH:mm:ss.fff} {logLevel,-11} {category}: {formatter(state, exception)}");
            if (exception is not null)
                line += Environment.NewLine + exception;
            TestContext.Out.WriteLine(line);
        }
    }
}
