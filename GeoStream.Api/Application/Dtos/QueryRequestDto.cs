using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoStream.Api.Application.Dtos
{
    public class QueryRequestDto
    {
        /// <summary>
        /// The name of the field to order the results by (e.g., "Name", "DateCreated").
        /// </summary>
        public string? OrderBy { get; set; }

        /// <summary>
        /// The direction of sorting: "asc" for ascending (default), "desc" for descending.
        /// Only applies if <see cref="OrderBy"/> is provided.
        /// </summary>
        public string? OrderDirection { get; set; }

        [DefaultValue((int)Domain.Enums.Status.Active)]
        public int? StatusId { get; set; }
    }
}
