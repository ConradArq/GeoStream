using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace MessageProcessorService.Infrastructure.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private static readonly object _lock = new object();
        private readonly TimeZoneInfo _timeZoneInfo;
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB

        public FileLogger(string filePath, TimeZoneInfo timeZoneInfo)
        {
            _filePath = filePath;
            _timeZoneInfo = timeZoneInfo;
        }

        IDisposable ILogger.BeginScope<TState>(TState state) => NullDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            string message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
                return;

            lock (_lock)
            {
                try
                {
                    DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZoneInfo);

                    if (File.Exists(_filePath) && new FileInfo(_filePath).Length > _maxFileSize)
                    {
                        RotateLogFile();
                    }

                    if (!File.Exists(_filePath))
                    {
                        using (var stream = File.Create(_filePath)) { }
                    }

                    File.AppendAllText(_filePath, $"{localTime:yyyy-MM-dd HH:mm:ss} [{logLevel}] {message}{Environment.NewLine}");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to write to log file: {_filePath}", ex);
                }
            }
        }

        private void RotateLogFile()
        {
            string newFileName = $"{_filePath}_{DateTime.UtcNow:yyyyMMddHHmmss}";
            File.Move(_filePath, newFileName);
        }

        private class NullDisposable : IDisposable
        {
            public static NullDisposable Instance { get; } = new NullDisposable();
            public void Dispose() { }
        }
    }
}
