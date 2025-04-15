using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoStream.Api.Domain.Interfaces.Models
{
    public interface IBaseDomainModel
    {
        public object Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime? LastModifiedDate { get; set; }
        public string? LastModifiedBy { get; set; }

        public int StatusId { get; set; }
    }
}
