using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessorService.Infrastructure.Persistence.MongoDB
{
    public class MongoDBSettings
    {
        public string Connection { get; set; } = string.Empty;
        public int MinutesThreshold { get; set; }
        public int IncidentMinutesThreshold { get; set; }
        public string Database { get; set; } = string.Empty;
        public string EmitterCollection { get; set; } = string.Empty;
        public string IncidentCollection { get; set; } = string.Empty;
    }
}
