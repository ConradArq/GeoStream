using MessageProcessorService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessorService.Domain.Models
{
    /// <summary>
    /// Represents incidents occurred to the asset associated with an Emitter read by the scanner. If there's no asset code 
    /// associated with the Emitter, incidents cannot be generated. When an Emitter is captured by the scanner, the system queries the 
    /// corresponding APIs to determine if any incidents occurred while passing by the scanner. 
    /// Incidents are stored in a dedicated collection in MongoDB, and this model represents a document in that collection.
    /// It also includes details about the scanner, its location, and the asset associated with the Emitter.
    /// </summary>
    public class IncidentEmitterLog : EmitterLog
    {
        public IncidentType IncidentType { get; set; }
    }
}
