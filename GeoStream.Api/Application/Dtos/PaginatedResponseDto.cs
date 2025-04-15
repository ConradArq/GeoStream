using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GeoStream.Api.Application.Dtos
{
    public class PaginatedResponseDto<TData> : ResponseDto<TData>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public PaginatedResponseDto(TData? data, int pageNumber, int pageSize, int totalItems, string? message = null)
            : base(data, message)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalItems = totalItems;
            TotalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 0;
        }
    }
}
