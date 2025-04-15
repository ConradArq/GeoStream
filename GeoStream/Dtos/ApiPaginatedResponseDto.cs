using System.Drawing.Printing;
using System.Net;
using System.Text.Json.Serialization;

namespace GeoStream.Dtos
{
    public class ApiPaginatedResponseDto<TData> : ApiResponseDto<TData>
    {
        [JsonPropertyOrder(100)]
        public Pagination pagination { get; set; }

        public ApiPaginatedResponseDto()
        {
            pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 10,
                TotalPages = 10
            };
        }

        public ApiPaginatedResponseDto(int pageNumber, int pageSize, int totalItems, int totalPages, HttpStatusCode statusCode, string? message, TData? data, List<string>? errors)
            : base(statusCode, message, data, errors)
        {
            pagination = new Pagination
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        public class Pagination
        {
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalItems { get; set; }
            public int TotalPages { get; set; }
        }
    }
}
