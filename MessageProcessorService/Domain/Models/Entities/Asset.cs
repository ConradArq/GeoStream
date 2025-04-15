using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessorService.Domain.Models.Entities
{
    public class Asset
    {
        public int Id { get; set; }
        public string LicenseCode { get; set; } = string.Empty;
    }
}
