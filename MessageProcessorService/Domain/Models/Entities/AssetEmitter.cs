using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessorService.Domain.Models.Entities
{
    public class AssetEmitter
    {
        public bool IsActive { get; set; }
        public string Emitter { get; set; } = string.Empty;

        public int AssetId { get; set; }
    }
}
