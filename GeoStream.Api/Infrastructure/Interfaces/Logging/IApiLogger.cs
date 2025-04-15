using GeoStream.Api.Infrastructure.Logging.Models.Enums;
using GeoStream.Api.Infrastructure.Logging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoStream.Api.Infrastructure.Interfaces.Logging
{
    public interface IApiLogger
    {
        void LogInfo(AuditLog logEntry);

        void LogError(ErrorLog logEntry);
    }
}
