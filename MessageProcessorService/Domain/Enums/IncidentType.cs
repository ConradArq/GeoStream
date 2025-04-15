using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessorService.Domain.Enums
{
    public enum IncidentType
    {
        UnexpectedStop = 1,
        IdleTooLong = 2,
        EarlyDeparture = 3
    }
}
