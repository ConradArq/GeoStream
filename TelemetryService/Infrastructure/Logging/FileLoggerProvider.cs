using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryService.Infrastructure.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _path;
        private readonly TimeZoneInfo _timeZoneInfo;

        public FileLoggerProvider(string path, TimeZoneInfo timeZoneInfo)
        {
            _path = path;
            _timeZoneInfo = timeZoneInfo;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_path, _timeZoneInfo);
        }

        public void Dispose()
        {
        }
    }
}
