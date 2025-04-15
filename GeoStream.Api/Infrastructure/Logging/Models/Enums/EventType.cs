using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoStream.Api.Infrastructure.Logging.Models.Enums
{
    public enum EventType
    {
        Create = 1,
        Update = 2,
        Delete = 3,
        Login = 4,
        Error = 5,
        Others = 6
    }
}
